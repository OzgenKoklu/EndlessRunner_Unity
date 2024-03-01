using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class PlayerCollisionDetection : MonoBehaviour
{
    public event EventHandler OnGroundHit;
    public event EventHandler OnRampContact;
    public event EventHandler OnGroundContactLost;
    public event EventHandler OnWallObstacleHit;
    public event EventHandler OnObstacleHit;
    public event EventHandler OnCoinGrabbed;
    private bool _isGroundContactLost = false;
    private float _timerForGroundedCheck = 0;

    [SerializeField] private Transform _raycastPointFeet;
    [SerializeField] private Transform _raycastPointBody;


    // Update is called once per frame
    void Update()
    {
        float raycastLength = 0.5f;

        Vector3 feetOrigin = _raycastPointFeet.position;
        Vector3 bodyOrigin = _raycastPointBody.position;

        RaycastHit hitFeet;
        RaycastHit hitBody;

        bool canFall = !Physics.Raycast(feetOrigin, Vector3.down, out hitFeet, raycastLength);

        if (canFall)
        {
            _timerForGroundedCheck += Time.deltaTime;
            if (_timerForGroundedCheck > 0.1f && !Player.Instance.IsPlayerMoving())
            {
               // _isGroundContactLost = true;
                OnGroundContactLost?.Invoke(this, EventArgs.Empty);
            }
        }

        if (Physics.Raycast(feetOrigin, Vector3.down, out hitFeet, raycastLength))
        {
            Debug.Log(hitFeet.transform.tag);
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

        if (Physics.Raycast(bodyOrigin, Vector3.forward, out hitBody, raycastLength))
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
                if (!coinBehaviour.IsCollected)
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

        if (Physics.Raycast(bodyOrigin, Vector3.left, out hitBody, raycastLength))
        {
            //modify for obstacles also
            if (hitBody.transform.tag == "WallObstacle")
            {
                
                //should not directly die if this hits to the wall while moving sideways
                OnWallObstacleHit?.Invoke(this, EventArgs.Empty);
            }
        }

        if (Physics.Raycast(bodyOrigin, Vector3.right, out hitBody, raycastLength))
        {
            //modify for obstacles also
            if (hitBody.transform.tag == "WallObstacle")
            {
                //should not directly die if this hits to the wall while moving sideways
                OnWallObstacleHit?.Invoke(this, EventArgs.Empty);
            }
        }


        // Debug visualization (optional)
        Debug.DrawRay(feetOrigin, Vector3.down * raycastLength, Color.red);
        Debug.DrawRay(bodyOrigin, Vector3.forward * raycastLength, Color.green);

    }

}
