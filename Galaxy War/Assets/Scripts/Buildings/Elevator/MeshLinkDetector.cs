using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Elevator
{
    public class MeshLinkDetector : MonoBehaviour
    {
        public NavMeshLink link = null;
        public Elevator main = null;
        public int maxSquadCount;
        public List<Squad.Core> inWait = new List<Squad.Core>();

        public int entryLevel = 0;

        private void Awake()
        {
            if (link == null)
                link = GetComponent<NavMeshLink>();
        }

        public MeshLinkDetector Avaiable(Squad.Core newSquad)
        {
            if (main.active && inWait.Count < maxSquadCount)
            {
                inWait.Add(newSquad);
                return this;
            }

            return null;
        }
    }
}