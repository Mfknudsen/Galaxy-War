using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public enum State { Idle, Walking, Partol, Fighting, Surviving }

    public class Core : MonoBehaviour
    {
        [Header("Dev")]
        public TextMesh StateText;

        [Header("Object Reference")]
        public bool CoreIsActive = false;
        public Director Director = null;
        [SerializeField] private Common Common = null;
        [HideInInspector] public int updateDelay = 1;
        [HideInInspector] public bool update = false, fixedUpdate = false;
        [HideInInspector] public Vector3[] lookDirections = new Vector3[0];

        [Header("States :")]
        public State State;

        [Header(" - Movement")]
        public float checkDist = 0.2f;
        public WaypointType waypointType;
        public Transform[] wayPoints = new Transform[0];
        private Transform curWaypoint = null;
        private Transform lastWaypoint = null;
        private NavMeshAgent Agent = null;

        [Header(" - Sight")]
        public Transform sightPos = null;
        public float sightRadius = 15;
        public float sightAngel = 50;
        public LayerMask terrainMask = 0, targetMask = 0;
        [SerializeField] private GameObject[] objInRadius = new GameObject[0];

        [Header(" - Interact")]
        public float interactDist = 2;
        public LayerMask interactMask = 0;
        [SerializeField] private GameObject[] interactableObjectsInRange = new GameObject[0], interactableInSight = new GameObject[0];
        [SerializeField] private GameObject selectedWeaponToTake = null;

        [Header(" - Cover")]
        public LayerMask coverMask = 0;
        public float overideDist = 2.5f;
        [SerializeField] private CoverSpot choosenCover = null;
        [SerializeField] private CoverSpot[] coversInRange = new CoverSpot[0];
        private bool inCover = false;
        private Vector3 posToCheck = Vector3.zero;
        private Vector3 checkedPos = Vector3.zero;

        [Header(" - Fighting")]
        public int inventorySize = 3;
        public bool outOfAmmo = false;
        public Weapon.WeaponType[] faveritWeaponTypes = new Weapon.WeaponType[0];
        public Transform weaponHolder = null;
        public GameObject curWeapon = null;
        public GameObject[] targableObjs = new GameObject[0];
        public GameObject[] weaponsInInventory = new GameObject[0];

        private void Awake()
        {
            weaponsInInventory = new GameObject[inventorySize];

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
                        IdleUpdate();
                        break;

                    case State.Walking:
                        WalkingUpdate();
                        break;

                    case State.Partol:
                        PatrolUpdate();
                        break;

                    case State.Fighting:
                        FightUpdate();
                        break;

                    case State.Surviving:
                        SurvivalUpdate();
                        break;
                }
            }

            SwitchState();
        }

        public void UpdateFixedCore()
        {

        }

        private void SwitchState()
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

                case State.Fighting:
                    FightingStateCheck();
                    break;

                case State.Surviving:
                    SurvivalStateCheck();
                    break;
            }
        }

        private void ConstantUpdate()
        {
            //
            //Enemy targeting
            objInRadius = Common.CheckObjectsInRadius(transform.position, sightRadius, targetMask);
            targableObjs = Common.UpdateInSight(sightPos, objInRadius, lookDirections, sightRadius, targetMask + terrainMask, sightAngel);

            //
            //Interaction
            interactableObjectsInRange = Common.CheckObjectsInRadius(transform.position, sightRadius, interactMask);
            interactableInSight = Common.UpdateInSight(sightPos, interactableObjectsInRange, lookDirections, sightRadius, interactMask + terrainMask, sightAngel);
        }

        #region UpdateState
        private void IdleUpdate()
        {
            StateText.text = "Idle";

            Agent.isStopped = true;
            Agent.SetDestination(transform.position);
        }

        private void WalkingUpdate()
        {
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

            posToCheck = wayPoints[wayPoints.Length - 1].position;
            if (posToCheck != checkedPos)
            {
                CoverSpot[] spots = Common.FindCover(posToCheck, sightRadius, coverMask);

                if (spots.Length != 0)
                {
                    choosenCover = Common.EvaluateCover(spots, objInRadius);

                    wayPoints[wayPoints.Length - 1] = choosenCover.transform;
                }

                checkedPos = posToCheck;
            }
            else if (choosenCover != null)
            {
                if (Common.CheckDistanceToPoint(transform.position, choosenCover.transform.position, 0.1f))
                {
                    inCover = true;

                    Agent.isStopped = true;
                }
            }
        }

        private void PatrolUpdate()
        {
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

            if (!inCover)
            {

            }
        }

        private void FightUpdate()
        {
            StateText.text = "Fighting";

            if (!inCover)
            {
                coversInRange = Common.FindCover(transform.position, sightRadius, coverMask);

                if (coversInRange.Length > 0 && choosenCover == null)
                {
                    CoverSpot checkSpot = Common.EvaluateCover(coversInRange, objInRadius);

                    if (choosenCover != checkSpot)
                    {
                        choosenCover = checkSpot;

                        bool check = (curWaypoint == null);

                        if (!check)
                            check = curWaypoint != choosenCover.transform;

                        if (check)
                        {
                            Cover coverContainer = choosenCover.CoverContainer;
                            coverContainer.usableCoverSpots.Remove(choosenCover.gameObject);
                            curWaypoint = choosenCover.transform;
                            Agent.isStopped = false;
                            Agent.SetDestination(curWaypoint.position);
                        }
                    }
                }
                else if (choosenCover != null)
                {
                    if (Common.CheckDistanceToPoint(transform.position, choosenCover.transform.position, 0.1f))
                    {
                        inCover = true;
                        Agent.isStopped = true;
                    }
                }
            }
        }

        private void SurvivalUpdate()
        {
            StateText.text = "Survival";

            // - Finding new weapon
            if (selectedWeaponToTake == null && outOfAmmo)
            {
                selectedWeaponToTake = Common.FindBestWeapon(transform.position, interactableInSight, weaponsInInventory, faveritWeaponTypes);

                if (selectedWeaponToTake != null)
                {
                    lastWaypoint = curWaypoint;
                    curWaypoint = selectedWeaponToTake.transform;
                    Agent.isStopped = false;
                    Agent.SetDestination(curWaypoint.position);
                }
            }
            else if (selectedWeaponToTake != null)
            {
                if (Common.CheckDistanceToPoint(transform.position, selectedWeaponToTake.transform.position, interactDist))
                {
                    Common.PickUpWeapon(this, selectedWeaponToTake.GetComponent<Weapon.WeaponHolder>());

                    if (lastWaypoint != null)
                    {
                        curWaypoint = lastWaypoint;
                        lastWaypoint = null;
                        Agent.isStopped = false;
                        Agent.SetDestination(curWaypoint.position);
                    }
                }
            }
        }
        #endregion

        #region SwitchState
        private void IdleStateCheck()
        {
            if (targableObjs.Length != 0)
            {
                State = State.Fighting;
            }
            else if (wayPoints.Length != 0)
            {
                if (waypointType == WaypointType.Patrol_Conteniusly || waypointType == WaypointType.Patrol_FromTo)
                    State = State.Partol;
                else if (waypointType == WaypointType.One_Way)
                    State = State.Walking;
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

        private void FightingStateCheck()
        {
            if (targableObjs.Length == 0)
            {
                State = State.Idle;
            }
            else if (outOfAmmo)
            {
                State = State.Surviving;
            }
        }

        private void SurvivalStateCheck()
        {
            if (!outOfAmmo && targableObjs.Length > 0)
                State = State.Fighting;
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
