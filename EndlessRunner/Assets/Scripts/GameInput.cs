using UnityEngine;
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

        _playerInputActions.Player.GoLeft.performed += _ => OnGoLeftAction?.Invoke(this, EventArgs.Empty);
        _playerInputActions.Player.GoRight.performed += _ => OnGoRightAction?.Invoke(this, EventArgs.Empty);
        _playerInputActions.Player.SlideUnder.performed += _ => OnSlideUnderAction?.Invoke(this, EventArgs.Empty);
        _playerInputActions.Player.Jump.performed += _ => OnJumpAction?.Invoke(this, EventArgs.Empty);     
    }

}
