using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class controller : MonoBehaviour
{
    [Header("Navigation:")]
    public VectorNavigation.NavVector navigationField = null;

    public Transform waypoint = null;
    public Vector3 lastPoint = Vector3.zero;

    private VectorNavigation.VectorAgent agent = null;

    private void Awake()
    {
        if (agent == null)
            agent = gameObject.AddComponent<VectorNavigation.VectorAgent>();

        agent.Setup(navigationField);
        agent.debugMode = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && navigationField.activeNodes.Count != 0)
        {
            lastPoint = waypoint.position;
            agent.SetDestination(lastPoint);
        }
    }
}
