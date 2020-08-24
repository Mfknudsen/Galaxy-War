using AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractWithAIMovement : MonoBehaviour
{
    [Header("Object Reference")]
    public Camera cam = null;

    [Header("Waypoint")]
    public AI.WaypointType Type;
    public GameObject Waypoint = null;
    public Transform waypointParent = null;
    public LayerMask groundMask = 0;
    public KeyCode CreateWaypointTrigger = KeyCode.Mouse1;
    private bool checkShiftCount = false;

    [Header("Unit Selection")]
    public LayerMask coreMask = 0;
    public KeyCode SelectionTrigger = KeyCode.Mouse0, KeepOldTrigger = KeyCode.LeftShift;
    public float checkBoxHeight = 50;
    [SerializeField] private List<Core> SelectedCores = new List<Core>();
    private bool setToPatrol = false;
    Vector2 uiPosition = Vector2.zero;


    [Header("--UI Visualization")]
    public Image image = null;

    void FixedUpdate()
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

        if (Input.GetKeyDown(SelectionTrigger))
        { }
        if (Input.GetKey(SelectionTrigger))
        { }
        if (Input.GetKeyUp(SelectionTrigger))
        { }

        if (Type != AI.WaypointType.One_Way)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Type = AI.WaypointType.One_Way;
        }
    }

    #region Send Commands
    private void CreateNewWaypoint()
    {
        if (SelectedCores.Count != 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
            {
                if (Vector3.Angle(Vector3.up, hit.normal) <= 70)
                {
                    GameObject obj = Instantiate(Waypoint);
                    obj.transform.position = hit.point;
                    obj.transform.parent = waypointParent;

                    foreach (Core c in SelectedCores)
                    {
                        if (!setToPatrol)
                        {
                            c.ReceiveNewWaypoint(obj, AI.WaypointType.One_Way, Input.GetKey(KeyCode.LeftShift));
                            obj.GetComponent<Waypoint>().Add();
                        }
                        else
                        {
                            AI.WaypointType t = c.waypointType;
                            if (t == AI.WaypointType.One_Way)
                                t = AI.WaypointType.Patrol_FromTo;

                            c.ReceiveNewWaypoint(obj, t, Input.GetKey(KeyCode.LeftShift));
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
        foreach (Core c in SelectedCores)
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

    private Mesh GenerateSelectionMesh(Vector3[] corners)
    {
        Vector3[] verts = new Vector3[8];
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };

        for (int i = 0; i < 4; i++)
            verts[i] = corners[i];
        for (int j = 4; j < 8; j++)
            verts[j] = corners[j - 4] + transform.forward * checkBoxHeight;

        Mesh result = new Mesh();
        result.vertices = verts;
        result.triangles = tris;

        return result;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Input.GetKey(KeepOldTrigger))
            SelectedCores.Clear();

        Core c = other.GetComponent<Core>();
        if (other.GetComponent<Selectable>() != null && c != null)
            SelectedCores.Add(c);
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
