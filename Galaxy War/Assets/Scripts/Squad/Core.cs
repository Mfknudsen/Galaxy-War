using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;
using UnityEngine.AI;

namespace Squad
{
    public class Core : MonoBehaviour
    {
        #region Values
        [Header("Dev:")]
        public bool Dev = false;
        public Transform[] Dev_WayPoints = new Transform[0];
        public bool Dev_Active = false;

        [Header("Object Reference:")]
        public Squad.Common calcSquad = null;
        public bool toUpdateSquad = true;

        [Header("Squad State:")]
        public bool containsPlayer = false;
        public int maxSquadMemberCount = 5;
        public List<AI.Core> members = new List<AI.Core>();
        public AI.State State = 0;

        [Header(" - Navigation:")]
        public AI.WaypointType waypointType = 0;
        public float dist = 2;
        public Vector3 curWaypoint = Vector3.zero, lastWaypoint = Vector3.zero;
        public Vector3[] prePositions = new Vector3[0];
        public List<Vector3> waypoints = new List<Vector3>();

        [Header(" - - Elevator Values:")]
        public bool toUseElevator = false;
        public bool inCover = false;
        public Squad.ElevatorUseState elevatorUse = 0;
        public NavMeshAgent Agent = null;
        public Elevator.Elevator mainPart = null;
        public int entry = 0, exit = 0;
        public Vector3 usePos = Vector3.zero, resumePoint = Vector3.zero;
        #endregion

        private void Awake()
        {
            if (Dev)
            {
                foreach (Transform t in Dev_WayPoints)
                    waypoints.Add(t.position);

                curWaypoint = waypoints[0];
            }

            if (Agent == null)
                Agent = gameObject.AddComponent<NavMeshAgent>();

            members[0].isLeader = true;
            prePositions = new Vector3[maxSquadMemberCount];

            calcSquad = ScriptableObject.CreateInstance("Squad.Common") as Squad.Common;
            calcSquad.Setup();

            foreach (AI.Core c in members)
                c.Agent.autoTraverseOffMeshLink = false;
        }

        private void Update()
        {
            AI.Core c = members[0];
            if (c.curWaypoint != curWaypoint)
            {
                calcSquad.UpdateMemberPath(members, curWaypoint, dist, containsPlayer);

                toUpdateSquad = false;
            }

            OffMeshLinkData data = c.Agent.nextOffMeshLinkData;
            if (data.valid)
            {
                if (!toUseElevator && data.offMeshLink != null)
                {
                    NavMeshPath path = c.Agent.path;
                    calcSquad.DebugPath(path, Color.red);
                    usePos = calcSquad.GetNextOffMeshLinkStartPosition(path, data.offMeshLink, 1);
                    calcSquad.GetElevatorInformation(this, c.Agent.nextOffMeshLinkData.offMeshLink, usePos);
                }
            }

            if (toUseElevator)
            {
                switch (elevatorUse)
                {
                    case ElevatorUseState.Setup:
                        resumePoint = curWaypoint;
                        curWaypoint = usePos;

                        elevatorUse = ElevatorUseState.Call;
                        break;

                    case ElevatorUseState.Call:
                        if (Vector3.Distance(c.transform.position, usePos) < 2)
                        {
                            calcSquad.CallElevator(mainPart, entry);
                            calcSquad.AddToWaitList(mainPart, entry, this);

                            elevatorUse = ElevatorUseState.Wait;
                        }
                        break;

                    case ElevatorUseState.Wait:
                        if (!inCover)
                        {
                            if (mainPart.waitCover[entry].Length > 0)
                            {
                                foreach (AI.Core member in members)
                                {
                                    GameObject obj = calcSquad.FindBestCover(mainPart.waitCover[entry], usePos);

                                    if (member.isLeader)
                                        curWaypoint = obj.transform.position;

                                    member.ReceiveNewWaypoint(obj.transform.position);
                                }
                            }

                            inCover = true;
                        }
                        break;

                    case ElevatorUseState.Enter:
                        if (curWaypoint != mainPart.platform.position)
                        {
                            curWaypoint = mainPart.platform.position;
                            toUpdateSquad = true;

                            if (!mainPart.nextFloorList.Contains(exit))
                                mainPart.nextFloorList.Add(exit);
                        }

                        if (mainPart.state == Elevator.State.Closing && mainPart.onPlatformList.Contains(this))
                            elevatorUse = ElevatorUseState.Use;
                        break;

                    case ElevatorUseState.Use:
                        if (mainPart.curLevel == mainPart.nextLevel && mainPart.curLevel == exit && mainPart.state == Elevator.State.Open)
                            elevatorUse = ElevatorUseState.Exit;
                        break;

                    case ElevatorUseState.Exit:
                        if (mainPart.curLevel == exit && mainPart.state == Elevator.State.Open && curWaypoint != resumePoint)
                        {
                            curWaypoint = resumePoint;
                        }
                        break;
                }
            }
        }

        public bool AddMemberToCount(AI.Core core, bool isPlayer)
        {
            if (members.Contains(core) || (isPlayer && containsPlayer))
                return false;
            else
                members = calcSquad.AddMemberToCount(members, core, maxSquadMemberCount, containsPlayer, isPlayer);

            return true;
        }

        public bool RemoveMemberFromCount(AI.Core core, bool isPlayer)
        {
            int preCount = members.Count;
            calcSquad.RemoveMemberFromCount(members, core, isPlayer);

            return false;
        }

        public void ReceiveNewWaypoint(Vector3 pos, AI.WaypointType type, bool isNew)
        {
            if (!containsPlayer)
            {
                if (type != waypointType)
                {
                    waypoints.Clear();
                    waypointType = type;
                }
                else if (isNew)
                    waypoints.Clear();

                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(members[0].transform.position, pos, 1, path))
                {
                    if (type == AI.WaypointType.Patrol_Conteniusly || type == AI.WaypointType.Patrol_FromTo)
                        waypoints.Add(members[0].transform.position);

                    waypoints.Add(pos);
                }
                else
                    Debug.Log("Could not use new Waypoint!");
            }
        }

        private void DestroySquad()
        {
            foreach (AI.Core member in members)
            {
                Destroy(member);
            }

            Destroy(gameObject);
        }
    }
}
