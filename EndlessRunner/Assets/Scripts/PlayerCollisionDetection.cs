using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class PlayerCollisionDetection : MonoBehaviour
{
    public event EventHandler OnGroundHit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (Player.Instance.IsPlayerJumping())
        {
            OnGroundHit?.Invoke(this, EventArgs.Empty);   
            Debug.Log("Triggered is grounded");
        }


    }

}
