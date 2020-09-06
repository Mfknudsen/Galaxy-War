using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class Controller : MonoBehaviour
    {
        #region Values

        [Header("System")]
        public bool DebugSystem = false;

        [Header("Object Reference")]
        public State State = 0;
        public List<Override> overrides = new List<Override>();
        public Rigidbody RB = null;
        public GameObject Cam = null;
        public Common common = null;

        [Header("Movement")]
        public float MoveSpeed = 1.0f;
        private float inputX = 0, inputZ = 0;
        private Vector3 TargetPos = Vector3.zero;

        [Header("Rotation")]
        public float RotSpeed = 1.0f;
        private float rotX = 0, rotY = 0, camX = 0;

        [Header("Jump")]
        public float jumpHeight = 3f;
        private bool hasDoubleJumped = false;

        [Header("Gravity")]
        public bool localGravity = false;
        public float gravity = -9.81f;
        private Vector3 velocity = Vector3.zero, gravDir = Vector3.up;

        [Header("Ground Check")]
        public bool isGrounded = false;
        public Transform groundCheck = null;
        public float groundDistance = 0.4f;
        public LayerMask groundMask = 0;

        [Header("Dash")]
        public bool RechargeAll = false;
        public int dashAmount = 2;
        public float dashSpeed = 2f, dashTime = 2f, dashRechargeTime = 2f;
        private int curDashAmount = 0;
        private float currentDashSpeed = 0, currentDashRecharge = 0, currentDashTime = 0;
        private Vector3 dashVector = Vector3.zero;

        [Header("Local Orientation")]
        public bool changeLocalOrientation = false;
        public float sphereRadius = 1;
        public float maxDistance = 1;
        public LayerMask layerMask = 0;
        private Vector3 goalNormal = Vector3.up, origin = Vector3.zero, direction = Vector3.zero;
        #endregion

        void Start()
        {
            common = ScriptableObject.CreateInstance("Common") as Common;
        }

        private void Update()
        {
            Vector2 moveAxis = common.GetAxises();
            inputX = moveAxis[0];
            inputZ = moveAxis[1];

            Vector2 rotAxis = common.GetMouseVector();
            rotX = rotAxis[0];
            rotY = rotAxis[1];
        }

        void FixedUpdate()
        {
            switch (State)
            {
                case State.Grounded:
                    Move();
                    Rot();
                    Gravity();
                    Dash();
                    break;

                case State.Airborne:
                    Move();
                    Rot();
                    Gravity();
                    Dash();
                    break;

                case State.Dashing:
                    Dash();
                    Rot();
                    break;

                case State.AirDash:
                    Dash();
                    Rot();
                    break;
            }
        }

        private void Move()
        {
            TargetPos = (RB.transform.forward * inputZ + RB.transform.right * inputX).normalized * MoveSpeed * Time.deltaTime;

            RB.MovePosition(transform.position + TargetPos);
        }

        private void Rot()
        {
            rotX *= Time.fixedDeltaTime;

            camX -= rotY * Time.fixedDeltaTime * RotSpeed * 10;
            camX = Mathf.Clamp(camX, -90f, 90f);

            Cam.transform.localRotation = Quaternion.Euler(camX, 0f, 0f);

            transform.RotateAround(transform.position, transform.up, rotX * RotSpeed * 10);
        }

        private void Gravity()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);

            Jump();

            RB.AddForce(velocity);

            if (localGravity)
                velocity += gravDir * gravity * Time.deltaTime;
            else
                velocity.y += gravity * Time.deltaTime;

            if (isGrounded)
            {
                if (localGravity)
                    velocity = gravDir * -2.0f;
                else
                    velocity.y = -2.0f;

                if (State == State.Airborne)
                    State = State.Grounded;
            }
            else if (State != State.Dashing && State != State.AirDash)
                State = State.Airborne;
        }

        private void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !hasDoubleJumped)
            {
                hasDoubleJumped = true;

                velocity = Vector3.zero;
                RB.velocity = Vector3.zero;
                RB.AddForce(transform.up * Mathf.Sqrt(jumpHeight * -2 * gravity), ForceMode.VelocityChange);
            }

            if (Input.GetKeyDown(KeyCode.Space) && State == State.Grounded)
            {
                hasDoubleJumped = false;

                velocity = Vector3.zero;
                RB.velocity = Vector3.zero;
                RB.AddForce(transform.up * Mathf.Sqrt(jumpHeight * -2 * gravity), ForceMode.VelocityChange);

                State = State.Airborne;
            }
        }

        private void Dash()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && dashAmount > curDashAmount && currentDashSpeed == 0)
            {
                currentDashSpeed = dashSpeed;

                if (inputX != 0 || inputZ != 0)
                    dashVector = (transform.forward * inputZ + transform.right * inputX).normalized;
                else
                    dashVector = transform.forward;

                if (State != State.AirDash && State != State.Dashing)
                {
                    if (State == State.Airborne)
                        State = State.AirDash;
                    else
                        State = State.Dashing;
                }

                velocity = Vector3.zero;
                RB.velocity = Vector3.zero;

                curDashAmount++;

                RB.velocity = dashVector * currentDashSpeed;
            }

            if (curDashAmount != 0)
            {
                if (currentDashRecharge >= dashRechargeTime)
                {
                    currentDashRecharge = 0;

                    if (RechargeAll)
                        curDashAmount = 0;
                    else
                        curDashAmount--;
                }
                else
                    currentDashRecharge += Time.fixedDeltaTime;
            }

            if (State == State.Dashing || State == State.AirDash)
            {
                if (dashTime <= currentDashTime)
                {
                    currentDashSpeed = 0;
                    currentDashTime = 0;
                    RB.velocity = Vector3.zero;

                    if (State == State.AirDash)
                        State = State.Airborne;
                    else
                        State = State.Grounded;
                }
                else
                    currentDashTime += Time.fixedDeltaTime;
            }
        }
    }
}

