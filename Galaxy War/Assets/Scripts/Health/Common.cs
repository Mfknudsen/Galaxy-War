using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public enum DamageType { Blunt, Fire, Shock, Sharp }
    public enum DamageTime { Instant, OverTimeJump, OverTimeFlow }
    public enum DamageEffectivnes { None, Life, Armor, Shield }
    public enum DamageBypass { None, Armor, Shield, Both }

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

        #region Applying Damage
        public float[] ApplyDamage(float[] curHealth, float dmg, DamageEffectivnes effect, DamageBypass? bypass)
        {
            float[] result = new float[3];
            float life = 0, armor = 0, shield = 0;


            ///
            ///Shield
            float[] shieldResult = ShieldDamage(shield, dmg, effect, bypass);
            result[2] = shieldResult[1];

            ///
            ///Armor
            float[] armorResult = ArmorDamage(armor, shieldResult[0], effect, bypass);
            result[1] = armorResult[1];

            ///
            ///Life
            result[0] = LifeDamage(life, armorResult[0], effect);

            return result;
        }

        public float LifeDamage(float curLife, float curDmg, DamageEffectivnes effect)
        {
            ///
            ///Values
            float endLife = curLife;
            float multiplier;


            ///
            /// Calculation Damage Based On Effectivness
            if (effect == DamageEffectivnes.Life)
                multiplier = 2;
            else
                multiplier = 1;

            endLife -= curDmg * multiplier;
            if (endLife <= 0)
                endLife = 0;

            return endLife;
        }

        public float[] ArmorDamage(float curArmor, float curDmg, DamageEffectivnes effect, DamageBypass? bypass)
        {
            ///
            ///Values
            float[] result = new float[2];
            float endArmor = curArmor;
            float dmg = curDmg / 2;
            float multiplier;

            ///
            ///Calculating Damage Based On Effectivness
            if (effect == DamageEffectivnes.None || effect == DamageEffectivnes.Life)
                multiplier = 2;
            else if (effect == DamageEffectivnes.Shield)
                multiplier = 0.5f;
            else
                multiplier = 4;

            endArmor -= dmg * multiplier;
            if (endArmor <= 0)
                endArmor = 0;

            ///
            ///Applying Damage Based On Bypass
            if (endArmor == 0)
            {
                if (bypass.Value == DamageBypass.Armor || bypass.Value == DamageBypass.Both)
                    result[0] = curDmg;
                else
                    result[0] = result[0] = ((dmg * multiplier) - curArmor) / multiplier;

                result[1] = 0;
            }
            else
            {
                result[0] = 0;
                result[1] = endArmor;
            }

            return result;
        }

        public float[] ShieldDamage(float curShield, float curDmg, DamageEffectivnes effect, DamageBypass? bypass)
        {
            ///
            ///Values
            float[] result = new float[2];
            float endShield = curShield;
            float dmg = curDmg;
            float multiplier;

            ///
            ///Calculating Damage Based On Effectivness
            if (effect == DamageEffectivnes.None || effect == DamageEffectivnes.Life)
                multiplier = 1;
            else if (effect == DamageEffectivnes.Armor)
                multiplier = 0.5f;
            else
                multiplier = 2;

            endShield -= dmg * multiplier;

            if (endShield <= 0)
                endShield = 0;

            ///
            ///Applying Damage Based On Bypass
            if (endShield == 0)
            {
                if (bypass.Value == DamageBypass.Shield || bypass.Value == DamageBypass.Both)
                    result[0] = dmg;
                else
                    result[0] = ((dmg * multiplier) - curShield) / multiplier;

                result[1] = 0;
            }
            else
            {
                result[0] = 0;
                result[1] = endShield;
            }

            return result;
        }
        #endregion
        #endregion

        #region Heal

        #endregion
    }
}
