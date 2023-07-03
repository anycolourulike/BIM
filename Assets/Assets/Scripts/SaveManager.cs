using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { set; get; }
    int playerLives = 25;
    int startingLevel = 2;
    

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Load();                       
    }    

    public void Save()
    {
        ES3.Save("playerLives", GameManager.Instance.playerLives);
        ES3.Save("levelsCompleted", GameManager.Instance.levelsCompleted);
    }

    public void Load()
    {
        if (ES3.KeyExists("playerLives"))
        {
            GameManager.Instance.playerLives = ES3.Load<int>("playerLives");
        }
        else
        {
            GameManager.Instance.playerLives = playerLives;
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
        if(GameManager.Instance.playerLives > 0)
        {
            GameManager.Instance.playerLives--;
            Save();
        }     
    }    

    //resets save file
    public void ResetSave()
    {
        ES3.DeleteFile("playerLives.es3");
        GameManager.Instance.playerLives = playerLives;

        ES3.DeleteFile("levelsCompleted.es3");
        GameManager.Instance.levelsCompleted = startingLevel;
        Save();
    }
}
