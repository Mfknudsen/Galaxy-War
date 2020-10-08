using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public class AssaultRifle : Weapon
    {
        private void Update()
        {
            if (Input.GetKeyDown(triggerOne))
                TriggerOne();
        }
    }
}
