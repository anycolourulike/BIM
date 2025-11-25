using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { set; get; }
    private PlayerLives playerlives;
    int playerlivesCurrent;
    int startingLevel = 2;
    

    void Awake()
    {
        playerlives = GetComponent<PlayerLives>();
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Load();                       
    }    

    public void Save()
    {
        ES3.Save("playerLives", playerlivesCurrent);
        ES3.Save("levelsCompleted", GameManager.Instance.levelsCompleted);
    }

    public void Load()
    {
        if (ES3.KeyExists("playerLives"))
        {
            playerlives.CurrentLives = ES3.Load<int>("playerLives");
        }
        else
        {
            playerlives.CurrentLives = playerlivesCurrent;
            Save();
            Debug.Log("No save file found");
        }

        if (ES3.KeyExists("levelsCompleted"))
        {
            GameManager.Instance.levelsCompleted = ES3.Load<int>("levelsCompleted");
        }
        else
        {
            GameManager.Instance.levelsCompleted = startingLevel;
            Save();
            Debug.Log("No save file found");
        }
    }

    public void CompleteLevel(int index)
    {  
          GameManager.Instance.levelsCompleted++;
          Save();        
    }

    public void OnPlayerDeath()
    {
        if(playerlives.CurrentLives > 0)
        {
            playerlives.CurrentLives--;
            Save();
        }     
    }    

    //resets save file
    public void ResetSave()
    {
        ES3.DeleteFile("playerLives.es3");
        playerlives.CurrentLives = playerlivesCurrent;

        ES3.DeleteFile("levelsCompleted.es3");
        GameManager.Instance.levelsCompleted = startingLevel;
        Save();
    }
}
