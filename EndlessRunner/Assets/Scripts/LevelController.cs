using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;


    private void Update()
    {
        int activeGroundPlanesCount = GameObject.FindGameObjectsWithTag("GroundPlane").Length;
        Debug.Log("active groundPlanes: " + activeGroundPlanesCount);

        if (activeGroundPlanesCount < 4)
        {
            _levelGenerator.SegmentMap(3, 10);
        }
    }
}
