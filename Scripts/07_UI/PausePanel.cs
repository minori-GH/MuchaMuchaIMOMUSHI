using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pause表示、Resume、Retry、StartScene戻り、Time.timeScale 制御
/// </summary>

public sealed class PausePanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backButton;
    [SerializeField] private string gameSceneName = "GameScene"; // [string:文字列型]
    [SerializeField] private string startSceneName = "StartScene";

    private bool hasValidReferences;

    private void Awake()
    {
        bool hasMissingReference = false;

        if (gameManager == null)
        {
            Debug.LogError("[PausePanel] GameManager is not assigned.", this);
            hasMissingReference = true;
        }

        if (panelCanvasGroup == null)
        {
            Debug.LogError("[PausePanel] Panel CanvasGroup is not assigned.", this);
            hasMissingReference = true;
        }

        if (resumeButton == null)
        {
            Debug.LogError("[PausePanel] ResumeButton is not assigned.", this);
            hasMissingReference = true;
        }

        if (retryButton == null)
        {
            Debug.LogError("[PausePanel] RetryButton is not assigned.", this);
            hasMissingReference = true;
        }

        if (backButton == null)
        {
            Debug.LogError("[PausePanel] BackButton is not assigned.", this);
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
        resumeButton.onClick.AddListener(Resume);
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
        resumeButton.onClick.RemoveListener(Resume);
        retryButton.onClick.RemoveListener(Retry);
        backButton.onClick.RemoveListener(BackToStart);
    }

    private void HandleStateChanged(GameState state)
    {
        // *ポーズ中か判定
        bool isPaused = state == GameState.Paused;

        SetPanelVisible(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else if (state == GameState.Playing)
        {
            Time.timeScale = 1f;
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        panelCanvasGroup.alpha = isVisible ? 1f : 0f; // [alpha:透明度0～1(100%)]
        panelCanvasGroup.interactable = isVisible; // [interactable:クリック]
        panelCanvasGroup.blocksRaycasts = isVisible; // [blocksRaycasts:クリックを受け取るか]
    }

    private void Resume()
    {
        gameManager.SetState(GameState.Playing);
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
