using System.Collections;
using UnityEngine;

/// <summary>
/// ゲーム開始前にプレイヤー・敵・PowerUpを停止し、
/// 指アイコンでスワイプ操作を案内するスクリプト
/// </summary>
/// 
public sealed class SwipeStartTutorialController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PowerUpController powerUpController;
    [SerializeField] private RectTransform fingerIcon;
    [SerializeField] private Vector2 moveOffset = new Vector2(120f, 120f); // 動く距離
    [SerializeField, Min(0.01f)] private float moveDuration = 0.45f; // 右上へ動く時間
    [SerializeField, Min(0f)] private float targetWaitDuration = 0.2f; // 待機時間
    [SerializeField, Min(0f)] private float waitDuration = 0.25f; // 次の動作までの待機時間

    private Coroutine animationCoroutine; // 実行中のanimationCoroutineを管理するための型
    private Vector2 initialAnchoredPosition; // 指アイコンの初期位置を保存

    // ＊参照を確認し、指アイコンの初期位置を保存
    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[SwipeStartTutorialController] GameManager is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerMovement == null)
        {
            Debug.LogError("[SwipeStartTutorialController] PlayerMovement is not assigned.", this);
            enabled = false;
            return;
        }

        if (powerUpController == null)
        {
            Debug.LogError("[SwipeStartTutorialController] PowerUpController is not assigned.", this);
            enabled = false;
            return;
        }

        if (fingerIcon == null)
        {
            Debug.LogError("[SwipeStartTutorialController] Finger Icon is not assigned.", this);
            enabled = false;
            return;
        }

        // 「指アイコンの初期位置を保存する」
        // [anchoredPosition : UIの RectTransform にある位置情報で、アンカーを基準とした座標を表す]
        initialAnchoredPosition = fingerIcon.anchoredPosition;
    }

    // ＊GameManagerの状態変更イベントを登録
    private void OnEnable()
    {
        // 「GameManagerの状態が変わったら、HandleGameStateChanged()を呼び出す」
        if (gameManager != null)
        {
            gameManager.OnStateChanged += HandleGameStateChanged;
        }
    }

    // ＊開始待ち処理を呼び出す
    private void Start()
    {
        StartWaitingToStart();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged -= HandleGameStateChanged;
        }

        StopFingerAnimation();
    }

    // ＊芋虫・敵・PowerUpを停止し、開始待ちにする
    private void StartWaitingToStart()
    {
        playerMovement.SetGameplayPaused(true);　// PlayerMovement に一時停止を指示
        EnemyMovement.SetAllGameplayPaused(true); // すべての敵の移動を停止
        powerUpController.SetGameplayPaused(true); // PowerUpControllerに一時停止を指示
        ShowFingerIcon(); // 指アイコンを表示
        gameManager.SetState(GameState.WaitingToStart); // ゲーム状態を WaitingToStart に変更
    }

    // ＊ゲーム状態に応じた処理
    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.WaitingToStart)
        {
            playerMovement.SetGameplayPaused(true);
            EnemyMovement.SetAllGameplayPaused(true);
            powerUpController.SetGameplayPaused(true);
            ShowFingerIcon();
            return;
        }

        if (state == GameState.Playing)
        {
            playerMovement.SetGameplayPaused(false);
            EnemyMovement.SetAllGameplayPaused(false);
            powerUpController.SetGameplayPaused(false);
            HideFingerIcon();
            return;
        }

        HideFingerIcon();
    }

    // ----------指アイコンのアニメーション----------

    // ＊指アイコンを表示する
    private void ShowFingerIcon()
    {
        fingerIcon.gameObject.SetActive(true);

        if (animationCoroutine != null)
        {
            return;
        }

        animationCoroutine = StartCoroutine(AnimateFingerIcon());
    }

    // ＊指アイコンを非表示にする
    private void HideFingerIcon()
    {
        StopFingerAnimation();
        fingerIcon.gameObject.SetActive(false);
    }

    // ＊アニメーションを停止して位置を戻す
    private void StopFingerAnimation()
    {
        // 「実行中のCoroutineを停止し、参照を初期化する」
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // 「指アイコンを初期位置へ戻す」
        if (fingerIcon != null)
        {
            fingerIcon.anchoredPosition = initialAnchoredPosition;
        }
    }

    // ＊指アイコンの往復アニメーションの管理
    private IEnumerator AnimateFingerIcon()
    {
        // 「初期位置に移動量を足して、目標位置を求める」
        Vector2 targetPosition = initialAnchoredPosition + moveOffset;

        while (true)
        {
            // 「右上へ移動」
            yield return MoveFinger(initialAnchoredPosition, targetPosition, moveDuration);

            // 「待機する」
            if (targetWaitDuration > 0f)
            {
                yield return new WaitForSeconds(targetWaitDuration);
            }

            fingerIcon.anchoredPosition = initialAnchoredPosition;

            // 「待機する」
            if (waitDuration > 0f)
            {
                yield return new WaitForSeconds(waitDuration);
            }
        }
    }

    // ＊指アイコンを動かす
    private IEnumerator MoveFinger(Vector2 fromPosition, Vector2 toPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            fingerIcon.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, t);
            yield return null;
        }

        fingerIcon.anchoredPosition = toPosition;
    }
}
