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
    public LayerMask groundMask = 0;
    public KeyCode CreateWaypointTrigger = KeyCode.Mouse1;
    private bool checkShiftCount = false;

    [Header("Unit Selection")]
    public float boxCheckHeight = 20;
    public LayerMask coreMask = 0;
    public KeyCode SelectionTrigger = KeyCode.Mouse0;
    private Core[] SelectedCores = new Core[0];
    private Vector3 startPos = Vector3.zero, endPos = Vector3.zero;
    private bool setToPatrol = false;

    [Header("--UI Visualization")]
    public Image image = null;

    void Update()
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
            startPos = GetVectorFromRay();
        if (Input.GetKey(SelectionTrigger))
            endPos = GetVectorFromRay();
        if (Input.GetKeyUp(SelectionTrigger))
        {
            if (startPos != endPos)
                SelectedCores = CreateRectDetection();
        }

        if (Type != AI.WaypointType.One_Way)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Type = AI.WaypointType.One_Way;
        }
    }

    #region Send Commands
    private void CreateNewWaypoint()
    {
        if (SelectedCores.Length != 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
            {
                if (Vector3.Angle(Vector3.up, hit.normal) <= 70)
                {
                    GameObject obj = Instantiate(Waypoint);
                    obj.transform.position = hit.point;

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
    private Vector3 GetVectorFromRay()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private Core[] CreateRectDetection()
    {
        List<Core> cores = new List<Core>();

        startPos.y = 0;
        endPos.y = boxCheckHeight;
        Vector3 center = startPos + ((endPos - startPos) / 2);
        Vector3 sizeDim = new Vector3(Mathf.Abs(endPos.x - startPos.x), boxCheckHeight, Mathf.Abs(endPos.z - startPos.z));
        Vector3 orientation = cam.transform.rotation.eulerAngles;
        orientation.z = 0;
        orientation.x = 0;

        Collider[] cols = Physics.OverlapBox(center, sizeDim, Quaternion.Euler(orientation), coreMask, QueryTriggerInteraction.Ignore);
        foreach (Collider c in cols)
        {
            Core core = c.gameObject.GetComponent<Core>();
            if (core != null)
                cores.Add(core);
        }

        return cores.ToArray();
    }

    public void SwithWaypointType()
    {
        if (setToPatrol)
            setToPatrol = false;
        else
            setToPatrol = true;
    }
    #endregion

    #region Visual
    private void VisualizeRectDetection()
    {

    }
    #endregion
}
