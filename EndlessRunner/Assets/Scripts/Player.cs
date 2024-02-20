using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private Vector3 _leftLanePosition = new Vector3(-2.3f, 0, 0);
    private Vector3 _middleLanePosition = new Vector3(0, 0, 0); 
    private Vector3 _rightLanePosition = new Vector3(2.3f, 0, 0);

    private Vector3 _upperPlanePosition = new Vector3(0, 2f, 0);
    private Vector3 _lowerPlanePosition = new Vector3(0, 0, 0);


    [SerializeField] private GameInput _gameInput;
    [SerializeField] private PlayerCollisionDetection _playerCollisionDetection;

    public event EventHandler OnJumpMade;
    public event EventHandler OnSlideMade;
    public event EventHandler OnSlideEnd;
    public event EventHandler OnGroundHit;
    public event EventHandler OnPlayerHealthDepleted;
    public event EventHandler OnWallCrash;
    public event EventHandler OnPlayerFalling;
    public event EventHandler OnCoinCountChanged;
    public event EventHandler OnPlayerHealthDecreased;
    public event EventHandler OnInvincibilityPeriodEnd;

    private int _playerHealth = 3;
    private int _playerCoinCount;
    private bool _isInvincible;

    private Vector3 _targetPosition;
    private float _playerSpeed = 10.0f;
    private const float _initialPlayerSpeed = 10f;
    private float _threshold = 0.1f;
    private bool _isMoving = false;
    private float _playerSpeedModifier = 1;

    private bool _isJumping = false;
    private Vector3 _originalPosition;
    private float _jumpHeight = 1.5f; // Height to jump
    private float _jumpSpeed = 3f;

    private bool _isPlayerInTransition = false;

    //henüz kullanmadým ama düþünücem
    public event EventHandler<OnPlayerStateChangedEventArgs> OnPlayerStateChanged;  
    public class OnPlayerStateChangedEventArgs: EventArgs { public PlayerState PlayerState; }



    private PlayerState _playerState;


    public enum PlayerState
    {
        Running,
        Sliding,
        Jumping,
        GameOver,
    }

    public enum Direction
    {
        Right,
        Left,
    }

    private void Awake()
    {
       Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _gameInput.OnGoLeftAction += _gameInput_OnGoLeftAction;
        _gameInput.OnGoRightAction += _gameInput_OnGoRightAction;
        _gameInput.OnJumpAction += _gameInput_OnJumpAction;
        _gameInput.OnSlideUnderAction += _gameInput_OnSlideUnderAction;
        _playerCollisionDetection.OnGroundHit += _playerCollisionDetection_OnGroundHit;
        _playerCollisionDetection.OnRampContact += _playerCollisionDetection_OnRampContact;
        _playerCollisionDetection.OnGroundContactLost += _playerCollisionDetection_OnGroundContactLost;
        _playerCollisionDetection.OnWallObstacleHit += _playerCollisionDetection_OnWallObstacleHit;
        _playerCollisionDetection.OnObstacleHit += _playerCollisionDetection_OnObstacleHit;
        _playerCollisionDetection.OnCoinGrabbed += _playerCollisionDetection_OnCoinGrabbed;
        GameManager.Instance.OnScoreMultiplierChanged += GameManager_OnScoreMultiplierChanged;

        _playerState = PlayerState.Running;
         _isInvincible = false ;
        _isMoving = false;
        _playerCoinCount = 0;
        _playerSpeed = _initialPlayerSpeed;
        _playerSpeedModifier = 1;
        OnCoinCountChanged?.Invoke(this, EventArgs.Empty);
    }

    // Call this method to initiate the jump action
    public void Jump()
    {
        if (!_isJumping)
        {
            _isJumping = true;
            _originalPosition = transform.position;
            StartCoroutine(PerformJump());
        }
    }
    // Coroutine for performing the jump
    private IEnumerator PerformJump()
    {
        Vector3 targetPosition = _originalPosition + Vector3.up * _jumpHeight; // Calculate target position for the jump

        // Smoothly transition the player's Y position to the target position
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(_originalPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * _jumpSpeed; // Adjust jump speed as needed
            yield return null;
        }

        // Smoothly return the player's Y position to the original position
        elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(targetPosition, _originalPosition, elapsedTime);
            elapsedTime += Time.deltaTime * _jumpSpeed; // Adjust jump speed as needed
            yield return null;
        }

        //this is to avoid player staying in the air
        if(_playerState == PlayerState.Jumping)
        {
            //player still didnt make any contact with ground lets trigger fall animation
            OnPlayerFalling?.Invoke(this, EventArgs.Empty);
            Vector3 newTargetPosition = FindClosestPath();
            elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                transform.position = Vector3.Lerp(_originalPosition, newTargetPosition, elapsedTime);
                elapsedTime += Time.deltaTime * _jumpSpeed; // Adjust jump speed as needed
                yield return null;
            }
            
        }
        // Reset jumping state after completing the jump
        _isJumping = false;
    }

    private void GameManager_OnScoreMultiplierChanged(object sender, EventArgs e)
    {
        _playerSpeedModifier = GameManager.Instance.GetSpeedModifier();
       
        _playerSpeed = _initialPlayerSpeed * _playerSpeedModifier;

        Debug.Log(_playerSpeed);
    }

    private void _playerCollisionDetection_OnCoinGrabbed(object sender, EventArgs e)
    {
        _playerCoinCount += 1; 
        OnCoinCountChanged?.Invoke(this, EventArgs.Empty);
    }

    private void _playerCollisionDetection_OnObstacleHit(object sender, EventArgs e)
    {
        if(!_isInvincible)
        {
            _playerHealth -= 1;
            OnPlayerHealthDecreased?.Invoke(this, EventArgs.Empty);

            if (_playerHealth == 0)
            {
                //actually I invoked this event to avoid blinking when character dies from losing hearts but I think it is not intuative to do it like this
                OnInvincibilityPeriodEnd?.Invoke(this, EventArgs.Empty);
                OnWallCrash?.Invoke(this, EventArgs.Empty);
                OnPlayerHealthDepleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StartCoroutine(StartInvincibilityPeriod(1.5f)); // 1.5 second invinsibility period
            }
        }       
    }

    private IEnumerator StartInvincibilityPeriod(float duration)
    {
        _isInvincible = true;

        yield return new WaitForSeconds(duration);

        _isInvincible = false;
        // after the duration, event is triggered
        OnInvincibilityPeriodEnd?.Invoke(this, EventArgs.Empty);
    }

    public int PlayerHealthAmount()
    {
        return _playerHealth;
    }

    private void PlayerHealtDepleted()
    {
        OnPlayerHealthDepleted?.Invoke(this, EventArgs.Empty);
    }

    private void _playerCollisionDetection_OnWallObstacleHit(object sender, EventArgs e)
    {
        OnWallCrash?.Invoke(this, EventArgs.Empty);
        PlayerHealtDepleted();
    }

    private void _playerCollisionDetection_OnGroundContactLost(object sender, EventArgs e)
    {
        Vector3 _newCharacterPosition = new Vector3(transform.position.x, _lowerPlanePosition.y, transform.position.z);
        Debug.Log("LowerPlanePosition Triggered");
        _targetPosition = _newCharacterPosition;
        _isMoving = true;
    }

    private void _playerCollisionDetection_OnGroundHit(object sender, EventArgs e)
    {
        Debug.Log("player: event triiggered: player state: " + _playerState);
        _playerState = PlayerState.Running;
        OnGroundHit?.Invoke(this, EventArgs.Empty);
    }

    private void _gameInput_OnSlideUnderAction(object sender, System.EventArgs e)
    {
        if(GameManager.Instance.IsGameOver()) return; 
        if(GameManager.Instance.IsGamePaused()) return;

        if (IsCharacterOnTheTrack() && IsPlayerRunning())
        {
            _playerState = PlayerState.Sliding;
            OnSlideMade?.Invoke(this, EventArgs.Empty);
            StartCoroutine(SlidingTimerTrigger());
        }
    }

    private void _gameInput_OnJumpAction(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver()) return;
        if (GameManager.Instance.IsGamePaused()) return;

        if (IsCharacterOnTheTrack() && IsPlayerRunning() && !IsPlayerJumping() && !_isJumping)
        {
            _playerState = PlayerState.Jumping;
            Jump();
            OnJumpMade?.Invoke(this, EventArgs.Empty);
           // StartCoroutine(TriggerDelay());
        }
    }

    IEnumerator SlidingTimerTrigger()
    {
        // Wait for 1 second
        yield return new WaitForSeconds(0.1f);

        _playerState = PlayerState.Running;
        // Set the trigger on the animator
        OnSlideEnd?.Invoke(this, EventArgs.Empty);
    }

    private void _gameInput_OnGoRightAction(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver()) return;
        if (GameManager.Instance.IsGamePaused()) return;

        if (IsCharacterOnTheTrack() && !IsPlayerJumping())
        {
            SelectMovePosition(Direction.Right);
        }      
    }

    private void _gameInput_OnGoLeftAction(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver()) return;
        if (GameManager.Instance.IsGamePaused()) return;

        if (IsCharacterOnTheTrack() && !IsPlayerJumping())
        {
            SelectMovePosition(Direction.Left);
        }     
    }
    
    private void SelectMovePosition(Direction direction)
    {
       switch(direction)
       {
            case Direction.Left:

                //does nothing if character is already on the left lane
                if (transform.position.x == _leftLanePosition.x)
                {
                    return;
                }

                if (transform.position.x == _rightLanePosition.x)
                {
                    Vector3 _newCharacterPosition = new Vector3(_middleLanePosition.x, transform.position.y, transform.position.z);
                    _targetPosition = _newCharacterPosition;
                    _isMoving = true;
                    OnJumpMade?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Vector3 _newCharacterPosition = new Vector3(_leftLanePosition.x, transform.position.y, transform.position.z);
                    _targetPosition = _newCharacterPosition;
                    _isMoving = true;
                    OnJumpMade?.Invoke(this, EventArgs.Empty);
                }
            break;
            case Direction.Right:

                //does nothing if character is already on the right lane
                if (transform.position.x == _rightLanePosition.x)
                {
                    return;
                }

                if (transform.position.x == _leftLanePosition.x)
                {
                    Vector3 _newCharacterPosition = new Vector3(_middleLanePosition.x, transform.position.y, transform.position.z);
                    _targetPosition = _newCharacterPosition;
                    _isMoving = true;
                    OnJumpMade?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Vector3 _newCharacterPosition = new Vector3(_rightLanePosition.x, transform.position.y, transform.position.z);
                    _targetPosition = _newCharacterPosition;
                    _isMoving = true;
                    OnJumpMade?.Invoke(this, EventArgs.Empty);
                }

                break;
        
        
        }
    }
    private void _playerCollisionDetection_OnRampContact(object sender, EventArgs e)
    {
        Vector3 _newCharacterPosition = new Vector3(transform.position.x, _upperPlanePosition.y, transform.position.z);

        _targetPosition = _newCharacterPosition;
        _isMoving = true;
    }



    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.IsGameOver()) return;
        if (GameManager.Instance.IsGamePaused()) return;

        //if character is somehow out of track while doing some action
        if (!_isMoving &&  !_isJumping &&  !IsCharacterOnTheTrack())
        {
            MoveCharacterToTrack();
        }


        if (_isMoving)
        {
            //maybe select a better animation and event for jumping between lanes.
            
            // Calculate the distance between current position and target position
            float distance = Vector3.Distance(transform.position, _targetPosition);

            // If the distance is greater than threshold, move the character towards the target position
            if (distance > _threshold)
            {
                // Calculate the new position using Lerp
                Vector3 newPosition = Vector3.Lerp(transform.position, _targetPosition, _playerSpeed * Time.deltaTime);

                // Update the character's position
                transform.position = newPosition;
            }
            else
            {
                transform.position = _targetPosition;

                // If the character is close enough to the target position, stop moving
                _isMoving = false;
                OnGroundHit?.Invoke(this, EventArgs.Empty);
            }
        }      
    }

    public int GetPlayerCoinCount()
    {
        return _playerCoinCount;
    }

    public bool IsPlayerOnUpperPlane()
    {
        return transform.position.y == _upperPlanePosition.y;
    }

    public bool IsPlayerRunning()
    {
        return _playerState == PlayerState.Running;
    }

    public bool IsPlayerJumping()
    {
        return _playerState == PlayerState.Jumping;
    }
    public bool IsPlayerSliding()
    {
        return _playerState == PlayerState.Sliding;
    }

    private void MoveCharacterToTrack()
    {

        Vector3 newTargetPosition = FindClosestPath();
        // Smoothly move the character towards the target position using Lerp
        transform.position = Vector3.Lerp(transform.position, newTargetPosition, Time.deltaTime * _playerSpeed);
    }

    private Vector3 FindClosestPath()
    {
        float distanceToLeftTrack = Vector3.Distance(transform.position, _leftLanePosition);
        float distanceToMiddleTrack = Vector3.Distance(transform.position, _middleLanePosition);
        float distanceToRightTrack = Vector3.Distance(transform.position, _rightLanePosition);

        // Find the minimum distance among the three tracks
        float minDistance = Mathf.Min(distanceToLeftTrack, distanceToMiddleTrack, distanceToRightTrack);

        // Set the target position based on the nearest track
        Vector3 closesetTargetPosition = transform.position;

        if (minDistance == distanceToLeftTrack)
        {
            closesetTargetPosition = _leftLanePosition;
        }
        else if (minDistance == distanceToMiddleTrack)
        {
            closesetTargetPosition = _middleLanePosition;
        }
        else if (minDistance == distanceToRightTrack)
        {
            closesetTargetPosition = _rightLanePosition;
        }
        return closesetTargetPosition;
    }

    public bool IsPlayerInTransition()
    {
        return _isPlayerInTransition;
    }

    public void SetPlayerTransition(bool isPlayerInTransition)
    {
        _isPlayerInTransition = isPlayerInTransition;
    }

    private bool IsCharacterOnTheTrack()
    {
        //returns if the character is on the track to avoid input spam.
        return transform.position.x == _rightLanePosition.x || transform.position.x == _leftLanePosition.x || transform.position.x == _middleLanePosition.x;
    }
}
