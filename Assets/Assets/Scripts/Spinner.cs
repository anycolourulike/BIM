using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
     float xAngle = 0;
     float yAngle = 1.1f;
     float zAngle = 0;
      
    void FixedUpdate()
    {
        transform.Rotate(xAngle , yAngle, zAngle * Time.deltaTime);
    }
}