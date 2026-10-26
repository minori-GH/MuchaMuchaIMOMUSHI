using UnityEngine;

/// <summary>
/// 別のコードから HandleSegmentHit() が呼ばれることで処理が始まり、
/// 敵が芋虫の体節に接触したとき、パワーアップの状態を確認し、
/// 体節の削除・一時停止・無敵時間・点滅を実行するクラス。
/// </summary>

public sealed class BodyDamageHandler : MonoBehaviour
{
    [SerializeField] private CaterpillarBody caterpillarBody;
    [SerializeField] private PowerUpController powerUpController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField, Min(0f)] private float playerHitStopDuration = 0.1f;

    // ＊必要な参照を確認する
    private void Awake()
    {
        if (caterpillarBody == null)
        {
            Debug.LogError("[BodyDamageHandler] CaterpillarBody is not assigned.", this);
            enabled = false;
            return;
        }

        if (powerUpController == null)
        {
            Debug.LogError("[BodyDamageHandler] PowerUpController is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerMovement == null)
        {
            Debug.LogError("[BodyDamageHandler] PlayerMovement is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerVisual == null)
        {
            Debug.LogWarning("[BodyDamageHandler] PlayerVisual is not assigned. Damage visual will not play.", this);
        }
    }

    // ＊接触時の処理を判断・実行する（外部から接触したsegmentとememyReactionを受け取って以下を実行）
    public void HandleSegmentHit(CaterpillarSegment segment, EnemyReaction enemyReaction)
    {
        // 「接触した体節が存在するか確認する」
        if (segment == null)
        {
            Debug.LogWarning("[BodyDamageHandler] Segment is null.", this);
            return;
        }

        // 「パワーアップによる無敵中なら終了」
        if (powerUpController.IsInvincibleActive)
        {
            return;
        }

        // 「被ダメージ後の無敵中なら終了」
        if (powerUpController.IsBodyDamageInvincibleActive)
        {
            return;
        }

        // 「EnemyDestroy 中なら、プレイヤーを一時停止して終了」
        if (powerUpController.IsEnemyDestroyActive)
        {
            StopPlayerForHit();
            return;
        }

        // 「接触した体節の番号を取得する」
        int segmentIndex = caterpillarBody.GetSegmentIndex(segment);
        //  [条件演算子（三項演算子） : 条件 ? 条件がtrueのときの値 : falseのときの値]
        // 「インデックスが0以上のとき、1を足して番号にする。0未満のときは-1にする」
        int segmentNumber = segmentIndex >= 0 ? segmentIndex + 1 : -1;
        // 「enemyReaction が存在するとき、そのコンポーネントが付いているGameObjectの名前を取得する」
        // 「存在しないとき、"Unknown Enemy" という文字列を取得する」
        string enemyName = enemyReaction != null ? enemyReaction.gameObject.name : "Unknown Enemy";

        Debug.Log(
            $"[BodyDamageHandler] Enemy hit body segment. Segment: {segment.name}, Index: {segmentIndex}, Number: {segmentNumber}, Enemy: {enemyName}",
            segment
        );

        // 「接触した体節を渡して、削除処理を依頼する」
        bool removed = caterpillarBody.RemoveSegmentsFrom(segment);

        // 「削除に失敗した場合は終了」
        if (!removed)
        {
            return;
        }

        // 「プレイヤーを一時停止する」
        StopPlayerForHit();

        // 「敵へ反応を依頼する」
        if (enemyReaction == null)
        {
            Debug.LogWarning("[BodyDamageHandler] EnemyReaction was not found. Enemy cannot react to a body hit.", this);
        }
        else
        {
            enemyReaction.ReactToBodyHit();
        }

        // 「被ダメージ後の無敵時間を開始する」
        powerUpController.StartBodyDamageInvincibility();

        // 「ダメージ点滅を開始する」
        if (playerVisual != null)
        {
            playerVisual.PlayDamageBlink();
        }
    }

    // ＊プレイヤーへ一時停止を依頼するメソッド
    private void StopPlayerForHit()
    {
        playerMovement.StopForSeconds(playerHitStopDuration);
    }
}


