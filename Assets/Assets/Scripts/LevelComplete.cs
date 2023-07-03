using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelComplete : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] UnityEvent onWin;   

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {
            onWin.Invoke();
            Player.GetComponent<MeshRenderer>().enabled = false;
            FindObjectOfType<Player>().PlayerComlpete();
            Player.GetComponent<BiffoMover>().enabled = false;          
        }        
    }    
}
