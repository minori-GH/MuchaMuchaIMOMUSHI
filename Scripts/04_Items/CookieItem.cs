using UnityEngine;

public sealed class CookieItem : MonoBehaviour
{
    [SerializeField] private PowerUpController powerUpController;

    public void SetPowerUpController(PowerUpController targetPowerUpController)
    {
        powerUpController = targetPowerUpController;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null)
        {
            return;
        }

        if (powerUpController == null)
        {
            Debug.LogError("[CookieItem] PowerUpController is not assigned.", this);
            return;
        }

        powerUpController.ApplyRandomPowerUp();
        Destroy(gameObject);
    }
}
