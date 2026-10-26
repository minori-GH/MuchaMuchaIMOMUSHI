using UnityEngine;

/// <summary>
/// Enemyが体節に接触したことを通知するスクリプト
/// </summary>
public sealed class SegmentHitDetector : MonoBehaviour
{
    [SerializeField] private string enemyTag = "Enemy";

    private CaterpillarSegment segment;
    private BodyDamageHandler bodyDamageHandler;

    // *初期化
    public void Initialize(CaterpillarSegment owningSegment, BodyDamageHandler damageHandler)
    {
        // *代入する
        segment = owningSegment;
        bodyDamageHandler = damageHandler;
    }

    // *BodyがEnemyに触れたときに呼ぶ
    private void OnTriggerEnter2D(Collider2D other)
    {
        // *Enemyタグでない場合はその場で終了
        if (!IsEnemy(other))
        {
            return;
        }

        // *Intializeされてない場合はその場で終了
        if (segment == null || bodyDamageHandler == null)
        {
            Debug.LogError("[SegmentHitDetector] is not initialized.", this);
            enabled = false;
            return;
        }

        EnemyReaction enemyReaction = other.GetComponentInParent<EnemyReaction>();
        bodyDamageHandler.HandleSegmentHit(segment, enemyReaction);
    }
    
    // *Enemyタグかどうかの判定
    private bool IsEnemy(Collider2D other)
    {
        return other.CompareTag(enemyTag);
    }
}
