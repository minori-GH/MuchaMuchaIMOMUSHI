using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backButton;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string startSceneName = "StartScene";

    private bool hasValidReferences;

    private void Awake()
    {
        bool hasMissingReference = false;

        if (gameManager == null)
        {
            Debug.LogError("[GameOverPanel] GameManager が設定されていません。", this);
            hasMissingReference = true;
        }

        if (panelCanvasGroup == null)
        {
            Debug.LogError("[GameOverPanel] Panel CanvasGroup が設定されていません。", this);
            hasMissingReference = true;
        }

        if (retryButton == null)
        {
            Debug.LogError("[GameOverPanel] RetryButton が設定されていません。", this);
            hasMissingReference = true;
        }

        if (backButton == null)
        {
            Debug.LogError("[GameOverPanel] BackButton が設定されていません。", this);
            hasMissingReference = true;
        }

        if (hasMissingReference)
        {
            enabled = false;
            return;
        }

        hasValidReferences = true;
    }

    private void OnEnable()
    {
        if (!hasValidReferences)
        {
            return;
        }

        gameManager.OnStateChanged += HandleStateChanged;
        retryButton.onClick.AddListener(Retry);
        backButton.onClick.AddListener(BackToStart);
    }

    private void Start()
    {
        if (!hasValidReferences)
        {
            return;
        }

        HandleStateChanged(gameManager.CurrentState);
    }

    private void OnDisable()
    {
        if (!hasValidReferences)
        {
            return;
        }

        gameManager.OnStateChanged -= HandleStateChanged;
        retryButton.onClick.RemoveListener(Retry);
        backButton.onClick.RemoveListener(BackToStart);
    }

    private void HandleStateChanged(GameState state)
    {
        bool isGameOver = state == GameState.GameOver;

        SetPanelVisible(isGameOver);

        if (isGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        panelCanvasGroup.alpha = isVisible ? 1f : 0f;
        panelCanvasGroup.interactable = isVisible;
        panelCanvasGroup.blocksRaycasts = isVisible;
    }

    private void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void BackToStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }
}

