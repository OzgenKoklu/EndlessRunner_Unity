using System;
using UnityEngine;

public class BlinkingAnimation : MonoBehaviour
{
    private float _blinkInterval = 0.2f; 
    private float _blinkTimer;
    private bool _shouldBlink = false;
    private Renderer _renderer;

    private void Start()
    {
        // Cache the Renderer component once instead of calling GetComponent multiple times
        _renderer = GetComponent<Renderer>();


        Player.Instance.OnPlayerHealthDecreased += HandlePlayerHealthDecreased;
        Player.Instance.OnInvincibilityPeriodEnd += HandleInvincibilityPeriodEnd;
    }

    private void OnDestroy()
    {
        // Unsubscribe from player events to prevent memory leaks
        Player.Instance.OnPlayerHealthDecreased -= HandlePlayerHealthDecreased;
        Player.Instance.OnInvincibilityPeriodEnd -= HandleInvincibilityPeriodEnd;
    }

    private void HandleInvincibilityPeriodEnd(object sender, EventArgs e)
    {
        _shouldBlink = false;
        // Directly set the renderer's enabled state instead of toggling it unnecessarily
        if (!_renderer.enabled)
        {
            _renderer.enabled = true;
        }
    }

    private void HandlePlayerHealthDecreased(object sender, EventArgs e)
    {
        _shouldBlink = true;
    }


    private void Update()
    {
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
            _renderer.enabled = !_renderer.enabled; // Toggle visibility
            _blinkTimer = 0f; // Reset the timer
        }
    }
}
