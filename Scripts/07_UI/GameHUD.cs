using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// PlayerStats.OnCalorieChanged を購読してcal表示、Pauseボタン対応
/// </summary>
public sealed class GameHUD : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI calText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private string calorieFormat = "{0}cal";

    private bool hasValidReferences;

    private void Awake()
    {
        bool hasMissingReference = false;

        if (playerStats == null)
        {
            Debug.LogError("[GameHUD] PlayerStats is not assigned.", this);
            hasMissingReference = true;
        }

        if (gameManager == null)
        {
            Debug.LogError("[GameHUD] GameManager is not assigned.", this);
            hasMissingReference = true;
        }

        if (calText == null)
        {
            Debug.LogError("[GameHUD] CalText is not assigned.", this);
            hasMissingReference = true;
        }

        if (pauseButton == null)
        {
            Debug.LogError("[GameHUD] PauseButton is not assigned.", this);
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

        //  [+=:登録する]
        // 「calが変わったらUpdateCalorieText()を実行する」
        playerStats.OnCalorieChanged += UpdateCalorieText;
        // 「Pauseボタン押したらRequestPause()を実行する」
        pauseButton.onClick.AddListener(RequestPause);
    }

    private void Start()
    {
        if (!hasValidReferences)
        {
            return;
        }

        UpdateCalorieText(playerStats.TotalCalorie);
    }

    private void OnDisable()
    {
        if (!hasValidReferences)
        {
            return;
        }

        playerStats.OnCalorieChanged -= UpdateCalorieText;
        pauseButton.onClick.RemoveListener(RequestPause);
    }

    private void UpdateCalorieText(int totalCalorie)
    {
        calText.text = string.Format(calorieFormat, totalCalorie);
    }
    private void RequestPause()
    {
        if (gameManager.CurrentState == GameState.Playing)
        {
            gameManager.SetState(GameState.Paused);
        }
    }
}

