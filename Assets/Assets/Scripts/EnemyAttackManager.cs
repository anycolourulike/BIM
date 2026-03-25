using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
 
public class EnemyAttackManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Config
    // -------------------------------------------------------
    [Header("References")]
    [SerializeField] public Transform player;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private UIManager uiManager;
 
    [Header("Attack Distances")]
    [SerializeField] public float meleeDistance   = 2f;
    [SerializeField] public float rangedDistance  = 4f;
    [SerializeField] private float followUpOffset = 1.5f;
 
    [Header("NavMesh Sampling")]
    [SerializeField] private float navSampleRadius = 2f;
 
    // -------------------------------------------------------
    // State
    // -------------------------------------------------------
    public List<EnemyAI> allEnemies          = new();
    public List<EnemyAI> primaryAttackers    = new();
    public List<EnemyAI> secondaryAttackers  = new();
    public List<EnemyAI> backupAttackers     = new();
 
    private bool _playerInside;
 
    public enum Role { None, Primary, Secondary, Backup }
 
    // -------------------------------------------------------
    // Unity
    // -------------------------------------------------------
    private void Awake()
    {
        var col      = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
 
        player        = other.transform;
        _playerInside = true;
 
        uiManager.AssignAttackManager(this);
        uiManager.ShowEnemyBtns(true);
        EvaluateRoles();
    }
 
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
 
        player        = null;
        _playerInside = false;
 
        uiManager.ShowEnemyBtns(false);
    }
 
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
        EvaluateRoles(); // promote remaining enemies
    }
 
    public bool IsEnemyInZone(EnemyAI enemy) => allEnemies.Contains(enemy);
 
    // -------------------------------------------------------
    // Role Assignment
    // -------------------------------------------------------
    public void EvaluateRoles()
    {
        allEnemies.Sort((a, b) => b.ScoreForAttack().CompareTo(a.ScoreForAttack()));
 
        primaryAttackers.Clear();
        secondaryAttackers.Clear();
        backupAttackers.Clear();
 
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if      (i == 0) primaryAttackers.Add(allEnemies[i]);
            else if (i == 1) secondaryAttackers.Add(allEnemies[i]);
            else             backupAttackers.Add(allEnemies[i]);
        }
    }
 
    public Role GetRole(EnemyAI enemy)
    {
        if (primaryAttackers.Contains(enemy))   return Role.Primary;
        if (secondaryAttackers.Contains(enemy)) return Role.Secondary;
        if (backupAttackers.Contains(enemy))    return Role.Backup;
        return Role.None;
    }
 
    // -------------------------------------------------------
    // Permissions
    // -------------------------------------------------------
    public bool CanAttack(EnemyAI enemy)
    {
        if (primaryAttackers.Contains(enemy)) return true;

        // Secondary attacks only if primary is gone
        if (secondaryAttackers.Contains(enemy))
            return primaryAttackers.Count == 0;

        // Backup attacks only if both are gone
        if (backupAttackers.Contains(enemy))
            return primaryAttackers.Count == 0 && secondaryAttackers.Count == 0;

        return false;
    }
 
    public void ReleaseSlot(EnemyAI enemy)
    {
        primaryAttackers.Remove(enemy);
        secondaryAttackers.Remove(enemy);
 
        if (allEnemies.Contains(enemy) && !backupAttackers.Contains(enemy))
            backupAttackers.Add(enemy);
    }
 
    // -------------------------------------------------------
    // Force Engage
    // -------------------------------------------------------
    public void ForceEngage(EnemyAI enemyAi)
    {
        if (enemyAi == null || enemyAi.Fighter == null) return;
 
        enemyAi.Fighter.canAct = true;
        enemyAi.GetComponent<StateMachine>()?.SetState(enemyAi.AttackState);
 
        Debug.Log($"{enemyAi.name} forced to engage.");
    }
    
    public void AlertAllEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;
            enemy.GetComponent<StateMachine>()?.SetState(enemy.AttackState);
        }
    }
 
    // -------------------------------------------------------
    // Attack Positioning
    // -------------------------------------------------------
    /// <summary>
    /// Returns a NavMesh position around the player for the given enemy to move to.
    /// </summary>
    public Vector3 GetAttackPosition(EnemyAI enemy)
    {
        if (player == null) return enemy.transform.position;

        Role role = GetRole(enemy);

        float range = role switch
        {
            Role.Primary   => meleeDistance,
            Role.Secondary => meleeDistance + followUpOffset,
            Role.Backup    => rangedDistance,
            _              => meleeDistance
        };

        // Each enemy gets an offset angle so they don't stack
        int   index = allEnemies.IndexOf(enemy);
        int   count = Mathf.Max(allEnemies.Count, 1);
        float angle = index * (360f / count) * Mathf.Deg2Rad;

        Vector3 offset  = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * range;
        Vector3 desired = player.position + offset;

        desired = ResolveObstruction(enemy, desired, offset, range);

        return NavMesh.SamplePosition(desired, out NavMeshHit hit, 1f, NavMesh.AllAreas)
            ? hit.position
            : enemy.transform.position;
    }
 
    private float GetRangeForEnemy(EnemyAI enemy)
    {
        if (enemy.CombatTarget == null) return meleeDistance;
        return enemy.CombatTarget.enemyType == CombatTarget.EnemyType.Melee
            ? meleeDistance
            : rangedDistance;
    }
 
    private Vector3 ResolveObstruction(EnemyAI enemy, Vector3 desired, Vector3 offset, float range)
    {
        Vector3 enemyPos  = enemy.transform.position;
        Vector3 playerPos = player.position;
        Vector3 toPlayer  = (playerPos - enemyPos).normalized;
        float   dist      = Vector3.Distance(enemyPos, playerPos);
 
        if (!Physics.Raycast(enemyPos, toPlayer, dist, obstacleMask))
            return desired;
 
        // Step sideways
        Vector3 perp        = Vector3.Cross(offset, Vector3.up).normalized * 1.5f;
        Vector3 sidestepped = desired + perp;
 
        Vector3 toSidestepped = (playerPos - sidestepped).normalized;
        float   sideDist      = Vector3.Distance(sidestepped, playerPos);
 
        if (!Physics.Raycast(enemyPos, toSidestepped, sideDist, obstacleMask))
            return sidestepped;
 
        // Pull back
        return playerPos + offset.normalized * (range * 0.7f);
    }
 
    // -------------------------------------------------------
    // Group Health
    // -------------------------------------------------------
    public float AverageGroupHealth()
    {
        float total = 0f;
        int   count = 0;
 
        foreach (var e in allEnemies)
        {
            if (e?.Health == null || e.Health.isDead) continue;
            total += e.Health.healthPts;
            count++;
        }
 
        return count == 0 ? 0f : total / count;
    }
 
    // -------------------------------------------------------
    // UI
    // -------------------------------------------------------
    public List<GameObject> GetEnemies()
    {
        var result = new List<GameObject>();
        foreach (var ai in allEnemies)
            if (ai != null) result.Add(ai.gameObject);
        return result;
    }
}