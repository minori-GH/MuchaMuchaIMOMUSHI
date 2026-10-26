using UnityEngine;

/// <summary>
/// すでにクリア済み、または10000cal未満なら何もしない。
/// 条件を満たしたらクリア済みにして、GameManagerへClear状態にしてもらう
/// </summary>

public sealed class ClearConditionChecker : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int clearCalorie = 10000;

    private bool isCleared;

    private void Awake()
    {
        if (playerStats == null)
        {
            Debug.LogError("[ClearConditionChecker] PlayerStats が設定されていません。", this);
            enabled = false;
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("[ClearConditionChecker] GameManager が設定されていません。", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        playerStats.OnCalorieChanged += CheckClearCondition;
    }

    private void Start()
    {
        CheckClearCondition(playerStats.TotalCalorie);
    }

    private void OnDisable()
    {
        playerStats.OnCalorieChanged -= CheckClearCondition;
    }

    private void CheckClearCondition(int totalCalorie)
    {
        if (isCleared || totalCalorie < clearCalorie)
        {
            return;
        }

        isCleared = true;
        gameManager.SetState(GameState.Clear);
    }
}
