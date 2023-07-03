using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PickUps : Item
{
    [SerializeField] UnityEvent onCollect;
    [SerializeField] PickUpType thisPickUp;
    [SerializeField] Player player;
    Timer timer;

    public enum PickUpType
    {       
        shrink,
        bounce,
        fifteenSeconds,
        thirtySeconds,
    }

    private void Start()
    {
        timer = FindObjectOfType<Timer>();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        
        switch (thisPickUp)
        {                
                case PickUpType.shrink:
                    player.StartCoroutine("Shrink");
                    break;
                case PickUpType.bounce:
                    player.StartCoroutine("Bounce");
                    break;
                case PickUpType.fifteenSeconds:
                    timer.IncreaseTime15();
                    player.StartCoroutine("ShowTEXT15");
                    break;
                case PickUpType.thirtySeconds:
                    timer.IncreaseTime30();
                    player.StartCoroutine("ShowTEXT30");
                    break;
        }
    }      
}
