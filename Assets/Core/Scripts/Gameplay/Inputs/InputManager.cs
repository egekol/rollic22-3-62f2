using System;
using Core.Scripts.Gameplay.Managers;
using Core.Scripts.Lib.Utility;
using Lib.Debugger;
using Unity.Mathematics;

namespace Core.Scripts.Gameplay.Inputs
{
    public class InputManager : Singleton<InputManager>
    {

        public InputState InputState { get; private set; }
        public InputDirection InputDirection { get; set; }
        
        public void OnScrollUpdated(float2 scrollPosition)
        {
            if (InputState is InputState.Disabled)
            {
                return;
            }
            
            InputState = InputState.Scrolling;
            if (math.abs(scrollPosition.x) > math.abs(scrollPosition.y))
            {
                InputDirection = scrollPosition.x > 0 ? InputDirection.Right : InputDirection.Left;
            }
            else if (math.abs(scrollPosition.y) > math.abs(scrollPosition.x))
            {
                InputDirection = scrollPosition.y > 0 ? InputDirection.Up : InputDirection.Down;
            }
            else
            {
                InputDirection = InputDirection.None;
            }

            GameSettings.Instance.OnInputUpdated(InputState, InputDirection);
        }
        
        public void SetInputState(InputState state)
        {
            InputState = state;
            if (state == InputState.Disabled)
            {
                InputDirection = InputDirection.None;
            }
        }

        public void OnScrollStarted(float2 startPos)
        {
            
        }
    }
    
    
    public enum InputDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    
    public enum InputState
    {
        None,
        Disabled,
        Enabled,
        Scrolling,
    }
}