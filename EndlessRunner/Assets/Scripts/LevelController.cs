using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;

    private bool _isSpanedThisUpdate = false;
    private void Update()
    {
        int activeGroundPlanesCount = GameObject.FindGameObjectsWithTag("GroundPlane").Length;
        _isSpanedThisUpdate = false;
        // Debug.Log("active groundPlanes: " + activeGroundPlanesCount);
        //cant solve the big gap bug, I think spawning logic triggers more than once.
        if (activeGroundPlanesCount < 4 & !_isSpanedThisUpdate)
        {
            _levelGenerator.SegmentMap(3, 10);
            _isSpanedThisUpdate = true;
        }
    }
}
