using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshDirector : MonoBehaviour
{
    public int updateDelay = 60;
    private int count = 0;

    public List<NavMeshSurface> surfaces = new List<NavMeshSurface>();

    void Start()
    {

    }

    void Update()
    {
        UpdateSurfaces();
    }

    private void UpdateSurfaces()
    {
        count++;

        if (count % updateDelay == 0)
        {
            count = 0;

            foreach (NavMeshSurface s in surfaces)
            {
                s.BuildNavMesh();
            }
        }
    }
}
