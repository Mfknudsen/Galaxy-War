using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public enum WeaponSize { Main, Sidearm, Melee }
    public enum WeaponType { Rifle, Shotgun, Pistol, Sniper, Flamethrower, Knife, Baton }
    public enum TriggerType { SingelShot, SemiAuto, FullAuto }
    public enum AmmoType { Bullet, Flame, Thunder }

    public class Common : MonoBehaviour
    {
        #region Global Values
        public WeaponSize weaponSize = 0;
        public WeaponType weaponType = 0;
        public bool isEmpty = false;

        [Header("Prefabs")]
        public GameObject bulletPrefab = null;
        public GameObject flamePrefab = null;
        public GameObject thunderPrefab = null;
        #endregion

        #region Setup
        public Trigger SetupTrigger(GameObject obj, TriggerType mainType, TriggerType secType, bool forPlayer, KeyCode main, KeyCode sec)
        {
            Trigger result = obj.AddComponent<Trigger>();
            result.mainType = mainType;
            result.secondaryType = secType;

            result.playerUse = forPlayer;

            result.triggerOne = main;
            result.triggerTwo = sec;

            return result;
        }
        #endregion

        #region Calculations
        public bool CheckTimeDelay(float curTime, float delay)
        {
            return (curTime >= delay);
        }

        public float AddTime(float curTime, float toAdd)
        {
            return curTime + toAdd * Time.deltaTime;
        }
        #endregion

        #region CreateShot
        public GameObject SelectAmmoInstantiation(AmmoType type, Transform origin, Transform parent, float? speed = 0)
        {
            GameObject result = null;

            if (type == AmmoType.Bullet)
                InstantiateBullet(bulletPrefab, origin, parent, speed);
            else if (type == AmmoType.Flame)
                InstantiateFlame(flamePrefab, origin, parent);
            else if (type == AmmoType.Thunder)
                InstantiateThunder(thunderPrefab, origin, parent);

            return result;
        }

        public GameObject InstantiateBullet(GameObject prefab, Transform origin, Transform parent, float? speed = 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = origin.position;
            obj.transform.rotation = origin.rotation;
            obj.transform.parent = parent;

            Bullet b = obj.GetComponent<Bullet>();
            if (speed != 0)
                b.speed = speed.GetValueOrDefault();

            return obj;
        }

        public GameObject InstantiateFlame(GameObject prefab, Transform origin, Transform parent, float? spped = 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = origin.position;
            obj.transform.rotation = origin.rotation;
            obj.transform.parent = parent;

            return obj;
        }

        public GameObject InstantiateThunder(GameObject prefab, Transform origin, Transform parent, float? speed = 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = origin.position;
            obj.transform.rotation = origin.rotation;
            obj.transform.parent = parent;

            return obj;
        }
        #endregion
    }
}
