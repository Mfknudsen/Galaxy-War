using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private int useCount = 0;

    public void Add()
    {
        useCount++;
    }

    public void Remove()
    {
        useCount--;

        if (useCount <= 0)
            Destroy(gameObject);
    }
}
