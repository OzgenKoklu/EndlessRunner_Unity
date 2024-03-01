using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _continueButton;
    private void Awake()
    {
        _restartButton.onClick.AddListener(() =>
        {
            GameManager.Instance.UnpauseGame();
            SceneManager.LoadScene(0);
        });

        _continueButton.onClick.AddListener(() =>
        {            
            GameManager.Instance.UnpauseGame();
        });

    }

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
