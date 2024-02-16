using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatedGroundPlane : MonoBehaviour
{
    private float _speed = 20;
    private int _backwardBoundry = -40;
    private float _speedModifier = 1;

    private Vector3 repeatPos = new Vector3(0, 0, 170);

    private void Start()
    {
        GameManager.Instance.OnGameEnd += Instance_OnGameEnd;
    }

    private void Instance_OnGameEnd(object sender, System.EventArgs e)
    {
        StopMovement();
    }

    private void Update()
    {
        MoveBackward();
        MoveWhenOutOfBounds();
    }

    private void MoveBackward()
    {
        transform.Translate(Vector3.back * Time.deltaTime * _speed * _speedModifier);
    }

    private void MoveWhenOutOfBounds()
    {
        if (transform.position.z < _backwardBoundry )
        {
            if (gameObject.CompareTag("GroundPlane"))
            {
                transform.position = repeatPos;

            }
            else
            {
                //destroys the start plane
                Destroy(gameObject);
            }
           
        }     
    }

    private void StopMovement()
    {
        _speed = 0;
    }
}
