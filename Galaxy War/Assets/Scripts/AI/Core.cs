using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AI;

//temp
using TMPro;

namespace AI
{
    public enum State { Idle, Walking, Partol, Cover, Fighting }

    public class Core : MonoBehaviour
    {
        [Header("Dev")]
        public TextMesh StateText;

        [Header("Object Reference")]
        public bool CoreIsActive = false;
        public LayerMask groundMask, targetMask;
        public State State;
        public AI.Director Director = null;
        [HideInInspector] public int updateDelay = 1;
        [HideInInspector] public bool update = false, fixedUpdate = false;

        private AI.Common Common = null;
        [HideInInspector] public Vector3[] lookDirections = new Vector3[0];

        [Header("Movement")]
        public float checkDist = 0.2f;
        public AI.WaypointType waypointType;
        [HideInInspector] public Transform[] wayPoints = new Transform[0];
        private Transform curWaypoint = null;
        private Transform lastWaypoint = null;
        private NavMeshAgent Agent = null;

        [Header("Sight")]
        public float sightRadius = 15;
        public float sightAngel = 50;
        private GameObject[] targableObjs = new GameObject[0];
        private GameObject[] objInRadius = new GameObject[0];

        [Header("Weapon")]
        public GameObject[] weaponsInInventory = new GameObject[3];
        public GameObject curWeapon = null;

        private void Awake()
        {
            Director = GameObject.FindGameObjectWithTag("AI Director").GetComponent<Director>();
            Director.AddNewCore(this);
        }

        public void StartCore(Vector3[] dir)
        {
            lookDirections = dir;
            if (!CoreIsActive)
            {
                Common = gameObject.AddComponent<Common>();
                Agent = gameObject.AddComponent<NavMeshAgent>();
                Agent.isStopped = false;
                CoreIsActive = true;
            }
        }

        public void EndCore()
        {
            Director.RemoveCore(this);
        }

        public void UpdateCore()
        {
            if (CoreIsActive)
            {
                ConstantUpdate();

                switch (State)
                {
                    case State.Idle:
                        StateText.text = "Idle";
                        if (wayPoints.Length != 0)
                        {
                            if (waypointType == WaypointType.Patrol_Conteniusly || waypointType == WaypointType.Patrol_FromTo)
                                State = State.Partol;
                            else if (waypointType == WaypointType.One_Way)
                                State = State.Walking;
                        }
                        break;

                    case State.Walking:
                        StateText.text = "Walking";

                        if (curWaypoint == null && lastWaypoint != null)
                        {
                            curWaypoint = lastWaypoint;
                            Agent.isStopped = false;
                            Agent.SetDestination(curWaypoint.position);
                        }
                        else if (curWaypoint == null)
                        {
                            Agent.isStopped = false;
                            curWaypoint = wayPoints[0];
                            Agent.SetDestination(curWaypoint.position);
                        }

                        if (wayPoints.Length != 0)
                        {
                            if (Common.CheckDistanceToPoint(transform.position, curWaypoint.position, checkDist))
                            {
                                lastWaypoint = curWaypoint;
                                curWaypoint = Common.GetNextWaypoint(wayPoints, curWaypoint, waypointType);

                                wayPoints = Common.RemovePosition(wayPoints, lastWaypoint);
                                lastWaypoint = null;
                            }

                            Agent.SetDestination(curWaypoint.position);
                        }
                        break;

                    case State.Partol:
                        StateText.text = "Patrol";

                        if (curWaypoint == null && lastWaypoint != null)
                        {
                            curWaypoint = lastWaypoint;
                            Agent.isStopped = false;
                            Agent.SetDestination(curWaypoint.position);
                        }
                        else if (curWaypoint == null)
                        {
                            Agent.isStopped = false;
                            curWaypoint = wayPoints[0];
                            Agent.SetDestination(curWaypoint.position);
                        }

                        if (Common.CheckDistanceToPoint(transform.position, curWaypoint.position, checkDist))
                        {
                            curWaypoint = Common.GetNextWaypoint(wayPoints, curWaypoint, waypointType);

                            Agent.SetDestination(curWaypoint.position);
                        }
                        break;

                    case State.Cover:
                        StateText.text = "Cover";
                        break;

                    case State.Fighting:
                        StateText.text = "Fighting";
                        break;
                }
            }

            UpdateState();
        }

        public void UpdateFixedCore()
        {

        }

        private void UpdateState()
        {
            switch (State)
            {
                case State.Idle:
                    IdleStateCheck();
                    break;

                case State.Walking:
                    WalkingStateCheck();
                    break;

                case State.Partol:
                    PatrolStateCheck();
                    break;

                case State.Cover:
                    CoverStateCheck();
                    break;

                case State.Fighting:
                    FightingStateCheck();
                    break;
            }
        }

        private void ConstantUpdate()
        {
            objInRadius = Common.CheckObjectsInRadius(transform.position, sightRadius, targetMask);
            targableObjs = Common.UpdateInSight(transform, objInRadius, lookDirections, sightRadius, targetMask, sightAngel);
        }

        #region SwitchState
        private void IdleStateCheck()
        {
            if (targableObjs.Length != 0)
            {
                State = State.Fighting;
            }
        }

        private void WalkingStateCheck()
        {
            if (targableObjs.Length != 0)
                State = State.Fighting;
            if (wayPoints.Length == 0)
                State = State.Idle;
        }

        private void PatrolStateCheck()
        {
            if (targableObjs.Length != 0)
            {
                State = State.Fighting;
                lastWaypoint = curWaypoint;
                curWaypoint = null;
                Agent.isStopped = true;
            }
            if (wayPoints.Length == 0)
            {
                State = State.Idle;
                curWaypoint = null;
            }
        }

        private void CoverStateCheck()
        {

        }

        private void FightingStateCheck()
        {
            if (targableObjs.Length == 0)
            {
                State = State.Idle;
            }
        }
        #endregion

        #region Input
        public void ReceiveNewWaypoint(GameObject newPoint, AI.WaypointType type, bool contenius)
        {
            bool restart = false;

            if (type != waypointType || !contenius)
            {
                foreach (Transform t in wayPoints)
                    t.gameObject.GetComponent<Waypoint>().Remove();

                wayPoints = new Transform[0];
                restart = true;
            }

            if ((type == AI.WaypointType.Patrol_Conteniusly || type == AI.WaypointType.Patrol_FromTo))
            {
                if (wayPoints.Length == 0)
                {
                    GameObject obj = Instantiate(newPoint);
                    obj.transform.position = transform.position;

                    wayPoints = Common.AddNewPosition(wayPoints, obj.transform, false);
                    wayPoints = Common.AddNewPosition(wayPoints, newPoint.transform, true);
                }
                else
                {
                    wayPoints = Common.AddNewPosition(wayPoints, newPoint.transform, contenius);
                }
            }

            if (type == AI.WaypointType.One_Way)
            {
                wayPoints = Common.AddNewPosition(wayPoints, newPoint.transform, contenius);
            }

            waypointType = type;

            if (restart)
            {
                curWaypoint = wayPoints[0];

                Agent.SetDestination(curWaypoint.position);
            }
        }

        public void StopAll()
        {
            wayPoints = new Transform[0];

            Agent.isStopped = true;
        }
        #endregion
    }
}
