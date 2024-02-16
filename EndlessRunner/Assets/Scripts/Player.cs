using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public event EventHandler OnPlayerHealthDecreased;
    public event EventHandler OnInvincibilityPeriodEnd;

    private int _playerHealth = 3;
    private bool _isInvincible;


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

        _playerState = PlayerState.Running;
         _isInvincible = false ;

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
        transform.position = _newCharacterPosition;
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

        if (IsCharacterOnTheTrack() && IsPlayerRunning())
        {
            _playerState = PlayerState.Jumping;
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

        if (IsCharacterOnTheTrack())
        {
            MoveBetweenLanes(Direction.Right);
        }      
    }

    private void _gameInput_OnGoLeftAction(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver()) return;
        if (GameManager.Instance.IsGamePaused()) return;

        if (IsCharacterOnTheTrack())
        {
            MoveBetweenLanes(Direction.Left);
        }     
    }

    private void MoveBetweenLanes(Direction direction)
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
                    transform.position = _newCharacterPosition;
                }
                else
                {
                    Vector3 _newCharacterPosition = new Vector3(_leftLanePosition.x, transform.position.y, transform.position.z);
                    transform.position = _newCharacterPosition;
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
                    transform.position = _newCharacterPosition;
                }
                else
                {
                    Vector3 _newCharacterPosition = new Vector3(_rightLanePosition.x, transform.position.y, transform.position.z);
                    transform.position = _newCharacterPosition;
                }

                break;
        
        
        }
    }
    private void _playerCollisionDetection_OnRampContact(object sender, EventArgs e)
    {
        Vector3 _newCharacterPosition = new Vector3(transform.position.x, _upperPlanePosition.y, transform.position.z);

        transform.position = _newCharacterPosition;
    }



    // Update is called once per frame
    void Update()
    {
        
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

    private bool IsCharacterOnTheTrack()
    {
        //returns if the character is on the track to avoid input spam.
        return transform.position.x == _rightLanePosition.x || transform.position.x == _leftLanePosition.x || transform.position.x == _middleLanePosition.x;
    }
}
