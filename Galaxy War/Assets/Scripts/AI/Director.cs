using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public enum WaypointType { One_Way, Patrol_FromTo, Patrol_Conteniusly }

    public class Director : Common
    {
        [Header("Object Reference")]
        public int Count = 0;
        public Core c;
        public List<Core> CoreInCloseRange;
        public List<Core> CoreInMediumRange;
        public List<Core> CoreInLongRange;

        [Header("Distance")]
        public Transform Player;
        public int closeToMedium = 10, mediumToLong = 20;

        [Header("Update Counter")]
        public int distDelay = 10;
        public int mediumDelay = 3, longDelay = 5;
        private int counter = 0, fixedCounter = 0;

        private Vector3[] LookDirections;

        private void Start()
        {
            CoreInCloseRange = new List<Core>();
            CoreInMediumRange = new List<Core>();
            CoreInLongRange = new List<Core>();

            LookDirections = GetSightDirections(5000);

            AddNewCore(c);
        }

        private void Update()
        {
            counter++;

            UpdateCoreDistance();

            if (counter % mediumDelay == 0)
            {
                foreach (Core C in CoreInMediumRange)
                    C.UpdateCore();
            }
            else if (counter % longDelay == 0)
            {
                foreach (Core C in CoreInLongRange)
                    C.UpdateCore();
            }

            foreach (Core C in CoreInCloseRange)
                C.UpdateCore();

        }

        private void FixedUpdate()
        {
            fixedCounter++;

            if (fixedCounter % mediumDelay == 0)
            {
                foreach (Core C in CoreInMediumRange)
                    C.UpdateFixedCore();
            }
            else if (fixedCounter % longDelay == 0)
            {
                foreach (Core C in CoreInMediumRange)
                    C.UpdateFixedCore();
            }
            else
            {
                foreach (Core C in CoreInCloseRange)
                    C.UpdateFixedCore();
            }
        }

        public void AddNewCore(Core newCore)
        {
            Vector3 corePos = newCore.transform.position;

            if (newCore != null)
            {
                float dist = Vector3.Distance(Player.position, corePos);

                if (dist < closeToMedium)
                {
                    CoreInCloseRange.Add(newCore);
                    c.updateDelay = 1;
                }
                else if (dist < mediumToLong)
                {
                    CoreInMediumRange.Add(newCore);
                    c.updateDelay = mediumDelay;
                }
                else
                {
                    CoreInLongRange.Add(newCore);
                    c.updateDelay = longDelay;
                }

                newCore.StartCore(LookDirections);
            }
        }

        public void RemoveCore(Core core)
        {
            int i = core.updateDelay;

            if (i == 1)
                CoreInCloseRange.Remove(core);
            else if (i == mediumDelay)
                CoreInMediumRange.Remove(core);
            else
                CoreInLongRange.Remove(core);
        }

        private void UpdateCoreDistance()
        {
            Vector3 curPos = Player.position;
            List<Core> allCores = new List<Core>();
            foreach (Core c in CoreInCloseRange)
                allCores.Add(c);
            foreach (Core c in CoreInMediumRange)
                allCores.Add(c);
            foreach (Core c in CoreInLongRange)
                allCores.Add(c);

            CoreInCloseRange.Clear();
            CoreInMediumRange.Clear();
            CoreInLongRange.Clear();

            foreach (Core C in allCores)
            {
                float dist = Vector3.Distance(curPos, C.transform.position);

                if (dist < closeToMedium)
                {
                    CoreInCloseRange.Add(C);
                    c.updateDelay = 1;
                }
                else if (dist < mediumToLong)
                {
                    CoreInMediumRange.Add(C);
                    c.updateDelay = mediumDelay;
                }
                else
                {
                    CoreInLongRange.Add(C);
                    c.updateDelay = longDelay;
                }
            }
        }
    }
}
