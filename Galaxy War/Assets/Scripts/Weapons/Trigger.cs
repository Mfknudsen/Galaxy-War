using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public class Trigger : MonoBehaviour
    {
        #region Values
        public TriggerType mainType = 0, secondaryType = 0;
        public KeyCode triggerOne = KeyCode.Mouse0, triggerTwo = KeyCode.Mouse1;

        [HideInInspector] public bool playerUse = false;
        private bool mainFire = false, secondaryFire;

        #endregion

        #region Trigger Checks
        public bool checkMainTrigger()
        {
            bool result = mainFire;

            if (result && mainType != TriggerType.FullAuto)
                mainFire = false;

            return result;
        }

        public bool checkSecondaryTrigger()
        {
            bool result = secondaryFire;

            if (result && secondaryType != TriggerType.FullAuto)
                secondaryFire = false;

            return result;
        }
        #endregion

        #region Player Trigger
        private void Update()
        {
            if (playerUse)
            {
                if (mainType != TriggerType.FullAuto)
                {
                    mainFire = Input.GetKeyDown(triggerOne);
                    secondaryFire = Input.GetKeyDown(triggerTwo);
                }
                else
                {
                    mainFire = Input.GetKey(triggerOne);
                    secondaryFire = Input.GetKey(triggerTwo);
                }
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
}
