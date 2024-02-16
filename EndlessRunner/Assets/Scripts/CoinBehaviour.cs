using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    private Vector3 coinObjectPoolLocation = new Vector3(0, -5, 0);


    //Bad sort of object pooling example.
    public void GoToObjectPoolLocation()
    {
        transform.position = coinObjectPoolLocation;
    }
}
