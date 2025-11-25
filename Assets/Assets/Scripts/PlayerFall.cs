using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFall : MonoBehaviour
{ 
    [SerializeField] GameObject Player;
    [SerializeField] GameObject PlayerDeath;      

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player.GetComponent<Player>().HandlePlayerDeath();
            //Play Death Anim
        }
    }
}
