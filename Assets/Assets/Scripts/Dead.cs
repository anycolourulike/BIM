using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dead : IState
{
    Mover _mover;
    Fighter _fighter;
    Health _health;
    EnemyAI _enemyAI;

    public Dead(EnemyAI enemyAI, Fighter fighter, Mover mover, Health health)
    {
        _enemyAI = enemyAI;
        _mover = mover;
        _fighter = fighter;
        _health = health;
    }

    public void OnEnter()
    {     
        

    }

    public void OnExit()
    {
        
    }

    public void Tick()
    {
       //Handle Player Ai Death UI / Lives / Saving 
       //Handle Enemy Death
       //Dropper
       //LevellingUp
       //Scoreboard
    }
}
