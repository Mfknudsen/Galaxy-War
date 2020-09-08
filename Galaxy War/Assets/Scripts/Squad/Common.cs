using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Squad
{
    public class Common : ScriptableObject
    {
        #region Values
        private AI.Common calcAI = null;
        #endregion

        public void Setup()
        {
            calcAI = CreateInstance("AI.Common") as AI.Common;
        }

        #region Management
        public List<GameObject> AddMemberToCount(List<GameObject> members, GameObject obj, int maxCount, bool hasPlayer, bool isPlayer)
        {
            List<GameObject> result = new List<GameObject>();
            result = members;
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

        #endregion

        #region Navigation
        public void UpdateMemberPath(GameObject[] members, Vector3 pos, Vector3[] prePosition, bool containsPlayer)
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
                    c.ReceiveNewWaypoint(pos);

                    if (c.Agent.nextOffMeshLinkData.offMeshLink != null)
                    {
                        if (ElevatorOnPath(c.Agent.nextOffMeshLinkData.offMeshLink))
                        {
                            prePoints[0] = c.Agent.nextOffMeshLinkData.offMeshLink.transform.position;
                            c.ReceiveNewWaypoint(pos);
                        }
                    }

                    if (members.Length > 1)
                    {
                        for (int i = 1; i < members.Length; i++)
                        {
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

        public void EnterOnElevator(GameObject[] members, Elevator.Elevator main, GameObject[] spots, Vector3 origin)
        {
            List<Vector3> prePoints = new List<Vector3>();
            foreach (GameObject obj in members)
            {
                AI.Core c = obj.GetComponent<AI.Core>();

                Vector3 result = calcAI.CalculatePointAroundOrigin(prePoints.ToArray(), origin, 2, 1, LayerMask.NameToLayer("Elevator"));

                c.ReceiveNewWaypoint(result);
            }
        }
        #endregion
    }
}
