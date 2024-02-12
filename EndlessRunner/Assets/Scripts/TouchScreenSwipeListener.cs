using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;

public class SwipeListener : MonoBehaviour
{
    private Vector2 startPosition;
    private PlayerInputActions _playerInputActions;

    private void OnEnable()
    {
      //  _playerInputActions.TouchControls.started += OnTouchStart;
      //  TouchControls.touchEnded.ended += OnTouchEnd;
    }

    private void OnDisable()
    {
       // TouchControls.touchStarted.started -= OnTouchStart;
        //TouchControls.touchEnded.ended -= OnTouchEnd;
    }

    private void OnTouchStart(InputAction.CallbackContext context)
    {
        startPosition = context.ReadValue<Vector2>();
    }

    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        Vector2 endPosition = context.ReadValue<Vector2>();
        Vector2 swipe = endPosition - startPosition;

        if (swipe.x < -minSwipeDistance)
        {
            // Trigger your button action
            Debug.Log("Left swipe detected!");
        }
    }

    // Adjust these values based on your needs
    private float minSwipeDistance = 50f;
}