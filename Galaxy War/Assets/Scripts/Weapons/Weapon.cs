using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public enum WeaponSize { Main, Sidearm, Melee }
    public enum WeaponType { Rifle, Shotgun, Pistol, Sniper, Flamethrower, Knife, Baton }
    public enum TriggerType { SingelShot, SemiAuto, FullAuto }
    public enum AmmoType { Bullet, Flame, Thunder }

    public class Weapon : MonoBehaviour, Trigger
    {
        [Header("Weapon Information:")]
        [SerializeField] protected WeaponSize size = 0;
        [SerializeField] protected WeaponType type = 0;

        [Header(" - Projectile:")]
        [SerializeField] protected GameObject projectile = null;

        [Header(" - Magazine:")]
        [SerializeField] protected bool readyToShot = false;
        [SerializeField] protected int maxMagSize = 0;
        [SerializeField] protected int curMagSize = 0;

        [Header(" - Reload:")]
        [SerializeField] protected bool toReload = false;
        [SerializeField] protected float reloadTime = 0.5f;
        protected Coroutine reloadDelay = null;

        [Header(" - Trigger One:")]
        [SerializeField] protected KeyCode triggerOne = KeyCode.Mouse0;

        [Header(" - Trigger Two:")]
        [SerializeField] protected KeyCode triggerTwo = KeyCode.Mouse1;

        public virtual void TriggerOne()
        {
            throw new System.NotImplementedException("Trigger need an override");
        }

        public virtual void TriggerTwo()
        {
            throw new System.NotImplementedException("Trigger need an override");
        }
    }
}
