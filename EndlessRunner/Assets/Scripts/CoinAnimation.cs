using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    private float _rotationSpeed = 100f; 
    void LateUpdate()
    {
        // Rotate the coin around the y-axis
        RotateCoin();
    }
    private void RotateCoin()
    {
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
    }
}