using System;
using UnityEngine;

/// <summary>
/// PlayerにかかっているPowerUpの状態を管理するクラス
/// </summary>
public sealed class PowerUpController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PowerUpData[] powerUpDataList;
    // 「体節ダメージ後の短時間無敵の効果時間」
    [SerializeField, Min(0f)] private float bodyDamageInvincibleDuration = 1f;

    // 「残り時間を保存するフィールド」
    private float speedUpRemainingTime;
    private float invincibleRemainingTime;
    private float enemyDestroyRemainingTime;
    private float bodyDamageInvincibleRemainingTime;
    private bool isGameplayPaused;

    // 「PowerUpが発動したことを他のクラスへ通知するためのイベント」
    public event Action<PowerUpData> OnPowerUpApplied;

    // 「現在効果中かどうかを外から読む読み込み専用プロパティ」
    // 「invincibleRemainingTimeが0より大きければtrueを返す」
    public bool IsSpeedUpActive => speedUpRemainingTime > 0f;
    public bool IsInvincibleActive => invincibleRemainingTime > 0f;
    public bool IsEnemyDestroyActive => enemyDestroyRemainingTime > 0f;
    public bool IsBodyDamageInvincibleActive => bodyDamageInvincibleRemainingTime > 0f;

    // ＊現在発動しているPowerUpの種類と残り時間を、ほかのスクリプトに教えるためのメソッド
    public bool TryGetActivePowerUp(out PowerUpType powerUpType, out float remainingTime)
    {
        if (IsSpeedUpActive)
        {
            powerUpType = PowerUpType.SpeedUp;
            remainingTime = speedUpRemainingTime;
            return true;
        }

        if (IsInvincibleActive)
        {
            powerUpType = PowerUpType.Invincible;
            remainingTime = invincibleRemainingTime;
            return true;
        }

        if (IsEnemyDestroyActive)
        {
            powerUpType = PowerUpType.EnemyDestroy;
            remainingTime = enemyDestroyRemainingTime;
            return true;
        }

        powerUpType = PowerUpType.SpeedUp;
        remainingTime = 0f;
        return false;
    }

    private void Awake()
    {
        if (playerMovement == null)
        {
            Debug.LogError("[PowerUpController] PlayerMovement is not assigned.", this);
            enabled = false;
            return;
        }

        // 「配列そのものがない、または配列の中身が0個の場合はエラー」
        if (powerUpDataList == null || powerUpDataList.Length == 0)
        {
            Debug.LogError("[PowerUpController] PowerUpDataList is not assigned.", this);
            enabled = false;
            return;
        }
    }

    // 「各効果の残り時間を毎フレーム減らす」
    private void Update()
    {
        if (isGameplayPaused)
        {
            return;
        }

        TickSpeedUp();
        TickInvincible();
        TickEnemyDestroy();
        TickBodyDamageInvincible();
    }

    public void SetGameplayPaused(bool isPaused)
    {
        isGameplayPaused = isPaused;
    }

    public void ApplyRandomPowerUp()
    {
        // 「まず、現在の効果を終了」
        EndCurrentPowerUp();

        // 「抽選に成功しなかったときはエラー」
        // 「抽選の結果何が当たったかをここで受けとる」
        if (!TryChoosePowerUp(out PowerUpData powerUpData))
        {
            Debug.LogWarning("[PowerUpController] No selectable power up option. Check weights.", this);
            return;
        }

        Debug.Log($"[PowerUpController] Selected PowerUp: {powerUpData.PowerUpType}", this);

        // 「抽選に成功したらpowerUpDataを受け取ってApplyPowerUpに進む」
        ApplyPowerUp(powerUpData);
    }

    // 「ダメージ後無敵タイマーを最初からスタート」
    public void StartBodyDamageInvincibility()
    {
        bodyDamageInvincibleRemainingTime = bodyDamageInvincibleDuration;
    }

    // 「抽選されたPowerUpの種類によって処理を分ける」
    private void ApplyPowerUp(PowerUpData powerUpData)
    {
        // 「powerUpData.PowerUpType の中身を見る」
        switch (powerUpData.PowerUpType)
        {
            case PowerUpType.SpeedUp:
                ApplySpeedUp(powerUpData.Duration, powerUpData.SpeedMultiplier);
                break;
            case PowerUpType.Invincible:
                invincibleRemainingTime = powerUpData.Duration;
                break;
            case PowerUpType.EnemyDestroy:
                enemyDestroyRemainingTime = powerUpData.Duration;
                break;
        }

        // [?. :誰かこのイベントを受け取る人がいるならInvokeするという安全な呼び方]
        // [Invoke :イベントを発生させる]
        OnPowerUpApplied?.Invoke(powerUpData);
    }

    // 「duration何秒続くか、speedMultiplier速度をどのくらい変えるか受け取ってSpeedUp」
    private void ApplySpeedUp(float duration, float speedMultiplier)
    {
        // 「SpeedUpの残り時間を設定」
        speedUpRemainingTime = duration;
        // 「PlayerMovement に速度変更を依頼」
        playerMovement.SetMoveDurationMultiplier(speedMultiplier);
    }

    // 「現在のPowerUpを終了する」
    private void EndCurrentPowerUp()
    {
        EndSpeedUp();
        EndInvincible();
        EndEnemyDestroy();
    }

    private void EndSpeedUp()
    {
        // 「現在SpeedUp中だったか記録」
        bool wasSpeedUpActive = speedUpRemainingTime > 0f;

        // 「タイマーを０へ」
        speedUpRemainingTime = 0f;

        // 「SpeedUp中だった場合」
        if (wasSpeedUpActive)
        {
            // 「PlayerMovementの速度倍率を通常状態へ戻す」
            playerMovement.ResetMoveDurationMultiplier();
        }
    }

    private void EndInvincible()
    {
        invincibleRemainingTime = 0f;
    }

    private void EndEnemyDestroy()
    {
        enemyDestroyRemainingTime = 0f;
    }

    // 「抽選されたPowerUpDataも外へ渡す」
    private bool TryChoosePowerUp(out PowerUpData selectedPowerUpData)
    {
        // 「まだ何も選ばれていない」
        selectedPowerUpData = null;
        // 「重み合計 = 0からスタート」
        float totalWeight = 0f;

        // 「powerUpDataList の中身を1個ずつ powerUpData に入れて処理する」
        foreach (PowerUpData powerUpData in powerUpDataList)
        {
            // 「有効なWeightだけ足す」
            if (powerUpData != null && powerUpData.Weight > 0f)
            {
                totalWeight += powerUpData.Weight;
            }
        }

        // 「重みが合計0なら抽選不能」
        if (totalWeight <= 0f)
        {
            return false;
        }

        // 「ランダム値をつくる」
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // 「もう一度PowerUpを順番に見る」
        foreach (PowerUpData powerUpData in powerUpDataList)
        {
            // 「無効なDataは飛ばす」
            if (powerUpData == null || powerUpData.Weight <= 0f)
            {
                continue;
            }

            currentWeight += powerUpData.Weight;

            // 「当選判定」
            if (randomValue <= currentWeight)
            {
                selectedPowerUpData = powerUpData;
                return true;
            }
        }

        return false;
    }

    // 「SpeedUpの時間を毎フレーム減らす」
    private void TickSpeedUp()
    {
        // 「すでに0なら何もしない」
        if (speedUpRemainingTime <= 0f)
        {
            return;
        }

        // 「次の残り時間を計算」
        float nextRemainingTime = speedUpRemainingTime - Time.deltaTime;

        // 「0以下になったらSpeedUpを終了」
        if (nextRemainingTime <= 0f)
        {
            EndSpeedUp();
            return;
        }

        speedUpRemainingTime = nextRemainingTime;
    }

    private void TickInvincible()
    {
        invincibleRemainingTime = TickTimer(invincibleRemainingTime);
    }

    private void TickEnemyDestroy()
    {
        enemyDestroyRemainingTime = TickTimer(enemyDestroyRemainingTime);
    }

    private void TickBodyDamageInvincible()
    {
        bodyDamageInvincibleRemainingTime = TickTimer(bodyDamageInvincibleRemainingTime);
    }

    // 「PowerUpController自身のフィールドを直接使わず渡されたremainingTimeだけを計算する」
    private static float TickTimer(float remainingTime)
    {
        if (remainingTime <= 0f)
        {
            return 0f;
        }

        // 「時間を減らす」
        remainingTime -= Time.deltaTime;
        // 「0とremainingTimeのうち、大きい方を返す」
        return Mathf.Max(0f, remainingTime);
    }
}