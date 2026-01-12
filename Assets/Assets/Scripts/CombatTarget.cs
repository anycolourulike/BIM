using UnityEngine;

public class CombatTarget : MonoBehaviour
{
    public enum EnemyType { Ranged, Melee }
    public EnemyType enemyType = EnemyType.Ranged;

    [Header("Melee Settings")]
    public float meleeSpeed = 3f;
    public float meleeDistance = 3f;

    [Header("Ranged Settings")]
    public FieldOFView fieldOfView;

    
    private Vector3 prevTargetPos;
    private Vector3 targetVelocity;
    private float targetSpeed;

    void Start()
    {
        prevTargetPos = transform.position;
    }

    void Update()
    {
        // Calculate target velocity for predictive aiming
        Vector3 curPos = transform.position;
        targetVelocity = (curPos - prevTargetPos) / Time.deltaTime;
        targetSpeed = targetVelocity.magnitude;
        prevTargetPos = curPos;
    }

    public Transform CurrentPosition()
    {
        return this.transform;
    }

    public Vector3 TargetFuturePos(Vector3 shooterPos, float projectileSpeed)
    {
        if (fieldOfView != null && !fieldOfView.CanSeePlayer(transform))
            return transform.position;

        return GetPredictedPosition(transform.position, targetVelocity, shooterPos, projectileSpeed);
    }

    private Vector3 GetPredictedPosition(Vector3 targetPos, Vector3 targetVel, Vector3 shooterPos, float projectileSpeed)
    {
        Vector3 displacement = targetPos - shooterPos;
        float a = Vector3.Dot(targetVel, targetVel) - projectileSpeed * projectileSpeed;
        float b = 2f * Vector3.Dot(targetVel, displacement);
        float c = Vector3.Dot(displacement, displacement);

        float discriminant = b * b - 4f * a * c;
        if (discriminant < 0 || Mathf.Abs(a) < 0.001f)
            return targetPos;

        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDisc) / (2f * a);
        float t2 = (-b - sqrtDisc) / (2f * a);
        float t = Mathf.Min(t1 > 0 ? t1 : float.MaxValue, t2 > 0 ? t2 : float.MaxValue);

        if (t == float.MaxValue)
            return targetPos;

        return targetPos + targetVel * t;
    }
}