using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class test : MonoBehaviour
{
    public bool done = true;
    public List<controller> agents = new List<controller>();
    int frame = 0;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            agents.Add(transform.GetChild(i).GetComponent<controller>());
    }

    void Update()
    {
        frame++;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (controller c in agents)
                c.getNewDestination(c.transform.position + c.transform.forward * 75);

            done = false;
        }

        if (!done)
        {
            bool run = true;

            foreach (controller c in agents)
            {
                if (!c.ready)
                {
                    run = false;
                    break;
                }
            }

            if (run)
            {
                Debug.Log("Move: " + frame);

                foreach (controller c in agents)
                    c.moveA();

                done = true;
            }
        }
    }
}
