using UnityEngine;

/// <summary>
/// 現在発動しているPowerUpを確認し、その種類に応じた芋虫の見た目変更を指示するクラス
/// </summary>

public sealed class PlayerPowerUpVisualController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PowerUpController powerUpController;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private PlayerMoveSquashAnimator moveSquashAnimator;
    [SerializeField, Min(0.01f)] private float speedUpAnimationSpeedMultiplier = 1.5f; // SpeedUp中のうねうね速度倍率

    private bool hasActivePowerUp; // PowerUp演出が有効な状態か
    private bool isSpeedUpVisualActive; // うねうね速度の倍率変更が適用されているか
    private PowerUpType currentPowerUpType; // 現在適用しているPowerUpの種類

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[PlayerPowerUpVisualController] GameManager is not assigned.", this);
            enabled = false;
            return;
        }

        if (powerUpController == null)
        {
            Debug.LogError("[PlayerPowerUpVisualController] PowerUpController is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerVisual == null)
        {
            Debug.LogError("[PlayerPowerUpVisualController] PlayerVisual is not assigned.", this);
            enabled = false;
            return;
        }

        if (moveSquashAnimator == null)
        {
            Debug.LogError("[PlayerPowerUpVisualController] PlayerMoveSquashAnimator is not assigned.", this);
            enabled = false;
            return;
        }
    }

    private void LateUpdate()
    {
        // 「現在のゲーム状態がPlayingではない場合、」
        if (gameManager.CurrentState != GameState.Playing)
        {
            // 「演出解除して終了」
            ResetVisualEffects();
            return;
        }

        // 「PowerUpが発動していない場合、」
        if (!powerUpController.TryGetActivePowerUp(out PowerUpType powerUpType, out float remainingTime))
        {
            // 「演出解除して終了」
            ResetVisualEffects();
            return;
        }

        // 「初めてPowerUpが発動した、または、PowerUpの種類が変わった場合」
        if (!hasActivePowerUp || currentPowerUpType != powerUpType)
        {
            // 「演出解除して終了」
            ResetVisualEffects();
            // 「PowerUpを有効状態にする」
            hasActivePowerUp = true;
            // 「PowerUpの種類を記録する」
            currentPowerUpType = powerUpType;
        }

        // 「同じクラス内の ApplySpeedUpVisual() を呼び出す」
        ApplySpeedUpVisual(powerUpType);
        // 「別のクラスである PlayerVisual の ApplyPowerUpVisual() を呼び出す」
        playerVisual.ApplyPowerUpVisual(powerUpType, remainingTime);
    }

    private void OnDisable()
    {
        ResetVisualEffects();
    }

    // ＊うねうね速度を変更
    private void ApplySpeedUpVisual(PowerUpType powerUpType)
    {
        // 「PowerUpがSpeedUpでなければ、そのまま終了」
        if (powerUpType != PowerUpType.SpeedUp)
        {
            return;
        }

        // 「すでにSpeedUpのアニメーション変更が適用されていれば、処理を終了」
        if (isSpeedUpVisualActive)
        {
            return;
        }

        // 「SetWaveSpeedMultiplier() を呼び出す」
        moveSquashAnimator.SetWaveSpeedMultiplier(speedUpAnimationSpeedMultiplier);
        // 「速度変更済み状態へ変更」
        isSpeedUpVisualActive = true;
    }

    // ＊演出を解除
    private void ResetVisualEffects()
    {
        if (!hasActivePowerUp && !isSpeedUpVisualActive)
        {
            return;
        }

        if (isSpeedUpVisualActive)
        {
            moveSquashAnimator.ResetWaveSpeedMultiplier();
            isSpeedUpVisualActive = false;
        }

        if (playerVisual != null)
        {
            playerVisual.ClearPowerUpVisual();
        }

        hasActivePowerUp = false;
    }
}

