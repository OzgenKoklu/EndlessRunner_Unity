using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    public float rotationSpeed = 100f; 
    void LateUpdate()
    {
        // Rotate the coin around the y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}