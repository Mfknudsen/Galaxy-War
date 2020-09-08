//Unity Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Custom Libraries
using AI;

namespace Commander
{
    public class InteractWithAIMovement : MonoBehaviour
    {
        #region Values
        [Header("Object Reference")]
        public Camera cam = null;

        [Header("Waypoint")]
        public AI.WaypointType Type;
        public GameObject Waypoint = null;
        public Transform waypointParent = null;
        public LayerMask detectionMask = 0;
        public KeyCode CreateWaypointTrigger = KeyCode.Mouse1;
        private bool checkShiftCount = false;

        [Header("Unit Selection")]
        public Transform creationTransform = null;
        public LayerMask coreMask = 0;
        public KeyCode SelectionTrigger = KeyCode.Mouse0, KeepOldTrigger = KeyCode.LeftShift;
        public float checkBoxHeight = 50;
        public MeshCollider col = null;
        public float maxTime = 0.5f;
        private float time = 0;
        [SerializeField] private List<Squad.Core> SelectedCores = new List<Squad.Core>();
        [SerializeField] private Mesh currentDetectionMesh = null;
        private bool setToPatrol = false;
        private Vector2[] uiMousePos = new Vector2[2];

        [Header(" - UI Visualization")]
        public Image image = null;
        #endregion

        public void UpdateInteraction()
        {
            if (checkShiftCount)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    Type = AI.WaypointType.One_Way;
                    checkShiftCount = false;
                    setToPatrol = false;
                }
            }

            if (Input.GetKeyDown(CreateWaypointTrigger))
                CreateNewWaypoint();

            #region Selection Detection
            if (Input.GetKeyDown(SelectionTrigger))
            {
                uiMousePos[0] = Input.mousePosition;
            }

            if (Input.GetKey(SelectionTrigger))
            {
                uiMousePos[1] = Input.mousePosition;
            }

            if (Input.GetKeyUp(SelectionTrigger) && Vector2.Distance(uiMousePos[0], uiMousePos[1]) >= 10)
            {
                Vector2[] corners = GetBoundingBox(uiMousePos[0], uiMousePos[1]);

                if (!Input.GetKey(KeepOldTrigger))
                    SelectedCores.Clear();

                currentDetectionMesh = GenerateSelectionMesh(corners);
                col.sharedMesh = currentDetectionMesh;
            }
            #endregion

            if (Type != AI.WaypointType.One_Way)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Type = AI.WaypointType.One_Way;
            }

            if (currentDetectionMesh != null)
            {
                time += Time.deltaTime;

                if (time >= maxTime)
                {
                    currentDetectionMesh = null;
                    col.sharedMesh = null;
                    time = 0;
                    uiMousePos = new Vector2[2];
                }
            }
        }

        #region Send Commands
        private void CreateNewWaypoint()
        {
            if (SelectedCores.Count != 0)
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, detectionMask))
                {
                    if (Vector3.Angle(Vector3.up, hit.normal) <= 70)
                    {
                        GameObject obj = Instantiate(Waypoint);
                        obj.transform.position = hit.point;
                        obj.transform.parent = waypointParent;

                        foreach (Squad.Core c in SelectedCores)
                        {
                            if (!setToPatrol)
                            {
                                c.ReceiveNewWaypoint(obj.transform.position, AI.WaypointType.One_Way, Input.GetKey(KeyCode.LeftShift));
                                obj.GetComponent<Waypoint>().Add();
                            }
                            else
                            {
                                AI.WaypointType t = c.waypointType;
                                if (t == AI.WaypointType.One_Way)
                                    t = AI.WaypointType.Patrol_FromTo;

                                c.ReceiveNewWaypoint(obj.transform.position, t, Input.GetKey(KeyCode.LeftShift));
                                obj.GetComponent<Waypoint>().Add();
                            }
                        }
                    }
                }
            }

            if (!checkShiftCount)
                checkShiftCount = true;
        }

        public void TogglePatrolMode()
        {
            foreach (Squad.Core c in SelectedCores)
            {
                AI.WaypointType t = c.waypointType;

                if (t == WaypointType.Patrol_Conteniusly)
                    c.waypointType = WaypointType.Patrol_FromTo;
                else
                    c.waypointType = WaypointType.Patrol_Conteniusly;
            }
        }
        #endregion

        #region Selection
        private Vector2[] GetBoundingBox(Vector2 start, Vector2 end)
        {
            Vector2 p1 = Vector2.zero, p2 = Vector2.zero, p3 = Vector2.zero, p4 = Vector2.zero;

            if (start.x < end.x)
            {
                if (start.y > end.y)
                {
                    p1 = start;
                    p2 = new Vector2(end.x, start.y);
                    p3 = new Vector2(start.x, end.y);
                    p4 = end;
                }
                else
                {
                    p1 = new Vector2(start.x, end.y);
                    p2 = end;
                    p3 = start;
                    p4 = new Vector2(end.x, start.y);
                }
            }
            else
            {
                if (start.y > end.y)
                {
                    p1 = new Vector2(end.x, start.y);
                    p2 = start;
                    p3 = end;
                    p4 = new Vector2(start.x, end.y);
                }
                else
                {
                    p1 = end;
                    p2 = new Vector2(start.x, end.y);
                    p3 = new Vector2(end.x, start.y);
                    p4 = start;
                }
            }

            Vector2[] result = { p1, p2, p3, p4 };
            return result;
        }

        private Mesh GenerateSelectionMesh(Vector2[] corners)
        {
            Ray[] rays = new Ray[4];
            for (int i = 0; i < 4; i++)
            {
                rays[i] = cam.ScreenPointToRay(corners[i]);
            }

            float[] dist = new float[4];
            RaycastHit hit;
            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(rays[i], out hit, Mathf.Infinity, detectionMask, QueryTriggerInteraction.Collide))
                    dist[i] = hit.distance;
            }

            Vector3[] verts = new Vector3[8];
            int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };

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

            Mesh result = new Mesh();
            result.name = "Detection Mesh";
            result.vertices = verts;
            result.triangles = tris;

            return result;
        }

        private void OnTriggerStay(Collider other)
        {
            Squad.Core C = other.GetComponent<Core>().currentSquad;
            if (C != null && !SelectedCores.Contains(C))
                SelectedCores.Add(C);
        }
        #endregion

        #region Visual
        private void VisualizeRectDetection()
        {

        }
        #endregion

        #region ReceiveCommands
        public void SwithWaypointType()
        {
            if (setToPatrol)
                setToPatrol = false;
            else
                setToPatrol = true;
        }
        #endregion
    }
}