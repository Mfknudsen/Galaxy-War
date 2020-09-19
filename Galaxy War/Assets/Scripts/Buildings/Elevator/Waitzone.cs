using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elevator
{
    public class Waitzone : MonoBehaviour
    {
        public Cover[] GetCover(float radius, LayerMask coverMask)
        {
            List<Cover> result = new List<Cover>();

            Collider[] hits = Physics.OverlapSphere(transform.position, radius, coverMask, QueryTriggerInteraction.Ignore);

            foreach (Collider col in hits)
            {
                Cover c = col.GetComponent<Cover>();
                if (c != null)
                {
                    if (!result.Contains(c))
                        result.Add(c);
                }
            }

            return result.ToArray();
        }
    }
}