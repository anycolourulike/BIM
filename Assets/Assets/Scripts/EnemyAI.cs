using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Fighter))]
[RequireComponent(typeof(FieldOFView))]
public class EnemyAI : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector
    // -------------------------------------------------------
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
    public float waypointTolerance   = 1f;
    public float waypointDwellTime   = 1.7f;

    [Header("Suspicion")]
    [Tooltip("Delay before attacking after player is spotted")]
    public float suspicionDelay = 0.5f;

    // -------------------------------------------------------
    // State Machine
    // -------------------------------------------------------
    private StateMachine _stateMachine;
    public Patrol PatrolState;
    public Attack AttackState;
    public Dead   DeadState;

    private float _suspicionTimer;

    // -------------------------------------------------------
    // Unity
    // -------------------------------------------------------
    private void Awake()
    {
        Fighter      = Fighter      ?? GetComponent<Fighter>();
        Mover        = Mover        ?? GetComponent<Mover>();
        FOV          = FOV          ?? GetComponent<FieldOFView>();
        Health       = Health       ?? GetComponent<Health>();
        CombatTarget = CombatTarget ?? GetComponent<CombatTarget>();
        Anim         = Anim         ?? GetComponent<Animator>();

        BuildStateMachine();
    }

    private void Start()
    {
        enemyAttackManager.RegisterEnemy(this);
    }

    private void Update()
    {
        TickSuspicion();
        _stateMachine.Tick();
    }

    private void OnDestroy()
    {
        enemyAttackManager.UnregisterEnemy(this);
    }

    // -------------------------------------------------------
    // State Machine Setup
    // -------------------------------------------------------
    private void BuildStateMachine()
    {
        _stateMachine = new StateMachine();

        Vector3 startWaypoint = PatrolPath != null
            ? PatrolPath.GetWaypoint(0)
            : transform.position;

        PatrolState = new Patrol(Anim, this, PatrolPath, Mover, patrolSpeedFraction,
                                 waypointDwellTime, waypointTolerance, Mathf.Infinity, 0,
                                 startWaypoint);
        AttackState = new Attack(this, Fighter, Mover, enemyAttackManager);
        DeadState   = new Dead(this, Mover, enemyAttackManager);

        _stateMachine.AddAnyTransition(DeadState,   () => Health != null && Health.isDead);
        _stateMachine.AddTransition(PatrolState, AttackState, () => FOV != null && FOV.canSeePlayer);
        _stateMachine.SetState(PatrolState);
    }

    // -------------------------------------------------------
    // Suspicion
    // -------------------------------------------------------
    private void TickSuspicion()
    {
        if (FOV != null && FOV.canSeePlayer)
            _suspicionTimer = Mathf.Max(0f, _suspicionTimer - Time.deltaTime);
        else
            _suspicionTimer = suspicionDelay;
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------
    public float ScoreForAttack()
    {
        float health  = Health  != null ? Health.healthPts : 0f;
        float stamina = Fighter != null ? Fighter.stamina  : 0f;
        return health + stamina * 0.5f;
    }

    public Fighter.Direction GetDirectionToPlayer()
    {
        if (enemyAttackManager?.player == null) return Fighter.Direction.Up;

        Vector3 dir = (enemyAttackManager.player.position - transform.position).normalized;
        return Mathf.Abs(dir.x) > Mathf.Abs(dir.z)
            ? (dir.x > 0 ? Fighter.Direction.Right : Fighter.Direction.Left)
            : (dir.z > 0 ? Fighter.Direction.Up    : Fighter.Direction.Down);
    }
}
