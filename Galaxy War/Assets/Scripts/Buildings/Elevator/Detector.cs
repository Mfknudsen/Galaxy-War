using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Elevator.Elevator elev = null;
    public Transform parentTransform = null;
    public List<GameObject> objs = new List<GameObject>();
    public List<Transform> parents = new List<Transform>();

    void OnTriggerEnter(Collider other)
    {
        if (!elev.objOnPlatform.Contains(other.gameObject) && !other.gameObject.isStatic && other.gameObject.name != "Door")
        {
            elev.objOnPlatform.Add(other.gameObject);

            objs.Add(other.gameObject);
            parents.Add(other.transform.parent);
            other.transform.parent = parentTransform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (elev.objOnPlatform.Contains(other.gameObject))
        {
            elev.objOnPlatform.Remove(other.gameObject);

            int removeIndex = -1;
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i] == other.gameObject)
                    removeIndex = i;
            }

            if (removeIndex != -1)
            {
                other.transform.parent = parents[removeIndex];

                parents.RemoveAt(removeIndex);
                objs.RemoveAt(removeIndex);
            }
        }
    }
}

