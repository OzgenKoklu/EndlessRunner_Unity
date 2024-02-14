using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    public event EventHandler OnGoLeftAction;
    public event EventHandler OnGoRightAction;
    public event EventHandler OnSlideUnderAction;
    public event EventHandler OnJumpAction;

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        Instance = this;
        _playerInputActions = new PlayerInputActions();

        _playerInputActions.Player.Enable();

        _playerInputActions.Player.GoLeft.performed += GoLeft_performed;
        _playerInputActions.Player.GoRight.performed += GoRight_performed;
        _playerInputActions.Player.SlideUnder.performed += SlideUnder_performed;
        _playerInputActions.Player.Jump.performed += Jump_performed;


    }

    private void GoLeft_performed(InputAction.CallbackContext obj)
    {
        OnGoLeftAction?.Invoke(this, EventArgs.Empty);
    }

    private void GoRight_performed(InputAction.CallbackContext obj)
    {
        OnGoRightAction.Invoke(this, EventArgs.Empty);
    }

    private void SlideUnder_performed(InputAction.CallbackContext obj)
    {
        OnSlideUnderAction.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        OnJumpAction.Invoke(this, EventArgs.Empty);
    }
}
