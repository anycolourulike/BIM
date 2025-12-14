using UnityEngine;

public class Patrol : IState
{
    private readonly PatrolPath _patrolPath;
    private readonly Mover _mover;
    private readonly EnemyAI _enemyAI;

    private readonly float _patrolSpeedFraction;
    private readonly float _waypointTolerance;
    private readonly float _waypointDwellTime;

    private int _currentWaypointIndex;
    private float _timeSinceArrivedAtWaypoint;
    private float _currentDwellDuration;
    private Vector3 _nextPosition;

    private readonly Animator _anim;

    public Patrol(Animator anim, EnemyAI enemyAI, PatrolPath patrolPath, Mover mover, float patrolSpeedFraction,
                  float waypointDwellTime, float waypointTolerance, float timeSinceArrivedAtWaypoint,
                  int currentWaypointIndex, Vector3 nextPosition)
    {
        _anim = anim;
        _enemyAI = enemyAI;
        _patrolPath = patrolPath;
        _mover = mover;

        _patrolSpeedFraction = patrolSpeedFraction;
        _waypointDwellTime = waypointDwellTime;
        _waypointTolerance = waypointTolerance;

        _timeSinceArrivedAtWaypoint = timeSinceArrivedAtWaypoint;
        _currentWaypointIndex = currentWaypointIndex;
        _nextPosition = nextPosition;
    }

    public void Tick()
    {
        UpdateTimers();
        PatrolBehaviour();
    }

    public void OnEnter() { }
    public void OnExit() { }

    private void UpdateTimers()
    {
        _timeSinceArrivedAtWaypoint += Time.deltaTime;
    }

    private void PatrolBehaviour()
    {
        if (_patrolPath == null) return;

        // Check if arrived at current waypoint
        if (AtWaypoint())
        {
            _timeSinceArrivedAtWaypoint = 0f;
            _currentDwellDuration = Random.Range(0.5f, 3f); // random pause at waypoint
            _anim.SetFloat("Locomotion", 0f);
            CycleWaypoint();
        }

        _nextPosition = GetCurrentWaypoint();

        // Start moving if dwell time passed
        if (_timeSinceArrivedAtWaypoint > _currentDwellDuration)
        {
            _anim.SetFloat("Locomotion", 1f);
            _mover.StartMoveAction(_nextPosition, _patrolSpeedFraction);
        }
        else
        {
            _anim.SetFloat("Locomotion", 0f);
        }
    }

    private bool AtWaypoint()
    {
        float distanceToWaypoint = Vector3.Distance(_enemyAI.transform.position, GetCurrentWaypoint());
        return distanceToWaypoint < _waypointTolerance;
    }

    private void CycleWaypoint()
    {
        _currentWaypointIndex = _patrolPath.GetNextIndex(_currentWaypointIndex);
    }

    private Vector3 GetCurrentWaypoint()
    {
        return _patrolPath.GetWaypoint(_currentWaypointIndex);
    }
}
