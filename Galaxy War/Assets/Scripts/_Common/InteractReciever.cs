using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractReciever : MonoBehaviour
{
    private bool Active = false;

    public bool GetInteraction()
    {
        if (Active)
            return false;
        else
        {
            Active = true;
            return true;
        }
    }
}
