using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class testai : MonoBehaviour
{
    public Transform t;
    NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.AddComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(t.position);

        Debug.Log(agent.speed);
    }
}
