using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoorOpen : MonoBehaviour
{
    [SerializeField] float doorOpenTime = 30f;
    [SerializeField] Player player;
    public float currentTime;
    bool doorIsOpen = false;
    Animator anim; 

    private void Start()
    {
        currentTime = doorOpenTime;
        anim = GetComponent<Animator>();        
    }
    private void Update()
    {
        if (doorIsOpen == true)
        {
            currentTime -= Time.deltaTime;
        }

        if (currentTime <= 0)
        {            
            anim.SetTrigger("DoorClose");
            doorIsOpen = false;
            currentTime = doorOpenTime;          
        }
    }
    
    public void DoorOpenTimer()
    {
        if( doorIsOpen != true)
        {            
            anim.SetTrigger("DoorOpen");
            player.doorOpenTimer = doorOpenTime - 5f;
            player.StartCoroutine("DoorOpenedTimer");
            doorIsOpen = true;            
        }  
    }
}
