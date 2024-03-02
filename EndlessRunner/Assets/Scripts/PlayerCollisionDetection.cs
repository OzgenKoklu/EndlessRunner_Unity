using System;
using UnityEngine;

public class PlayerCollisionDetection : MonoBehaviour
{
    public event EventHandler OnGroundHit;
    public event EventHandler OnRampContact;
    public event EventHandler OnGroundContactLost;
    public event EventHandler OnWallObstacleHit;
    public event EventHandler OnObstacleHit;
    public event EventHandler OnCoinGrabbed;
    private float _timerForGroundedCheck = 0;

    [SerializeField] private Transform _raycastPointFeet;
    [SerializeField] private Transform _raycastPointBody;

    private const float RaycastLength = 0.5f;

    void Update()
    {
        CheckGroundStatus();

        CheckBodyCollisions();

        DebugRaycasts();
    }

    private void CheckGroundStatus()
    {
        Vector3 feetOrigin = _raycastPointFeet.position;

        RaycastHit hitFeet;

        bool canFall = !Physics.Raycast(feetOrigin, Vector3.down, out hitFeet, RaycastLength);

        if (canFall)
        {
            _timerForGroundedCheck += Time.deltaTime;
            if (_timerForGroundedCheck > 0.1f && !Player.Instance.IsPlayerMoving())
            {
                // _isGroundContactLost = true;
                OnGroundContactLost?.Invoke(this, EventArgs.Empty);
            }
        }

        if (Physics.Raycast(feetOrigin, Vector3.down, out hitFeet, RaycastLength))
        {
            if (hitFeet.transform.tag == "GroundPlane")
            {
                _timerForGroundedCheck = 0;
                if (Player.Instance.IsPlayerJumping())
                {
                    OnGroundHit?.Invoke(this, EventArgs.Empty);
                }

            }

            if (hitFeet.transform.tag == "Platform")
            {
                _timerForGroundedCheck = 0;
            }
            if (hitFeet.transform.tag == "Ramp")
            {
                _timerForGroundedCheck = 0;
            }
        }
    }
    private void CheckBodyCollisions()
    {
        Vector3 bodyOrigin = _raycastPointBody.position;

        RaycastHit hitBody;

        if (Physics.Raycast(bodyOrigin, Vector3.forward, out hitBody, RaycastLength))
        {
            if (hitBody.transform.tag == "SlideObstacle")
            {
                Debug.Log("Is player sliding: " + Player.Instance.IsPlayerSliding());
                if (Player.Instance.IsPlayerSliding()) return;
                OnObstacleHit?.Invoke(this, EventArgs.Empty);
            }

            if (hitBody.transform.tag == "JumpObstacle")
            {
                if (Player.Instance.IsPlayerJumping()) return;
                OnObstacleHit?.Invoke(this, EventArgs.Empty);
            }

            if (hitBody.transform.tag == "WallObstacle")
            {
                OnWallObstacleHit?.Invoke(this, EventArgs.Empty);
            }
            if (hitBody.transform.tag == "Coin")
            {
                //object pooling için bu coin baþka bir yere taþýnabilir.
                CoinBehaviour coinBehaviour = hitBody.transform.GetComponent<CoinBehaviour>();

                //Not as intuative since they all go to 0,5,0 relative to their perant object. Not good practice.
                if (!coinBehaviour.IsCoinCollected())
                {
                    OnCoinGrabbed?.Invoke(this, EventArgs.Empty);
                }

                coinBehaviour.RelocateToCollectedCoinLocation();
            }
            if (hitBody.transform.tag == "Ramp")
            {
                OnRampContact?.Invoke(this, EventArgs.Empty);
            }

        }

        if (Physics.Raycast(bodyOrigin, Vector3.left, out hitBody, RaycastLength))
        {
            //modify for obstacles also
            if (hitBody.transform.tag == "WallObstacle")
            {

                //should not directly die if this hits to the wall while moving sideways
                OnWallObstacleHit?.Invoke(this, EventArgs.Empty);
            }
        }

        if (Physics.Raycast(bodyOrigin, Vector3.right, out hitBody, RaycastLength))
        {
            //modify for obstacles also
            if (hitBody.transform.tag == "WallObstacle")
            {
                //should not directly die if this hits to the wall while moving sideways
                OnWallObstacleHit?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void DebugRaycasts()
    {
        Vector3 feetOrigin = _raycastPointFeet.position;
        Vector3 bodyOrigin = _raycastPointBody.position;
        Debug.DrawRay(feetOrigin, Vector3.down * RaycastLength, Color.red);
        Debug.DrawRay(bodyOrigin, Vector3.forward * RaycastLength, Color.green);
        Debug.DrawRay(bodyOrigin, Vector3.left * RaycastLength, Color.blue);
        Debug.DrawRay(bodyOrigin, Vector3.right * RaycastLength, Color.yellow);
    }

}
