using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TyporiGame
{
    public class PlayerInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        // public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool interact;
        
        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        
        public void OnMove(InputValue value)
        {
            move = value.Get<Vector2>();
        }

        // public void OnLook(InputValue value)
        // {
        //     look = value.Get<Vector2>();
        // }

        public void OnJump(InputValue value)
        {
            jump = value.isPressed;
        }
        
        public void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
        }

        public void OnInteract(InputValue value)
        {
            interact = value.isPressed;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }  
}

