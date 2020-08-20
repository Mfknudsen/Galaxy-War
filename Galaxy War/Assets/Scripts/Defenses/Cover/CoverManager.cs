using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverManager : MonoBehaviour
{
    public List<Cover> Covers = new List<Cover>();
    List<Cover> coversToUpdate = new List<Cover>();

    public bool setupComplete = false;
    private int index = 0;

    private void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Cover");

        foreach (GameObject o in objs)
            Covers.Add(o.GetComponent<Cover>());
        coversToUpdate = Covers;
    }

    private void FixedUpdate()
    {
        if (!setupComplete)
        {
            List<Cover> toRemoveFromUpdate = new List<Cover>();

            coversToUpdate[index].UpdateCover();

            if (coversToUpdate[index].done)
                toRemoveFromUpdate.Add(coversToUpdate[index]);

            foreach (Cover c in toRemoveFromUpdate)
                coversToUpdate.Remove(c);

            index++;

            if (index >= coversToUpdate.Count)
                index = 0;

            if (coversToUpdate.Count == 0)
                setupComplete = true;
        }
    }
}
