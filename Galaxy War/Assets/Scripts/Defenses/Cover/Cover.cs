using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    [Header("Dev Mode")]
    public bool Dev = false;

    [Header("Object Reference")]
    public GameObject CoverSpot = null;
    public bool done = false;

    public List<GameObject> usableCoverSpots = new List<GameObject>();
    private List<GameObject> activeCoverSpots = new List<GameObject>();

    public LayerMask terrainMask = 0;
    public LayerMask coverMask = 0;
    public LayerMask coverspotMask = 0;
    public LayerMask dontOverlapMask = 0;
    public int checkRayCount = 120;
    public float checkNormalRadius = 0.5f;
    public float checkSpotHeight = 2f, checkSpotRadius, heightOffset = 0.1f;

    List<Vector3[]> CreationPoints = new List<Vector3[]>();
    List<Vector3> NormalDirections = new List<Vector3>();
    bool readyToCreateSpots = false;
    int index = 0;
    float addition = 0;
    Vector3 lastCreate = Vector3.zero;

    public void UpdateCover()
    {
        if (!done)
        {
            if (readyToCreateSpots)
            {
                if (index < NormalDirections.Count)
                    CreateCoverspots(CreationPoints[index], NormalDirections[index]);
                else
                {
                    if (Dev)
                        Debug.Log("Spots Created: " + activeCoverSpots.Count);
                    done = true;
                }
            }
            else
                SetUpCoverSpots();
        }
    }

    private void SetUpCoverSpots()
    {
        float extent = 360 / checkRayCount;
        List<Vector3> hitPos = new List<Vector3>();

        for (int i = 0; i < checkRayCount; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(extent * i), 0, Mathf.Sin(extent * i)).normalized;
            Vector3 pos = transform.position + dir * checkNormalRadius;

            RaycastHit hit;
            if (Physics.Raycast(pos, -dir, out hit, checkNormalRadius, coverMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Vector3 n = hit.normal;
                    if (!NormalDirections.Contains(n))
                    {
                        NormalDirections.Add(n);
                        hitPos.Add(hit.point);
                    }
                }
            }
        }
        if (Dev)
            Debug.Log("Normals Detected: " + NormalDirections.Count);

        for (int i = 0; i < NormalDirections.Count; i++)
            CreationPoints.Add(GetStartToEndFromNormal(NormalDirections[i], hitPos[i]));

        if (Dev)
            Debug.Log("Spot Areas Created: " + CreationPoints.Count);

        readyToCreateSpots = true;
    }

    private Vector3[] GetStartToEndFromNormal(Vector3 normal, Vector3 origin)
    {
        Vector3 dir = Vector3.Cross(-normal, Vector3.up);
        Vector3 currentPos = origin + normal * checkSpotRadius * 1.01f;
        Vector3 startPoint = Vector3.zero, endPoint = Vector3.zero;
        float moveSpeed = 0.01f;

        bool firstCheck = false;
        while (!firstCheck)
        {
            if (Physics.Raycast(currentPos, -normal, checkSpotRadius * 1.5f, coverMask, QueryTriggerInteraction.Ignore))
            {
                startPoint = currentPos;
            }
            else
                firstCheck = true;

            currentPos += dir * moveSpeed;
        }

        bool secondCheck = false;
        currentPos -= dir * moveSpeed;
        while (!secondCheck)
        {
            currentPos -= dir * moveSpeed;

            if (Physics.Raycast(currentPos, -normal, checkSpotRadius * 1.5f, coverMask, QueryTriggerInteraction.Ignore))
            {
                endPoint = currentPos;
            }
            else
                secondCheck = true;
        }

        if (startPoint != Vector3.zero && endPoint != Vector3.zero && Vector3.Distance(startPoint, endPoint) >= checkSpotRadius * 2.1f)
            return new Vector3[] { startPoint, endPoint };

        return new Vector3[0];
    }

    private void CreateCoverspots(Vector3[] points, Vector3 normal)
    {
        if (points.Length > 0)
        {
            Vector3 dir = -Vector3.Cross(-normal, Vector3.up);
            Vector3 curPoint = points[0] + dir * checkSpotRadius, endPoint = points[1];
            float moveSpeed = 0.1f, dist;

            dist = Vector3.Distance(curPoint, endPoint);
            Vector3 checkPoint = curPoint + dir * addition;

            if (lastCreate == Vector3.zero || Vector3.Distance(lastCreate, checkPoint) >= checkSpotRadius * 2 + 0.15f)
            {
                RaycastHit hit;
                if (Physics.Raycast(checkPoint, -Vector3.up, out hit, Mathf.Infinity, terrainMask, QueryTriggerInteraction.Ignore))
                {
                    checkPoint.y = hit.point.y + checkSpotRadius + heightOffset;
                }
                else if (Physics.Raycast(checkPoint, Vector3.up, out hit, Mathf.Infinity, terrainMask, QueryTriggerInteraction.Ignore))
                {
                    checkPoint.y = hit.point.y + checkSpotRadius + heightOffset;
                }

                Collider[] colliders = Physics.OverlapCapsule(checkPoint, checkPoint + Vector3.up * checkSpotHeight, checkSpotRadius, dontOverlapMask, QueryTriggerInteraction.Ignore);
                if (colliders.Length == 0)
                {
                    lastCreate = checkPoint;

                    GameObject obj = Instantiate(CoverSpot);
                    obj.transform.position = checkPoint;
                    obj.transform.rotation = Quaternion.LookRotation(-normal, Vector3.up);
                    obj.transform.parent = transform;
                    obj.GetComponent<CoverSpot>().CoverContainer = this;

                    activeCoverSpots.Add(obj);
                    usableCoverSpots.Add(obj);
                }
            }

            addition += moveSpeed;

            if (addition >= dist)
            {
                lastCreate = Vector3.zero;
                addition = 0;
                index++;
            }
        }
        else
        {
            lastCreate = Vector3.zero;
            addition = 0;
            index++;
        }
    }
}
