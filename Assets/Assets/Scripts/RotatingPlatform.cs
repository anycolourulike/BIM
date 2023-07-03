using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    float xAngle = 0;
    [SerializeField] float yAngle = 0.8f;
    float zAngle = 0;

    void FixedUpdate()
    {
        transform.Rotate(xAngle, yAngle, zAngle * Time.deltaTime);
    }
}