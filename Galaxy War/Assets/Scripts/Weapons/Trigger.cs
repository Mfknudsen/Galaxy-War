using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    #region Values
    [HideInInspector] public bool playerUse = false;
    private bool mainFire = false, secondaryFire;

    [HideInInspector] public KeyCode triggerOne = KeyCode.Mouse0, triggerTwo = KeyCode.Mouse1;
    #endregion

    #region Trigger Checks
    public bool checkMainTrigger()
    {
        bool result = mainFire;

        if (result)
            mainFire = false;

        return result;
    }

    public bool checkSecondaryTrigger()
    {
        bool result = secondaryFire;

        if (result)
            secondaryFire = false;

        return result;
    }
    #endregion

    #region Player Trigger
    private void Update()
    {
        if (playerUse)
        {
            mainFire = Input.GetKeyDown(triggerOne);
            secondaryFire = Input.GetKeyDown(triggerTwo);
        }
    }
    #endregion

    #region AI Trigger
    public void triggerNow()
    {
        mainFire = true;
    }
    #endregion
}
