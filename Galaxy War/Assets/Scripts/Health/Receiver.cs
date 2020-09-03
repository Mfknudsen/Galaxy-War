using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public class Receiver : MonoBehaviour
    {
        List<Damage> damageReceived = new List<Damage>();

        public void ReceiveNewDamage(Damage container)
        {
            if (!damageReceived.Contains(container))
                damageReceived.Add(container);
        }

        public List<float[]> CheckDamage()
        {
            List<float[]> result = new List<float[]>();

            List<Damage> toRemove = new List<Damage>();
            foreach (Damage d in damageReceived)
            {
                result.Add(d.CheckDamage());
                if (d.CheckDamageState())
                    toRemove.Add(d);
            }

            foreach (Damage d in toRemove)
                damageReceived.Remove(d);

            return result;
        }
    }
}
