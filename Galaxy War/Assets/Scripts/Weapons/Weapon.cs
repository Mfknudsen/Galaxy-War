using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public enum WeaponSize { Main, Sidearm, Melee }
    public enum WeaponType { Rifle, Shotgun, Pistol, Sniper, Flamethrower, Knife, Baton }
    public enum TriggerType { SingelShot, SemiAuto, FullAuto }
    public enum AmmoType { Bullet, Flame, Thunder }

    public abstract class Weapon : MonoBehaviour, Trigger
    {
        #region Values
        [Header("Weapon Information:")]
        [SerializeField] protected string name = "";
        [SerializeField] protected WeaponSize size = 0;
        [SerializeField] protected WeaponType type = 0;

        [Header(" - Projectile:")]
        [SerializeField] protected GameObject projectile = null;
        [SerializeField] protected Transform projectileSpawnPoint = null;
        [SerializeField] protected Transform magSpawnPoint = null;

        [Header(" - Reload:")]
        [SerializeField] protected bool readyToShot = false;
        [SerializeField] protected bool toReload = false;
        [SerializeField] protected float reloadTime = 0.5f;
        protected int triggerToReload = 0;

        [Header(" - Trigger One:")]
        [SerializeField] protected KeyCode triggerOne = KeyCode.Mouse0;
        [SerializeField] protected float delayTimeOne = 0.1f;
        [SerializeField] protected int maxMagOne = 50;
        protected int curMagOne = 0;

        [Header(" - Trigger Two:")]
        [SerializeField] protected KeyCode triggerTwo = KeyCode.Mouse1;
        [SerializeField] protected float delayTimeTwo = 0.5f;
        [SerializeField] protected int maxMagTwo = 50;
        protected int curMagTwo = 0;

        [Header(" - Projectile:")]
        [SerializeField] protected bool changeProjetile = false;
        [SerializeField] protected bool useGravity = false;
        [SerializeField] protected float speed = 1, gravity = -9.81f;

        //Delay:
        protected float maxDelayTime = 0;
        protected float curDelayTime = 0;
        #endregion

        #region Triggers
        public virtual void TriggerOne()
        {
            throw new System.NotImplementedException("Trigger need an override");
        }

        public virtual void TriggerTwo()
        {
            throw new System.NotImplementedException("Trigger need an override");
        }
        #endregion

        #region Delay
        public void DelayTimer(float? maxDelayTime = 0)
        {
            if (this.maxDelayTime == 0 && readyToShot)
            {
                this.maxDelayTime = maxDelayTime.Value;
                curDelayTime = 0;
                readyToShot = false;
            }
            else
            {
                curDelayTime += Time.deltaTime;

                if (curDelayTime >= this.maxDelayTime)
                {
                    this.maxDelayTime = 0;
                    readyToShot = true;
                }
            }
        }
        #endregion

        #region Reload
        public void Reload(int? trigger)
        {
            if (!toReload && trigger.HasValue)
            {
                if (trigger.Value == 1)
                    curMagOne = 0;
                else if (trigger.Value == 2)
                    curMagTwo = 0;

                triggerToReload = trigger.Value;

                DelayTimer(reloadTime);
                toReload = true;
            }
            else
            {
                DelayTimer();

                if (maxDelayTime == 0)
                {
                    toReload = false;
                    if (triggerToReload == 1)
                        curMagOne = maxMagOne;
                    else if (triggerToReload == 2)
                        curMagTwo = maxMagTwo;
                }
            }
        }
        #endregion
    }
}
