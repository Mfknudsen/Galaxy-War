using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public enum Override { Free, Static, Off, }
    public enum State { Grounded, Airborne, Dashing, AirDash, BarJump }

    public class Common : ScriptableObject
    {
        #region Input
        public Vector2 GetAxises()
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        public Vector2 GetMouseVector()
        {
            return new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        }
        #endregion

        #region Wall-Walking Calculations
        private Vector3 lastSuccessfullWallTrace = Vector3.zero;
        private bool isWallWalkingAllowed = true;
        private float feelerSize = 1;
        private bool rayFound = false;

        public void SetupWallWalkingParameters(float size)
        {
            feelerSize = size;
        }

        public Quaternion LocalOrientationChange(Transform origin, float maxDist, float radius, LayerMask mask)
        {
            Vector3 goalNormal = GetAverageWallWalkingNormal(origin, maxDist, radius, mask);

            Vector3 newNormal = SmoothWallNormal(origin, goalNormal);
            Quaternion result = Quaternion.FromToRotation(origin.up, newNormal) * origin.rotation;

            return result;
        }


        public float Fraction(Vector3 dir, Vector3 normal)
        {
            Vector3 fracDir = Vector3.Reflect(dir, normal);
            float result = Vector3.Angle(-dir, fracDir);

            return result;
        }

        public RaycastHit Shared_TraceCapsule(Vector3 startPoint, Vector3 endPoint, float feelerSize, LayerMask physicsMask)
        {
            RaycastHit result;

            Debug.DrawRay(startPoint, endPoint);
            ///Change "endPoint" to "endPoint - startPoint"
            //Bruger en SphereCast da det kan også tager i betrægning om hvorvidt firguren har tilstrækelige plads til at bevæge sig hen til overfladen
            if (Physics.SphereCast(startPoint, feelerSize, endPoint - startPoint, out result, feelerSize, physicsMask, QueryTriggerInteraction.Ignore))
            {
                rayFound = true;
                return result;
            }

            return result;
        }

        public Vector3 SmoothWallNormal(Transform transform, Vector3 goalNormal)
        {
            Vector3 result = goalNormal;
            float fraction = Fraction(transform.up, goalNormal);

            if (fraction < 1)
            {
                float diff = Vector3.Dot(goalNormal, transform.up);

                if (diff < 0.98f)
                {
                    Vector3 normalDiff = goalNormal - transform.up;

                    if (diff == -1)
                    {
                        if (Vector3.Dot(transform.right, goalNormal) != -1)
                            normalDiff = goalNormal - transform.up;
                        else
                            normalDiff = goalNormal - (-transform.up);
                    }

                    result = (transform.up + normalDiff * fraction);
                }
            }

            if (result.magnitude < 0.01)
                result = Vector3.up;

            return result;
        }

        public Vector3 GetAngelsFromWallNormal(RaycastHit hit)
        {
            GameObject obj = new GameObject();
            obj.transform.position = hit.point;
            obj.transform.up = hit.normal;
            obj.transform.forward = Vector3.Cross(obj.transform.up, obj.transform.forward);

            if (obj.transform.right.magnitude < 0.001)
                return Vector3.zero;
            else
            {
                obj.transform.forward = Vector3.Cross(obj.transform.right, obj.transform.up);

                Vector3 angels = obj.transform.rotation.eulerAngles;
                return angels;
            }
        }

        public bool ValidWallTrace(RaycastHit trace)
        {
            if (rayFound)
            {
                GameObject entity = trace.collider.gameObject;
                rayFound = false;

                if (entity != null)
                {
                    if (entity.tag == "Clingable" && trace.normal != Vector3.zero)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<Vector3> TraceWallNormal(Vector3 startPoint, Vector3 endPoint, List<Vector3> result, float feelerSize, LayerMask physicsMask)
        {
            RaycastHit localTrace = Shared_TraceCapsule(startPoint, endPoint, feelerSize, physicsMask);
            Debug.DrawRay(startPoint, endPoint);

            if (ValidWallTrace(localTrace))
            {
                result.Insert(0, localTrace.normal);
                return result;
            }

            return result;
        }

        public Vector3 GetAverageWallWalkingNormal(Transform startTransform, float extraRange, float feelerSize, LayerMask physicsMaskOverride)
        {
            LayerMask physicsMask = physicsMaskOverride;

            Vector3 startPoint = startTransform.position;

            int numTraces = 8;
            List<Vector3> wallNormals = new List<Vector3>();
            List<Vector3> checkNormals = new List<Vector3>();

            float wallWalkingRange = extraRange;
            Vector3 directionVector = Vector3.zero;
            bool normalFound = true;

            checkNormals = TraceWallNormal(startPoint, lastSuccessfullWallTrace * wallWalkingRange, wallNormals, feelerSize, physicsMask);
            if (lastSuccessfullWallTrace == Vector3.zero || wallNormals.Count != checkNormals.Count)
            {
                normalFound = false;
                float x = 0, z = 0;

                for (int i = 0; i < (numTraces); i++)
                {
                    if (i == 0)
                    {
                        x = 0; z = 1;
                    }
                    else if (i == 1)
                    {
                        x = 1; z = 1;
                    }
                    else if (i == 2)
                    {
                        x = 1; z = 0;
                    }
                    else if (i == 3)
                    {
                        x = 1; z = -1;
                    }
                    else if (i == 4)
                    {
                        x = 0; z = -1;
                    }
                    else if (i == 5)
                    {
                        x = -1; z = -1;
                    }
                    else if (i == 6)
                    {
                        x = -1; z = 0;
                    }
                    else if (i == 7)
                    {
                        x = -1; z = 1;
                    }

                    directionVector = (startTransform.forward * z + startTransform.right * x).normalized * wallWalkingRange;

                    checkNormals = TraceWallNormal(startPoint, directionVector, wallNormals, feelerSize, physicsMask);
                    if (wallNormals.Count != checkNormals.Count)
                    {
                        lastSuccessfullWallTrace = directionVector;
                        wallNormals = checkNormals;
                        normalFound = true;
                        break;
                    }
                }
            }

            //Check above.
            if (!normalFound)
            {
                directionVector = startTransform.up * wallWalkingRange;

                checkNormals = TraceWallNormal(startPoint, directionVector, wallNormals, feelerSize, physicsMask);
                if (wallNormals.Count != checkNormals.Count)
                {
                    wallNormals = checkNormals;
                    normalFound = true;
                }
            }

            if (!normalFound)
            {
                for (int i = 0; i < 8; i++)
                {

                }
            }


            if (normalFound)
            {
                //CheckUnder
                directionVector = -startTransform.up * wallWalkingRange;

                checkNormals = TraceWallNormal(startPoint, directionVector, wallNormals, feelerSize, physicsMask);
                if (wallNormals.Count != checkNormals.Count)
                {
                    wallNormals = checkNormals;
                }

                return wallNormals[0];
            }

            return Vector3.up;
        }
        #endregion
    }
}