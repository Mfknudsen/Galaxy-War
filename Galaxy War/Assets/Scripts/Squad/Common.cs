using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Squad
{
    #region Enums
    public enum SquadManageState { Calculate, UpdateMembers }
    public enum ElevatorUseState { Setup, Call, Wait, Enter, Use, Exit }
    #endregion

    public class Common : ScriptableObject
    {
        #region Values
        private AI.Common calcAI = null;
        #endregion

        #region Dev
        public void DebugPath(NavMeshPath path, Color c)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], c);
        }
        #endregion

        public void Setup()
        {
            calcAI = CreateInstance("AI.Common") as AI.Common;
        }

        #region Management
        public List<AI.Core> AddMemberToCount(List<AI.Core> members, AI.Core obj, int maxCount, bool hasPlayer, bool isPlayer)
        {
            List<AI.Core> result = members;

            if ((members.Count < maxCount) && !(isPlayer && hasPlayer))
            {
                if (isPlayer && !hasPlayer)
                {
                    result.Insert(0, obj);
                }
                else
                    result.Add(obj);
            }

            return result;
        }

        public List<AI.Core> RemoveMemberFromCount(List<AI.Core> members, AI.Core core, bool isPlayer)
        {
            if (members.Contains(core))
                members.Remove(core);

            return members;
        }
        #endregion

        #region Navigation
        public void UpdateMemberPath(List<AI.Core> members, Vector3 pos, float radius, bool containsPlayer)
        {
            Vector3[] prePoints = new Vector3[members.Count];
            prePoints[0] = pos;

            if (containsPlayer)
            {

            }
            else
            {
                AI.Core c = members[0];
                c.Agent.isStopped = false;
                c.ReceiveNewWaypoint(pos);
            }

            if (members.Count > 1)
            {
                for (int i = 1; i < members.Count; i++)
                {
                    AI.Core c = members[i];
                    c.Agent.isStopped = false;
                    Vector3 newPoint = calcAI.CalculatePointAroundOrigin(prePoints, pos, radius, 3, -1);

                    if (Vector3.Distance(newPoint, prePoints[i]) > 1)
                    {
                        prePoints[i] = newPoint;
                        c.ReceiveNewWaypoint(newPoint);
                    }
                }
            }
        }

        public Vector3 GetNextOffMeshLinkStartPosition(NavMeshPath path, OffMeshLink link, float dist)
        {
            foreach (Vector3 v in path.corners)
            {
                if (Vector3.Distance(v, link.startTransform.position) <= dist)
                    return link.startTransform.position;
                else if (Vector3.Distance(v, link.endTransform.position) <= dist)
                    return link.endTransform.position;
            }

            return Vector3.zero;
        }
        #endregion

        #region Waitzone
        public GameObject FindBestCover(Cover[] covers, Vector3 entry)
        {
            GameObject result = covers[0].usableCoverSpots[0];
            float angel = Vector3.Angle(entry, result.transform.position); ;

            foreach (Cover main in covers)
            {
                foreach (GameObject c in main.usableCoverSpots)
                {
                    float checkAngel = Vector3.Angle(entry, c.transform.position);

                    if (checkAngel < angel)
                    {
                        angel = checkAngel;
                        result = c;
                    }
                }
            }

            return result;
        }
        #endregion

        #region ElevatorUse
        public bool ElevatorOnPath(OffMeshLink link)
        {
            Elevator.MeshLinkDetector check = null;

            if (link != null)
                check = link.GetComponent<Elevator.MeshLinkDetector>();
            else
                Debug.Log(link);

            if (check != null)
                return true;

            return false;
        }

        public void GetElevatorInformation(Squad.Core core, OffMeshLink link, Vector3 startPos)
        {
            Elevator.Elevator mainPart = link.GetComponent<Elevator.MeshLinkDetector>().main;
            core.mainPart = mainPart;

            if (link.startTransform.position == startPos)
            {
                core.entry = GetLevel(mainPart, link.startTransform.position);
                core.exit = GetLevel(mainPart, link.endTransform.position);
            }
            else
            {
                core.entry = GetLevel(mainPart, link.endTransform.position);
                core.exit = GetLevel(mainPart, link.startTransform.position);
            }

            if (core.entry != core.exit || mainPart != null)
                core.toUseElevator = true;
        }

        public int GetLevel(Elevator.Elevator mainPart, Vector3 pos)
        {
            if (mainPart != null)
            {
                for (int i = 0; i < mainPart.linkPoints.Length; i++)
                {
                    if (mainPart.linkPoints[i].transform.position == pos)
                        return i;
                }
            }

            return -1;
        }

        public void CallElevator(Elevator.Elevator mainPart, int entryLevel)
        {
            if (!mainPart.floorsToVisit.Contains(entryLevel))
                mainPart.floorsToVisit.Add(entryLevel);
        }

        public bool IsElevatorComingToFloor(Elevator.Elevator mainPart, int level)
        {
            if (mainPart.floorsToVisit.Contains(level))
                return true;

            return false;
        }

        public void AddToWaitList(Elevator.Elevator mainPart, int level, Squad.Core core)
        {
            mainPart.linkPoints[level].GetComponent<Elevator.MeshLinkDetector>().inWait.Add(core);
        }

        public void EnterOnElevator(Squad.Core core, Elevator.Elevator mainPart)
        {
            core.curWaypoint = mainPart.platform.position;
        }
        #endregion
    }
}
