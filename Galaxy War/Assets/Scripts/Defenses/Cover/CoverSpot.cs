using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CoverSpot : MonoBehaviour
{
    [Header("Cover Value")]
    public float CoverValue = 0;

    [Header("Manager")]
    public bool active = false;
    public Cover CoverContainer = null;

    [Header("Calculations")]
    public float maxRadius = 200, minRadius;
    public float radius = 2;
    public LayerMask coverspotMask = 0;
    [HideInInspector] public float minAngel = 0, maxAngel = 0;
    private List<GameObject> objsFound = new List<GameObject>();
    private List<GameObject> leftSpots = new List<GameObject>();
    private List<GameObject> rightSpots = new List<GameObject>();
    private int count = 0;

    private void Update()
    {
        if (!active)
        {
            if (count % 2 == 0)
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, radius, coverspotMask, QueryTriggerInteraction.Ignore);

                foreach (Collider c in cols)
                {
                    GameObject obj = c.gameObject;

                    if (obj != gameObject && !objsFound.Contains(obj) && Vector3.Angle(obj.transform.forward, transform.forward) < 90 && Vector3.Angle(-transform.forward, (obj.transform.position - transform.position).normalized) <= 95)
                        objsFound.Add(c.gameObject);
                }

                foreach (GameObject obj in objsFound)
                {
                    float sideAngel = Vector3.Angle(transform.right, (obj.transform.position - transform.position).normalized);

                    if (sideAngel <= 90 && !rightSpots.Contains(obj))
                        rightSpots.Add(obj);
                    else if (sideAngel > 90 && !leftSpots.Contains(obj))
                        leftSpots.Add(obj);
                }

                foreach (GameObject obj in leftSpots)
                {
                    float angel = Vector3.Angle(transform.forward, obj.transform.forward) + minRadius;
                    Debug.Log(angel);

                    if (angel > minAngel)
                        minAngel = angel;
                }

                foreach (GameObject obj in rightSpots)
                {
                    float angel = Vector3.Angle(transform.forward, obj.transform.forward) + minRadius;
                    Debug.Log(angel);

                    if (angel > maxAngel)
                        maxAngel = angel;
                }

                if (maxAngel < minRadius)
                    maxAngel = minRadius;
                if (minAngel < minRadius)
                    minAngel = minRadius;

                CoverValue = minAngel + maxAngel;
            }

            count++;

            if (count == 100)
                count = 0;
        }
    }
}
