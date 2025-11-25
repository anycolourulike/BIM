using System.Collections;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public enum WeaponType { Melee, Ranged }
    public enum Direction { Up = 0, Down = 1, Left = 2, Right = 3 }

    [Header("Animation & Weapon Settings")]
    public Animator anim;
    public string attackStateName = "Attack"; // animation state (optional)
    public WeaponType currentWeapon = WeaponType.Melee;

    [Header("Combat State")]
    public bool isAttacking;
    public bool isDefending;
    public bool isEnemy;
    public bool canAct = true;

    [Header("Stamina Settings")]
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float staminaCostAttack = 20f;
    public float staminaCostParry = 15f;
    public float staminaCostShoot = 25f;

    [Header("Timing")]
    [Tooltip("Hold >= this value => defend. Less => attack.")]
    public float holdThreshold = 0.28f; // hold threshold for long-press (0.25-0.35 recommended)
    [Tooltip("Allowed time difference (s) between defend start and attack time for a successful parry.")]
    public float parryWindow = 0.20f;   // mobile-friendly parry window

    [Header("Enemy / Targeting")]
    public Transform[] potentialTargets = new Transform[4]; // assigned from outside, up to 4
    public int selectedTargetIndex = 0;
    public Transform target => (selectedTargetIndex >= 0 && selectedTargetIndex < potentialTargets.Length) ? potentialTargets[selectedTargetIndex] : null;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 18.3f;

    // Internal: per-direction press tracking
    private struct PressState
    {
        public bool pressed;
        public float pressStartTime;
        public Direction direction;
    }
    private PressState[] presses = new PressState[4];

    // Defense tracking (used to evaluate parry timing)
    private bool[] isDefendingDir = new bool[4];
    private float[] defendStartTime = new float[4];

    // Animator hashes (avoid string lookups at runtime)
    private static readonly int DefendHash = Animator.StringToHash("Defend");
    private static readonly int AttackDirHash = Animator.StringToHash("AttackDirection");
    private static readonly int DefendDirHash = Animator.StringToHash("DefendDirection");
    private static readonly int EquipHash = Animator.StringToHash("Equip");

    void Awake()
    {
        // initialize press states
        for (int i = 0; i < 4; i++)
        {
            presses[i] = new PressState { pressed = false, pressStartTime = 0f, direction = (Direction)i };
            isDefendingDir[i] = false;
            defendStartTime[i] = -9999f;
        }
    }

    void Update()
    {
        // Stamina regen every frame, clamped
        if (stamina < maxStamina)
            stamina = Mathf.Clamp(stamina + staminaRegenRate * Time.deltaTime, 0f, maxStamina);

        // For enemies, a simple AI tick could go here (kept minimal)
        if (isEnemy)
        {
            EnemyTick();
        }
    }

    #region Input hooks (UI should call these)
    // Call on pointer down / button pressed for the given direction
    public void OnDirectionPress(int dir)
    {
        if (!canAct) return;
        if (dir < 0 || dir > 3) return;

        presses[dir].pressed = true;
        presses[dir].pressStartTime = Time.time;
        presses[dir].direction = (Direction)dir;
        // We do not immediately decide attack vs defend until release; for defend we require holdThreshold
        // However defend must be recognizable while held for parry timing - so set defend when threshold is reached.
        StartCoroutine(CheckHoldStart((Direction)dir));
    }

    // Call on pointer up / button released for the given direction
    public void OnDirectionRelease(int dir)
    {
        if (dir < 0 || dir > 3) return;

        var press = presses[dir];
        if (!press.pressed)
            return;

        float held = Time.time - press.pressStartTime;
        presses[dir].pressed = false;

        // If we entered defend state (isDefendingDir) keep defend until released; release now
        if (isDefendingDir[dir])
        {
            EndDefend((Direction)dir);
            return;
        }

        // If not defending, interpret as attack (tap)
        if (held < holdThreshold)
        {
            TryAttack((Direction)dir);
        }
    }

    // Optional: call this if the player cancels input (e.g., UI closes)
    public void CancelAllInputs()
    {
        for (int i = 0; i < 4; i++)
        {
            presses[i].pressed = false;
            if (isDefendingDir[i])
                EndDefend((Direction)i);
        }
    }
    #endregion

    #region Hold detection
    private IEnumerator CheckHoldStart(Direction dir)
    {
        int i = (int)dir;
        float start = presses[i].pressStartTime;
        // Wait until threshold or release
        while (presses[i].pressed)
        {
            if (Time.time - start >= holdThreshold)
            {
                // Start defend (if we have stamina and can act)
                if (!isDefendingDir[i] && stamina >= staminaCostParry && canAct)
                {
                    StartDefend(dir);
                }
                yield break;
            }
            yield return null;
        }
    }
    #endregion

    #region Attack / Defend execution
    private void TryAttack(Direction dir)
    {
        if (!canAct) return;
        if (currentWeapon == WeaponType.Ranged)
        {
            if (stamina < staminaCostShoot) return;
            stamina -= staminaCostShoot;
            // Fire projectile at target
            if (target != null)
            {
                FireProjectile(target.position);
            }
            // Play ranged attack animation (use attack direction for variation)
            PlayAttackAnimation(dir);
        }
        else // Melee
        {
            if (stamina < staminaCostAttack) return;
            stamina -= staminaCostAttack;
            // Perform attack: if target exists, resolve parry / hit
            if (target != null)
            {
                // Evaluate parry on target
                Fighter targetFighter = target.GetComponent<Fighter>();
                if (targetFighter != null)
                {
                    bool parried = targetFighter.TryParry(dir, parryWindow, Time.time);
                    if (parried)
                    {
                        OnAttackParried(targetFighter, dir);
                    }
                    else
                    {
                        OnAttackHit(targetFighter, dir);
                    }
                }
                else
                {
                    // No fighter component -- hit implicit target
                    OnAttackHit(null, dir);
                }
            }
            else
            {
                // No target selected: maybe swing in direction for visuals
                PlayAttackAnimation(dir);
            }
        }
    }

    private void StartDefend(Direction dir)
    {
        int i = (int)dir;
        if (!canAct) return;
        if (stamina < staminaCostParry) return;

        isDefendingDir[i] = true;
        defendStartTime[i] = Time.time;
        isDefending = true; // general flag (true while any dir is defending)
        // Do not immediately deduct stamina on start; deduct on successful parry or optionally while holding.
        // Here we deduct once on start to reserve stamina:
        stamina -= staminaCostParry;

        // Animator: set defend params
        if (anim != null)
        {
            anim.SetBool(DefendHash, true);
            anim.SetInteger(DefendDirHash, (int)dir);
        }
    }

    private void EndDefend(Direction dir)
    {
        int i = (int)dir;
        if (!isDefendingDir[i]) return;
        isDefendingDir[i] = false;
        defendStartTime[i] = -9999f;

        // If no other direction is defending, clear general flag
        bool anyDef = false;
        for (int k = 0; k < 4; k++) if (isDefendingDir[k]) { anyDef = true; break; }
        isDefending = anyDef;

        // Animator
        if (!anyDef && anim != null)
        {
            anim.SetBool(DefendHash, false);
            // Optionally signal defend ended
        }
    }

    // Called by attacker to let defender attempt parry.
    // Returns true if parry succeeded.
    public bool TryParry(Direction incomingAttackDirection, float window, float attackTime)
    {
        int d = (int)incomingAttackDirection;
        // Defender must be defending in the same direction
        if (!isDefendingDir[d]) return false;

        // Defender must have started defend within window of attack time (allow pre-hold or slight late)
        float dt = Mathf.Abs(attackTime - defendStartTime[d]);
        if (dt <= window)
        {
            // Successful parry
            // Optionally, consume stamina (already deducted on start) or apply additional effect
            OnSuccessfulParry(incomingAttackDirection);
            return true;
        }

        // Not within timing window -> fail
        return false;
    }
    #endregion

    #region Attack/Parry callbacks
    private void OnAttackParried(Fighter defender, Direction dir)
    {
        // Play attack parried animation or recoil
        PlayAttackAnimation(dir);
        // Optionally apply small stun, stamina drain, etc.
        // Example: attacker loses a fraction of remaining stamina
        stamina = Mathf.Max(0f, stamina - (staminaCostAttack * 0.2f));
        // Inform defender (already handled in TryParry)
    }

    private void OnAttackHit(Fighter defender, Direction dir)
    {
        // Play attack hit animation and apply damage
        PlayAttackAnimation(dir);
        // Here you would call defender.ApplyDamage(...)
        if (defender != null)
        {
            // Placeholder: call a damage method if exists
            // defender.ApplyDamage(damageAmount);
        }
    }

    private void OnSuccessfulParry(Direction dir)
    {
        // Play parry success feedback (sound, animation)
        if (anim != null)
        {
            // Optionally play separate parry animation
            anim.SetTrigger("ParrySuccess");
        }
    }
    #endregion

    #region Animation helpers
    private void PlayAttackAnimation(Direction dir)
    {
        if (anim == null) return;
        anim.SetInteger(AttackDirHash, (int)dir);
        // Option A: use triggers or play specific state:
        anim.SetTrigger("Attack");
        // If you prefer playing a specific layer/state:
        // anim.Play(attackStateName);
    }
    #endregion

    #region Projectile
    private void FireProjectile(Vector3 targetPos)
    {
        if (projectilePrefab == null) return;
        GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up * 1.0f, Quaternion.identity);
        // Expect projectile to have a script with Launch(Vector3) method or rigidbody
        var p = proj.GetComponent<Projectile>();
        if (p != null) p.Launch(targetPos);
        else
        {
            // fallback: set velocity if rigidbody present
            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (targetPos - proj.transform.position).normalized;
                rb.velocity = dir * projectileSpeed;
            }
        }
        // Play ranged attack animation
        if (anim != null) PlayAttackAnimation(Direction.Up); // or use stored dir if you want
    }
    #endregion

    #region Target selection & equip
    public void SelectTarget(int index)
    {
        if (index < 0 || index >= potentialTargets.Length) return;
        selectedTargetIndex = index;
        // Optional: update UI highlight, etc.
    }

    public void ToggleEquip()
    {
        // Toggle melee/ranged (inventory integration later)
        currentWeapon = (currentWeapon == WeaponType.Melee) ? WeaponType.Ranged : WeaponType.Melee;
        if (anim != null) anim.SetTrigger(EquipHash);
    }
    #endregion

    #region Simple Enemy AI hook (placeholder)
    private float aiDecisionTimer = 0f;
    private float aiDecisionInterval = 0.5f;

    private void EnemyTick()
    {
        // Minimal AI example: choose a random attack direction occasionally
        aiDecisionTimer -= Time.deltaTime;
        if (aiDecisionTimer <= 0f)
        {
            aiDecisionTimer = aiDecisionInterval;
            if (canAct && target != null)
            {
                // Example: simple melee attack if in range
                Direction chosen = (Direction)Random.Range(0, 4);
                // Execute attack (simulate press+release)
                TryAttack(chosen);
            }
        }
    }
    #endregion
}
