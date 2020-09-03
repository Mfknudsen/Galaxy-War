using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public class AssaultRifle : MonoBehaviour
    {
        #region Values
        [Header("Object Reference")]
        Common common = null;

        [Header("Trigger Setup")]
        public Trigger trigger = null;
        public TriggerType mainType = 0, secType = 0;
        public KeyCode mainTrigger = KeyCode.Mouse0, secTrigger = KeyCode.Mouse1;
        public bool forPlayer = false;

        [Header("Ammo")]
         private bool active = false;
        #endregion

        private void Awake()
        {
            common = gameObject.AddComponent<Common>();

            trigger = common.SetupTrigger(gameObject, mainType, secType, forPlayer, mainTrigger, secTrigger);
        }
    }
}
