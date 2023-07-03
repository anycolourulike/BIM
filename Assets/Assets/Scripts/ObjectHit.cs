using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectHit : MonoBehaviour
{
    [SerializeField] GameObject Player;    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Player.GetComponent<Player>().PlayerCrash();
            GetComponent<MeshRenderer>().material.color = Color.red;  
        }                  
    }
}
