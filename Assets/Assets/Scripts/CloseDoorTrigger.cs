using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour
{
    public Player player;
    public GameObject playerObj;
    public DoorOpen doorOpen;

    void Start()
    {
       playerObj = GameObject.Find("Player (Biffo)");
       player = playerObj.GetComponent<Player>();
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == player)
        {            
           player.DisableText();
           doorOpen.currentTime = 0;
        }
    }
}
