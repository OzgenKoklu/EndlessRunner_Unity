using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button _restartButton;

    [SerializeField] private Transform _newHighScoreText;
    [SerializeField] private Transform _highScoreText;

    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Awake()
    {
        _restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });

    }

    private void Start()
    {
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;
        
        Hide();
    }

    private void GameManager_OnGameEnd(object sender, GameManager.OnGameEndEventArgs e)
    {
        Show();
        if (!e.IsHighScore)
        {
            HideHighScoreText();
        }
        else
        {
            ShowHighScoreText(GameManager.Instance.GetHighScore());
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowHighScoreText(int score)
    {
        _newHighScoreText.gameObject.SetActive(true);
        _highScoreText.gameObject.SetActive(true);
        _scoreText.text = score.ToString();
    }
    private void HideHighScoreText()
    {
        _newHighScoreText.gameObject.SetActive(false);
        _highScoreText.gameObject.SetActive(false);
    }
}
