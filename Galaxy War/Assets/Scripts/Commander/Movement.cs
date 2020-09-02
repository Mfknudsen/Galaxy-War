using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commander
{
    public class Movement : MonoBehaviour
    {
        #region Values
        public bool mouseMovement = true;
        public bool keyboardMovement = true;

        public Vector2 heightBounderies = new Vector2(10, 50), scroolBounderies = new Vector2(10, 100);
        public float moveSpeed = 1, rotSpeed = 1, scroolSpeed = 1;

        private float xMove = 0, zMove = 0;
        private float yRot = 0, curRot = 0;
        private float scrool = 0, curScroll = 0;

        private Vector3 dirVector = Vector3.zero;
        #endregion

        private void Awake()
        {
            curRot = transform.rotation.eulerAngles.y;
        }

        public void Setup(float mSpeed, float rSpeed, float sSpeed, Vector2 hBounds, Vector2 sBounds)
        {
            moveSpeed = mSpeed;
            rotSpeed = rSpeed;
            scroolSpeed = sSpeed;

            heightBounderies = hBounds;
            scroolBounderies = sBounds;
        }

        public void UpdateMovement()
        {
            GetInput();
            CalculateVectorMovement();
            CalculateQuaternionRotation();
            CalculateHeight();
            CalculateScroolZoom();
        }

        private void GetInput()
        {
            xMove = Input.GetAxis("Horizontal");
            zMove = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
                yRot = 0;
            else if (Input.GetKey(KeyCode.Q))
                yRot = -1;
            else if (Input.GetKey(KeyCode.E))
                yRot = 1;
            else
                yRot = 0;

            scrool = Input.GetAxis("Mouse ScrollWheel");
        }

        private void CalculateVectorMovement()
        {
            dirVector = (transform.forward * zMove + transform.right * xMove).normalized;

            dirVector *= moveSpeed * Time.deltaTime;

            transform.position += dirVector;
        }

        private void CalculateQuaternionRotation()
        {
            curRot += yRot * rotSpeed * Time.deltaTime;

            transform.rotation = Quaternion.AngleAxis(curRot, transform.up);
        }

        private void CalculateHeight()
        {

        }

        private void CalculateScroolZoom()
        {

        }
    }
}
