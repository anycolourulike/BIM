using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] public Transform player;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private UIManager uiManager;

    [Header("Distances")]
    [SerializeField] public float meleeDistance = 2f;
    [SerializeField] public float rangedDistance = 4f;
    [SerializeField] private float followUpOffset = 1.5f;

    [Header("Enemy Groups")]
    public  List<EnemyAI> allEnemies = new();
    public  List<EnemyAI> primaryAttackers = new();
    public  List<EnemyAI> secondaryAttackers = new();
    public  List<EnemyAI> backupAttackers = new();

    [Header("Player Detection")] public BoxCollider zoneCollider;
    
    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
        zoneCollider.isTrigger = true;
    }
    
    private bool playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        uiManager.ShowEnemyBtns(true);
        uiManager.ShowWeaponBtns(true);
        playerInside = true;
        player = other.transform;

        EvaluateRoles();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        uiManager.ShowEnemyBtns(false);
        uiManager.ShowWeaponBtns(false);
        playerInside = false;
        player = null;

        // Optional: clear roles or disengage enemies
    }


    public const int MAX_ATTACKERS = 2;

    // -------------------------------------------------------
    // Registration
    // -------------------------------------------------------
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyAI enemy)
    {
        allEnemies.Remove(enemy);
        primaryAttackers.Remove(enemy);
        secondaryAttackers.Remove(enemy);
        backupAttackers.Remove(enemy);
    }
    
    public bool IsEnemyInZone(EnemyAI enemy)
    {
        return allEnemies.Contains(enemy);
    }
    
    // -------------------------------------------------------
    // Role assignment
    // -------------------------------------------------------
    public void EvaluateRoles()
    {
        // Sort by score: highest attackers get priority
        allEnemies.Sort((a, b) => 
            b.ScoreForAttack().CompareTo(a.ScoreForAttack()));

        primaryAttackers.Clear();
        secondaryAttackers.Clear();
        backupAttackers.Clear();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (i == 0)
                primaryAttackers.Add(allEnemies[i]);
            else if (i == 1)
                secondaryAttackers.Add(allEnemies[i]);
            else
                backupAttackers.Add(allEnemies[i]);
        }
    }

    // -------------------------------------------------------
    // Permissions
    // -------------------------------------------------------
    public bool CanAttack(EnemyAI enemy)
    {
        return primaryAttackers.Contains(enemy) || secondaryAttackers.Contains(enemy);
    }

    public void ReleaseSlot(EnemyAI enemy)
    {
        primaryAttackers.Remove(enemy);
        secondaryAttackers.Remove(enemy);

        if (allEnemies.Contains(enemy) && !backupAttackers.Contains(enemy))
            backupAttackers.Add(enemy);
    }

    public enum Role { None, Primary, Secondary, Backup }

    public Role GetRole(EnemyAI enemy)
    {
        if (primaryAttackers.Contains(enemy))  return Role.Primary;
        if (secondaryAttackers.Contains(enemy)) return Role.Secondary;
        if (backupAttackers.Contains(enemy))  return Role.Backup;
        return Role.None;
    }
    
    // -------------------------------------------------------
    //  Call for Help
    // -------------------------------------------------------
    public void ForceEngage(EnemyAI enemyAi)
    {
        if (enemyAi == null || enemyAi.Fighter == null) return;

        // If the ally is currently backup or idle, allow them to attack
        // You may need to adjust role assignment if using roles
        enemyAi.Fighter.canAct = true;

        // Optionally, immediately set their state to Attack if using state machine
        if (enemyAi.AttackState != null)
        {
            enemyAi.GetComponent<StateMachine>()?.SetState(enemyAi.AttackState);
        }

        Debug.Log($"{enemyAi.name} is forced to engage the player!");
    }

    // -------------------------------------------------------
    // Enemy Positioning Around Player
    // -------------------------------------------------------
    public void PositionEnemies()
    {
        if (!playerInside || player == null) return;

        int count = allEnemies.Count;
        if (count == 0) return;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            EnemyAI enemyAI = allEnemies[i];
            if (enemyAI == null) continue;

            CombatTarget enemy = enemyAI.GetComponent<CombatTarget>();
            if (enemy == null) continue;

            float angle = (i * angleStep) * Mathf.Deg2Rad;

            float baseDistance = enemy.enemyType == CombatTarget.EnemyType.Melee
                ? meleeDistance
                : rangedDistance;

            if (secondaryAttackers.Contains(enemyAI))
                baseDistance += followUpOffset;

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * baseDistance;
            Vector3 targetPos = player.position + offset;

            // LOS check
            if (Physics.Raycast(
                enemy.transform.position,
                (player.position - enemy.transform.position).normalized,
                Vector3.Distance(enemy.transform.position, player.position),
                obstacleMask))
            {
                // Step sideways
                Vector3 perp = Vector3.Cross(offset, Vector3.up).normalized * 1.5f;
                targetPos += perp;

                // Still obstructed? Pull back slightly.
                if (Physics.Raycast(
                    enemy.transform.position,
                    (player.position - targetPos).normalized,
                    Vector3.Distance(targetPos, player.position),
                    obstacleMask))
                {
                    targetPos = player.position + offset.normalized * (baseDistance * 0.7f);
                }
            }

            enemy.transform.position = targetPos;
            enemy.transform.LookAt(player);
        }
    }

    // -------------------------------------------------------
    // Group Health
    // -------------------------------------------------------
    public float AverageGroupHealth()
    {
        float total = 0f;
        int count = 0;

        foreach (var e in allEnemies)
        {
            if (e?.Health == null) continue;
            if (e.Health.isDead) continue;

            total += e.Health.healthPts;
            count++;
        }

        return count == 0 ? 0f : total / count;
    }

    // -------------------------------------------------------
    // UI Integration
    // -------------------------------------------------------
    public List<GameObject> GetEnemies()
    {
        List<GameObject> result = new();

        foreach (var ai in allEnemies)
        {
            if (ai != null)
                result.Add(ai.gameObject);
        }

        return result;
    }
}
