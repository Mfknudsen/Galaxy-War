using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public bool ready = true;

        [Header("Squad:")]
        public int squadCapacity = 1;
        public List<GameObject> objOnPlatform = new List<GameObject>();
        public List<Squad.Core> onPlatformList = new List<Squad.Core>();
        public int inWait = 0;

        [Header("Door:")]
        public float doorSpeed = 1;
        public Transform[] doors = new Transform[0];
        public Transform[] openPoints = new Transform[0];
        public Transform[] closePoints = new Transform[0];
        private int curIndex = -1;

        [Header("Levels:")]
        public float platformSpeed = 1;
        public Transform[] levelTransforms = new Transform[0];
        public int nextLevel = 0;
        public int curLevel = 0;

        [Header(" - Navigation:")]
        public float randomNavDist = 1;
        public LayerMask elevatorMask = 0;
        public bool setupDone = false;
        public GameObject[] prefabs = new GameObject[2];
        public GameObject[] linkPoints = new GameObject[0];
        public GameObject[] carvePoints = new GameObject[0];


        [Header(" - Direction:")]
        public bool up = true;
        public List<int> nextFloorList = new List<int>();
        public List<int> floorsToVisit = new List<int>();

        [Header(" - WaitZone")]
        public LayerMask coverMask = 0;
        public Waitzone[] zones = new Waitzone[0];
        public float[] zonesRadius = new float[0];
        public Cover[][] waitCover = new Cover[0][];
        private CoverSpot[][] waitCoverspots = new CoverSpot[0][];

        [Header(" - NavMesh Carving:")]
        public bool carve = false;
        public float checkDist = 0.1f;
        #endregion

        private void Awake()
        {
            foreach (GameObject carve in carvePoints)
            {
                carve.GetComponent<MeshRenderer>().enabled = false;
                carve.GetComponent<NavMeshObstacle>().enabled = true;
            }
            if (curLevel == nextLevel)
                carvePoints[curLevel].GetComponent<NavMeshObstacle>().enabled = false;

            state = State.Opening;

            if (!platform.gameObject.activeSelf)
                platform.gameObject.SetActive(true);

            SetupOffMeshLink();
            SetupWaitZone();

            for (int i = 0; i < linkPoints.Length; i++)
            {
                MeshLinkDetector d = linkPoints[i].GetComponent<MeshLinkDetector>();
                d.main = this;
                d.maxSquadCount = squadCapacity;
                d.entryLevel = i;
            }

            platform.transform.position = levelTransforms[curLevel].transform.position;

            setupDone = true;
        }

        private void Update()
        {
            switch (state)
            {
                //
                //Wait for Squad Entry
                case State.Open:
                    Open();
                    break;

                //
                //Close the Door
                case State.Closing:
                    Closing();
                    break;

                //
                //Open the Door
                case State.Opening:
                    Opening();
                    break;

                //
                //Move the Platform
                case State.Running:
                    Running();
                    break;
            }
        }

        #region Updates
        private void Open()
        {
            inWait = linkPoints[nextLevel].GetComponent<MeshLinkDetector>().inWait.Count;

            if (curLevel != nextLevel)
            {
                state = State.Closing;
            }
            else if (inWait > 0 && onPlatformList.Count < squadCapacity)
            {
                for (int i = 0; i < (squadCapacity - onPlatformList.Count); i++)
                {
                    MeshLinkDetector detect = linkPoints[curLevel].GetComponent<MeshLinkDetector>();

                    if (detect.inWait.Count - 1 >= i)
                        AllowSquadsIn(detect.inWait[i]);
                }
            }
            else if (onPlatformList.Count == 0 && levelTransforms[curLevel].GetComponentInChildren<MeshLinkDetector>().inWait.Count == 0)
            {
                CheckVisitList();
            }
            else if (AllSquadsIsInside(onPlatformList.ToArray(), objOnPlatform.ToArray()))
            {
                nextLevel = nextFloorList[0];
            }
        }

        private void Closing()
        {
            if (CloseDoor())
            {
                ready = true;
                StartCoroutine(delayCarveActive(curLevel, true));
                StartCoroutine(delayAgentEnable(false, State.Running));
            }
            else if (ready)
            {
                foreach (Squad.Core core in onPlatformList)
                {
                    foreach (AI.Core ai in core.members)
                    {
                        ai.Agent.isStopped = true;
                        ai.Agent.enabled = false;
                    }
                }

                ready = false;
            }
        }

        private void Opening()
        {
            if (OpenDoor())
            {
                carvePoints[nextLevel].GetComponent<NavMeshObstacle>().enabled = false;
                StartCoroutine(delayAgentEnable(true, State.Open));
            }
        }

        private void Running()
        {
            if (Vector3.Distance(platform.position, levelTransforms[nextLevel].position) < 0.2f)
            {
                platform.position = levelTransforms[nextLevel].position;
                curLevel = nextLevel;

                state = State.Opening;
            }
            else
                MovePlatform();
        }
        #endregion

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
            waitCover = new Cover[levelTransforms.Length][];

            for (int i = 0; i < waitCover.Length; i++)
                waitCover[i] = zones[i].GetCover(zonesRadius[i], coverMask);
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
            if (curIndex == -1)
            {
                int closestIndex = doors.Length - 1;
                for (int i = 0; i < doors.Length; i++)
                {
                    if (Vector3.Distance(doors[i].position, linkPoints[curLevel].transform.position) < Vector3.Distance(doors[closestIndex].position, linkPoints[curLevel].transform.position))
                        closestIndex = i;
                }
                curIndex = closestIndex;
            }

            if (Vector3.Distance(doors[curIndex].position, openPoints[curIndex].position) < 0.1f)
            {
                doors[curIndex].position = openPoints[curIndex].position;
                curIndex = -1;
                return true;
            }
            else
                doors[curIndex].position += (openPoints[curIndex].position - doors[curIndex].position).normalized * doorSpeed * Time.deltaTime;

            return false;
        }

        private bool CloseDoor()
        {
            if (curIndex == -1)
            {
                int closestIndex = doors.Length - 1;
                for (int i = 0; i < doors.Length; i++)
                {
                    if (Vector3.Distance(doors[i].position, linkPoints[curLevel].transform.position) < Vector3.Distance(doors[closestIndex].position, linkPoints[curLevel].transform.position))
                        closestIndex = i;
                }
                curIndex = closestIndex;
            }

            if (Vector3.Distance(doors[curIndex].position, closePoints[curIndex].position) < 0.1f)
            {
                doors[curIndex].position = closePoints[curIndex].position;
                curIndex = -1;
                return true;
            }
            else
                doors[curIndex].position += (closePoints[curIndex].position - doors[curIndex].position).normalized * doorSpeed * Time.deltaTime;

            return false;
        }
        #endregion

        #region Platform
        private void MovePlatform()
        {
            platform.position += (levelTransforms[nextLevel].position - levelTransforms[curLevel].position).normalized * platformSpeed * Time.deltaTime;
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
            if (!onPlatformList.Contains(core))
            {
                StartCoroutine(delayEnterState(core));

                onPlatformList.Add(core);

                return true;
            }

            return false;
        }

        private bool AllSquadsIsInside(Squad.Core[] squadCores, GameObject[] curOnPlatform)
        {
            foreach (Squad.Core core in squadCores)
            {
                foreach (AI.Core c in core.members)
                {
                    if (!curOnPlatform.Contains(c.gameObject) || !c.onPoint)
                        return false;
                }
            }

            return true;
        }

        //Delays
        IEnumerator delayEnterState(Squad.Core squadCore)
        {
            yield return 0;

            squadCore.elevatorUse = Squad.ElevatorUseState.Enter;
        }

        IEnumerator delayAgentEnable(bool toEnable, State next)
        {
            yield return 2;

            foreach (Squad.Core core in onPlatformList)
            {
                foreach (AI.Core agent in core.members)
                    agent.Agent.enabled = toEnable;
            }

            state = next;
        }

        IEnumerator delayCarveActive(int level, bool toActive)
        {
            yield return 0;

            carvePoints[level].GetComponent<NavMeshObstacle>().enabled = toActive;
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