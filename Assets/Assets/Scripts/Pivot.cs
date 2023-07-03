using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot : MonoBehaviour
{
    [SerializeField] float smoothTime = 1f;
    [SerializeField] Vector3 start;
    [SerializeField] Vector3 end;


    private void Update()
    {
        float t = Mathf.PingPong(Time.time, smoothTime) / smoothTime;
        transform.eulerAngles = Vector3.Lerp(start, end, t);
    }

}
