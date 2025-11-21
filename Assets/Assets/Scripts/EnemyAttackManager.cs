using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackManager : MonoBehaviour
{
    public Transform player;
    public List<CombatTarget> enemies;         // Max 4 enemies
    public LayerMask obstacleMask;

    [Header("Attack Distances")]
    public float meleeDistance = 2f;
    public float rangedDistance = 4f;
    public float followUpOffset = 1.5f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 18.3f;

    [HideInInspector] public List<CombatTarget> primaryAttackers = new List<CombatTarget>();
    [HideInInspector] public List<CombatTarget> secondaryAttackers = new List<CombatTarget>();

    void Start()
    {
        if (enemies.Count > 4)
            enemies = enemies.GetRange(0, 4);

        AssignRoles();
        PositionEnemies();
    }

    void Update()
    {
        foreach (CombatTarget enemy in enemies)
        {
            if (enemy.enemyType == CombatTarget.EnemyType.Ranged)
            {
                if (enemy.fieldOfView != null && enemy.fieldOfView.CanSeePlayer(player))
                {
                    Vector3 targetPos = enemy.TargetFuturePos(player.position, projectileSpeed);
                    FireProjectile(enemy, targetPos);
                }
            }
            else if (enemy.enemyType == CombatTarget.EnemyType.Melee)
            {
                float distance = Vector3.Distance(enemy.transform.position, player.position);
                if (distance > meleeDistance)
                {
                    Vector3 moveDir = (player.position - enemy.transform.position).normalized;
                    enemy.transform.position += moveDir * enemy.meleeSpeed * Time.deltaTime;
                    enemy.transform.LookAt(player);
                }
            }
        }
    }

    void AssignRoles()
    {
        primaryAttackers.Clear();
        secondaryAttackers.Clear();

        int count = enemies.Count;

        // Assign primary attackers (first two)
        for (int i = 0; i < Mathf.Min(2, count); i++)
            primaryAttackers.Add(enemies[i]);

        // Assign secondary attackers (next two)
        for (int i = 2; i < count; i++)
            secondaryAttackers.Add(enemies[i]);
    }

    void PositionEnemies()
    {
        int count = enemies.Count;
        if (count == 0) return;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            CombatTarget enemy = enemies[i];
            float angle = i * angleStep * Mathf.Deg2Rad;
            float distance = enemy.enemyType == CombatTarget.EnemyType.Melee ? meleeDistance : rangedDistance;

            // Adjust for secondary attackers
            if (secondaryAttackers.Contains(enemy))
                distance += followUpOffset;

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
            Vector3 targetPos = player.position + offset;

            // Line-of-sight check
            if (Physics.Raycast(enemy.transform.position, (player.position - enemy.transform.position).normalized,
                Vector3.Distance(enemy.transform.position, player.position), obstacleMask))
            {
                Vector3 perp = Vector3.Cross(offset, Vector3.up).normalized * 1.5f;
                targetPos += perp;

                if (Physics.Raycast(enemy.transform.position, (player.position - targetPos).normalized,
                    Vector3.Distance(targetPos, player.position), obstacleMask))
                {
                    targetPos = player.position + offset.normalized * (distance * 0.7f);
                }
            }

            enemy.transform.position = targetPos;
            enemy.transform.LookAt(player);
        }
    }

    void FireProjectile(CombatTarget enemy, Vector3 targetPos)
    {
        GameObject proj = Instantiate(projectilePrefab, enemy.transform.position + Vector3.up * 1f, Quaternion.identity);
        proj.GetComponent<Projectile>().Launch(targetPos);
    }
}