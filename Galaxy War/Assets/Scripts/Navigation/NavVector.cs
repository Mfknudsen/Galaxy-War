using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorNavigation
{
    [ExecuteInEditMode]
    public class NavVector : MonoBehaviour
    {
        [Header("Debug:")]
        public bool dev_Active = false;
        public float dev_Dist = 1;
        public Color dev_Col = Color.white;

        [Header("Editor:")]
        [SerializeField] private bool editActivate = false;

        [Header("Object Reference:")]
        public int state = 0;
        public LayerMask detectionMask = 0;
        public GameObject navObject = null;
        public Transform startTransform = null;
        public float dist = 1;
        public List<VectorNode> startNodes = new List<VectorNode>();
        public List<VectorNode> activeNodes = new List<VectorNode>();
        private List<Vector3> indexList = new List<Vector3>();
        public Vector3[] directions = new Vector3[26];
        public Vector3 offset = Vector3.zero;
        public float height = 1, length = 1, width = 1;

        private void Update()
        {
            if (editActivate)
            {

                activeNodes.Clear();
                indexList.Clear();

                offset = startTransform.position - (startTransform.forward * length * 0.5f + startTransform.right * width * 0.5f + startTransform.up * height * 0.5f);

                SetDirections();

                SetupNavVectorField();
                SetNodeNeighbors();
                SetNodeActive();

                startNodes.Clear();

                editActivate = false;
            }

            if (dev_Active)
            {
                foreach (VectorNode node in activeNodes)
                    Debug.DrawLine(node.postition + node.direction, node.postition + node.direction + (-node.normal * dev_Dist), dev_Col);
            }
        }

        private void SetupNavVectorField()
        {
            float xCount = width / dist, yCount = height / dist, zCount = length / dist;

            for (int x = 0; x < xCount + 1; x++)
            {
                Vector3 right = startTransform.right * x * dist;
                for (int y = 0; y < yCount + 1; y++)
                {
                    Vector3 up = startTransform.up * y * dist;
                    for (int z = 0; z < zCount + 1; z++)
                    {
                        Vector3 forward = startTransform.forward * z * dist;

                        Vector3 dir = (right + up + forward);

                        VectorNode newNode = ScriptableObject.CreateInstance("VectorNavigation.VectorNode") as VectorNode;
                        newNode.postition = offset;
                        newNode.direction = dir;
                        startNodes.Add(newNode);
                        indexList.Add(newNode.GetRelativPosition());
                    }
                }
            }
        }

        private void SetDirections()
        {
            Vector3 calc = Vector3.zero;
            for (int i = 0; i < 26; i++)
            {
                if (i == 0)
                    calc = Vector3.forward;
                else if (i == 1)
                    calc = Vector3.forward + Vector3.up;
                else if (i == 2)
                    calc = Vector3.forward + Vector3.up - Vector3.right;
                else if (i == 3)
                    calc = Vector3.forward + Vector3.up + Vector3.right;
                else if (i == 4)
                    calc = Vector3.forward - Vector3.right;
                else if (i == 5)
                    calc = Vector3.forward + Vector3.right;
                else if (i == 6)
                    calc = Vector3.forward - Vector3.up;
                else if (i == 7)
                    calc = Vector3.forward - Vector3.up - Vector3.right;
                else if (i == 8)
                    calc = Vector3.forward - Vector3.up + Vector3.right;
                else if (i == 9)
                    calc = -Vector3.forward;
                else if (i == 10)
                    calc = -Vector3.forward + Vector3.up;
                else if (i == 11)
                    calc = -Vector3.forward + Vector3.up - Vector3.right;
                else if (i == 12)
                    calc = -Vector3.forward + Vector3.up + Vector3.right;
                else if (i == 13)
                    calc = -Vector3.forward - Vector3.right;
                else if (i == 14)
                    calc = -Vector3.forward + Vector3.right;
                else if (i == 15)
                    calc = -Vector3.forward - Vector3.up;
                else if (i == 16)
                    calc = -Vector3.forward - Vector3.up - Vector3.right;
                else if (i == 17)
                    calc = -Vector3.forward - Vector3.up + Vector3.right;
                else if (i == 18)
                    calc = Vector3.up;
                else if (i == 19)
                    calc = Vector3.up - Vector3.right;
                else if (i == 20)
                    calc = Vector3.up + Vector3.right;
                else if (i == 21)
                    calc = -Vector3.right;
                else if (i == 22)
                    calc = Vector3.right;
                else if (i == 23)
                    calc = -Vector3.up;
                else if (i == 24)
                    calc = -Vector3.up - Vector3.right;
                else if (i == 25)
                    calc = -Vector3.up + Vector3.right;

                directions[i] = startTransform.right * dist * calc.x + startTransform.up * dist * calc.y + startTransform.forward * dist * calc.z;
            }
        }

        private void SetNodeActive()
        {
            foreach (VectorNode node in startNodes)
            {
                node.active = false;

                RaycastHit hit;
                if (Physics.Raycast(node.postition + node.direction, -transform.up, out hit, dist, detectionMask, QueryTriggerInteraction.Ignore))
                {
                    node.active = true;
                    node.normal = hit.normal;
                }
                else if (Physics.Raycast(node.postition + node.direction, transform.up, out hit, dist, detectionMask, QueryTriggerInteraction.Ignore))
                {
                    node.active = true;
                    node.normal = hit.normal;
                }
                else if (Physics.Raycast(node.postition + node.direction, -transform.right, out hit, dist, detectionMask, QueryTriggerInteraction.Ignore))
                {
                    node.active = true;
                    node.normal = hit.normal;
                }
                else if (Physics.Raycast(node.postition + node.direction, transform.right, out hit, dist, detectionMask, QueryTriggerInteraction.Ignore))
                {
                    node.active = true;
                    node.normal = hit.normal;
                }
                else if (Physics.Raycast(node.postition + node.direction, -transform.forward, out hit, dist, detectionMask, QueryTriggerInteraction.Ignore))
                {
                    node.active = true;
                    node.normal = hit.normal;
                }
                else if (Physics.Raycast(node.postition + node.direction, transform.forward, out hit, dist, detectionMask, QueryTriggerInteraction.Ignore))
                {
                    node.active = true;
                    node.normal = hit.normal;
                }

                if (node.active && !activeNodes.Contains(node))
                    activeNodes.Add(node);
                else if (!node.active && activeNodes.Contains(node))
                    activeNodes.Remove(node);
            }
        }

        private void SetNodeNeighbors()
        {
            foreach (VectorNode node in startNodes)
            {
                foreach (Vector3 dir in directions)
                {
                    VectorNode neighborNode = GetNodeFromPosition(node.GetRelativPosition() + dir);
                    if (neighborNode != null)
                    {
                        node.neighbors.Add(neighborNode);
                    }
                }

            }
        }

        private VectorNode GetNodeFromPosition(Vector3 pos)
        {
            int index = GetIndexFromPosition(pos);

            if (index != -1)
                return startNodes[index];
            else
                return null;
        }

        private int GetIndexFromPosition(Vector3 pos)
        {
            return indexList.IndexOf(pos);
        }
    }
}
