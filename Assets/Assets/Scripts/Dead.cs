using UnityEngine;

public class Dead : IState
{
    private readonly EnemyAI _enemyAI;
    private readonly Mover _mover;
    private readonly EnemyAttackManager _enemyAttackManager;

    public Dead(EnemyAI enemyAI, Mover mover, EnemyAttackManager enemyAttackManager)
    {
        _enemyAI = enemyAI;
        _mover = mover;
        _enemyAttackManager = enemyAttackManager;
    }

    public void OnEnter()
    {
        // Death animation and Fighter/collider shutdown are owned by Health.Die().
        // Dead only tears down navigation and attack-role membership.

        if (_mover != null)
            _mover.Deactivate();

        if (_enemyAttackManager != null)
            _enemyAttackManager.UnregisterEnemy(_enemyAI);

        // TODO: when the pickup/loot system lands, corpse collider handling goes
        // here - it needs to keep one collider alive as the loot interaction trigger.
    }

    public void Tick()
    {
        // Future:
        // - Drop loot
        // - Spawn XP orbs
        // - Add score
        // - Handle corpse cleanup / despawn
        //
        // For now the corpse just persists as an environment block.
    }

    public void OnExit()
    {
        // Dead never transitions out, but IState requires it.
    }
}
