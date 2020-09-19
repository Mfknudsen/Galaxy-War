﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace VectorNavigation
{
    public class VectorAgent : MonoBehaviour
    {
        public bool debugMode = false;

        public float MoveSpeed = 1, RotSpeed;

        [Header("Pathfinding:")]
        public NavVector field = null;
        private VectorPathfinding pathFinder = null;
        private Coroutine currentPathCalculation = null;

        [Header("Enabels:")]
        public bool enableMovement = true;
        public bool enableRotation = true;

        [Header("Movement:")]
        public float moveSpeed = 2.5f;
        public float moveStoppindDist = 0.1f;
        private int moveIndex = 0;
        private bool checkPoints = true;
        private Vector3 lastPos = Vector3.zero;
        [SerializeField] private Vector3[] movePoints = new Vector3[0];

        [Header("Rotation:")]
        public float rotSpeed = 1;

        private void Update()
        {
            #region Debug:
            if (debugMode)
                pathFinder.DebugPath();
            #endregion

            if (enableMovement)
                Move();

            if (enableRotation)
                Rotate();
        }

        #region Movement Calculations:
        private void Move()
        {
            if (checkPoints)
            {
                movePoints = pathFinder.GetMovePoints();
                if (movePoints.Length > 0)
                    lastPos = transform.position;
            }

            if (movePoints.Length == 0)
            {
                moveIndex = 0;
                checkPoints = true;
            }
            else
                checkPoints = false;

            if (movePoints.Length > 0 && moveIndex < movePoints.Length)
            {
                transform.position += movePoints[moveIndex] * moveSpeed * Time.deltaTime;

                if (Vector3.Distance(transform.position, lastPos + movePoints[moveIndex]) <= moveStoppindDist)
                {
                    lastPos += movePoints[moveIndex];
                    moveIndex++;
                    Debug.Log(moveIndex);
                }
            }
        }
        #endregion

        #region Rotation Calculations:
        private void Rotate()
        {

        }
        #endregion

        #region Input:
        public void Setup(NavVector newField)
        {
            field = newField;
            pathFinder = ScriptableObject.CreateInstance("VectorNavigation.VectorPathfinding") as VectorNavigation.VectorPathfinding;
            pathFinder.Setup(field, true);
        }

        public void SetDestination(Vector3 newDestination)
        {
            movePoints = new Vector3[0];
            currentPathCalculation = StartCoroutine(pathFinder.FindPath(newDestination, transform.position));
        }
        #endregion
    }
}