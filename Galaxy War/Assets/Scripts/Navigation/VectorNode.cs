using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorNavigation
{
    public class VectorNode : ScriptableObject
    {
        public VectorNode preNode = null;
        public List<VectorNode> neighbors = new List<VectorNode>();

        public Vector3 postition = Vector3.zero;
        public Vector3 direction = Vector3.zero;
        public Vector3 normal = Vector3.up;

        public bool isDone = false;
        public bool active = false;

        public Vector3 GetRelativPosition()
        {
            return postition + direction;
        }
    }
}
