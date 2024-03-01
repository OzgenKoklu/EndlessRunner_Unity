using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    private Vector3 _collectedCoinLocation = new Vector3(0, -5, 0);

    // Encapsulated property to control access to the IsCollected state
    public bool IsCollected { get; private set; } = false;

    private void OnEnable()
    {
        IsCollected = false;
    }
    public void RelocateToCollectedCoinLocation()
    {
        IsCollected= true;
        //when coin is collected by the player, it just changes transform first, after it leaves the map it goes back to the pool
        transform.position = _collectedCoinLocation;
    }

}
