using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReciever : MonoBehaviour
{
    private bool active = false;

    public bool ReceiveTrigger()
    {
        if (active)
            return false;
        else
            active = true;

        return true;
    }

    public bool CheckReceiver()
    {
        if (!active)
            return false;

        active = false;
        return true;
    }
}
