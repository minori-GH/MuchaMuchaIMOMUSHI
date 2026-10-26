using UnityEngine;

/// <summary>
/// EnemyAI:プレイヤーとの距離を確認し、進む方向・停止・見た目の向きを決める
/// enemyMovement.TrySetDirection(direction);で進行方向を渡す
/// EnemyMovement:移動できるかを調べ、指定された方向への移動や停止を処理する
/// </summary>

[RequireComponent(typeof(EnemyMovement))]
public sealed class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private Transform playerHead;
    // 「元のEnemy画像はどちらを正面として描いてあるか」
    [SerializeField] private Vector2Int visualForwardDirection = Vector2Int.up;
    // 「Enemyが方向転換したときに停止する秒数」
    [SerializeField, Min(0f)] private float turnPauseDuration = 0.15f;
    // 「Playerからどれくらい離れたら停止するか」
    [SerializeField, Min(0f)] private float activeDistance = 20f;

    // 「距離による停止状態」
    private bool isPausedByDistance;
    //  [Quaternion : Unityで回転を表す型]
    // 「Enemyの開始時の方向を保存する」
    private Quaternion initialRotation;

    //  [static : すべてのEnemyAIで共有する]
    //  [readonly : このフィールドに別の配列を再代入できない]
    // 「4方向を配列にし、Enemyが次の方向を探すときに4方向を順番に調べる」
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // ＊Awake()：設定を確認し、初期の回転を保存する。
    private void Awake()
    {
        if (enemyMovement == null)
        {
            Debug.LogError("[EnemyAI] EnemyMovement is not assigned.", this);
            enabled = false;
            return;
        }

        // 「渡された値が上下左右のどれかではない場合」
        if (!IsCardinal(visualForwardDirection))
        {
            Debug.LogError(
                $"[EnemyAI] Visual Forward Direction が上下左右の値でない: {visualForwardDirection}",
                this
            );
            enabled = false;
            return;
        }

        // 「ゲーム開始時のEnemyの向きを保存する」
        initialRotation = transform.rotation;
    }

    // ＊Start()：敵の現在の進行方向に見た目を合わせる。
    private void Start()
    {
        // 「現在Enemyが進もうとしている方向を取得して UpdateVisualDirection に渡す」
        // 「後述の UpdateVisualDirection のメソッドで進行方向と見た目を合わせる」
        UpdateVisualDirection(enemyMovement.CurrentDirection);
    }

    // 「プレイヤーの頭の Transform を受け取り、playerHead に登録する」
    public void SetPlayerHead(Transform targetPlayerHead)
    {
        playerHead = targetPlayerHead;
    }

    // ＊Update()：毎フレーム、距離や移動状態を確認して方向を決める。
    private void Update()
    {
        if (EnemyMovement.IsGameplayPaused)
        {
            return;
        }

        // 「プレイヤーが登録されているか確認」
        if (playerHead == null)
        {
            return;
        }

        // 「プレイヤーとの距離による停止状態を確認」
        UpdateDistancePauseState();

        // 「遠すぎて停止中なら終了」
        if (isPausedByDistance)
        {
            return;
        }

        // 「Enemyが現在移動中、または停止中なら終了」
        if (enemyMovement.IsMoving || enemyMovement.IsStopped)
        {
            return;
        }

        //  [out : メソッドから呼び出し元に値を渡す]
        // 「①進む方向が見つかったか　→　戻り値の boolを受け取る」
        // 「②見つかった方向　→　out 引数の direction を受け取る」
        // 「進行方向が見つかった場合だけ、受け取った方向を使って次の処理をする」
        if (TryChooseBestDirection(out Vector2Int direction))
        {
            // 「選んだ方向が現在の進行方向と違うかを調べる。違うなら true」
            bool isTurning = direction != enemyMovement.CurrentDirection;

            // 「EnemyMovement の TrySetDirection(direction) を呼び出し、方向設定に失敗したら終了」
            if (!enemyMovement.TrySetDirection(direction))
            {
                return;
            }

            // 「見た目の向きを選んだ方向に合わせる」
            UpdateVisualDirection(direction);

            // 「実際に向きが変わった場合だけ、設定した秒数だけ移動を止める」
            if (isTurning)
            {
                enemyMovement.StopForSeconds(turnPauseDuration);
            }
        }
    }

    // ＊反対方向を除いて方向を探し、見つからなかった場合は反対方向も許可してもう一度探す
    private bool TryChooseBestDirection(out Vector2Int bestDirection)
    {
        // 「allowOppositeDirection = false　から処理をはじめる」
        if (TryChooseBestDirection(false, out bestDirection))
        {
            // 「方向が見つかったらその結果をを返して終了」
            return true;
        }

        // 「見つからなかったので反対方向も含めて探し直してその結果を返す」
        return TryChooseBestDirection(true, out bestDirection);
    }

    // ＊進める方向の中からPlayerに近づける方向を探す
    private bool TryChooseBestDirection(bool allowOppositeDirection, out Vector2Int bestDirection)
    {
        // 「まだ方向が見つかっていない状態にする」
        bestDirection = Vector2Int.zero;
        // 「Enemyが現在いるマスの座標を取得」
        Vector2Int enemyCell = enemyMovement.CurrentCell;
        // 「プレイヤーのワールド座標を、マスの座標へ変換」
        Vector2Int targetCell = GridUtility.WorldToCell(playerHead.position);
        // 「int の最大値を入れる」
        int bestDistance = int.MaxValue;

        // 「4方向を一つずつ調べる」
        foreach (Vector2Int direction in Directions)
        {
            // 「反対方向を許可しておらず、現在の進行方向が設定されていて、調べている方向がその反対方向のとき」」
            if (!allowOppositeDirection
                && enemyMovement.CurrentDirection != Vector2Int.zero
                && direction == -enemyMovement.CurrentDirection)
            {
                //  [continue : 今回の候補の処理を飛ばし、次の候補へ進む]
                continue;
            }
            // 「壁などがあって進めない方向も候補から外す」
            if (!enemyMovement.CanMoveDirection(direction))
            {
                continue;
            }

            // 「その方向へ1マス進んだ場合の座標を求める」
            Vector2Int nextCell = enemyCell + direction;
            // 「次のマスとプレイヤーのマスとの距離の二乗を求める」
            int x = nextCell.x - targetCell.x;
            int y = nextCell.y - targetCell.y;
            int distance = x * x + y * y;

            // 「※ < の場合、先に調べた方向が残る」
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = direction;
            }
        }

        // 「方向が見つかっていれば true、見つからず (0, 0) のままなら false を返す」
        return bestDirection != Vector2Int.zero;
    }

    // ＊敵の見た目を、進む方向に合わせて回転させる
    private void UpdateVisualDirection(Vector2Int direction)
    {
        // 「上下左右以外の値なら回転処理を終了する」
        if (!IsCardinal(direction))
        {
            return;
        }

        // 「進みたい方向」
        float targetAngle = GetDirectionAngle(direction);
        // 「元の画像が向いている方向」
        float forwardAngle = GetDirectionAngle(visualForwardDirection);
        //  [transform.rotation : GameObject全体]
        //  [Quaternion.Euler() : 指定した角度から回転を作る]
        // 「X軸・Y軸の回転は、Z軸だけを回転させる」
        transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, targetAngle - forwardAngle);
    }

    // ＊プレイヤーとの距離が離れたら停止する
    private void UpdateDistancePauseState()
    {
        // 「プレイヤーと敵のY座標の差を絶対値にする」
        float distanceY = Mathf.Abs(playerHead.position.y - transform.position.y);
        // 「activeDistance よりも離れたたら停止」
        bool shouldPauseByDistance = distanceY > activeDistance;

        // 「現在の停止状態と新しく計算した状態が同じなら何もしない」
        if (isPausedByDistance == shouldPauseByDistance)
        {
            return;
        }
        // 「状態が変わったら、状態を保存して、EnemyMovementに伝える」
        isPausedByDistance = shouldPauseByDistance;
        enemyMovement.SetDistancePaused(isPausedByDistance);
    }

    // ----------向きの計算など----------

    // ＊上下左右かを判定
    private static bool IsCardinal(Vector2Int direction)
    {
        // 「4つのうちどれか1つと一致すれば true を返す」
        return direction == Vector2Int.up
            || direction == Vector2Int.down
            || direction == Vector2Int.left
            || direction == Vector2Int.right;
    }

    // ＊方向を角度に変換する
    private static float GetDirectionAngle(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
        {
            return 90f; //上90℃
        }

        if (direction == Vector2Int.down)
        {
            return -90f; //下-90℃
        }

        if (direction == Vector2Int.left)
        {
            return 180f; //左180℃
        }

        return 0f; //右0℃
    }
}

