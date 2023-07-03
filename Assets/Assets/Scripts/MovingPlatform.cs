using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Vector3 start = Vector3.zero;
    [SerializeField] Vector3 end = Vector3.zero;
    [SerializeField] float smoothTime = 1f;

    private void Update()
    {
        float t = Mathf.PingPong(Time.time, smoothTime) / smoothTime;
        transform.position = Vector3.Lerp(start, end, t);
    }
}
