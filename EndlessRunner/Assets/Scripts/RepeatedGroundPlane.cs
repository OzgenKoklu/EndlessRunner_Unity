using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatedGroundPlane : MonoBehaviour
{
    private float _speed = 10;
    private int _backwardBoundry = -45;
    private float _speedModifier = 1;

    private Vector3 repeatPos = new Vector3(0, 0, 170);

    private void Start()
    {
        GameManager.Instance.OnGameEnd += Instance_OnGameEnd;

        //this is for adapting the speed, naming isnt intuative, I know. Maybe a seperate 
        GameManager.Instance.OnScoreMultiplierChanged += GameManager_OnScoreMultiplierChanged;
    }

    private void GameManager_OnScoreMultiplierChanged(object sender, System.EventArgs e)
    {
        _speedModifier = GameManager.Instance.GetSpeedModifier();
    }

    private void GameManager_OnScoreChanged(object sender, GameManager.OnScoreChangedEventArgs e)
    {
        throw new System.NotImplementedException();
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
        if (transform.position.z < _backwardBoundry)
        {
            // Collect all child pool items in a list before processing
            List<PoolItem> childrenPoolItems = new List<PoolItem>();
            foreach (Transform child in transform)
            {
                PoolItem poolItem = child.GetComponent<PoolItem>();

                if (poolItem != null) // Ensure the child has a PoolItem component
                {
                    childrenPoolItems.Add(poolItem);
                }
            }

            foreach (var poolItem in childrenPoolItems)
            {
                if (poolItem.levelObjectInfo != null)
                {
                    ObjectPoolManager.Instance.ReturnToPool(poolItem.levelObjectInfo.ObjectName, poolItem.gameObject);
                   // Debug.Log(poolItem.gameObject.name + " named child successfully returned to the pool");
                }

            }

            // Now handle the GroundPlane itself
            PoolItem groundPlanePoolItem = GetComponent<PoolItem>();
            if (groundPlanePoolItem != null && groundPlanePoolItem.levelObjectInfo != null)
            {
                ObjectPoolManager.Instance.ReturnToPool(groundPlanePoolItem.levelObjectInfo.ObjectName, gameObject);
                //Debug.Log("GroundPlane successfully returned to the pool");
            }
        }
    }


    private void StopMovement()
    {
        _speed = 0;
    }
}
