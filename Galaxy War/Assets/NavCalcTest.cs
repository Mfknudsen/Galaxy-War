using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class NavCalcTest : MonoBehaviour
    {
        public NavigationCalculations n = null;
        public Transform start = null, end = null;
        public LayerMask elevatorLinkMask = 0;

        private void Awake()
        {
            n = ScriptableObject.CreateInstance("NavigationCalculations") as NavigationCalculations;
            n.Setup(elevatorLinkMask);
        }

        void Update()
        {
            if (n != null)
                n.CalculateNavPath(start.position, end.position, true);
            else
                n = ScriptableObject.CreateInstance("NavigationCalculations") as NavigationCalculations;
        }
    }
}
