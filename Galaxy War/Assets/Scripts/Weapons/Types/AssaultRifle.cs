using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public class AssaultRifle : MonoBehaviour
    {
        #region Values
        [Header("Object Reference")]
        public Common common = null;
        public Trigger trigger = null;
        public Transform ammoSpawner = null;
        public Transform ammoParent = null;

        [Header("Trigger Setup")]
        public bool forPlayer = false;
        public TriggerType mainType = 0, secType = 0;
        public KeyCode mainTrigger = KeyCode.Mouse0, secTrigger = KeyCode.Mouse1;

        [Header("Activation")]
        //Main
        public float mainDelayTime = 0.5f;
        private bool mainActive = false;
        //Secondary
        public float secDelayTime = 0.5f;
        private bool secActive = false;

        [Header("Instantiation")]
        public bool readyToFire = true;
        public GameObject bulletPrefab = null, thunderPrefab = null;
        private Coroutine curDelayCorutine = null;
        private float curDelayTime = 0;
        #endregion

        private void Awake()
        {
            common = gameObject.AddComponent<Common>();
            trigger = common.SetupTrigger(gameObject, mainType, secType, forPlayer, mainTrigger, secTrigger);
        }

        private void Update()
        {
            mainActive = trigger.checkMainTrigger();
            secActive = trigger.checkSecondaryTrigger();

            if (readyToFire)
            {
                if (mainActive)
                {

                    common.InstantiateBullet(bulletPrefab, ammoSpawner, ammoParent);

                    curDelayTime = mainDelayTime;
                    readyToFire = false;
                }
                else if (secActive)
                {
                    common.InstantiateThunder(thunderPrefab, ammoSpawner, ammoParent);

                    curDelayTime = secDelayTime;
                    readyToFire = false;
                }
            }
            else if (curDelayCorutine == null && curDelayTime != 0)
                curDelayCorutine = StartCoroutine(delayActive(mainDelayTime));
        }

        private IEnumerator delayActive(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            curDelayTime = 0;
            readyToFire = true;
            curDelayCorutine = null;
        }
    }
}
