using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Elevator
{
    public enum State { Open, Closing, Opening, Running }

    public class Elevator : MonoBehaviour
    {
        #region Values
        [Header("Object Reference:")]
        public State state = 0;
        public Detector detect = null;
        public Transform platform = null;
        public bool active = true;

        [Header("Squad:")]
        public int squadCapacity = 1;
        public int curSquadCount = 0;
        public List<Squad.Core> onPlatform = new List<Squad.Core>();
        public int inWait = 0;

        [Header("Levels:")]
        public float platformSpeed = 1;
        public Transform[] levelTransforms = new Transform[0];
        public NavMeshLink[] navLinkObjs = new NavMeshLink[0];
        private bool navLinkActive = false;
        public int nextLevel = 0;
        public int curLevel = 0;

        [Header(" - Navigation:")]
        public float randomNavDist = 1;
        public LayerMask elevatorMask = 0;
        public Transform navTransform = null;
        public bool setupDone = false;
        public GameObject[] prefabs = new GameObject[2];
        public GameObject[] linkPoints = new GameObject[0];

        [Header(" - Door Values:")]
        public float doorSpeed = 1;
        public GameObject[] doors = new GameObject[0];
        public Transform[] closedPoints = new Transform[0];
        public Transform[] openPoints = new Transform[0];

        [Header(" - Direction:")]
        public bool up = true;
        public List<int> nextFloorList = new List<int>();
        public List<int> floorsToVisit = new List<int>();

        [Header(" - WaitZone")]
        public LayerMask coverMask = 0;
        public int[] pointCount = new int[0];
        public float[] zonesRadius = new float[0];
        private List<bool[]> isPointUsed = new List<bool[]>();
        private List<Transform[]> UseableWaitZonePoints = new List<Transform[]>();
        #endregion

        private void Awake()
        {
            foreach (GameObject obj in linkPoints)
            {
                MeshLinkDetector d = obj.GetComponent<MeshLinkDetector>();
                d.main = this;
                d.maxSquadCount = squadCapacity;
            }

            detect = platform.GetChild(0).GetComponent<Detector>();
            detect.elev = this;

            SetupOffMeshLink();
            SetupWaitZone();

            for (int i = 0; i < navLinkObjs.Length; i++)
                navLinkObjs[i].gameObject.GetComponent<MeshLinkDetector>().entryLevel = i;

            setupDone = true;
        }

        private void Update()
        {
            switch (state)
            {
                //
                //Wait for Squad Entry
                case State.Open:
                    inWait = linkPoints[nextLevel].GetComponent<MeshLinkDetector>().inWait.Count;

                    if (curLevel != nextLevel)
                    {
                        state = State.Closing;

                        navLinkObjs[curLevel].enabled = false;
                        navLinkActive = false;
                    }
                    else if (!navLinkActive)
                    {
                        navLinkObjs[curLevel].enabled = true;
                        navLinkActive = true;
                    }
                    else if (inWait > 0 && onPlatform.Count < squadCapacity)
                    {
                        for (int i = 0; i < (squadCapacity - onPlatform.Count); i++)
                        {
                            MeshLinkDetector detect = linkPoints[curLevel].GetComponent<MeshLinkDetector>();

                            if (detect.inWait.Count - 1 >= i)
                                AllowSquadsIn(detect.inWait[i]);
                        }
                    }
                    else if (onPlatform.Count == 0 && levelTransforms[curLevel].GetComponentInChildren<MeshLinkDetector>().inWait.Count == 0)
                    {
                        CheckVisitList();
                    }

                    break;

                //
                //Close the Door
                case State.Closing:
                    if (CloseDoor())
                        state = State.Running;
                    break;

                //
                //Open the Door
                case State.Opening:
                    if (OpenDoor())
                        state = State.Open;
                    break;

                //
                //Move the Platform
                case State.Running:
                    if (Vector3.Distance(platform.position, levelTransforms[nextLevel].position) < 0.1f)
                    {
                        platform.position = levelTransforms[nextLevel].position;
                        curLevel = nextLevel;

                        state = State.Opening;
                    }
                    else
                        MovePlatform();
                    break;
            }
        }


        #region Setup
        private void SetupOffMeshLink()
        {
            for (int i = 0; i < linkPoints.Length; i++)
            {
                GameObject start = null;
                if (i < linkPoints.Length - 1)
                {
                    start = Instantiate(prefabs[0]);
                    start.transform.parent = linkPoints[i].transform;
                    start.transform.localPosition = Vector3.zero;
                    start.transform.localScale = Vector3.one;
                }

                if (start != null)
                {
                    int startIndex = i + 1;

                    for (int j = startIndex; j < linkPoints.Length; j++)
                    {
                        GameObject end = Instantiate(prefabs[1]);
                        end.transform.parent = linkPoints[i].transform;
                        end.transform.position = linkPoints[j].transform.position;
                        start.transform.localScale = Vector3.one;

                        OffMeshLink newLink = linkPoints[i].AddComponent<OffMeshLink>();
                        newLink.startTransform = start.transform;
                        newLink.endTransform = end.transform;
                        newLink.UpdatePositions();
                    }

                    /*
                        for (int j = 0; j < linkPoints.Length - i - 1; i++)
                        {
                            int startIndex = i + j + 1;

                            GameObject end = Instantiate(prefabs[1]);
                            end.transform.parent = linkPoints[i].transform;
                            end.transform.position = linkPoints[startIndex].transform.position;

                            OffMeshLink newLink = linkPoints[i].AddComponent<OffMeshLink>();
                            newLink.startTransform = start.transform;
                            newLink.endTransform = end.transform;
                            newLink.UpdatePositions();
                        }*/
                }
            }
        }

        private void SetupWaitZone()
        {
            pointCount = new int[zonesRadius.Length];

            for (int i = 0; i < zonesRadius.Length; i++)
            {
                List<Transform> spots = new List<Transform>();
                Collider[] cols = Physics.OverlapSphere(linkPoints[i].transform.position, zonesRadius[i], coverMask, QueryTriggerInteraction.Collide);

                foreach (Collider c in cols)
                {
                    CoverSpot spot = c.GetComponent<CoverSpot>();

                    if (spot != null)
                        spots.Add(spot.transform);
                }

                UseableWaitZonePoints.Add(spots.ToArray());
                isPointUsed.Add(new bool[spots.Count]);

                pointCount[i] = spots.Count;
            }
        }

        private void CheckVisitList()
        {
            if (floorsToVisit.Count > 0)
            {
                nextLevel = floorsToVisit[0];
                floorsToVisit.Remove(nextLevel);
            }
        }
        #endregion

        #region Door
        private bool OpenDoor()
        {
            if (doors.Length > 0)
            {
                doors[curLevel].transform.position = Vector3.Lerp(doors[curLevel].transform.position, openPoints[curLevel].position, doorSpeed);

                if (Vector3.Distance(doors[curLevel].transform.position, openPoints[curLevel].position) < 0.1f)
                {
                    doors[curLevel].transform.position = openPoints[curLevel].position;

                    return true;
                }
            }
            else
                return true;

            return false;
        }

        private bool CloseDoor()
        {
            if (doors.Length > 0)
            {
                doors[curLevel].transform.position = Vector3.Lerp(doors[curLevel].transform.position, closedPoints[curLevel].position, doorSpeed);

                if (Vector3.Distance(doors[curLevel].transform.position, closedPoints[curLevel].position) < 0.1f)
                {
                    doors[curLevel].transform.position = closedPoints[curLevel].position;

                    return true;
                }
            }
            else
                return true;

            return false;
        }
        #endregion

        #region Platform
        private void MovePlatform()
        {
            platform.position += (levelTransforms[nextLevel].position - levelTransforms[curLevel].position).normalized * platformSpeed * Time.deltaTime;
        }

        public Transform GetPlatformWayPoint()
        {
            Transform result = null;
            Vector3 randomDir = Random.insideUnitSphere * randomNavDist;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDir, out navHit, randomNavDist, elevatorMask);

            if (navHit.normal != Vector3.zero)
            {
                Vector3 final = new Vector3(navHit.position.x, navHit.position.y + 1, navHit.position.z);
                navTransform.position = final;
                result = navTransform;
            }

            return result;
        }
        #endregion

        #region Interaction
        public void ChangeNextLevel(int newLevel)
        {
            curLevel = newLevel;
        }

        public int GetLevel(Vector3 endPoint)
        {
            foreach (GameObject obj in linkPoints)
            {
                if (obj.transform.position == endPoint)
                    return obj.GetComponent<MeshLinkDetector>().entryLevel;
            }

            return -1;
        }
        #endregion

        #region Decission
        public bool AllowSquadsIn(Squad.Core core)
        {
            if (!onPlatform.Contains(core))
            {
                onPlatform.Add(core);
                core.elevatorUse = Squad.ElevatorUseState.Enter;
                foreach (AI.Core c in core.members)
                    c.Agent.autoTraverseOffMeshLink = true;
                core.calcSquad.EnterOnElevator(core, this);

                return true;
            }

            return false;
        }

        private void AllSquadsIsInside()
        {
            for (int i = 0; i < onPlatform.Count; i++)
            {
                foreach (AI.Core obj in onPlatform[i].members)
                {

                }
            }
        }
        #endregion

        #region WaitZone
        public Transform GetWaitZonePoint()
        {
            Transform result = null;

            return result;
        }
        #endregion
    }
}