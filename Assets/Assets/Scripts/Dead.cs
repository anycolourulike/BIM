using UnityEngine;

public class Dead : IState
{
    private readonly EnemyAI _enemyAI;
    private readonly Mover _mover;
    private readonly Fighter _fighter;
    private readonly Health _health;

    public Dead(EnemyAI enemyAI, Fighter fighter, Mover mover, Health health)
    {
        _enemyAI = enemyAI;
        _mover = mover;
        _fighter = fighter;
        _health = health;
    }

    public void OnEnter()
    {
        // Stop all movement and attacks immediately
        if (_mover != null) _mover.CancelNav();
        //if (_fighter != null) _fighter.Cancel();

        // Play death animation
        if (_enemyAI.Anim != null)
        {
            _enemyAI.Anim.SetTrigger("Death");
        }

        // Remove from attack coordination so others reassign roles correctly
        EnemyAttackManager.Instance?.ReleaseSlot(_enemyAI);
        EnemyAttackManager.Instance?.UnregisterEnemy(_enemyAI);

        // disable collider to avoid blocking pathing
        var boxCollider = _enemyAI.GetComponent<BoxCollider>();
        if (boxCollider != null) boxCollider.enabled = false;
        var sphereCollider = _enemyAI.GetComponent<SphereCollider>();
        if (sphereCollider != null) sphereCollider.enabled = false;
        _mover.enabled = false;
    }

    public void Tick()
    {
        // Future:
        // - Drop loot
        // - Spawn XP orbs
        // - Add score
        // - Check player quest logic
        // - Handle corpse cleanup
        
        // For now: corpse does nothing
    }

    public void OnExit()
    {
        // Dead state never exits, but required by interface.
    }
}

