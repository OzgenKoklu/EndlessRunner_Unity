using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUI: MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Transform _hearthTemplate;
    [SerializeField] private Transform _hearthContainer;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _newHighText;
    [SerializeField] private Button _pauseGameButton;
    [SerializeField] private TextMeshProUGUI _scoreMultiplierText;

    private void Awake()
    {
        _hearthTemplate.gameObject.SetActive(false);
        _newHighText.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Player.Instance.OnPlayerHealthDecreased += Player_OnHealthChanged;
        GameManager.Instance.OnScoreChanged += Instance_OnScoreChanged;
        GameManager.Instance.OnHighScoreBeaten += Instance_OnHighScoreBeaten;
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;
        GameManager.Instance.OnScoreMultiplierChanged += GameManager_OnScoreMultiplierChanged;
        UpdateVisual();

        _pauseGameButton.onClick.AddListener(() =>
        {
            GameManager.Instance.PauseGame();
        });
    }

    private void GameManager_OnScoreMultiplierChanged(object sender, System.EventArgs e)
    {
        float scoreMultiplier = GameManager.Instance.GetScoreMultiplier();
        if(scoreMultiplier > 1f && scoreMultiplier < 1.5f)
        {
            _scoreMultiplierText.color = Color.white;
        }
        else if(scoreMultiplier >= 1.5f && scoreMultiplier < 3.5f)
        {
            _scoreMultiplierText.color = Color.yellow;
        }
        else if(scoreMultiplier >= 3.5 && scoreMultiplier <4.5f)
        {
            _scoreMultiplierText.color = Color.red;
        }
        else
        {
            _scoreMultiplierText.color = Color.cyan;
        }
        _scoreMultiplierText.text = "X" + scoreMultiplier.ToString();
    }

    private void GameManager_OnGameEnd (object sender, GameManager.OnGameEndEventArgs e)
    {
        Hide();
    }

    private void Instance_OnHighScoreBeaten(object sender, System.EventArgs e)
    {
        _newHighText.gameObject.SetActive(true);
        _scoreText.color = Color.green;
    }

    private void Instance_OnScoreChanged(object sender, GameManager.OnScoreChangedEventArgs e)
    {
        _scoreText.text = e.currentScore.ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Player_OnHealthChanged(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        foreach (Transform child in _hearthContainer)
        {
            if (child == _hearthTemplate) continue;
            Destroy(child.gameObject);
        }

        int playerHealthAmount = Player.Instance.PlayerHealthAmount();
        for (int i = 0; i < playerHealthAmount; i++)
        {
            Transform hearthTransform = Instantiate(_hearthTemplate, _hearthContainer);
            hearthTransform.gameObject.SetActive(true);
        }

    }
}
