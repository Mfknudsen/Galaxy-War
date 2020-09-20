using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace VectorNavigation
{
    public class VectorPathfinding : ScriptableObject
    {
        public bool active = false;
        public NavVector navField = null;
        public bool pathFound = false;

        private VectorPathNode start = null;
        private Vector3 endPoint = Vector3.zero, position = Vector3.zero;

        private List<VectorPathNode> path = new List<VectorPathNode>(), open = new List<VectorPathNode>(), closed = new List<VectorPathNode>();
        private List<Vector3> overlayList = new List<Vector3>();

        public void DebugPath()
        {
            foreach (VectorPathNode node in closed)
            {
                if (!node.isStart)
                    Debug.DrawLine(node.position, node.lastNode.position, Color.blue);
            }
            foreach (VectorPathNode node in open)
            {
                if (!node.isStart)
                    Debug.DrawLine(node.position, node.lastNode.position, Color.red);
            }
            foreach (VectorPathNode node in path)
            {
                if (!node.isStart)
                    Debug.DrawLine(node.position, node.lastNode.position, Color.green);
            }
        }

        public void Setup(NavVector field, bool enable)
        {
            navField = field;
            active = enable;
        }

        #region Calculations:
        public void SetupAroundNode(VectorPathNode node)
        {
            foreach (VectorNode neighbor in node.vecNode.neighbors)
            {
                if (!IsOverlaying(neighbor.GetRelativPosition()) && neighbor.active)
                {
                    VectorPathNode newNode = ScriptableObject.CreateInstance("VectorNavigation.VectorPathNode") as VectorPathNode;
                    newNode.Setup(neighbor, position, endPoint);
                    newNode.lastNode = node;
                    newNode.name = node.name + 1;
                    newNode.lastName = node.name;
                    open.Add(newNode);
                    overlayList.Add(newNode.position);

                }
            }
        }

        private VectorNode GetClosestNode(Vector3 pos)
        {
            VectorNode result = navField.activeNodes[0];

            float curDist = Vector3.Distance(result.GetRelativPosition(), pos);

            foreach (VectorNode node in navField.activeNodes)
            {
                if (node != result && node.active)
                {
                    float newDist = Vector3.Distance(node.GetRelativPosition(), pos);

                    if (newDist < curDist || !result.active)
                    {
                        result = node;
                        curDist = newDist;
                    }
                }
            }

            return result;
        }

        private VectorPathNode GetNextPathNode(List<VectorPathNode> list)
        {
            if (list.Count == 0)
                Debug.Log("List contains nothing");

            if (list.Count == 1)
                return list[0];
            else if (list.Count > 1)
            {
                List<VectorPathNode> best = new List<VectorPathNode>();
                float curBest = list[0].GetWorth();

                foreach (VectorPathNode node in list)
                {
                    if (node.GetWorth() < curBest)
                    {
                        curBest = node.GetWorth();
                        best.Clear();
                        best.Add(node);
                    }
                    else if (node.GetWorth() == curBest)
                        best.Add(node);
                }

                if (best.Count > 1)
                {
                    int curIndex = 0;

                    for (int i = 0; i < best.Count; i++)
                    {
                        if (best[i].endDistance < best[curIndex].endDistance)
                            curIndex = i;
                    }

                    return best[curIndex];
                }
                else if (best.Count == 1)
                    return best[0];
            }

            return null;
        }

        private bool IsOverlaying(Vector3 pos)
        {
            return overlayList.Contains(pos);
        }

        public IEnumerator FindPath(Vector3 lastPoint, Vector3 startPoint)
        {
            position = startPoint;

            pathFound = false;

            open.Clear();
            closed.Clear();
            path.Clear();
            overlayList.Clear();

            VectorPathNode current = CreateInstance("VectorNavigation.VectorPathNode") as VectorPathNode;
            current.Setup(GetClosestNode(position), position, lastPoint);
            current.isStart = true;
            start = current;
            open.Add(start);
            overlayList.Add(start.position);

            endPoint = GetClosestNode(lastPoint).GetRelativPosition();

            int count = 0;
            while (!pathFound && open.Count > 0)
            {
                current = GetNextPathNode(open);
                if (current != null)
                {
                    if (current.position != endPoint)
                        SetupAroundNode(current);
                    else
                    {
                        path.Add(current);
                        pathFound = true;
                    }
                    closed.Add(current);
                    open.Remove(current);

                    count++;
                }

                if (count >= 1000 || current == null)
                {
                    Debug.Log("Failed to reach end");
                    break;
                }
            }

            count = 0;
            if (pathFound)
            {
                while (!path.Contains(start))
                {
                    if (!path.Contains(path[0].lastNode))
                        path.Insert(0, path[0].lastNode);

                    count++;
                    if (count >= 10000)
                    {
                        Debug.Log("Failed to find path");
                        break;
                    }
                }
            }

            yield return null;
        }
        #endregion

        #region Input:
        public void ClearPath()
        {
            pathFound = false;
            path.Clear();
            open.Clear();
            closed.Clear();
        }
        #endregion

        #region Output:
        public Vector3[] GetMovePoints()
        {
            if (path.Count > 0)
            {
                Vector3[] result = new Vector3[path.Count - 1];

                for (int i = 1; i < path.Count; i++)
                {
                    result[i - 1] = path[i].position - path[i - 1].position;
                }

                return result;
            }

            return new Vector3[0];
        }
        #endregion
    }
}
