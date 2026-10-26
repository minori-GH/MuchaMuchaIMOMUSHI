using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "Caterpillar Game/Power Up Data")]
public sealed class PowerUpData : ScriptableObject
{
    [SerializeField] private PowerUpType powerUpType;
    [Min(0f)]
    [SerializeField] private float weight = 1f;
    [Min(0.1f)]
    [SerializeField] private float duration = 5f;
    [Min(0.01f)]
    [SerializeField] private float speedMultiplier = 1.5f;

    public PowerUpType PowerUpType => powerUpType;
    public float Weight => weight;
    public float Duration => duration;
    public float SpeedMultiplier => speedMultiplier;
}

