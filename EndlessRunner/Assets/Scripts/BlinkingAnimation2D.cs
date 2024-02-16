using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingAnimation2D : MonoBehaviour
{
    private float _blinkInterval = 0.2f;
    private float _blinkTimer;
    bool _blinkTime = true;

    private void Start()
    {
       StartCoroutine(StartBlinkTimer(1.5f));
    }
    private IEnumerator StartBlinkTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        _blinkTime = false;

        CheckIfStayedDisabled();
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
            GetComponent<Graphic>().enabled = !GetComponent<Graphic>().enabled;
            _blinkTimer = 0f;
        }
    }

    //If renderer stays disabled, enables it.
    private void CheckIfStayedDisabled()
    {
        if (!GetComponent<Graphic>().enabled)
        {
            GetComponent<Graphic>().enabled = !GetComponent<Graphic>().enabled;
        }
    }
}
