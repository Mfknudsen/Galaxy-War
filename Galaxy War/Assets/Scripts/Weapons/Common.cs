using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public enum WeaponSize { Main, Sidearm, Melee }

    public enum WeaponType { Rifle, Shotgun, Pistol, Sniper, Flamethrower, Knife, Baton }

    public class Common : MonoBehaviour
    {
        public WeaponSize weaponSize = 0;
        public WeaponType weaponType = 0;
        public bool isEmpty = false;

        public bool CheckTimeDelay(float curTime, float delay)
        {
            return (curTime >= delay);
        }

        public float AddTime(float curTime, float toAdd)
        {
            return curTime + toAdd * Time.deltaTime;
        }

        public GameObject InstantiateBullet(GameObject prefab, Transform origin, Transform parent, float speed)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = origin.position;
            obj.transform.rotation = origin.rotation;
            obj.transform.parent = parent;

            Bullet b = obj.GetComponent<Bullet>();
            b.speed = speed;

            return obj;
        }

        public GameObject InstantiateFlame(GameObject prefab, Transform origin, Transform parent)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = origin.position;
            obj.transform.rotation = origin.rotation;
            obj.transform.parent = parent;

            return obj;
        }

        public GameObject InstantiateThunder(GameObject prefab, Transform origin, Transform parent)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = origin.position;
            obj.transform.rotation = origin.rotation;
            obj.transform.parent = parent;

            return obj;
        }
    }
}
