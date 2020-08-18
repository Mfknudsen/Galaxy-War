using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace AI
{
    public class Common : MonoBehaviour
    {
        #region GlobalValues
        int waypointConteniusDir = 1;
        #endregion

        #region Sight
        public Vector3[] GetSightDirections(int ViewDirectionCount)
        {
            Vector3[] directions = new Vector3[ViewDirectionCount];

            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float angleIncrement = Mathf.PI * 2 * goldenRatio;

            for (int i = 0; i < ViewDirectionCount; i++)
            {
                float t = (float)i / ViewDirectionCount;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = angleIncrement * i;

                float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                float z = Mathf.Cos(inclination);

                directions[i] = new Vector3(x, y, z).normalized;
            }

            return directions;
        }

        public GameObject[] UpdateInSight(Transform origin, GameObject[] objInRadius, Vector3[] ViewDirection, float dist, LayerMask mask, float? angel = 0)
        {
            List<GameObject> result = new List<GameObject>();
            int maxCount = 0, curCount = 0;

            foreach (GameObject obj in objInRadius)
            {
                if (Vector3.Angle((obj.transform.position - origin.position).normalized, origin.forward) <= angel || angel == 0)
                    maxCount++;
            }

            for (int i = 0; i < ViewDirection.Length; i++)
            {
                Vector3 dir = origin.TransformDirection(ViewDirection[i]);

                if (Vector3.Angle(origin.forward, dir) <= angel || angel == 0)
                {
                    Ray ray = new Ray(origin.position, dir);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, dist, mask, QueryTriggerInteraction.Ignore))
                    {
                        GameObject obj = hit.collider.gameObject;
                        if (obj != null)
                        {
                            if (!result.Contains(obj))
                            {
                                Debug.DrawRay(origin.position, dir * hit.distance, Color.red);
                                result.Add(obj);
                                curCount++;
                            }
                        }

                    }
                }

                if (curCount == maxCount)
                    break;
            }
            return result.ToArray();
        }

        public GameObject[] CheckObjectsInRadius(Vector3 pos, float radius, LayerMask objMask)
        {
            List<GameObject> objInRange = new List<GameObject>();
            Collider[] colliders = Physics.OverlapSphere(pos, radius, objMask, QueryTriggerInteraction.Ignore);
            foreach (Collider C in colliders)
                objInRange.Add(C.gameObject);

            return objInRange.ToArray();
        }

        public bool InSightLine(GameObject[] objsInSight, GameObject target)
        {
            if (objsInSight.Contains(target))
                return true;

            return false;
        }
        #endregion

        #region Movement
        public Transform GetNextWaypoint(Transform[] waypoints, Transform currentGoal, AI.WaypointType type)
        {
            int count = waypoints.Length;

            if (type == AI.WaypointType.One_Way)
            {
                for (int i = 0; i < count; i++)
                {
                    if (waypoints[i] == currentGoal)
                    {
                        if (i != count - 1)
                            return waypoints[i + 1];
                    }
                }
            }
            else if (type == AI.WaypointType.Patrol_Conteniusly)
            {
                for (int i = 0; i < count; i++)
                {
                    if (waypoints[i] == currentGoal)
                    {
                        if (i + 1 != count)
                            return waypoints[i + 1];
                        else
                            return waypoints[0];
                    }
                }
            }
            else if (type == AI.WaypointType.Patrol_FromTo)
            {
                if (waypointConteniusDir == -1)
                {
                    for (int i = count - 1; i > -1; i--)
                    {
                        if (i == 1)
                            waypointConteniusDir = 1;

                        if (waypoints[i] == currentGoal && (i - 1) >= 0)
                            return waypoints[i - 1];
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (i == count - 2)
                            waypointConteniusDir = -1;

                        if (waypoints[i] == currentGoal)
                            return waypoints[i + 1];
                    }
                }

            }

            return currentGoal;
        }

        public bool CheckDistanceToPoint(Vector3 origin, Vector3 target, float dist)
        {
            float distCheck = Vector3.Distance(origin, target);

            if (distCheck <= dist)
                return true;

            return false;
        }

        public Transform[] AddNewPosition(Transform[] waypoints, Transform newPoint, bool contenius)
        {
            List<Transform> result = new List<Transform>();

            if (contenius && waypoints.Length != 0)
            {
                foreach (Transform p in waypoints)
                    result.Add(p);
                result.Add(newPoint);
            }
            else
                result.Add(newPoint);

            return result.ToArray();
        }

        public Transform[] RemovePosition(Transform[] waypoints, Transform toRemove)
        {
            List<Transform> result = new List<Transform>();

            foreach (Transform t in waypoints)
                result.Add(t);

            toRemove.gameObject.GetComponent<Waypoint>().Remove();
            result.Remove(toRemove);

            return result.ToArray();
        }

        public bool ShouldJump(bool[] checklist)
        {
            foreach (bool b in checklist)
            {
                if (!b)
                {
                    return false;
                }
            }

            return true;
        }

        public void Jump()
        {

        }
        #endregion

        #region Interaction
        public bool InteractWithTarget(GameObject target)
        {
            InteractReciever reciever = target.GetComponent<InteractReciever>();

            if (reciever == null)
                return false;
            else
                return reciever.GetInteraction();
        }

        public bool ShouldInteract(bool[] checklist)
        {
            foreach (bool b in checklist)
            {
                if (!b)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Fighting
        public int[] SetTargetPriority(GameObject[] targets)
        {
            int count = targets.Length;
            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = 0;
            }

            return result;
        }

        public Quaternion AimAtTarget(Transform origin, Vector3 target, float rotSpeed)
        {
            Quaternion result = Quaternion.LookRotation(target - origin.position, origin.up);

            return result;
        }

        public bool ShouldAttackTarget(bool[] checklist)
        {
            foreach (bool b in checklist)
            {
                if (!b)
                {
                    return false;
                }
            }

            return false;
        }
        #endregion

        #region Animation

        #endregion
    }
}
