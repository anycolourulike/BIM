using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] TextMeshProUGUI timeText;
    public float timeRemaining = 150f;
    bool timerIsRunning = false;    

    private void Awake()
    {
        timerIsRunning = true;
    }

    private void OnEnable()
    {
       Player.playerHasDied += StopTimer;
    }

    private void OnDisable()
    {
        Player.playerHasDied -= StopTimer;
    }

    public void GamePaused()
    {        
        Time.timeScale = 0;       
        timerIsRunning = false;        
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        timerIsRunning = true;
    }

    void StopTimer()
    {
        timerIsRunning = false;
    }   

    void Update()
    {        
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                player.OutOfTime();
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }
    
    public void IncreaseTime15()
    {
        timeRemaining += 15f;
    }

    public void IncreaseTime30()
    {
        timeRemaining += 30f;
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;                
        float seconds = Mathf.FloorToInt(timeToDisplay);
        timeText.text = string.Format("{000}", seconds);
    }
}