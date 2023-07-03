using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
