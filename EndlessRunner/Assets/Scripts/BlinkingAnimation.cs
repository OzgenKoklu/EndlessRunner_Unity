using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingAnimation : MonoBehaviour
{
    private float _blinkInterval = 0.2f; 
    private float _blinkTimer;
    bool _blinkTime = false;

    private void Start()
    {
        Player.Instance.OnPlayerHealthDecreased += Player_OnPlayerHealthDecreased;
        Player.Instance.OnInvincibilityPeriodEnd += Player_OnInvincibilityPeriodEnd;
    }

    private void Player_OnInvincibilityPeriodEnd(object sender, System.EventArgs e)
    {
        _blinkTime = false;


        //enables renderer if it remains disabled
        if (!GetComponent<Renderer>().enabled)
        {
            GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
        }
    }

    private void Player_OnPlayerHealthDecreased(object sender, System.EventArgs e)
    {
        _blinkTime = true;
    }

    private void Update()
    {
        if (_blinkTime)
        {
            Blink();
        }
    }


    private void Blink()
    {
        _blinkTimer += Time.deltaTime;

        if (_blinkTimer >= _blinkInterval)
        {
            GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
            _blinkTimer = 0f;
        }
    }
}
