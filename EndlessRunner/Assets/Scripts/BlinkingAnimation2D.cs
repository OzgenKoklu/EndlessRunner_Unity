using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingAnimation2D : MonoBehaviour
{
    private float _blinkInterval = 0.2f;
    private float _blinkTimer;
    private bool _shouldBlink = true;
    private Graphic _graphicComponent;

    private void Start()
    {
        // Cache the Graphic component once
        _graphicComponent = GetComponent<Graphic>();

       StartCoroutine(StartBlinkTimer(1.5f));
    }
    private IEnumerator StartBlinkTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        _shouldBlink = false;

        EnsureGraphicIsEnabled();
    }

    private void Update()
    {
        //to make this clear this code only blinks when heart is loaded on the scene. Since each time player loses a hearth it loads new hearts, they just blink and never blink anymore
        //Hence this script does not sub to any event. Don't know why I did this way. Just did.
        if (_shouldBlink)
        {
            PerformBlink();
        }
    }

    private void PerformBlink()
    {
        _blinkTimer += Time.deltaTime;

        if (_blinkTimer >= _blinkInterval)
        {
            _graphicComponent.enabled = !_graphicComponent.enabled; // Toggle the graphic's visibility
            _blinkTimer = 0; // Reset the timer
        }
    }

    //If renderer stays disabled, enables it.
    private void EnsureGraphicIsEnabled()
    {
        if (!_graphicComponent.enabled)
        {
            _graphicComponent.enabled = true;
        }
    }
}
