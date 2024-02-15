using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Player player;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

    }

    private void Start()
    {
        animator.SetTrigger("StartRunning");
        player.OnJumpMade += Player_OnJumpMade;
        player.OnSlideMade += Player_OnSlideMade;
        player.OnSlideEnd += Player_OnSlideEnd;
        player.OnGroundHit += Player_OnGroundHit;
        player.OnWallCrash += Player_OnWallCrash;
    }

    private void Player_OnWallCrash(object sender, System.EventArgs e)
    {
        animator.SetTrigger("Crashwall_trigger");
    }

    private void Player_OnGroundHit(object sender, System.EventArgs e)
    {
        animator.SetTrigger("JumpEnd_trigger");
    }

    private void Player_OnSlideEnd(object sender, System.EventArgs e)
    {
        animator.SetTrigger("SlideEnd_trigger");
    }

    private void Player_OnSlideMade(object sender, System.EventArgs e)
    {
        animator.SetTrigger("Slide_trigger");
    }

    private void Player_OnJumpMade(object sender, System.EventArgs e)
    {
        animator.SetTrigger("Jump_trigger");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
