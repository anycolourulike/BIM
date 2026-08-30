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

    [Header("Role Evaluation")]
    [Tooltip("How often roles are recomputed while the player is inside the zone")]
    [SerializeField] private float roleReevalInterval = 0.25f;
 
    // -------------------------------------------------------
    // State
    // -------------------------------------------------------
    public List<EnemyAI> allEnemies          = new();
    public List<EnemyAI> primaryAttackers    = new();
    public List<EnemyAI> secondaryAttackers  = new();
    public List<EnemyAI> backupAttackers     = new();
 
    private bool  _playerInside;
    private float _roleReevalTimer;
 
    public enum Role { None, Primary, Secondary, Backup }
 
    // -------------------------------------------------------
    // Unity
    // -------------------------------------------------------
    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
        else
            Debug.LogError($"{name}: EnemyAttackManager needs a Collider to act as its trigger zone.", this);
    }

    private void Update()
    {
        if (!_playerInside) return;

        _roleReevalTimer -= Time.deltaTime;
        if (_roleReevalTimer <= 0f)
        {
            _roleReevalTimer = roleReevalInterval;
            EvaluateRoles();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player           = ResolvePlayerRoot(other);
        _playerInside    = true;
        _roleReevalTimer = 0f;

        if (uiManager != null)
        {
            uiManager.player  = player;
            uiManager.fighter = player.GetComponentInChildren<Fighter>();
            uiManager.AssignAttackManager(this);
            uiManager.ShowEnemyBtns(true);
        }

        EvaluateRoles();
    }

    /// <summary>
    /// A player collider may sit on a child object; walk up to the rigidbody/root
    /// so <see cref="player"/> is the actual player transform.
    /// </summary>
    private static Transform ResolvePlayerRoot(Collider other)
        => other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player        = null;
        _playerInside = false;

        primaryAttackers.Clear();
        secondaryAttackers.Clear();
        backupAttackers.Clear();

        if (uiManager != null)
        {
            uiManager.ShowEnemyBtns(false);
            uiManager.SetAttackManager(null);
        }
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
        // Drop any destroyed enemies that never got unregistered.
        allEnemies.RemoveAll(e => e == null);

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

        EvaluateRoles(); // promote the next-best attacker into the vacated slot
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

        // Base stand-off distance is set by the enemy's own combat style
        // (a ranged enemy shouldn't be shoved into melee range, and vice versa)...
        float range = GetRangeForEnemy(enemy);

        // ...then pushed out by role so multiple attackers don't share one ring.
        range += role switch
        {
            Role.Secondary => followUpOffset,
            Role.Backup    => followUpOffset * 2f,
            _              => 0f
        };

        // Each enemy gets an offset angle so they don't stack
        int   index = allEnemies.IndexOf(enemy);
        int   count = Mathf.Max(allEnemies.Count, 1);
        float angle = index * (360f / count) * Mathf.Deg2Rad;

        Vector3 offset  = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * range;
        Vector3 desired = player.position + offset;

        desired = ResolveObstruction(enemy, desired, offset, range);

        return NavMesh.SamplePosition(desired, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas)
            ? hit.position
            : enemy.transform.position;
    }
 
    public float GetRangeForEnemy(EnemyAI enemy)
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
            if (e == null || e.Health == null || e.Health.isDead) continue;
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