using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Player _player;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

    }

    private void Start()
    {
        // Register player events
        SubscribeToPlayerEvents();

        // Set initial animation state
        _animator.SetTrigger("StartRunning");
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayerEvents();
    }
    private void SubscribeToPlayerEvents()
    {
        _player.OnJumpMade += Player_OnJumpMade;
        _player.OnSlideMade += Player_OnSlideMade;
        _player.OnSlideEnd += Player_OnSlideEnd;
        _player.OnGroundHit += Player_OnGroundHit;
        _player.OnWallCrash += Player_OnWallCrash;
        _player.OnPlayerFalling += Player_OnPlayerFalling;
        GameManager.Instance.OnScoreMultiplierChanged += Instance_OnScoreMultiplierChanged;
    }

    private void UnsubscribeFromPlayerEvents()
    {
        _player.OnJumpMade -= Player_OnJumpMade;
        _player.OnSlideMade -= Player_OnSlideMade;
        _player.OnSlideEnd -= Player_OnSlideEnd;
        _player.OnGroundHit -= Player_OnGroundHit;
        _player.OnWallCrash -= Player_OnWallCrash;
        _player.OnPlayerFalling -= Player_OnPlayerFalling;
        GameManager.Instance.OnScoreMultiplierChanged -= Instance_OnScoreMultiplierChanged;
    }


    private void Player_OnPlayerFalling(object sender, System.EventArgs e) => _animator.SetTrigger("Fall_trigger");
    private void Instance_OnScoreMultiplierChanged(object sender, System.EventArgs e) => _animator.speed = GameManager.Instance.GetSpeedModifier();
    private void Player_OnWallCrash(object sender, System.EventArgs e) => _animator.SetTrigger("Crashwall_trigger");
    private void Player_OnGroundHit(object sender, System.EventArgs e) => _animator.SetTrigger("JumpEnd_trigger");
    private void Player_OnSlideEnd(object sender, System.EventArgs e) => _animator.SetTrigger("SlideEnd_trigger");
    private void Player_OnSlideMade(object sender, System.EventArgs e) => _animator.SetTrigger("Slide_trigger");
    private void Player_OnJumpMade(object sender, System.EventArgs e) => _animator.SetTrigger("Jump_trigger");
}
