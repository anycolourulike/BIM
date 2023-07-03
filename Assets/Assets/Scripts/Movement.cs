using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    // PARAMETERS - for tuning, typically set in the editor
    // CACHE - e.g. references for readability or speed
    // STATE - private instance (member) variables

    [SerializeField] float mainThrust = 100f;
    [SerializeField] float rotationThrust = 1f;
    [SerializeField] AudioClip mainEngine;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem leftThrusterParticles;
    [SerializeField] ParticleSystem rightThrusterParticles;

    bool MoveUp;
    bool MoveRight; 
    bool MoveLeft;

    Rigidbody rb;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Fly();
    }

    // Update is called once per frame
    public void PointerDownUp()
    {
        MoveUp = true;
    }

    public void PointerUpUp()
    {
        MoveUp = false;
    }

    public void PointerDownRight()
    {
        MoveRight = true;
    }

    public void PointerUpRight()
    {
        MoveRight = false;
    }

    public void PointerDownLeft()
    {
        MoveLeft = true;
    }

    public void PointerUpleft()
    {
        MoveLeft = false;
    }

    void Fly()
    {
        if (MoveUp)
        {
            rb.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
            audioSource.PlayOneShot(mainEngine);
            mainEngineParticles.Play();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();

        }
        if (MoveRight)
            {
                ApplyRotation(-rotationThrust);
                if (!rightThrusterParticles.isPlaying)
                {
                    rightThrusterParticles.Play();
                }
            }
        else if (MoveLeft)
        {
                ApplyRotation(rotationThrust);
                if (!leftThrusterParticles.isPlaying)
                {
                    leftThrusterParticles.Play();
                }
        }
        else
        {
                rightThrusterParticles.Stop();
                leftThrusterParticles.Stop();
        }         
        
    }

    void ApplyRotation(float rotationThisFrame)
    {
        rb.freezeRotation = true;  // freezing rotation so we can manually rotate
        transform.Rotate(Vector3.forward * rotationThisFrame * Time.deltaTime);
        rb.freezeRotation = false;  // unfreezing rotation so the physics system can take over
    }
}
