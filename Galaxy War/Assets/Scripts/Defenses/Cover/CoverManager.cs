using System.Collections.Generic;
using UnityEngine;

public class CoverManager : MonoBehaviour
{
    [Header("Object Reference")]
    public bool setupComplete = false;
    public List<Cover> Covers = new List<Cover>();
    private int index = 0;
    private List<Cover> coversToUpdate = new List<Cover>();


    private void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Cover");

        foreach (GameObject o in objs)
            Covers.Add(o.GetComponent<Cover>());
        coversToUpdate = Covers;
    }

    private void FixedUpdate()
    {
        if (!setupComplete && coversToUpdate.Count != 0)
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
        else if (coversToUpdate.Count != 0)
            setupComplete = true;
    }
}
