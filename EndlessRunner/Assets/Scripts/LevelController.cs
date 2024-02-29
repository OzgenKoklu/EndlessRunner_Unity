using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Player;

public class LevelController : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;

    private bool _isSpawnedThisUpdate = false;

    void LateUpdate()
    {
      //  int activeGroundPlanesCount = GameObject.FindGameObjectsWithTag("GroundPlane").Length;

        // Debug.Log("active groundPlanes: " + activeGroundPlanesCount);
        //cant solve the big gap bug, I think spawning logic triggers more than once.
        if (ObjectPoolManager.Instance.HasGroundPlaneInPool() && !_isSpawnedThisUpdate && !RepeatedGroundPlane.IsReturningToPoolInProgress() && !ObjectPoolManager.Instance.IsObjectPoolingInProgress())
        {
            _levelGenerator.SegmentMap(3, 10);
            _isSpawnedThisUpdate = true;

            StartCoroutine(ResetSpawnedThisUpdateFlag());
        }
    }

    IEnumerator ResetSpawnedThisUpdateFlag()
    {
        // Wait for 0.1 seconds
        yield return new WaitForSeconds(0.1f);
        _isSpawnedThisUpdate = false;          
    }

}
