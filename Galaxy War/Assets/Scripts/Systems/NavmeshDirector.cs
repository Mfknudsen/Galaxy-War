using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshDirector : MonoBehaviour
{
    public static NavmeshDirector Instance { get; private set; }

    [Header("Object Reference:")]
    public List<NavMeshSurface> surfaces = new List<NavMeshSurface>();
    public int updateDelay = 3;
    private int count = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
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

    public void AddSurface(NavMeshSurface surface)
    {
        surfaces.Add(surface);
    }

    public void RemoveSurface(NavMeshSurface surface)
    {
        surfaces.Remove(surface);
    }
}
