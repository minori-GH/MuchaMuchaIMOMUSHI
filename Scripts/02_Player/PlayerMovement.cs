using UnityEngine;

/// <summary>
/// プレイヤーを実際に移動させるスクリプト
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementCollisionChecker))]
public sealed class PlayerMovement : MonoBehaviour
{
    // 「ゲーム開始時の方向」
    [SerializeField] private Vector2Int initialDirection = Vector2Int.up;
    // 「1マス移動する時間」
    [Min(0.01f)]
    [SerializeField] private float moveDuration = 0.15f;

    // [private/public 型名 変数名:(型名)を入れるための(変数名)という名前の箱を作る]
    private Rigidbody2D rb; // プレイヤーのRigidbody2D
    private MovementCollisionChecker collisionChecker; // 移動判定
    private float moveDurationMultiplier = 1f; // 移動速度を何倍にするか
    private Vector2Int currentDirection; // 今向いている方向
    private Vector2Int requestedDirection; // プレイヤーが入力した方向
    private Vector2Int currentCell; // 現在のマス
    private Vector2Int targetCell; // 移動先のマス
    private Vector2 moveStartPosition; // 移動開始地点
    private Vector2 moveTargetPosition; // 移動終了地点
    private float moveElapsedTime; // 移動開始から何秒経過したか
    private float stopRemainingTime; // 一時停止の残り時間
    private bool isPermanentlyStopped; // 永久停止中かの判定
    private bool isGameplayPaused; // チュートリアルで停止中かの判定
    private bool isMoving; // 移動中かの判定

    // [=> : get](外部から読み取れるようにするが、書き換えはできない)
    public Vector2Int CurrentDirection => currentDirection;
    public Vector2Int CurrentCell => currentCell;
    public Vector2Int TargetCell => targetCell;
    public bool IsMoving => isMoving && !IsStopped;
    public bool IsStopped => isPermanentlyStopped || isGameplayPaused || stopRemainingTime > 0f;

    // *停止時間を設定する
    public void StopForSeconds(float seconds)
    {
        // 「外部から、停止させたい秒数を受け取り、渡された値のうち大きい方を返す」
        stopRemainingTime = Mathf.Max(0f, seconds);
    }

    // *永久停止状態にする
    public void StopPermanently()
    {
        // *GameOverやClearなど、終了時に再開しない停止として使う
        isPermanentlyStopped = true;
        // *移動中フラグを解除する
        isMoving = false;
    }

    //　＊スタート前の再開できる一時停止状態にする
    public void SetGameplayPaused(bool isPaused)
    {
        isGameplayPaused = isPaused;

        if (isGameplayPaused)
        {
            isMoving = false;
        }
    }

    // *外部から移動速度を上げるための関数
    public void SetMoveDurationMultiplier(float multiplier)
    {
        moveDurationMultiplier = Mathf.Max(0.01f, multiplier);
    }

    // *移動速度を元に戻すための関数
    public void ResetMoveDurationMultiplier()
    {
        moveDurationMultiplier = 1f;
    }

    private void Awake()
    {
        // *必要なコンポーネントを取得する
        rb = GetComponent<Rigidbody2D>();
        collisionChecker = GetComponent<MovementCollisionChecker>();

        // [!:否定]
        // 「上下左右じゃなかったら」
        if (!IsCardinal(initialDirection))
        {
            Debug.LogError(
                $"{nameof(PlayerMovement)} initial direction には必ず方向を入れる。 Current value は {initialDirection}",
                this
            );
            enabled = false;
            return;
        }

        // 「initialDirectionが正しい値の場合はcurrentDirectionに代入する」
        currentDirection = initialDirection;
    }

    private void Start()
    {
        // 「現在のマスを取得する」（WorldToCellはワールド座標をマス座標に変換する）
        currentCell = GridUtility.WorldToCell(rb.position);
        // 「最初は同じ」
        targetCell = currentCell;

        // 「プレイヤーをマス中央へ移動させる」
        rb.MovePosition(GridUtility.CellToWorld(currentCell));
    }

    private void FixedUpdate()
    {
        if (isGameplayPaused)
        {
            return;
        }

        if (isPermanentlyStopped)
        {
            return;
        }

        // 「停止中は、タイマーだけ更新して終了する」
        if (IsStopped)
        {
            TickStopTimer();
            return;
        }

        // 「移動中であれば続けて移動する」
        if (isMoving)
        {
            ContinueMove();
            return;
        }

        // 「移動中でない場合は移動可能か試す」
        TryStartMove();
    }

    // *入力された方向を保存する
    public void SetRequestedDirection(Vector2Int direction)
    {
        // 「上下左右じゃなければ無視」
        if (!IsCardinal(direction))
        {
            return;
        }

        // 「requestedDirectionの方向を保存する」
        requestedDirection = direction;
    }

    private void TryStartMove()
    {
        // 「rerequestedDirectionが（0,0）でないとき、かつ、その方向へ動ける場合」
        if (requestedDirection != Vector2Int.zero && CanMove(requestedDirection))
        {
            // 「現在の方向currentDirectionをrequestedDirectionに変更する」
            currentDirection = requestedDirection;
            // 「変更したら(0,0)に戻す」
            requestedDirection = Vector2Int.zero;
        }

        // 「進めない場合は終了」
        if (!CanMove(currentDirection))
        {
            return;
        }

        targetCell = currentCell + currentDirection;
        moveStartPosition = rb.position;
        moveTargetPosition = GridUtility.CellToWorld(targetCell);
        moveElapsedTime = 0f;
        isMoving = true;
    }

    private void ContinueMove()
    {
        // [+=:自身の変数に右辺の値を足して再び代入する加算代入演算子]
        // [fixedDeltaTime:FixedUpdateが呼ばれる間隔]
        // 「moveElapsedTime（経過時間）にTime.fixedDeltaTime（FixedUpdateが呼ばれる間隔）を加算する」
        moveElapsedTime += Time.fixedDeltaTime;
        // [float:小数の箱]
        // [duration:移動時間]
        // [Mathf.Max:大きい方を返す]
        float duration = Mathf.Max(0.01f, moveDuration / moveDurationMultiplier);
        // [Mathf.Clamp01:0～1の間に収める]
        // 「移動の進み具合（moveElapsedTime / durationで何％進んだか計算）をtに保存
        float t = Mathf.Clamp01(moveElapsedTime / duration);
        // 「今の進み具合に応じた位置をnextPositionへ入れる」
        Vector3 nextPosition = Vector3.Lerp(moveStartPosition, moveTargetPosition, t);
        // 「計算した位置へプレイヤーを移動する」
        rb.MovePosition((Vector2)nextPosition);

        // 「t(進行率)が100%でないときContinueMove終了」
        if (t < 1f)
        {
            return;
        }

        // 「到着したら最終位置に移動」
        rb.MovePosition(moveTargetPosition);
        // 「現在マスを更新」
        currentCell = targetCell;
        // 「移動終了」
        isMoving = false;
    }

    // *残り時間を減らす
    private void TickStopTimer()
    {
        //  [Time.fixedDeltaTime : 物理更新1回分の時間間隔]
        stopRemainingTime -= Time.fixedDeltaTime;

        if (stopRemainingTime < 0f)
        {
            stopRemainingTime = 0f;
        }
    }

    // *direction方向へ移動できる(CanMove)か判定する関数
    private bool CanMove(Vector2Int direction)
    {
        // 「targetPosition（現在のマスに方向を足したもの）をワールド座標へ変換して計算」
        Vector2 targetPosition = GridUtility.CellToWorld(currentCell + direction);
        // 「そのtargetPositionに移動できるかどうかを伝える」
        return collisionChecker.CanMoveTo(targetPosition);
    }

    // *directionが上下左右か判定する関数
    private static bool IsCardinal(Vector2Int direction)
    {
        // [==:等しい]、[||:または]
        return direction == Vector2Int.up
            || direction == Vector2Int.down
            || direction == Vector2Int.left
            || direction == Vector2Int.right;
    }
}
