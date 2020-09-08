using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using AI;
using Elevator;

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

        [Header("Squad State:")]
        public bool containsPlayer = false;
        public int maxSquadMemberCount = 5;
        public List<GameObject> members = new List<GameObject>();
        public AI.State State = 0;

        [Header(" - Navigation:")]
        public AI.WaypointType waypointType = 0;
        public Vector3 curWaypoint = Vector3.zero, lastWaypoint = Vector3.zero;
        public Vector3[] prePositions = new Vector3[0];
        public List<Vector3> waypoints = new List<Vector3>();

        [Header(" - Elevator Values:")]
        public bool toUseElevator = false;
        public Elevator.MeshLinkDetector entryLinkDetector = null;
        public NavMeshAgent Agent = null;
        public Elevator.Elevator mainPart = null;
        [SerializeField] private int entry = 0, exit = 0;
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

            members[0].GetComponent<AI.Core>().isLeader = true;
            prePositions = new Vector3[maxSquadMemberCount];

            calcSquad = ScriptableObject.CreateInstance("Squad.Common") as Squad.Common;
            calcSquad.Setup();
        }

        private void Update()
        {
            if (Dev_Active)
            {
                calcSquad.UpdateMemberPath(members.ToArray(), curWaypoint, prePositions, containsPlayer);
                for (int i = 0; i < members.Count; i++)
                    prePositions[i] = members[i].GetComponent<AI.Core>().curWaypoint;
            }
        }

        public bool AddMemberToCount(GameObject obj, bool isPlayer)
        {
            if (members.Contains(obj) || (isPlayer && containsPlayer))
                return false;
            else
                members = calcSquad.AddMemberToCount(members, obj, maxSquadMemberCount, containsPlayer, isPlayer);

            return true;
        }

        public void RemoveMemberFromCount(GameObject obj, bool isPlayer)
        {
            if (isPlayer && containsPlayer)
                containsPlayer = false;

            if (members.Contains(obj))
                members.Remove(obj);

            if (members.Count == 0)
                DestroySquad();
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
            foreach (GameObject member in members)
            {
                Destroy(member);
            }

            Destroy(gameObject);
        }
    }
}
