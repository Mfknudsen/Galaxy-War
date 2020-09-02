//Unity Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commander
{
    public enum State { Freeroam, Cinematic, Static };

    public class CommanderCore : MonoBehaviour
    {
        #region Values
        [Header("Object Reference")]
        public Camera cam = null;

        public bool active = true;

        [Header("Movement")]
        public float scroolSpeed = 1;
        public float moveSpeed = 1, rotSpeed = 1;
        Vector2 heightBounds = new Vector2(10, 50), scroolBounds = new Vector2(10, 50);


        [Header("Interaction")]
        public Movement controller = null;
        public InteractWithAIMovement interacter = null;
        #endregion

        private void Awake()
        {
            if (controller == null)
                controller = gameObject.AddComponent<Movement>();
            controller.Setup(moveSpeed, rotSpeed, scroolSpeed, heightBounds, scroolBounds);

            if (interacter == null)
                interacter = gameObject.AddComponent<InteractWithAIMovement>();
            interacter.cam = cam;
        }

        private void Update()
        {
            controller.UpdateMovement();
            interacter.UpdateInteraction();
        }
    }
}
