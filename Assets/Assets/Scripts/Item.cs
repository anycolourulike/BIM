using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{   
    protected Player _player;
    // Start is called before the first frame update
    void Start()
    {
        _player = FindObjectOfType<Player>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }
}
