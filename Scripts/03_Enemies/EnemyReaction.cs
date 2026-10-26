using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public sealed class EnemyReaction : MonoBehaviour
{
    [SerializeField, Min(0f)] private float bodyHitPauseDuration = 1.5f;

    private EnemyMovement enemyMovement;
    // [ bool : true か false のどちらかを返す。初期値は false ]

    private bool isDestroying;

    public bool IsDestroying => isDestroying;

    // [ void : 処理の結果を返さなないメソッド]
    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public void ReactToBodyHit()
    { 
        if (isDestroying)
        {
            return;
        }

        // * EnemyMovement に停止処理を依頼する
        enemyMovement.StopForSeconds(bodyHitPauseDuration);
    }

    public void DestroyEnemy()
    {
        if (isDestroying)
        {
            return;
        }

        isDestroying = true;
        // [ Destroy : Unityが用意している破壊用のメソッド]
        // [ gameObject : このこのスクリプトが付いている GameObject 指す]
        Destroy(gameObject);
    }
}
