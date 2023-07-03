using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    [SerializeField] Vector3 start = Vector3.zero;
    [SerializeField] Vector3 end = Vector3.zero;
    [SerializeField] float smoothTime = 1f;


    private void Update()
    {
        float t = Mathf.PingPong(Time.time, smoothTime) / smoothTime;
        transform.position = Vector3.Lerp(start, end, t);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }
}
