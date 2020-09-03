using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public class Life : MonoBehaviour
    {
        [Header("Object Reference")]
        Receiver receiver = null;

        public float maxLife = 100, maxArmor = 50, maxShield = 50;
        private float curLife = 0, curArmor = 0, curShield = 0;


        private void Awake()
        {
            curLife = maxLife;
            if (receiver == null)
                receiver = gameObject.AddComponent<Receiver>();
        }

        private void Update()
        {
            List<float[]> dmg = receiver.CheckDamage();
            float[] dmgHolders = new float[3];

            foreach (float[] array in dmg)
            {
                for (int i = 0; i < array.Length; i++)
                    dmgHolders[i] += array[i];
            }

            if(dmg.Count > 0)
            {

            }
        }
    }
}
