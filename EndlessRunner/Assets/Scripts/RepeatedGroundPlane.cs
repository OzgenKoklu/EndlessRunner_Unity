using System.Collections.Generic;
using UnityEngine;

public class RepeatedGroundPlane : MonoBehaviour
{
    private float _speed = 10f;
    private int _backwardBoundry = -45;
    private float _speedModifier = 1f;

    private void Start()
    {
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;

        GameManager.Instance.OnScoreMultiplierChanged += GameManager_OnScoreMultiplierChanged;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameEnd -= GameManager_OnGameEnd;

        GameManager.Instance.OnScoreMultiplierChanged -= GameManager_OnScoreMultiplierChanged;
    }

    private void GameManager_OnScoreMultiplierChanged(object sender, System.EventArgs e)
    {
        _speedModifier = GameManager.Instance.GetSpeedModifier();
    }

    private void GameManager_OnGameEnd(object sender, System.EventArgs e)
    {
        StopMovement();
    }

    private void Update()
    {
        if (_speed > 0) 
        {
            MoveBackward();
            MoveWhenOutOfBounds();
        }
    }

    private void MoveBackward()
    {
        transform.Translate(Vector3.back * Time.deltaTime * _speed * _speedModifier);
    }

    private void MoveWhenOutOfBounds()
    {
        if (transform.position.z < _backwardBoundry)
        {
            ReturnChildrenToPool();
            ReturnSelfToPool();
        }
    }

    private void ReturnChildrenToPool()
    {
        // Collect all child pool items in a list before processing
        List<PoolItem> childrenPoolItems = new List<PoolItem>();
        foreach (Transform child in transform)
        {
            PoolItem poolItem = child.GetComponent<PoolItem>();

            if (poolItem != null) 
            {
                childrenPoolItems.Add(poolItem);
            }
        }

        foreach (var poolItem in childrenPoolItems)
        {
            if (poolItem.GetLevelObjectSO() != null)
            {
                ObjectPoolManager.Instance.ReturnToPool(poolItem.GetLevelObjectSO().ObjectName, poolItem.gameObject);
                
            }
        }
    }

    private void ReturnSelfToPool()
    {
        PoolItem groundPlanePoolItem = GetComponent<PoolItem>();
        if (groundPlanePoolItem != null && groundPlanePoolItem.GetLevelObjectSO() != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(groundPlanePoolItem.GetLevelObjectSO().ObjectName, gameObject);
            //Debug.Log("GroundPlane successfully returned to the pool");
        }
    }


    private void StopMovement()
    {
        _speed = 0;
    }
}
