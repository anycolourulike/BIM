using UnityEngine;

public class Fighter : MonoBehaviour
{
    public enum WeaponType { Unarmed, Melee, Ranged }
    public enum Direction { Up, Down, Left, Right }

    [Header("Player Input")] //Assigned In Inspector
    [SerializeField] private PlayerTouchMovement_RB playerTouch;
    [SerializeField] private EnemyAttackManager enemyAttackManager;
    [SerializeField] private EnemyAI enemyAi;

    [Header("Animation & Weapon")]
    public Animator anim;
    public WeaponType currentWeapon = WeaponType.Melee;

    [Header("Animator Layers")]
    public int unarmedLayer = 0;
    public int swordMovementLayer = 1;
    public int swordAttackLayer = 2;
    public int swordDefendLayer = 3;

    [Header("Combat State")]
    public bool isEnemy;
    public bool canAct = true;

    [Header("Stamina")]
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float staminaCostAttack = 20f;
    public float staminaCostParry = 15f;
    public float staminaCostShoot = 25f;

    [Header("Timing")]
    public float holdThreshold = 0.28f;
    public float parryWindow = 0.2f;

    [Header("Targeting")] 
    public GameObject player; //assign in Inspector
    public GameObject currentTarget;
    public Transform[] potentialTargets = new Transform[4];
    public int selectedTargetIndex = 0;
    private Vector3 currentTargetVector;
    private float targetRefreshTimer;
    private float refreshInterval = 0.2f;
    private bool hasTarget;
    public Transform target => 
        (selectedTargetIndex >= 0 && selectedTargetIndex < potentialTargets.Length) 
        ? potentialTargets[selectedTargetIndex] 
        : null;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 18f;
    
    private float firstAttacker = 1f;
    private float secondAttacker = 2f;
    private float thirdAttacker = 3f;
    public float distanceFromPlayer;
    private Mover _mover;

    private readonly string[] attackStates = { "Up", "Down", "Left", "Right" };
    private readonly string[] defendStates = { "Up", "Down", "Left", "Right" };

    private struct DirectionState
    {
        public bool pressed;
        public float pressTime;
        public bool defending;
    }

    private DirectionState[] directions = new DirectionState[4];

    #region Unity Methods

    private void Start()
    {
        _mover = GetComponent<Mover>();
    }

    private void Update()
    {
        RegenStamina();
        HandleHoldDefend();
        TargetTimer();
        
        if (!isEnemy && currentTarget != null)
            RotateTowards(currentTarget.transform);
    }

    #endregion

    #region Input Handling

    public void PressDirection(int dir)
    {
        if (!canAct) return;
        if (dir < 0 || dir >= directions.Length) return;
        directions[dir].pressed = true;
        directions[dir].pressTime = Time.time;
    }

    public void ReleaseDirection(int dir)
    {
        if (dir < 0 || dir >= directions.Length) return;
        if (!directions[dir].pressed) return;

        float heldTime = Time.time - directions[dir].pressTime;
        directions[dir].pressed = false;

        if (directions[dir].defending)
        {
            EndDefend((Direction)dir);
            return;
        }

        if (heldTime < holdThreshold)
            TryAttack((Direction)dir);
    }

    public void CancelAllInputs()
    {
        for (int i = 0; i < directions.Length; i++)
        {
            directions[i].pressed = false;
            if (directions[i].defending) EndDefend((Direction)i);
        }
    }

    #endregion
    
    #region Targeting
    
    public void RotateTowards(Transform target)
{
    if (target == null) return;

    Vector3 direction = target.position - transform.position;
    direction.y = 0f;

    if (direction.sqrMagnitude < 0.001f) return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        targetRotation,
        10f * Time.deltaTime
    );
}
    
    public void SelectTarget(int index, bool useEnemyManager = true)
    {
        selectedTargetIndex = index;
        if (useEnemyManager)
        {
            RefreshTargetPosition();
        }
        else if (index >= 0 && index < potentialTargets.Length && potentialTargets[index] != null)
        {
            currentTarget = potentialTargets[index].gameObject;
        }
    }
    
    public void SelectEnemyIndex(int index)
    {
        selectedTargetIndex = index;
        RefreshTargetPosition();
        targetRefreshTimer = 0f;
    }
    
    private void RefreshTargetPosition()
    {
        if (enemyAttackManager == null) return;

        var enemies = enemyAttackManager.GetEnemies();
        if (enemies == null || selectedTargetIndex < 0 || selectedTargetIndex >= enemies.Count) return;

        var enemy = enemies[selectedTargetIndex];
        if (enemy == null) return;

        currentTargetVector = enemy.transform.position;
        hasTarget = true;
    }

    private void TargetTimer()
    {
        targetRefreshTimer += Time.deltaTime;
        if (targetRefreshTimer >= refreshInterval)
        {
            RefreshTargetPosition();
            targetRefreshTimer = 0f;
        }
    }
    
    
    public Vector3 GetTargetLocation()
    {
        return transform.position;
    }
    
    #endregion

    #region Combat Actions

    public void ToggleEquip()
    {
        if (!canAct) return;

        if (currentWeapon == WeaponType.Unarmed)
        {
            EquipMeleeWeapon();
        }
        else
        {
            UnequipWeapon();
        }
    }

    private void EquipMeleeWeapon()
    {
        currentWeapon = WeaponType.Melee;
        SetLayerWeight(swordMovementLayer, 1f);
        anim?.SetTrigger("Draw Sword");
        if (CompareTag("Player") && playerTouch != null)
        {
            playerTouch.WeaponEquipped = true;
        }
    }

    private void UnequipWeapon()
    {
        anim?.SetTrigger("Sheath Sword");
        SetLayerWeight(swordMovementLayer, 0f);
        currentWeapon = WeaponType.Unarmed;
        currentTarget = null;
        if (CompareTag("Player") && playerTouch != null)
        {
            playerTouch.WeaponEquipped = false;
        }
    }

    public void TryAttack(Direction dir, float damageMultiplier = 1f)
    {
        if (!canAct || stamina < staminaCostAttack) return;

        stamina -= staminaCostAttack;
        PlayAttackAnimation(dir, damageMultiplier);
    }

    private void StartDefend(Direction dir)
    {
        int index = (int)dir;
        if (!canAct || stamina < staminaCostParry) return;

        directions[index].defending = true;
        stamina -= staminaCostParry;

        PlayDefendAnimation(dir);
    }

    private void EndDefend(Direction dir)
    {
        int index = (int)dir;
        directions[index].defending = false;

        bool anyDefending = false;
        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i].defending)
            {
                anyDefending = true;
                break;
            }
        }

        if (!anyDefending) SetLayerWeight(swordDefendLayer, 0f);
    }

    #endregion

    #region Animations

    private void PlayAttackAnimation(Direction dir, float damageMultiplier)
    {
        SetLayerWeight(swordAttackLayer, 1f);
        anim?.Play(attackStates[(int)dir], swordAttackLayer, 0f);
        // CombatSystem.Instance.DealDamage(target, baseDamage * damageMultiplier);
        if (isActiveAndEnabled)
            StartCoroutine(ResetAttackLayer());
    }

    private void PlayDefendAnimation(Direction dir)
    {
        SetLayerWeight(swordDefendLayer, 1f);
        anim?.Play(defendStates[(int)dir], swordDefendLayer, 0f);
    }

    private void SetLayerWeight(int layer, float weight)
    {
        anim?.SetLayerWeight(layer, weight);
    }

    private System.Collections.IEnumerator ResetAttackLayer()
    {
        yield return null; // let the Animator enter the new state
        float length = anim != null
            ? anim.GetCurrentAnimatorStateInfo(swordAttackLayer).length
            : 0f;
        yield return new WaitForSeconds(length);
        SetLayerWeight(swordAttackLayer, 0f);
    }

    #endregion

    #region Stamina

    private void RegenStamina()
    {
        if (stamina < maxStamina)
            stamina = Mathf.Clamp(stamina + staminaRegenRate * Time.deltaTime, 0f, maxStamina);
    }

    private void HandleHoldDefend()
    {
        if (!canAct) return;

        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i].pressed && !directions[i].defending)
            {
                if (Time.time - directions[i].pressTime >= holdThreshold &&
                    stamina >= staminaCostParry)
                {
                    StartDefend((Direction)i);
                }
            }
        }
    }

    #endregion

    #region Enemy AI

    private float aiTimer = 0f;
    private float aiInterval = 0.5f;
    /// <summary>
    /// Distance to Player
    /// </summary>
    private float distanceToPlayer;
    
    private void EnemyTick()
    {
        var role = enemyAttackManager.GetRole(enemyAi);
        AssignAttackPriority(role);
    
        if (target != null)
            RotateTowards(target); // every frame, outside timer

        aiTimer -= Time.deltaTime;
        if (aiTimer > 0f) return;
        aiTimer = aiInterval;

        if (!canAct) return;
        _mover.MoveTo(target.position, 1f);
        Direction dir = (Direction)Random.Range(0, 4);
        TryAttack(dir);
    }
    
    private void AssignAttackPriority(EnemyAttackManager.Role currentRole)
    {
        switch (currentRole)
        {
            case EnemyAttackManager.Role.Primary:
                distanceFromPlayer = firstAttacker;
                break;

            case EnemyAttackManager.Role.Secondary:
                distanceFromPlayer = secondAttacker;
                break;

            case EnemyAttackManager.Role.Backup:
                distanceFromPlayer = thirdAttacker;
                break;

            default:
                break;
        }
    }
    #endregion
}
