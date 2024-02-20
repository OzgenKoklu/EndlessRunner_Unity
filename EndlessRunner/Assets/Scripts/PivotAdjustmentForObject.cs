using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotAdjustmentForObject : MonoBehaviour
{
    // Offset to adjust for the pivot point
    [SerializeField] private Vector3 _pivotOffset;

    // Function to set the position while considering the pivot offset
    private void Awake()
    {
        
    }

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition + _pivotOffset;
    }
}
