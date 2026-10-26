using UnityEngine;

public sealed class EnemyWingFlapAnimator : MonoBehaviour
{
    [SerializeField, Range(0f, 0.9f)] private float flapAmount = 0.12f;
    [SerializeField, Min(0f)] private float flapSpeed = 8f;

    private Vector3 initialScale;
    private float phase;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        phase = 0f;
    }

    private void LateUpdate()
    {
        phase += Time.deltaTime * flapSpeed;
        float widthMultiplier = 1f + Mathf.Sin(phase) * flapAmount;

        // Œ©‚½–Ú—p‚Ìq‚¾‚¯‚ğ‰¡‚ÉLk‚µA“–‚½‚è”»’è‚É‚ÍG‚ê‚È‚¢B
        transform.localScale = new Vector3(
            initialScale.x * widthMultiplier,
            initialScale.y,
            initialScale.z
        );
    }

    private void OnDisable()
    {
        transform.localScale = initialScale;
    }
}
