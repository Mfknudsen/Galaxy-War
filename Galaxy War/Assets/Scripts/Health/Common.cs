using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public enum DamageType { Blunt, Fire, Shock, Sharp }
    public enum DamageTime { Instant, OverTimeJump, OverTimeFlow }
    public enum DamageEffectivnes { None, Life, Armor, Shield }

    public class Common : MonoBehaviour
    {
        #region Damage
        public Damage CreateNewDamage(float dmg, DamageType type, DamageTime time, float? overTime = 0)
        {
            Damage result = new Damage();
            result.Setup(type, time, dmg, overTime);

            return result;
        }

        public void SendDamage(Receiver r, Damage toSend)
        {
            if (r != null)
                r.ReceiveNewDamage(toSend);
        }

        public float ApplyLifeDamage()
        {
            float result = 0;
            
            return result;
        }

        public float ApplyArmorDamage()
        {
            float result = 0;

            return result;
        }

        public float[] ApplyShieldDamage(float curShield, float[] curDmg, DamageEffectivnes effect, bool? bypassShield = false)
        {
            float[] result = new float[3];
            float endShield = curShield;
            float dmg = curDmg[2];

            if (effect == DamageEffectivnes.None || effect == DamageEffectivnes.Life)
                endShield -= dmg;
            else if (effect == DamageEffectivnes.Armor)
                endShield -= dmg / 2;
            else
                endShield -= dmg * 2;

            if (endShield <= 0)
                endShield = 0;

            if (endShield == 0 || bypassShield.GetValueOrDefault())
            {
                result = new float[] { curDmg[0], curDmg[1], endShield };
            }
            else
            {
                result[2] = endShield;
            }

            return result;
        }
        #endregion

        #region Heal

        #endregion
    }
}
