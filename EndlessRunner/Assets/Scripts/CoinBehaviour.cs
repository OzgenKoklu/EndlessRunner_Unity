using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    private Vector3 _collectedCoinLocation = new Vector3(0, -5, 0);

    private bool _isCollected = false;

    private void OnEnable()
    {
        _isCollected = false;
    }
    public void RelocateToCollectedCoinLocation()
    {
        _isCollected= true;
        //when coin is collected by the player, it just changes transform first, after it leaves the map it goes back to the pool
        transform.position = _collectedCoinLocation;
    }

    public bool IsCoinCollected()
    {
        return _isCollected;
    }

}
