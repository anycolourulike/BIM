using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerBackwardsSlow : MonoBehaviour
{
     float xAngle = 0;
     float yAngle = -0.7f;
     float zAngle = 0;

    void FixedUpdate()
    {
        transform.Rotate(xAngle, yAngle, zAngle * Time.deltaTime);
    }
}
