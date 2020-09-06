using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Elevator
{
    public enum State { Open, Closing, Opening, Running }

    public class Elevator : MonoBehaviour
    {
        [Header("Object Reference")]
        public State state = 0;
        public Detector detect = null;
        public Transform platform = null;
        public bool active = true;

        [Header("Squad")]
        public int squadCapacity = 1;
        public int curSquadCount = 0;
        public List<List<Squad.Core>> waitList = new List<List<Squad.Core>>();

        [Header("Levels")]
        public float platformSpeed = 1;
        public Transform[] levelTransforms = new Transform[0];
        public NavMeshLink[] navLinkObjs = new NavMeshLink[0];
        private bool navLinkActive = false;
        [SerializeField] private int nextLevel = 0;
        private int curLevel = 0;

        [Header(" - Navigation")]
        public bool setupDone = false;
        public GameObject[] prefabs = new GameObject[2];
        public GameObject[] linkPoints = new GameObject[0];

        [Header(" - Door Values")]
        public float doorSpeed = 1;
        public GameObject[] doors = new GameObject[0];
        public Transform[] closedPoints = new Transform[0];
        public Transform[] openPoints = new Transform[0];

        private void Awake()
        {
            detect = platform.GetChild(0).GetComponent<Detector>();
            detect.elev = this;

            SetupOffMeshLink();

            for (int i = 0; i < navLinkObjs.Length; i++)
                navLinkObjs[i].gameObject.GetComponent<MeshLinkDetector>().entryLevel = i;
        }

        private void Update()
        {
            switch (state)
            {
                //
                //Wait for Squad Entry
                case State.Open:
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

        public void ChangeNextLevel(int newLevel)
        {
            curLevel = newLevel;
        }

        private void MovePlatform()
        {
            platform.position += (levelTransforms[nextLevel].position - levelTransforms[curLevel].position).normalized * platformSpeed * Time.deltaTime;
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

        #region Interaction
        public void AddToWaitList(Squad.Core newCore, int levelEntry, bool? priority)
        {
            if (levelEntry >= 0 && levelEntry <= waitList.Count - 1)
            {
                if (!waitList[levelEntry].Contains(newCore) && (!priority.HasValue || priority.Value != true))
                    waitList[levelEntry].Add(newCore);
                else
                {
                    for (int i = 0; i < waitList[levelEntry].Count; i++)
                    {
                        if (!waitList[levelEntry][i].containsPlayer && !waitList[levelEntry].Contains(newCore))
                            waitList[levelEntry].Add(newCore);
                    }
                }
            }
        }

        public void RemoveFromWaitList(Squad.Core oldCore)
        {
            foreach (List<Squad.Core> list in waitList)
            {
                if (list.Contains(oldCore))
                {
                    list.Remove(oldCore);
                    break;
                }
            }
        }
        #endregion
    }
}