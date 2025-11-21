using System.Collections;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public enum WeaponType { Melee, Ranged }

    [Header("Animation & Weapon Settings")]
    public Animator anim;
    public string animName;
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

    [Header("Enemy AI Settings")]
    public float attackRange = 2f;          // Melee range
    public float rangedRange = 6f;          // Ranged range
    public float attackCooldown = 1.5f;
    public float parryChance = 0.3f;
    public Transform target;                // Player or enemy target
    public GameObject projectilePrefab;
    public float projectileSpeed = 18.3f;

    [Header("Player Input")]
    private bool buttonPressed;
    private int touchCount = 0;
    private float resetTouches = 2f;
    private Coroutine resetCoroutine;

    void Update()
    {
        // Regenerate stamina
        if (stamina < maxStamina) stamina += staminaRegenRate * Time.deltaTime;

        if (isEnemy)
        {
            EnemyLogic();
        }
        else
        {
            HandleInput();
        }
    }

    #region Player Input
    private void HandleInput()
    {
        if (!canAct) return;

        if (buttonPressed)
        {
            touchCount++;
            if (touchCount == 1)
            {
                isAttacking = true;
                isDefending = false;

                if (resetCoroutine != null) StopCoroutine(resetCoroutine);
                resetCoroutine = StartCoroutine(ResetTouchCount());
            }
            else if (touchCount == 2)
            {
                isAttacking = false;
                isDefending = true;
            }

            buttonPressed = false;
        }
    }

    IEnumerator ResetTouchCount()
    {
        yield return new WaitForSeconds(resetTouches);
        touchCount = 0;
        isAttacking = false;
        isDefending = false;
        resetCoroutine = null;
    }

    public void HandleButtonPress()
    {
        buttonPressed = true;
        Invoke(nameof(ButtonPressed), 0.8f);
    }

    private void ButtonPressed()
    {
        if (anim == null) return;

        if (currentWeapon == WeaponType.Melee)
        {
            if (isAttacking && stamina >= staminaCostAttack)
            {
                stamina -= staminaCostAttack;
                PerformAction("Sword Attack");
            }
            else if (isDefending && stamina >= staminaCostParry)
            {
                stamina -= staminaCostParry;
                PerformAction("Sword Defend");
            }
        }
        else if (currentWeapon == WeaponType.Ranged && stamina >= staminaCostShoot)
        {
            stamina -= staminaCostShoot;
            FireProjectile(target.position);
        }
    }

    #endregion

    #region Enemy AI
    private void EnemyLogic()
    {
        if (target == null || !canAct) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // Decide weapon type based on player weapon
        if (currentWeapon == WeaponType.Melee && distance > rangedRange && !isEnemy)
            currentWeapon = WeaponType.Ranged;

        if (currentWeapon == WeaponType.Melee)
        {
            EnemyMeleeLogic(distance);
        }
        else
        {
            EnemyRangedLogic(distance);
        }
    }

    private void EnemyMeleeLogic(float distance)
    {
        if (distance > attackRange)
        {
            Vector3 moveDir = (target.position - transform.position).normalized;
            transform.position += moveDir * 3f * Time.deltaTime;
            transform.LookAt(target);
        }
        else
        {
            if (stamina >= staminaCostAttack)
                StartCoroutine(PerformEnemyAttack());
            else if (stamina >= staminaCostParry && Random.value < parryChance)
                StartCoroutine(PerformEnemyParry());
        }
    }

    private void EnemyRangedLogic(float distance)
    {
        if (distance > rangedRange)
        {
            // Move closer to optimal ranged distance
            Vector3 moveDir = (target.position - transform.position).normalized;
            transform.position += moveDir * 2f * Time.deltaTime;
        }
        else if (stamina >= staminaCostShoot)
        {
            canAct = false;
            stamina -= staminaCostShoot;
            FireProjectile(target.position);
            StartCoroutine(RangedCooldown());
        }
        transform.LookAt(target);
    }

    private IEnumerator RangedCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAct = true;
    }

    private IEnumerator PerformEnemyAttack()
    {
        canAct = false;
        isAttacking = true;
        stamina -= staminaCostAttack;

        PerformAction("Sword Attack");

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAct = true;
    }

    private IEnumerator PerformEnemyParry()
    {
        canAct = false;
        isDefending = true;
        stamina -= staminaCostParry;

        PerformAction("Sword Defend");

        yield return new WaitForSeconds(attackCooldown);

        isDefending = false;
        canAct = true;
    }
    #endregion

    #region Combat Actions
    private void PerformAction(string layerName)
    {
        if (anim == null) return;

        int layerIndex = anim.GetLayerIndex(layerName);
        if (layerIndex >= 0)
        {
            anim.SetLayerWeight(layerIndex, 1);
            anim.Play(animName, layerIndex);
        }
    }

    private void FireProjectile(Vector3 targetPos)
    {
        if (projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
        proj.GetComponent<Projectile>().Launch(targetPos);
    }

    public void ResetAnimationLayer()
    {
        if (anim == null) return;

        anim.SetLayerWeight(1, 0);

        if (isDefending)
        {
            StartCoroutine(ResetTouchCount());
            anim.SetBool("DefendOver", true);
            anim.SetLayerWeight(2, 0);

            buttonPressed = false;
            isAttacking = false;
            isDefending = false;
        }
    }
    #endregion

    #region Input Buttons
    public void UpButtonPressed() { animName = "High"; HandleButtonPress(); }
    public void DownButtonPressed() { animName = "Low"; HandleButtonPress(); }
    public void LeftButtonPressed() { animName = "Left"; HandleButtonPress(); }
    public void RightButtonPressed() { animName = "Right"; HandleButtonPress(); }
    #endregion
}

