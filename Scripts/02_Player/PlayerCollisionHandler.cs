using UnityEngine;

/// <summary>
/// Head‚ªEnemy‚ÉG‚ê‚½‚Æ‚«APowerUpó‘Ô‚É‰‚¶‚Äˆ—‚ğ•ª‚¯‚éƒNƒ‰ƒX
///     EnemyDestroy’† ¨ Enemy‚ğ”j‰ó
///     Invincible’†   ¨ ‰½‚à‚µ‚È‚¢
///     ’Êí         ¨ Enemy‚ğ’â~‚µ‚ÄGameOver
/// </summary>
public sealed class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PowerUpController powerUpController;
    [SerializeField] private string enemyTag = "Enemy";
    [Min(0f)]
    [SerializeField] private float contactPauseDuration = 0.5f;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[PlayerCollisionHandler] GameManager ‚ª–¢İ’è‚Å‚·", this);
            enabled = false;
            return;
        }

        if (powerUpController == null)
        {
            Debug.LogError("[PlayerCollisionHandler] PowerUpController ‚ª–¢İ’è‚Å‚·", this);
            enabled = false;
            return;
        }

        if (GetComponent<PlayerMovement>() == null)
        {
            Debug.LogError("[PlayerCollisionHandler] must be attached to the Head object with PlayerMovement.", this);
            enabled = false;
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleEnemyContact(other);
    }

    private void HandleEnemyContact(Collider2D other)
    {
        if (gameManager.CurrentState != GameState.Playing)
        {
            return;
        }

        if (!IsEnemy(other))
        {
            return;
        }

        if (powerUpController.IsInvincibleActive)
        {
            return;
        }

        if (powerUpController.IsBodyDamageInvincibleActive)
        {
            return;
        }

        if (powerUpController.IsEnemyDestroyActive)
        {
            DestroyEnemy(other);
            return;
        }

        StopEnemy(other);
        gameManager.SetState(GameState.GameOver);
    }

    private bool IsEnemy(Collider2D other)
    {
        return other.CompareTag(enemyTag);
    }

    private void DestroyEnemy(Collider2D other)
    {
        EnemyReaction enemyReaction = other.GetComponentInParent<EnemyReaction>();

        if (enemyReaction == null)
        {
            Debug.LogWarning("[PlayerCollisionHandler] EnemyReaction was not found. Enemy cannot be destroyed.", this);
            return;
        }

        enemyReaction.DestroyEnemy();
    }

    private void StopEnemy(Collider2D other)
    {
        EnemyMovement enemyMovement = other.GetComponentInParent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.StopForSeconds(contactPauseDuration);
        }
    }
}

