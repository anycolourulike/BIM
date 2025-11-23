using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [Header("Primary Target")]
    public Transform player;

    [Header("Secondary Target (Optional)")]
    public Transform secondaryTarget;  // Can be null
    public bool useSecondaryTarget = false;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 10, -10);
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.05f;
    public float tiltAngle = 45f;

    private Vector3 velocity = Vector3.zero;
    private float currentYRotation;

    void LateUpdate()
    {
        if (!player) return;

        // --- Determine target position ---
        Vector3 targetPosition;

        if (useSecondaryTarget && secondaryTarget != null)
        {
            // Midpoint between player and secondary target
            targetPosition = (player.position + secondaryTarget.position) * 0.5f + offset;
        }
        else
        {
            targetPosition = player.position + offset;
        }

        // Smooth position follow
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // Smooth rotation to match primary target Y rotation
        float targetYRotation = player.eulerAngles.y;
        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(tiltAngle, currentYRotation, 0f);
    }

    // --- Public method to remove secondary target ---
    public void RemoveSecondaryTarget()
    {
        secondaryTarget = null;
        useSecondaryTarget = false;
    }
}