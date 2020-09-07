using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elevator
{
    public class Detector : MonoBehaviour
    {
        public Elevator elev = null;
        public LayerMask detectionMask = 0;
        public List<GameObject> onPlatform = new List<GameObject>();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.GameObject.isStatic)
                onPlatform.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (onPlatform.Contains(other.gameObject))
                onPlatform.Remove(other.gameObject);
        }
    }
}
