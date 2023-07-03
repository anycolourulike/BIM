using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorTriggerEvent : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] UnityEvent doorTrigger;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {            
            doorTrigger.Invoke();
        }
    }
}
