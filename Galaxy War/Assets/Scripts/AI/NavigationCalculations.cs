using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public class NavigationCalculations : ScriptableObject
    {
        LayerMask elevatorLinkMask = 0;
        Vector3 halfExtents = new Vector3(0.25f, 0.25f, 0.25f);

        public void Setup(LayerMask mask)
        {
            elevatorLinkMask = mask;
        }

        public NavMeshPath[] CalculateNavPath(Vector3 startPoint, Vector3 endPoint, bool? debug)
        {
            List<NavMeshPath> result = new List<NavMeshPath>();
            List<Vector3> pathPoints = new List<Vector3>();
            List<GameObject> linkPoints = new List<GameObject>();
            pathPoints.Add(startPoint);

            NavMeshPath direct = new NavMeshPath();

            bool pathFound = NavMesh.CalculatePath(startPoint, endPoint, NavMesh.AllAreas, direct);

            if (pathFound)
            {
                for (int i = 0; i < direct.corners.Length - 1; i++)
                {
                    if (debug.Value)
                        Debug.DrawLine(direct.corners[i], direct.corners[i + 1], Color.green);

                    GameObject obj = CheckCornerIsLink(direct.corners[i]);
                    if (obj != null)
                        linkPoints.Add(obj);
                }

                if (linkPoints.Count > 0)
                {
                    for (int i = 0; i < linkPoints.Count; i++)
                    {

                    }
                }
            }

            return result.ToArray();
        }

        public GameObject CheckCornerIsLink(Vector3 origin)
        {
            Collider[] cols = Physics.OverlapBox(origin, halfExtents, Quaternion.identity, elevatorLinkMask, QueryTriggerInteraction.Collide);
            if (cols.Length < 0)
                return cols[0].gameObject;

            return null;
        }
    }
}
