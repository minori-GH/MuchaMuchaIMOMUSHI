using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class StartSceneController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button encyclopediaButton;

    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string encyclopediaSceneName = "EncyclopediaScene";

    private void Awake()
    {
        if (startButton == null)
        {
            Debug.LogError("[StartSceneController] Start Button is not assigned.", this);
            enabled = false;
            return;
        }

        if (encyclopediaButton == null)
        {
            Debug.LogError("[StartSceneController] Encyclopedia Button is not assigned.", this);
            enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("[StartSceneController] Game scene name is empty.", this);
            enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(encyclopediaSceneName))
        {
            Debug.LogError("[StartSceneController] Encyclopedia scene name is empty.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        startButton.onClick.AddListener(HandleStartButtonClicked);
        encyclopediaButton.onClick.AddListener(HandleEncyclopediaButtonClicked);
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartButtonClicked);
        }

        if (encyclopediaButton != null)
        {
            encyclopediaButton.onClick.RemoveListener(HandleEncyclopediaButtonClicked);
        }
    }

    private void HandleStartButtonClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void HandleEncyclopediaButtonClicked()
    {
        SceneManager.LoadScene(encyclopediaSceneName);
    }
}

