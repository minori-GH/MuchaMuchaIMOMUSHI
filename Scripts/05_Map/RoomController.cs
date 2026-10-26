using UnityEngine;

public sealed class RoomController : MonoBehaviour
{
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;

    public Transform EntrancePoint => entrancePoint;
    public Transform ExitPoint => exitPoint;

    private void Awake()
    {
        if (entrancePoint == null)
        {
            Debug.LogError("[RoomController] EntrancePoint is not assigned.", this);
            enabled = false;
            return;
        }

        if (exitPoint == null)
        {
            Debug.LogError("[RoomController] ExitPoint is not assigned.", this);
            enabled = false;
            return;
        }
    }
}
