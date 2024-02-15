using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance  {get; private set; }
    public event EventHandler OnGameStop;
    private bool _isGameOver = false;
    private float _gameScore = 0;
    private float _scoreMultiplayer = 100;
    [SerializeField] private TextMeshProUGUI _scoreText;


    // Start is called before the first frame update

    private void Awake()
    {
        Instance = this;

        
    }
    void Start()
    {
        Player.Instance.OnPlayerHealthDepleted += Instance_OnGameOver;
        
    }



    private void Instance_OnGameOver(object sender, EventArgs e)
    {
        _isGameOver = true;
       
        OnGameStop?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        CalculateScore();
    }

    private void CalculateScore()
    {
        if (!_isGameOver)
        {
            _gameScore += _scoreMultiplayer * Time.deltaTime;
        }

    }
    private void LateUpdate()
    {
        int integerScore = (int)_gameScore;
        if (integerScore % 10 != 0) return;
        _scoreText.text = integerScore.ToString();
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }
}
