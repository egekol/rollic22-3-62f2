using System;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Scripts.Gameplay.Inputs
{

    public class InputAdapter : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSensitivity = 1f;
        private Camera _raycastCamera;
        
        public event Action<float2> OnScrollStart;
        public event Action<float2> OnScrollUpdate; 
        public event Action OnScrollEnd;
        
        private bool _isScrolling;
        private Vector2 _lastInputPosition;


        private void OnEnable()
        {
            OnScrollUpdate += OnScrollUpdated;
            OnScrollStart += OnScrollStarted;
            OnScrollEnd += OnScrollEnded;
        }

        private void OnDisable()
        {
            OnScrollUpdate -= OnScrollUpdated;
            OnScrollStart -= OnScrollStarted;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            bool inputPressed = Input.GetMouseButton(0);
            bool inputDown = Input.GetMouseButtonDown(0);
            bool inputUp = Input.GetMouseButtonUp(0);
            
            Vector2 currentInputPosition = Input.mousePosition;
            
            if (inputDown)
            {
                _isScrolling = true;
                _lastInputPosition = currentInputPosition;
                OnScrollStart?.Invoke(currentInputPosition);
            }
            else if (inputUp && _isScrolling)
            {
                _isScrolling = false;
                OnScrollEnd?.Invoke();
            }
            else if (inputPressed && _isScrolling)
            {
                Vector2 deltaPosition = (currentInputPosition - _lastInputPosition) * _scrollSensitivity;
                OnScrollUpdate?.Invoke(deltaPosition);
                _lastInputPosition = currentInputPosition;
            }
        }


        public void SetScrollSensitivity(float sensitivity)
        {
            _scrollSensitivity = sensitivity;
        }

        private void OnScrollEnded()
        {
            InputManager.Instance.OnScrollEnded();
        }

        public void OnScrollUpdated(float2 scrollPosition)
        {
            InputManager.Instance.OnScrollUpdated(scrollPosition);
        }

        private void OnScrollStarted(float2 startPos)
        {
            InputManager.Instance.OnScrollStarted(startPos);
        }
    }
}