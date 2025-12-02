using UnityEngine;
using UnityEngine.InputSystem;

public class Fighter : MonoBehaviour
{
    public enum WeaponType { Unarmed, Melee, Ranged }
    public enum Direction { Up, Down, Left, Right }
    PlayerTouchMovement_RB  playerTouch;

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
    public float holdThreshold = 0.28f; // seconds to enter defend
    public float parryWindow = 0.2f;

    [Header("Targeting")]
    public Transform[] potentialTargets = new Transform[4];
    public int selectedTargetIndex = 0;
    public Transform target => (selectedTargetIndex >= 0 && selectedTargetIndex < potentialTargets.Length) ? potentialTargets[selectedTargetIndex] : null;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 18f;

    // Input System
    private PlayerControls controls;
    private InputAction[] directionActions = new InputAction[4];

    // Input state
    private struct DirectionState
    {
        public bool pressed;
        public float pressTime;
        public bool defending;
    }
    private DirectionState[] directions = new DirectionState[4];

    // Animator hashes
    private static readonly int AttackDirHash = Animator.StringToHash("AttackDirection");
    private static readonly int DefendDirHash = Animator.StringToHash("DefendDirection");
    private static readonly int DefendHash = Animator.StringToHash("Defend");
    private static readonly int EquipHash = Animator.StringToHash("Equip");

    private void Awake()
    {
        if (isEnemy = false)
        {
            controls = new PlayerControls();
            directionActions[0] = controls.Player.AttackUp;
            directionActions[1] = controls.Player.AttackDown;
            directionActions[2] = controls.Player.AttackLeft;
            directionActions[3] = controls.Player.AttackRight;
            controls.Player.Equip.performed += ctx => ToggleEquip();
            playerTouch = GetComponent<PlayerTouchMovement_RB>();
        }

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            directionActions[i].started += ctx => OnPress(idx);
            directionActions[i].canceled += ctx => OnRelease(idx);
        }
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // Stamina regen
        if (stamina < maxStamina)
            stamina = Mathf.Clamp(stamina + staminaRegenRate * Time.deltaTime, 0f, maxStamina);

        // Check hold -> defend
        if (canAct)
        {
            for (int i = 0; i < 4; i++)
            {
                if (directions[i].pressed && !directions[i].defending)
                {
                    if (Time.time - directions[i].pressTime >= holdThreshold && stamina >= staminaCostParry)
                        StartDefend((Direction)i);
                }
            }
        }

        // Enemy AI (optional)
        if (isEnemy) EnemyTick();
    }

    #region Input
    private void OnPress(int dir)
    {
        if (!canAct) return;
        directions[dir].pressed = true;
        directions[dir].pressTime = Time.time;
    }

    private void OnRelease(int dir)
    {
        if (!directions[dir].pressed) return;

        float held = Time.time - directions[dir].pressTime;
        directions[dir].pressed = false;

        if (directions[dir].defending)
        {
            EndDefend((Direction)dir);
            return;
        }

        if (held < holdThreshold)
            TryAttack((Direction)dir);
    }

    public void CancelAllInputs()
    {
        for (int i = 0; i < 4; i++)
        {
            directions[i].pressed = false;
            if (directions[i].defending) EndDefend((Direction)i);
        }
    }
    #endregion

    #region Attack / Defend
    private void TryAttack(Direction dir)
    {
        if (!canAct) return;

        if (currentWeapon == WeaponType.Ranged)
        {
            if (stamina < staminaCostShoot) return;
            stamina -= staminaCostShoot;
            if (target != null) FireProjectile(target.position);
            PlayAttackAnimation(dir);
        }
        else
        {
            if (stamina < staminaCostAttack) return;
            stamina -= staminaCostAttack;

            if (target != null)
            {
                Fighter tgt = target.GetComponent<Fighter>();
                if (tgt != null && tgt.TryParry(dir, parryWindow, Time.time))
                    OnAttackParried(tgt, dir);
                else
                    OnAttackHit(tgt, dir);
            }
            else PlayAttackAnimation(dir);
        }
    }

    private void StartDefend(Direction dir)
    {
        int i = (int)dir;
        if (!canAct || stamina < staminaCostParry) return;

        directions[i].defending = true;
        stamina -= staminaCostParry;

        anim?.SetBool(DefendHash, true);
        anim?.SetInteger(DefendDirHash, i);
        SetLayerWeight(swordDefendLayer, 1f);
    }

    private void EndDefend(Direction dir)
    {
        int i = (int)dir;
        directions[i].defending = false;

        bool anyDefending = false;
        for (int j = 0; j < 4; j++) if (directions[j].defending) { anyDefending = true; break; }

        if (!anyDefending)
        {
            anim?.SetBool(DefendHash, false);
            SetLayerWeight(swordDefendLayer, 0f);
        }
    }

    public bool TryParry(Direction attackDir, float window, float attackTime)
    {
        int i = (int)attackDir;
        if (!directions[i].defending) return false;

        float dt = attackTime - directions[i].pressTime;
        if (dt >= 0f && dt <= window)
        {
            OnSuccessfulParry(attackDir);
            return true;
        }
        return false;
    }
    #endregion

    #region Animations
    private void PlayAttackAnimation(Direction dir)
    {
        if (anim == null) return;
        anim.SetInteger(AttackDirHash, (int)dir);
        anim.SetTrigger("Attack");

        float duration = anim.GetCurrentAnimatorStateInfo(swordAttackLayer).length;
        StartCoroutine(ActivateLayerTemporarily(swordAttackLayer, duration));
    }

    private System.Collections.IEnumerator ActivateLayerTemporarily(int layer, float duration)
    {
        SetLayerWeight(layer, 1f);
        yield return new WaitForSeconds(duration);
        SetLayerWeight(layer, 0f);
    }

    private void SetLayerWeight(int layer, float weight) => anim?.SetLayerWeight(layer, weight);

    private void OnAttackParried(Fighter defender, Direction dir)
    {
        PlayAttackAnimation(dir);
        stamina = Mathf.Max(0f, stamina - staminaCostAttack * 0.2f);
    }

    private void OnAttackHit(Fighter defender, Direction dir) => PlayAttackAnimation(dir);

    private void OnSuccessfulParry(Direction dir) => anim?.SetTrigger("ParrySuccess");
    #endregion

    #region Projectiles
    private void FireProjectile(Vector3 targetPos)
    {
        if (projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
        var p = proj.GetComponent<Projectile>();
        if (p != null) p.Launch(targetPos);
        else
        {
            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = (targetPos - proj.transform.position).normalized * projectileSpeed;
        }
        PlayAttackAnimation(Direction.Up); // optional: adjust direction
    }
    #endregion

    #region Target & Equip
    public void SelectTarget(int index) { if (index >= 0 && index < potentialTargets.Length) selectedTargetIndex = index; }
    public void ToggleEquip()
    {
        // If currently unarmed → equip sword
        if (currentWeapon == WeaponType.Unarmed)
        {
            currentWeapon = WeaponType.Melee;
            playerTouch.WeaponEquipped = true;
            // Activate the sword layer
            SetLayerWeight(swordAttackLayer, 1f);

            anim?.SetTrigger(EquipHash);
        }
        // If currently armed → return to unarmed
        else
        {
            currentWeapon = WeaponType.Unarmed;

            // Disable the sword layer
            SetLayerWeight(swordAttackLayer, 0f);

            anim?.SetTrigger(EquipHash);
        }
    }
    #endregion

    #region Enemy AI
    private float aiTimer = 0f;
    private float aiInterval = 0.5f;

    private void EnemyTick()
    {
        aiTimer -= Time.deltaTime;
        if (aiTimer <= 0f)
        {
            aiTimer = aiInterval;
            if (canAct && target != null)
            {
                Direction dir = (Direction)Random.Range(0, 4);
                TryAttack(dir);
            }
        }
    }
    #endregion
}