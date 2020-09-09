using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Squad
{
    #region Enums
    public enum SquadManageState { Calculate, UpdateMembers }
    public enum ElevatorUseState { Null, Call, Wait, Enter, Exit }
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
        public void UpdateMemberPath(List<AI.Core> members, Vector3 pos, Vector3[] prePosition, bool containsPlayer)
        {
            List<Vector3> prePoints = new List<Vector3>();
            prePoints.Add(pos);

            if (containsPlayer)
            {

            }
            else
            {
                if (Vector3.Distance(prePosition[0], pos) > 2)
                {
                    AI.Core c = members[0].GetComponent<AI.Core>();
                    c.Agent.isStopped = false;
                    c.ReceiveNewWaypoint(pos);

                    if (c.Agent.nextOffMeshLinkData.offMeshLink != null)
                    {
                        if (ElevatorOnPath(c.Agent.nextOffMeshLinkData.offMeshLink))
                        {
                            prePoints[0] = c.Agent.nextOffMeshLinkData.offMeshLink.transform.position;
                            c.ReceiveNewWaypoint(pos);
                        }
                    }

                    if (members.Count > 1)
                    {
                        for (int i = 1; i < members.Count; i++)
                        {
                            members[i].Agent.isStopped = false;
                            Vector3 newPoint = calcAI.CalculatePointAroundOrigin(prePoints.ToArray(), prePoints[0], 5, 3, -1);
                            if (Vector3.Distance(newPoint, prePosition[i]) > 2)
                            {
                                prePoints.Add(newPoint);
                                members[i].GetComponent<AI.Core>().ReceiveNewWaypoint(newPoint);
                            }
                        }
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
            for (int i = 0; i < mainPart.linkPoints.Length; i++)
            {
                if (mainPart.linkPoints[i].transform.position == pos)
                    return i;
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

            core.members[0].Agent.SetDestination(core.curWaypoint);
            Vector3[] prePoints = new Vector3[core.members.Count];
            prePoints[0] = core.curWaypoint;
            core.calcSquad.UpdateMemberPath(core.members, core.curWaypoint, prePoints, core.containsPlayer);
        }
        #endregion
    }
}
