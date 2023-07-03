using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip OnDie;
    [SerializeField] AudioClip OnLevelComplete;
    [SerializeField] AudioClip OnGameOver;
    [SerializeField] AudioClip OnCollect;
    [SerializeField] AudioClip OnPress;
    [SerializeField] AudioClip OnWindowOpen;
    [SerializeField] AudioClip OnWin;
    [SerializeField] AudioClip Ambient;
    [SerializeField] AudioClip Ambient2;
    [SerializeField] AudioClip Ambient3;
    Vector3 PlayAudioPoint;


    public float volume = 100f;
    public static AudioManager Instance { set; get; }    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }        
    }        

    void Update()
    {
        Vector3 PlayAudioPoint = new Vector3(transform.position.x, transform.position.y, 
                                            Camera.main.transform.position.z);
    }
       
    public void PlayDeath()
    {        
       AudioSource.PlayClipAtPoint(OnDie, PlayAudioPoint, volume);
    }

    public void PlayLevelComplete()
    {
        AudioSource.PlayClipAtPoint(OnLevelComplete, PlayAudioPoint, volume);
    }

    public void PlayGameOver()
    {
        AudioSource.PlayClipAtPoint(OnGameOver, PlayAudioPoint, volume);
    }

    public void PlayCollect()
    {
        AudioSource.PlayClipAtPoint(OnCollect, PlayAudioPoint, volume);
    }

    public void PlayPress()
    {
        AudioSource.PlayClipAtPoint(OnPress, PlayAudioPoint, volume);
    }

    public void PlayWindowOpen()
    {
        AudioSource.PlayClipAtPoint(OnWindowOpen, PlayAudioPoint, volume);
    }

    public void PlayWin()
    {
        AudioSource.PlayClipAtPoint(OnWin, PlayAudioPoint, volume);
    }

    public void PlayAmbient()
    {
        AudioSource.PlayClipAtPoint(Ambient, PlayAudioPoint, volume);
    }

    public void PlayAmbient2()
    {
        AudioSource.PlayClipAtPoint(Ambient2, PlayAudioPoint, volume);
    }

    public void PlayAmbient3()
    {
        AudioSource.PlayClipAtPoint(Ambient2, PlayAudioPoint, volume);
    }
}   
