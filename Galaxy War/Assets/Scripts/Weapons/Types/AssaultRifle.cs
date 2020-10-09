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


        public override void TriggerOne()
        {
            GameObject obj = Instantiate(projectile);
            obj.transform.position = projectileSpawnPoint.position;
            obj.transform.rotation = projectileSpawnPoint.rotation;

            if (changeProjetile)
            {
                Projectile pro = obj.GetComponent<Projectile>();

                if (pro != null)
                {
                    pro.SetSpeed(speed);
                    pro.SetGravity(gravity);
                    pro.SetUseGravity(useGravity);
                }
            }

        }

        public override void TriggerTwo()
        {

        }
    }
}
