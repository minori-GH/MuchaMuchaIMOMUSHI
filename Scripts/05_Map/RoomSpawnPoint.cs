using UnityEngine;

public sealed class RoomSpawnPoint : MonoBehaviour
{
    private const float GizmoRadius = 0.15f;
    private const float SelectedGizmoRadius = 0.3f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, GizmoRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SelectedGizmoRadius);
    }
}
