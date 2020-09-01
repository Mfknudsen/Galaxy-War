using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public enum WeaponSize { Main, Sidearm, Melee }

    public enum WeaponType { Rifle, Shotgun, Pistol, Sniper, Flamethrower, Knife, Baton }

    public class WeaponHolder : MonoBehaviour
    {
        public WeaponSize weaponSize = 0;
        public WeaponType weaponType = 0;
        public bool isEmpty = false;
    }
}
