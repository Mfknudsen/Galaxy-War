using Health;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public class RifleBullet : Projectile
    {
        public override void Update()
        {
            base.Update();

            Move();
        }
    }
}
