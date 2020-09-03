using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public class Damage : MonoBehaviour
    {
        public DamageType type = 0;
        public DamageTime time = 0;
        public float dmg = 1;

        public float overTime = 1;
        private float curTime = 0;
        private bool done = false;

        public void Setup(DamageType newType, DamageTime newTime, float newDmg, float? newOverTime = 0)
        {
            type = newType;
            time = newTime;
            dmg = newDmg;

            if (newOverTime.HasValue)
                overTime = newOverTime.Value;
        }

        public float[] CheckDamage()
        {
            float[] result = new float[3];

            if (!done)
            {
                switch (time)
                {
                    case DamageTime.Instant:
                        break;

                    case DamageTime.OverTimeFlow:
                        break;

                    case DamageTime.OverTimeJump:
                        break;
                }
            }

            return result;
        }

        public bool CheckDamageState()
        {
            return done;
        }
    }
}
