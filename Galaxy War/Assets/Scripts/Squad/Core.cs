using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AI;
using Elevator;

namespace Squad
{
    public class Core : MonoBehaviour
    {
        public bool containsPlayer = false;

        public int maxSquadMemberCount = 5;
        public List<GameObject> members = new List<GameObject>();

        public AI.State State = 0;
        public AI.WaypointType waypointType = 0;

        public bool toUseElevator = false;

        private void Awake()
        {
            members = new List<GameObject>(maxSquadMemberCount);
        }

        public void AddMemberToCount(GameObject obj, bool isPlayer)
        {

        }

        public void EnterOnElevator()
        {

        }

        public void ReceiveNewWaypoint(Vector3 pos, AI.WaypointType type, bool isNew)
        {
            List<Vector3> positions = new List<Vector3>();

            //
            //Next OffMeshLink is part of an Elevator
            if (State == State.Walking || State == State.Partol && entryLinkDetector == null)
            {
                Elevator.OffMeshLink link = Agent.nextOffMeshLinkData.offMeshLink;
                int entry = 0, exit = 0;

                if (link != null)
                {
                    entryLinkDetector = link.gameObject.GetComponent<Elevator.MeshLinkDetector>().Avaiable(currentSquad);

                    if (entryLinkDetector != null)
                    {
                        entry = entryLinkDetector.entryLevel;

                        if (link.endTransform.gameObject != null)
                        {
                            if (entryLinkDetector != null)
                            {
                                exit = entryLinkDetector.main.GetExitLevel(entry, link.endTransform.position);
                                mainPart = entryLinkDetector.main;
                                toUseElevator = true;
                            }
                            else
                                entry = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < members.Count; i++)
            {

            }

            foreach (GameObject member in members)
            {

            }
        }
    }
}
