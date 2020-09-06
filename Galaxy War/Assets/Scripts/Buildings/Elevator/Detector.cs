using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elevator
{
    public class Detector : MonoBehaviour
    {
        public Elevator elev = null;
        public LayerMask detectionMask = 0;

        private void OnTriggerEnter(Collider other)
        {

        }

        private void OnTriggerExit(Collider other)
        {

        }
    }
}
