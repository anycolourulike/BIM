using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Fighter))]
[RequireComponent(typeof(FieldOFView))]
public class EnemyAI : MonoBehaviour
{
    [Header("Components")]
    public Fighter Fighter;
    public Mover Mover;
    public FieldOFView FOV;
    public Health Health;
    public CombatTarget CombatTarget;
    public Animator Anim;
    public PatrolPath PatrolPath;
    public EnemyAttackManager enemyAttackManager;

    [Header("Patrol")]
    public float patrolSpeedFraction = 0.2f;
    public float waypointTolerance = 1f;
    public float waypointDwellTime = 1.7f;

    [Header("Suspicion (detection gating)")]
    [Tooltip("When player first seen, this delay counts down; Attack only triggers when canSeePlayer && suspicionTimer <= 0")]
    public float suspicionDelay = 0.5f;
    private float suspicionTimer = 0f;

    // State machine
    private StateMachine _stateMachine;
    public Patrol PatrolState;
    public Attack AttackState;
    public Dead DeadState;

    void Awake()
    {
        // caches
        Fighter = Fighter ?? GetComponent<Fighter>();
        Mover = Mover ?? GetComponent<Mover>();
        FOV = FOV ?? GetComponent<FieldOFView>();
        Health = Health ?? GetComponent<Health>();
        CombatTarget = CombatTarget ?? GetComponent<CombatTarget>();
        Anim = Anim ?? GetComponent<Animator>();

        // states
        _stateMachine = new StateMachine();
        PatrolState = new Patrol(Anim, this, PatrolPath, Mover, patrolSpeedFraction,
                                 waypointDwellTime, waypointTolerance, Mathf.Infinity, 0,
                                 PatrolPath != null ? PatrolPath.GetWaypoint(0) : transform.position);
        AttackState = new Attack(this, Fighter, Mover, FOV, Health, enemyAttackManager);
        DeadState = new Dead(this, Fighter, Mover, Health, enemyAttackManager);

        // transitions
        _stateMachine.AddAnyTransition(DeadState, () => Health != null && Health.isDead);
        _stateMachine.AddTransition(AttackState, PatrolState, () => FOV != null && FOV.canSeePlayer && suspicionTimer <= 0f);
        _stateMachine.AddTransition(PatrolState, AttackState, () => FOV != null && !FOV.canSeePlayer && suspicionTimer <= 0f);

        _stateMachine.SetState(PatrolState);
    }

    void Start()
    {
        // register with manager (manager will ignore null Instance)
        enemyAttackManager.RegisterEnemy(this);
    }

    void Update()
    {
        UpdateSuspicionTimer();
        _stateMachine.Tick();
    }

    void OnDestroy()
    {
        enemyAttackManager.UnregisterEnemy(this);
    }

    private void UpdateSuspicionTimer()
    {
        // If we see the player, start/reduce the suspicion timer towards zero.
        // Attack will only happen when both canSeePlayer AND suspicionTimer <= 0 (per requirement).
        if (FOV != null && FOV.canSeePlayer)
        {
            // if just started seeing the player, set timer to delay if not already counting down
            if (suspicionTimer > 0f)
            {
                suspicionTimer -= Time.deltaTime;
            }
            else
            {
                // do nothing; it's already <=0 and can trigger Attack via transition
            }
        }
        else
        {
            // when player not visible, reset suspicionTimer to the delay so that
            // when player next appears they require the delay to reach 0 to attack.
            suspicionTimer = suspicionDelay;
            // Note: you can change this behaviour if you want different semantics.
        }
    }

    // Score for AttackManager ordering
    public float ScoreForAttack()
    {
        float healthScore = Health != null ? Health.healthPts : 0f;
        float staminaScore = Fighter != null ? Fighter.stamina : 0f;
        return healthScore + staminaScore * 0.5f; // weight stamina less if desired
    }

    // Convert vector to one of four directions for Fighter
    public Fighter.Direction GetDirectionToPlayer()
    {
        if (enemyAttackManager == null || enemyAttackManager.player == null)
            return Fighter.Direction.Up;

        Vector3 dir = (enemyAttackManager.player.position - transform.position).normalized;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            return dir.x > 0 ? Fighter.Direction.Right : Fighter.Direction.Left;
        else
            return dir.z > 0 ? Fighter.Direction.Up : Fighter.Direction.Down;
    }
}
