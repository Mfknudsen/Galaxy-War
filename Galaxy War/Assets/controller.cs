using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour
{
    public bool ready = false;

    [Header("Navigation:")]
    public VectorNavigation.NavVector navigationField = null;

    public Transform waypoint = null;
    public Vector3 lastPoint = Vector3.zero;

    [Header(" - Agent:")]
    [SerializeField] private VectorNavigation.VectorAgent agent = null;
    public float moveSpeed = 1, rotSpeed = 1, stopDist = 0.25f;

    private void Awake()
    {
        if (agent == null)
            agent = gameObject.AddComponent<VectorNavigation.VectorAgent>();

        agent.Setup(navigationField);
        agent.debugMode = true;
        agent.moveSpeed = moveSpeed;
        agent.rotSpeed = rotSpeed;
        agent.moveStopDist = stopDist;

        agent.enableMovement = false;
        agent.enableRotation = false;
    }

    private void Update()
    {
        ready = agent.pathFound;
    }

    public void getNewDestination(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void moveA()
    {
        agent.enableMovement = true;
        agent.enableRotation = true;
    }
}
