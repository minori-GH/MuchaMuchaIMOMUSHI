using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementCollisionChecker))]
public sealed class EnemyMovement : MonoBehaviour
{
    // *敵が初期に向いている方向
    [SerializeField] private Vector2Int initialDirection = Vector2Int.down;
    // *1マスの移動にかかる時間
    [Min(0.01f)]
    [SerializeField] private float moveDuration = 0.5f;

    // *敵についているRigidbody2Dを入れる変数。実際の移動にはrb.MovePosition()を使う
    private Rigidbody2D rb;
    // *移動先に壁があるか、マップ外ではないかを判定するスクリプトを入れる変数
    private MovementCollisionChecker collisionChecker;
    // *現在進んでいる方向
    private Vector2Int currentDirection;
    // *現在いるマス
    private Vector2Int currentCell;
    // *移動先のマス
    private Vector2Int targetCell;
    // *移動開始時のワールド座標
    private Vector2 moveStartPosition;
    // *移動先のワールド座標
    private Vector2 moveTargetPosition;
    // *移動を開始してから何秒経ったかを保存する
    private float moveElapsedTime;
    // *停止の残り時間
    private float stopRemainingTime;
    // *敵が距離の影響で停止しているかどうか
    private bool isPausedByDistance;
    // *終了時に再開しない停止中かどうか
    private bool isPermanentlyStopped;
    // *敵がマスからマスへ移動中かどうか
    private bool isMoving;
    // *ゲーム終了時に全Enemyをまとめて停止する
    private static readonly List<EnemyMovement> ActiveEnemies = new List<EnemyMovement>();
    // *チュートリアル中の停止かどうか
    private static bool isGameplayPaused;

    // *外部から現在の移動方向を読むためのコード
    public Vector2Int CurrentDirection => currentDirection;
    // *外部から現在のマス座標を読むためのコード
    public Vector2Int CurrentCell => currentCell;
    // *外部から移動先のマス座標を読むためのコード
    public Vector2Int TargetCell => targetCell;
    // *外部から敵が移動中かどうかを読むためのコード
    public bool IsMoving => isMoving;
    public bool IsStopped => isPermanentlyStopped || isGameplayPaused || stopRemainingTime > 0f || isPausedByDistance;
    public static bool IsGameplayPaused => isGameplayPaused;

    // *一定時間停止するための関数
    public void StopForSeconds(float seconds)
    {
        // *停止時間がマイナスにならないようにするために記述
        stopRemainingTime = Mathf.Max(0f, seconds);
    }

    // *このEnemy1体を永久停止状態にする
    public void StopPermanently()
    {
        // *GameOverやClearなど、終了時に再開しない停止として使う
        isPermanentlyStopped = true;
        // *移動中フラグの解除
        isMoving = false;
        // *一時停止の残り時間を0にする
        stopRemainingTime = 0f;
    }

    // *登録されているEnemyをすべて永久停止させる
    // [static : 特定のメソッドではなく、全メソッドを対象にする]
    public static void StopAllPermanently()
    {
        // *後ろから前へ調べていく
        for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
        {
            EnemyMovement enemyMovement = ActiveEnemies[i];

            if (enemyMovement == null)
            {
                // *その位置の要素をListから削除
                ActiveEnemies.RemoveAt(i);
                // *次のEnemyの確認へ進む
                continue;
            }

            enemyMovement.StopPermanently();
        }
    }

    // ＊開始待ちなど、再開できる一時停止として使う。
    public static void SetAllGameplayPaused(bool isPaused)
    {
        isGameplayPaused = isPaused;

        if (!isGameplayPaused)
        {
            return;
        }

        for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
        {
            EnemyMovement enemyMovement = ActiveEnemies[i];

            if (enemyMovement == null)
            {
                ActiveEnemies.RemoveAt(i);
                continue;
            }

            enemyMovement.isMoving = false;
        }
    }

    // 「距離による停止状態を設定し、その状態を isPaused として受け取る」
    public void SetDistancePaused(bool isPaused)
    {
        isPausedByDistance = isPaused;
    }

    // *EnemyAIに方向のセットが成功したかどうか伝える
    public bool TrySetDirection(Vector2Int direction)
    {
        // *その方向に進めない場合はfalse
        if (!CanMoveDirection(direction))
        {
            return false;
        }

        // *現在の方向にdirectionを代入する
        currentDirection = direction;
        return true;
    }

    public bool CanMoveDirection(Vector2Int direction)
    {
        // *IsCardinal(上下左右のいずれか）かつ動ける状態のときにtrueを返す
        return IsCardinal(direction) && CanMove(direction);
    }

    // *Enemy一覧に自分自身が登録されていなければ追加する
    private void OnEnable()
    {
        if (!ActiveEnemies.Contains(this))
        {
            ActiveEnemies.Add(this);
        }
    }

    // *コンポーネントが無効になったときに一覧から自分を削除する
    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    private void Awake()
    {
        // *同じGameObjectについているコンポーネントを取得
        rb = GetComponent<Rigidbody2D>();
        collisionChecker = GetComponent<MovementCollisionChecker>();

        // *上下左右でないとき（※IsCardinalはコードの後半で設定している）
        if (!IsCardinal(initialDirection))
        {
            Debug.LogError(
                $"{nameof(EnemyMovement)} initial direction must be 上下左右. Current value: {initialDirection}",
                this // *このエラーがどのGameObject由来なのかをUnityに渡している。Consoleでクリックすると該当オブジェクトを見つけやすくなる。
            );
            // *このスクリプトを無効化
            enabled = false;
            return;
        }

        // *初期方向を、現在の移動方向として設定
        currentDirection = initialDirection;
    }

    private void Start()
    {
        // *敵の現在のワールド座標をマス座標に変換してcurrentCellに入れる
        currentCell = GridUtility.WorldToCell(rb.position);
        // *最初はまだ移動していないので移動先も現在地と同じにしている
        targetCell = currentCell;

        // *敵を現在のセルの中心座標へ移動させる
        rb.MovePosition(GridUtility.CellToWorld(currentCell));
    }

    // *物理演算用の更新関数
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

        if (isPausedByDistance)
        {
            if (stopRemainingTime > 0f)
            {
                TickStopTimer();
            }

            return;
        }

        // *移動中の場合
        if (isMoving)
        {
            // *目的地へ向かって移動を続ける
            ContinueMove();
            return;
        }

        // *停止中の場合
        if (IsStopped)
        {
            // *一時停止の残り時間を減らす
            TickStopTimer();
            return;
        }

        // *移動中・停止中でない場合、新しい1マス移動を開始できるか試す
        TryStartMove();
    }

    // *次のマスへ移動を開始する関数
    private void TryStartMove()
    {
        // *移動できない場合はそこで終了
        if (!CanMove(currentDirection))
        {
            return;
        }

        // *移動先のセル＝現在セル＋進もうとしている方向
        targetCell = currentCell + currentDirection;
        // *現在のワールド座標を移動開始地点として保存
        moveStartPosition = rb.position;
        // *移動先のセル座標をワールド座標に変換して、移動終了地点として保存
        moveTargetPosition = GridUtility.CellToWorld(targetCell);
        // *移動開始直後なので経過時間を0に戻す
        moveElapsedTime = 0f;
        // *移動中状態にする
        isMoving = true;
    }

    private void ContinueMove()
    {
        // *移動経過時間を増やす
        // [Time.fixedDeltaTime : FixedUpdate()が呼ばれる間隔]
        moveElapsedTime += Time.fixedDeltaTime;
        float duration = Mathf.Max(0.01f, moveDuration);
        // *移動の進行率を計算（durationに対するMoveElapsedTimeの値から進行度を計算している）
        // [Clamp01 : 0〜1の範囲に収める（0開始、1到着）]
        float t = Mathf.Clamp01(moveElapsedTime / duration);
        // *進行度tから開始地点と終了地点の間の位置を計算
        Vector3 nextPosition = Vector3.Lerp(moveStartPosition, moveTargetPosition, t);

        // *計算した位置へ敵を移動。nextPositionはVector3なのでVector2に変換している
        rb.MovePosition((Vector2)nextPosition);

        // *まだ目的地に到着していないとき
        if (t < 1f)
        {
            return;
        }

        rb.MovePosition(moveTargetPosition);
        // *現在のセルを移動先セルに変換
        currentCell = targetCell;
        // *移動を完了したので移動中フラグをfalseにする
        isMoving = false;
    }

    // *停止中に一時停止の残り時間を減らしていく
    private void TickStopTimer()
    {
        // *FixedUpdateが呼ばれるたびに残り時間を減らす
        stopRemainingTime -= Time.fixedDeltaTime;

        // *0以下にならないようにするために記述
        if (stopRemainingTime < 0f)
        {
            stopRemainingTime = 0f;
        }
    }

    // *指定した方向へ移動できるか判定する関数
    private bool CanMove(Vector2Int direction)
    {
        // *現在セルに方向を足して移動先のセルを計算し、ワールド座標へ変換
        Vector2 targetPosition = GridUtility.CellToWorld(currentCell + direction);
        // *移動できるかはMovementCollisionCheckerで判定
        return collisionChecker.CanMoveTo(targetPosition);
    }

    // *指定された方向が上下左右のどれかか判定する関数
    private static bool IsCardinal(Vector2Int direction)
    {
        // *上下左右ならOK
        return direction == Vector2Int.up
            || direction == Vector2Int.down
            || direction == Vector2Int.left
            || direction == Vector2Int.right;
    }
}
