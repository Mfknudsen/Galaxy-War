using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI
{
    public class Common : MonoBehaviour
    {
        #region GlobalValues
        int waypointConteniusDir = 1;
        #endregion

        #region Sight
        //
        //Finding Attackable Targets:
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

            //Setting up how many objects the ai can see
            if (angel != 0)
            {
                foreach (GameObject obj in objInRadius)
                {
                    if (Vector3.Angle((obj.transform.position - origin.position).normalized, origin.forward) <= angel + 5)
                        maxCount++;
                }
            }
            else
                maxCount = objInRadius.Length;

            //Check if the object is visible in the open
            foreach (GameObject obj in objInRadius)
            {
                Vector3 pos = obj.transform.position;
                Vector3 dir = pos - origin.position;

                if (Vector3.Angle(origin.forward, dir) <= angel || angel == 0)
                {
                    Ray ray = new Ray(origin.position, dir);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, dist, mask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.gameObject == obj && !result.Contains(obj))
                        {
                            Debug.DrawRay(origin.position, dir * hit.distance, Color.red);
                            result.Add(obj);
                            curCount++;
                        }
                    }
                }
            }

            //Check if the object is stil visible behind cover
            if (curCount != maxCount)
            {
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

                            if (objInRadius.Contains(obj) && !result.Contains(obj))
                            {
                                Debug.DrawRay(origin.position, dir * hit.distance, Color.red);
                                result.Add(obj);
                                curCount++;
                            }
                        }
                    }

                    if (curCount == maxCount)
                        break;
                }
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

        //
        //Finding Cover:
        public CoverSpot[] FindCover(Vector3 pos, float dist, LayerMask coverMask)
        {
            List<CoverSpot> result = new List<CoverSpot>();

            Collider[] colliders = Physics.OverlapSphere(pos, dist, coverMask, QueryTriggerInteraction.Ignore);
            foreach (Collider c in colliders)
            {
                Cover check = c.gameObject.GetComponent<Cover>();

                foreach (GameObject cs in check.usableCoverSpots)
                    result.Add(cs.GetComponent<CoverSpot>());
            }

            return result.ToArray();
        }

        public CoverSpot EvaluateCover(CoverSpot[] coversInRange, GameObject[] targets)
        {
            CoverSpot result = null;
            float curValue = 0;
            int curTargetsHit = 0;

            foreach (CoverSpot cs in coversInRange)
            {
                float value = 0;
                int targetsHit = 0;

                foreach (GameObject t in targets)
                {
                    Vector3 dir = (t.transform.position - cs.transform.position).normalized;

                    float toAdd = Vector3.Angle(cs.transform.forward, dir);
                    if (toAdd <= 75)
                    {
                        targetsHit++;
                        value += toAdd;
                    }
                }

                if (result == null)
                {
                    curValue = value;
                    result = cs;
                    curTargetsHit = targetsHit;
                }
                else if (value < curValue && value != 0 || targetsHit >= curTargetsHit)
                {
                    curTargetsHit = targetsHit;
                    curValue = value;
                    result = cs;
                }
            }

            return result;
        }

        //
        //Finding Weapon:
        public GameObject FindBestWeapon(Vector3 origin, GameObject[] objects, GameObject[] curInventory, Weapon.WeaponType[] faveredType)
        {
            GameObject result = null;
            List<Weapon.WeaponSize> weaponSize = new List<Weapon.WeaponSize>() { Weapon.WeaponSize.Main, Weapon.WeaponSize.Sidearm, Weapon.WeaponSize.Melee };

            //
            //Setup missing sizes based on current inventory
            foreach (GameObject obj in curInventory)
            {
                if (obj != null)
                {
                    Weapon.WeaponHolder curHolder = obj.GetComponent<Weapon.WeaponHolder>();
                    Weapon.WeaponSize curSize = curHolder.weaponSize;
                    if (weaponSize.Contains(curSize) && !curHolder.isEmpty)
                        weaponSize.Remove(curSize);
                }
            }

            //
            //Check the current weapons in range based on their size
            List<Weapon.WeaponHolder> holders = new List<Weapon.WeaponHolder>();
            List<GameObject> toCheck = new List<GameObject>();

            foreach (GameObject obj in objects)
                holders.Add(obj.GetComponent<Weapon.WeaponHolder>());

            for (int i = 0; i < weaponSize.Count; i++)
            {
                foreach (Weapon.WeaponHolder hold in holders)
                {
                    if (hold.weaponSize == weaponSize[i])
                    {
                        if (faveredType.Length > 0)
                        {
                            if (faveredType.Contains(hold.weaponType))
                            {
                                toCheck.Add(hold.gameObject);
                            }
                        }
                        else
                            toCheck.Add(hold.gameObject);
                    }
                }

                if (toCheck.Count > 0)
                    break;
            }

            //
            //Deside what weapon to go for based on distance
            float curDistance = 100;
            foreach (GameObject obj in toCheck)
            {
                Vector3 dir = obj.transform.position - origin;
                if (dir.magnitude < curDistance)
                {
                    result = obj;
                    curDistance = dir.magnitude;
                }
            }

            return result;
        }

        //
        //Finding Ammo:
        public void FindAmmo()
        {

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
        public void RetreiveAmmo()
        {

        }

        public void PickUpWeapon(Core currentAI, Weapon.WeaponHolder currentHolder)
        {
            int i = currentHolder.weaponSize.GetHashCode();

            if (currentAI.weaponsInInventory[i] != null)
                DiscardWeapon(currentAI.weaponsInInventory[0], currentHolder.transform);
            currentAI.weaponsInInventory[0] = currentHolder.gameObject;

            if (!currentHolder.isEmpty)
            {
                currentAI.curWeapon = currentHolder.gameObject;
                currentHolder.gameObject.transform.position = currentAI.weaponHolder.position;
                currentHolder.gameObject.transform.rotation = currentAI.weaponHolder.rotation;
                currentHolder.gameObject.transform.parent = currentAI.weaponHolder;
                currentHolder.gameObject.GetComponent<BoxCollider>().enabled = false;

                currentAI.outOfAmmo = false;
            }
        }

        public void DiscardWeapon(GameObject oldWeapon, Transform newPlacement)
        {
            oldWeapon.transform.position = newPlacement.position;
            oldWeapon.transform.rotation = newPlacement.rotation;
            oldWeapon.transform.parent = newPlacement.parent;
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
