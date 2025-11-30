using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : IState
{  
   private readonly PatrolPath _patrolPath;
   private readonly Mover _mover;
   private readonly float _patrolSpeedFraction;
   private float _currentDwellDuration;
   public Animator _anim;
   public float _waypointDwellTime;
   public float _waypointTolerence;
   public int _currentWaypointIndex;
   public float _timeSinceArrivedAtWaypoint;
   public Vector3 _nextPosition;
   EnemyAI _enemyAI;


   public Patrol(Animator anim, EnemyAI enemyAI, PatrolPath patrolPath, Mover mover, float patrolSpeedFraction, float waypointDwellTime,
       float waypointTolerence, float timeSinceArrivedAtWaypoint, int currentWaypointIndex, Vector3 nextPosition)
   {
     _patrolPath = patrolPath;
     _enemyAI = enemyAI;
     _mover = mover;
     _waypointTolerence = waypointTolerence;
     _waypointDwellTime = waypointDwellTime;
     _patrolSpeedFraction = patrolSpeedFraction;
     _timeSinceArrivedAtWaypoint = timeSinceArrivedAtWaypoint;
     _currentWaypointIndex = currentWaypointIndex;
     _nextPosition = nextPosition;
     _anim = anim;
   }

    void PatrolBehaviour()
    {        
        if (_patrolPath != null)
        {
            if (AtWaypoint())
            {
                _timeSinceArrivedAtWaypoint = 0f;
                _currentDwellDuration = Random.Range(0.5f, 3f);
                _anim.SetFloat("locomotion", 0f);
                CycleWaypoint(); 
            }
            _nextPosition = GetCurrentWaypoint();
        }
        if(_timeSinceArrivedAtWaypoint > _currentDwellDuration)
        {
            _anim.SetFloat("locomotion", 1f);
            _mover.StartMoveAction(_nextPosition, _patrolSpeedFraction);
        }
        else
        {
            _anim.SetFloat("locomotion", 0f);
        }
    }  
 
    private bool AtWaypoint()
    {
       float distanceToWaypoint = Vector3.Distance(_enemyAI.gameObject.transform.position, GetCurrentWaypoint());
       return distanceToWaypoint < _waypointTolerence;
    }       
 
    private void CycleWaypoint() 
    {
        _currentWaypointIndex = _patrolPath.GetNextIndex(_currentWaypointIndex);
    }                
 
    public Vector3 GetCurrentWaypoint() 
    {
        return _patrolPath.GetWaypoint(_currentWaypointIndex);
    } 

    private void UpdateTimers()
    {
        _timeSinceArrivedAtWaypoint += Time.deltaTime;
    }         

    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        
    }

    public void Tick()
    {
        PatrolBehaviour();
        UpdateTimers();
    }
}
