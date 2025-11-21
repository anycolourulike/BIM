using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
     [SerializeField] GameObject closestTarget = null;
     [SerializeField] GameObject footFX1;
     [SerializeField] GameObject footFX2;
     [SerializeField] PatrolPath patrolPath;
     [SerializeField] float suspicionTime = 3f;     
     [SerializeField] float cooldown = 2.5f;
     [SerializeField] float waypointTolerence = 1f; 
     [SerializeField] float waypointDwellTime = 1.7f;
     [SerializeField] Mover mover;
     public Transform patrolToTarget;
     public Vector3 nextPosition;
     private float coolDown;
     private float timerForNextAttack;
     private StateMachine stateMachine;

    [Range(0,1)]
    [SerializeField] float patrolSpeedFraction = 0.2f;
    float timeSinceArrivedAtWaypoint  
    = Mathf.Infinity;
    float timeSinceLastSawPlayer  
    = Mathf.Infinity;
    public List<GameObject> targetList
    = new List<GameObject>();
    int currentWaypointIndex = 0;

    private Health health;
    private FieldOFView FOV;
    private CombatTarget combatTarget;
    Fighter fighter;

    public bool isAttacking;  
    public bool isPatrolling;
    public bool isDead;

    Patrol patrol;
    Attack attack;
    Dead dead;

    void Awake()
    {
        
        stateMachine = new StateMachine();
        health = GetComponent<Health>();
        mover = GetComponent<Mover>();
        FOV = GetComponent<FieldOFView>();
        combatTarget = GetComponent<CombatTarget>();
        if (patrolPath != null) { nextPosition = patrolPath.GetWaypoint(1); }
        coolDown = 2.5f;
        timerForNextAttack = coolDown;

        //States       
        patrol = new Patrol(this, patrolPath, mover, waypointTolerence, waypointDwellTime, 
                            patrolSpeedFraction, timeSinceArrivedAtWaypoint, currentWaypointIndex, nextPosition);
        attack = new Attack(this, fighter, mover, FOV, health, timerForNextAttack, timeSinceLastSawPlayer, suspicionTime,
                             coolDown);  
        dead = new Dead(this, fighter, mover, health);  
        
              
        //  var patrolToLocation = new PatrolToLocation(this, patrolPath, mover, patrolSpeedFraction);

        /////Transitions/////
        void At(IState to, IState from, Func<bool> condition) => 
                stateMachine.AddTransition(to, from, condition);

        Func<bool> IsDead() => () => isDead == true;
        Func<bool> HasTarget() => () => FOV.canSeePlayer == true && isDead == false;
    }    

    // Start is called before the first frame update
    void Start()
    {
        stateMachine.SetState(patrol);
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Tick();
        Debug.Log(this.gameObject.name + " " + stateMachine._currentState);
    }

    public void UpdatePatrolToTarget(Transform targetPatrol)
    {
        patrolToTarget = targetPatrol;
        PatrolToTarget();
    }    

    void PatrolToTarget()
    {
        mover.MoveTo(patrolToTarget.position, 7f);
    }
}
