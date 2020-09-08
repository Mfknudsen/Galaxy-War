using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public class Core : MonoBehaviour
    {
        [Header("Dev:")]
        public TextMesh StateText;

        [Header("Object Reference:")]
        public bool CoreIsActive = false;
        public Director Director = null;
        [SerializeField] private Common Common = null;
        [HideInInspector] public int updateDelay = 1;
        [HideInInspector] public bool update = false, fixedUpdate = false;
        [HideInInspector] public Vector3[] lookDirections = new Vector3[0];

        [Header("Squad:")]
        public Squad.Core currentSquad = null;
        public bool isLeader = false;

        [Header("States:")]
        public State State;

        [Header(" - Movement")]
        public float moveSpeed = 1;
        public float checkDist = 0.2f;
        [HideInInspector] public NavMeshAgent Agent = null;
        public Vector3 curWaypoint = Vector3.zero;

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

        [Header(" - Elevator Use")]
        public bool toUseElevator = false;
        public ElevateUseState useState = 0;
        public Elevator.Elevator mainPart = null;
        private Vector3 resumePoint = Vector3.zero;
        private int entry = 0, exit = 0;
        private Elevator.MeshLinkDetector entryLinkDetector = null;

        private void Awake()
        {
            Common = ScriptableObject.CreateInstance("AI.Common") as AI.Common; ;

            weaponsInInventory = new GameObject[inventorySize];

            Director = GameObject.FindGameObjectWithTag("AI Director").GetComponent<Director>();
            Director.AddNewCore(this);

            transform.parent = currentSquad.transform;
            currentSquad.members[0] = gameObject;
        }

        public void StartCore(Vector3[] dir)
        {
            lookDirections = dir;

            if (!CoreIsActive)
            {
                Agent = gameObject.AddComponent<NavMeshAgent>();
                Agent.isStopped = false;
                Agent.speed = moveSpeed;
                Agent.autoTraverseOffMeshLink = false;

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

                    case State.UseElevator:
                        ElevatorUseUpdate();
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

                case State.UseElevator:
                    ElevatorUseCheck();
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

            /* if (curWaypoint == null && lastWaypoint != null)
             {
                 curWaypoint = lastWaypoint;
                 Agent.isStopped = false;
                 Agent.SetDestination(curWaypoint);
             }
             else if (curWaypoint == null)
             {
                 Agent.isStopped = false;
                 curWaypoint = wayPoints[0];
                 Agent.SetDestination(curWaypoint);
             }

             if (wayPoints.Length != 0)
             {
                 if (Common.CheckDistanceToPoint(transform.position, curWaypoint, checkDist))
                 {
                     lastWaypoint = curWaypoint;
                     curWaypoint = Common.GetNextWaypoint(wayPoints, curWaypoint, waypointType);

                     wayPoints = Common.RemovePosition(wayPoints, lastWaypoint);
                     lastWaypoint = Vector3.zero;
                 }

                 Agent.SetDestination(curWaypoint);
             }

             posToCheck = wayPoints[wayPoints.Length - 1];
             if (posToCheck != checkedPos)
             {
                 CoverSpot[] spots = Common.FindCover(posToCheck, sightRadius, coverMask);

                 if (spots.Length != 0)
                 {
                     choosenCover = Common.EvaluateCover(spots, objInRadius);

                     wayPoints[wayPoints.Length - 1] = choosenCover.transform.position;
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
            */
        }

        private void PatrolUpdate()
        {
            StateText.text = "Patrol";

            /*
            if (curWaypoint == null && lastWaypoint != null)
            {
                curWaypoint = lastWaypoint;
                Agent.isStopped = false;
                Agent.SetDestination(curWaypoint);
            }
            else if (curWaypoint == null)
            {
                Agent.isStopped = false;
                curWaypoint = wayPoints[0];
                Agent.SetDestination(curWaypoint);
            }

            if (Common.CheckDistanceToPoint(transform.position, curWaypoint, checkDist))
            {
                curWaypoint = Common.GetNextWaypoint(wayPoints, curWaypoint, waypointType);

                Agent.SetDestination(curWaypoint);
            }

            if (!inCover)
            {

            }
            */
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
                            check = curWaypoint != choosenCover.transform.position;

                        if (check)
                        {
                            Cover coverContainer = choosenCover.CoverContainer;
                            coverContainer.usableCoverSpots.Remove(choosenCover.gameObject);
                            curWaypoint = choosenCover.transform.position;
                            Agent.isStopped = false;
                            Agent.SetDestination(curWaypoint);
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
            /*
             if (selectedWeaponToTake == null && outOfAmmo)
             {
                 selectedWeaponToTake = Common.FindBestWeapon(transform.position, interactableInSight, weaponsInInventory, faveritWeaponTypes);

                 if (selectedWeaponToTake != null)
                 {
                     lastWaypoint = curWaypoint;
                     curWaypoint = selectedWeaponToTake.transform.position;
                     Agent.isStopped = false;
                     Agent.SetDestination(curWaypoint);
                 }
             }
             else if (selectedWeaponToTake != null)
             {
                 if (Common.CheckDistanceToPoint(transform.position, selectedWeaponToTake.transform.position, interactDist))
                 {
                     Common.PickUpWeapon(this, selectedWeaponToTake.GetComponent<Weapon.Common>());

                     if (lastWaypoint != null)
                     {
                         curWaypoint = lastWaypoint;
                         lastWaypoint = Vector3.zero;
                         Agent.isStopped = false;
                         Agent.SetDestination(curWaypoint);
                     }
                 }
             }
            */
        }

        private void ElevatorUseUpdate()
        {
            switch (useState)
            {
                case ElevateUseState.Null:
                    Agent.isStopped = true;
                    resumePoint = curWaypoint;
                    curWaypoint = Vector3.zero;

                    useState = ElevateUseState.Waiting;

                    break;

                case ElevateUseState.Waiting:
                    if (!mainPart.floorsToVisit.Contains(entry))
                        mainPart.floorsToVisit.Add(entry);

                    mainPart.AllowSquadsIn(entry, currentSquad);

                    if (mainPart.onPlatform.Contains(currentSquad))
                        useState = ElevateUseState.Enter;
                    break;

                case ElevateUseState.Enter:
                    break;

                case ElevateUseState.Use:
                    break;

                case ElevateUseState.Exit:
                    break;
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
            /*
               else if (wayPoints.Length != 0)
               {
                   if (waypointType == WaypointType.Patrol_Conteniusly || waypointType == WaypointType.Patrol_FromTo)
                       State = State.Partol;
                   else if (waypointType == WaypointType.One_Way)
                       State = State.Walking;
               }
           */
        }

        private void WalkingStateCheck()
        {
            if (targableObjs.Length != 0)
                State = State.Fighting;
            /*
            if (wayPoints.Length == 0)
                State = State.Idle;
            */

            if (entryLinkDetector != null && Agent.currentOffMeshLinkData.offMeshLink == entryLinkDetector)
                State = State.UseElevator;
        }

        private void PatrolStateCheck()
        {
            if (targableObjs.Length != 0)
            {
                State = State.Fighting;
                curWaypoint = Vector3.zero;
                Agent.isStopped = true;
            }

            if (Agent.currentOffMeshLinkData.offMeshLink != null && entryLinkDetector != null)
            {
                if (entryLinkDetector != null && Agent.currentOffMeshLinkData.offMeshLink.gameObject == entryLinkDetector.gameObject)
                    State = State.UseElevator;
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
            {
                State = State.Fighting;
            }
        }

        private void ElevatorUseCheck()
        {

        }
        #endregion

        #region Input
        public void ReceiveNewWaypoint(Vector3 newPoint)
        {
            bool restart = false;

            State = AI.State.Walking;
            curWaypoint = newPoint;
            Agent.isStopped = false;
            Agent.SetDestination(curWaypoint);

            /*
            if (type != waypointType || !contenius)
            {
                wayPoints = new Vector3[0];
                restart = true;
            }

            if ((type == AI.WaypointType.Patrol_Conteniusly || type == AI.WaypointType.Patrol_FromTo))
            {
                if (wayPoints.Length == 0)
                {

                    wayPoints = Common.AddNewPosition(wayPoints, transform.position, false);
                    wayPoints = Common.AddNewPosition(wayPoints, newPoint, true);
                }
                else
                {
                    wayPoints = Common.AddNewPosition(wayPoints, newPoint, contenius);
                }
            }

            if (type == AI.WaypointType.One_Way)
            {
                wayPoints = Common.AddNewPosition(wayPoints, newPoint, contenius);
            }

            waypointType = type;

            if (restart)
            {
                curWaypoint = wayPoints[0];

                Agent.SetDestination(curWaypoint);
            }
            */
        }

        public void StopAll()
        {
            curWaypoint = transform.position;

            Agent.isStopped = true;
        }
        #endregion
    }
}
