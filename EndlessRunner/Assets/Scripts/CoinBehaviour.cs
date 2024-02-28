using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    private Vector3 coinObjectPoolLocation = new Vector3(0, -5, 0);

    public bool IsCollected = false;

    private void OnEnable()
    {
        IsCollected = false;
    }

    


    //Bad sort of object pooling example.
    public void GoToObjectPoolLocation()
    {
        IsCollected= true;
        transform.position = coinObjectPoolLocation;
    }


}
