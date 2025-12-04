using UnityEngine;

public class Fighter : MonoBehaviour
{
    public enum WeaponType { Unarmed, Melee, Ranged }
    public enum Direction { Up, Down, Left, Right }

    [Header("Player")]
    [SerializeField] private PlayerTouchMovement_RB playerTouch;

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
    public Transform[] potentialTargets = new Transform[4];
    public int selectedTargetIndex = 0;
    public Transform target =>
        (selectedTargetIndex >= 0 && selectedTargetIndex < potentialTargets.Length)
        ? potentialTargets[selectedTargetIndex]
        : null;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 18f;

    // State names for animator.Play
    private readonly string[] attackStates = { "Up", "Down", "Left", "Right" };
    private readonly string[] defendStates = { "Up", "Down", "Left", "Right" };

    private struct DirectionState
    {
        public bool pressed;
        public float pressTime;
        public bool defending;
    }

    private DirectionState[] directions = new DirectionState[4];

    private void Update()
    {
        // Regen stamina
        if (stamina < maxStamina)
            stamina = Mathf.Clamp(stamina + staminaRegenRate * Time.deltaTime, 0f, maxStamina);

        // Hold detection
        if (canAct)
        {
            for (int i = 0; i < 4; i++)
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

        if (isEnemy) EnemyTick();
    }

    #region Input
    public void PressDirection(int dir)
    {
        if (!canAct) return;

        directions[dir].pressed = true;
        directions[dir].pressTime = Time.time;
    }

    public void ReleaseDirection(int dir)
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
    
    public void ToggleEquip()
    {
        if (!canAct) return;

        if (currentWeapon == WeaponType.Unarmed)
        {
            // Equip melee weapon
            currentWeapon = WeaponType.Melee;
            playerTouch.WeaponEquipped = true;

            // Enable attack layer so sword animations are visible
            SetLayerWeight(swordAttackLayer, 1f);

            // Play equip animation directly
            anim.SetTrigger("Draw Sword");
        }
        else
        {
            // Unequip weapon
            currentWeapon = WeaponType.Unarmed;
            playerTouch.WeaponEquipped = false;

            // Disable sword attack layer
            SetLayerWeight(swordAttackLayer, 0f);

            // Play unequip animation directly
            anim.SetTrigger("Sheath Sword");
        }
    }


    public void TryAttack(Direction dir)
    {
        if (!canAct) return;
        if (stamina < staminaCostAttack) return;

        stamina -= staminaCostAttack;

        PlayAttackAnimation(dir);
    }

    private void StartDefend(Direction dir)
    {
        if (!canAct || stamina < staminaCostParry) return;

        int i = (int)dir;
        directions[i].defending = true;
        stamina -= staminaCostParry;

        PlayDefendAnimation(dir);
    }

    private void EndDefend(Direction dir)
    {
        int i = (int)dir;
        directions[i].defending = false;

        // Check if any other direction is still defending
        bool anyDefending = false;
        for (int j = 0; j < 4; j++)
        {
            if (directions[j].defending)
            {
                anyDefending = true;
                break;
            }
        }

        if (!anyDefending)
        {
            SetLayerWeight(swordDefendLayer, 0f);
        }
    }
    #endregion

    #region Animations

    private void PlayAttackAnimation(Direction dir)
    {
        if (anim == null) return;

        int index = (int)dir;

        SetLayerWeight(swordAttackLayer, 1f);
        anim.Play(attackStates[index], swordAttackLayer, 0f);

        StartCoroutine(ResetAttackLayer());
    }

    private System.Collections.IEnumerator ResetAttackLayer()
    {
        yield return null; // allow 1 frame

        float length =
            anim.GetCurrentAnimatorStateInfo(swordAttackLayer).length;

        yield return new WaitForSeconds(length);
        SetLayerWeight(swordAttackLayer, 0f);
    }

    private void PlayDefendAnimation(Direction dir)
    {
        if (anim == null) return;

        SetLayerWeight(swordDefendLayer, 1f);
        anim.Play(defendStates[(int)dir], swordDefendLayer, 0f);
    }

    private void SetLayerWeight(int layer, float weight)
    {
        anim.SetLayerWeight(layer, weight);
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
            if (canAct)
            {
                Direction dir = (Direction)Random.Range(0, 4);
                TryAttack(dir);
            }
        }
    }
    #endregion
}
