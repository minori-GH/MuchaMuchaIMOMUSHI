using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class EncyclopediaSceneController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private string startSceneName = "StartScene";

    private void Awake()
    {
        if (backButton == null)
        {
            Debug.LogError("[EncyclopediaSceneController] Back Button is not assigned.", this);
            enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(startSceneName))
        {
            Debug.LogError("[EncyclopediaSceneController] Start scene name is empty.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        backButton.onClick.AddListener(HandleBackButtonClicked);
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }
    }

    private void HandleBackButtonClicked()
    {
        SceneManager.LoadScene(startSceneName);
    }
}