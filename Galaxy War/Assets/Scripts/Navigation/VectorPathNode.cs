﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorNavigation
{
    public class VectorPathNode : ScriptableObject
    {
        public bool isStart = false;

        public int name = 0;
        public int lastName = -1;

        public VectorNode vecNode = null;
        public VectorPathNode lastNode = null;

        public Vector3 position = Vector3.zero;
        public Vector3 normal = Vector3.zero;

        public float endDistance = 0;
        public float startDistance = 0;

        public void Setup(VectorNode node, Vector3 start, Vector3 end)
        {
            vecNode = node;

            position = node.postition + node.direction;
            normal = node.normal;

            startDistance = Vector3.Distance(position, start);
            endDistance = Vector3.Distance(position, end);
        }

        public float GetWorth()
        {
            return endDistance + startDistance;
        }
    }
}