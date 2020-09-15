using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elevator
{
    public class Waitzone : MonoBehaviour
    {
        public bool ready = false;
        public Transform[] cornerTransforms = new Transform[2];
        private Vector3[] corners = new Vector3[8];
        private int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };
        private Mesh colliderMesh = null;

        private void Start()
        {
            Vector3 p1, p2, p3, p4;
            Vector3 start = cornerTransforms[0].position, end = cornerTransforms[1].position;
            Vector3[] verts = new Vector3[8];
            float y;
            if (start.y < end.y)
                y = start.y - (end.y - start.y) / 2;
            else
                y = end.y - (start.y - end.y) / 2;

            if (start.x < end.x)
            {
                if (start.y > end.y)
                {
                    p1 = start;
                    p2 = new Vector3(end.x, start.y);
                    p3 = new Vector3(start.x, end.y);
                    p4 = end;
                }
                else
                {
                    p1 = new Vector3(start.x, end.y);
                    p2 = end;
                    p3 = start;
                    p4 = new Vector3(end.x, start.y);
                }
            }
            else
            {
                if (start.y > end.y)
                {
                    p1 = new Vector3(end.x, start.y);
                    p2 = start;
                    p3 = end;
                    p4 = new Vector3(start.x, end.y);
                }
                else
                {
                    p1 = end;
                    p2 = new Vector3(start.x, end.y);
                    p3 = new Vector3(end.x, start.y);
                    p4 = start;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                creationTransform.position = rays[i].origin;
                verts[i] = creationTransform.localPosition;
            }

            for (int i = 4; i < 8; i++)
            {
                creationTransform.position = rays[i - 4].origin + rays[i - 4].direction * dist[i - 4];
                verts[i] = creationTransform.localPosition;
            }

            colliderMesh = new Mesh();
            colliderMesh.name = "Detection Mesh";
            colliderMesh.vertices = verts;
            colliderMesh.triangles = tris;
        }
    }
}