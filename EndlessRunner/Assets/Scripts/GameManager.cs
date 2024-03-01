using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string PlayerPrefsHighScore = "HighestScore";
    public static GameManager Instance  {get; private set; }


    public event EventHandler<OnGameEndEventArgs> OnGameEnd;
    public class OnGameEndEventArgs : EventArgs { public bool IsHighScore; };
    public event EventHandler OnHighScoreBeaten;
    public event EventHandler OnScoreMultiplierChanged;
    public event EventHandler<OnScoreChangedEventArgs> OnScoreChanged;
    public class OnScoreChangedEventArgs : EventArgs { public int CurrentScore; };

    [SerializeField] private Transform _pauseGameScreen;

    private bool _isHighScoreBeaten = false;
    private bool _isGameOver = false;
    private bool _isGamePaused = false;
    private float _gameScore = 0;
    private int _integerGameScore = 0;
    private float _scoreMultiplier = 100;
    private float _multiplierTimer = 0;
    private int _highScore = 0;

    private float _lastScoreUpdateTime = 0f;


    private void Awake()
    {
        Instance = this;
        _highScore = PlayerPrefs.GetInt(PlayerPrefsHighScore);
    }

    void Start()
    {
        Player.Instance.OnPlayerHealthDepleted += OnGameOver;
        Player.Instance.OnPlayerHealthDecreased += OnHealthDecreased;
        ResetGameStats();
    }

    private void OnDestroy()
    {
        if(Instance == null ) return;
        Player.Instance.OnPlayerHealthDepleted -= OnGameOver;
        Player.Instance.OnPlayerHealthDecreased -= OnHealthDecreased;
    }

    private void OnHealthDecreased(object sender, EventArgs e)
    {
        _scoreMultiplier = 100;
        _multiplierTimer = 0;
        OnScoreMultiplierChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ResetGameStats()
    {
        _multiplierTimer = 0;
        _gameScore = 0;
        _integerGameScore = 0;
        _scoreMultiplier = 100;
        _isGamePaused = false;
        _isGameOver = false;
        _isHighScoreBeaten = false;
    }

    private void OnGameOver(object sender, EventArgs e)
    {
        _isGameOver = true;
        bool isNewHighScore = _integerGameScore > _highScore;
        if (isNewHighScore)
        {
            PlayerPrefs.SetInt(PlayerPrefsHighScore, _integerGameScore);
            PlayerPrefs.Save();
        }

        OnGameEnd?.Invoke(this, new OnGameEndEventArgs { IsHighScore = isNewHighScore });
    }


    void Update()
    {
        if (!_isGameOver)
        {
            CalculateScore();
            CheckIfHighScoreIsBeaten();
            CalculateScoreMultiplier();
        }
        ShowScoreOnUi();  
    }

    private void CalculateScoreMultiplier()
    {
        //caps the score multiplier to 5x
        if (_scoreMultiplier >= 500) return;

       _multiplierTimer += Time.deltaTime;
     
        if(_multiplierTimer > 3f)
        {
            _multiplierTimer = 0;
            _scoreMultiplier += 10;

            OnScoreMultiplierChanged?.Invoke(this, EventArgs.Empty);
        }        
    }

    private void CalculateScore()
    {
            _gameScore += _scoreMultiplier * Time.deltaTime;
    }

    private void CheckIfHighScoreIsBeaten()
    {
        if (_isHighScoreBeaten) return; 

        if( _integerGameScore > _highScore)
        {
            _isHighScoreBeaten = true;
            OnHighScoreBeaten?.Invoke(this, EventArgs.Empty);
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        _isGamePaused = true;
        _pauseGameScreen.gameObject.SetActive(true);
    }

    public void UnpauseGame()
    {
        _isGamePaused = false;
        Time.timeScale = 1.0f;
        _pauseGameScreen.gameObject.SetActive(false);
    }

    private void ShowScoreOnUi()
    {
        if (Time.time - _lastScoreUpdateTime >= 0.2f) // Update every 0.1 seconds (or adjust frame rate)
        {
            _integerGameScore = Mathf.FloorToInt(_gameScore);
            _lastScoreUpdateTime = Time.time;

            OnScoreChanged?.Invoke(this, new OnScoreChangedEventArgs
            {
                CurrentScore = _integerGameScore
            });
        }
    }

    public float GetSpeedModifier()
    {
        float clampedMultiplier = Mathf.Clamp(_scoreMultiplier, 100, 500);
        return 1 + (clampedMultiplier - 100) / 400f; // returns a value between 1 and 2 based on the multiplier.
    }

    public float GetScoreMultiplier()
    {
        return _scoreMultiplier / 100;
    }

    public int GetHighScore()
    {
        return _integerGameScore;
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }

    public bool IsGamePaused()
    {
        return _isGamePaused;
    }
}
