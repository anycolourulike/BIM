using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static UnityAction loadMenu;
    public static UnityAction reloadLevel;
        
    int currentScene;
    int sceneToLoad;

    public static LevelManager Instance { set; get; }

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

    private void Start()
    {
        reloadLevel += ReloadLevel;
        loadMenu += LoadMenu;
    }

    public void Resume()
    {
        SaveManager.Instance.Load();
        var lastLevelPlayed = GameManager.Instance.levelsCompleted;
        SceneManager.LoadScene(lastLevelPlayed);
    }

    public void ReloadLevel()
    {
        SaveManager.Instance.Load();
        var lastLevelPlayed = GameManager.Instance.levelsCompleted;
        SceneManager.LoadScene(lastLevelPlayed);
    }    

    public void LoadNextLevel()
    {       
        var sceneToSave = SceneManager.GetActiveScene().buildIndex + 1;
        SaveManager.Instance.CompleteLevel(sceneToSave);     
        if((sceneToSave > 2 && sceneToSave % 2 == 0)) 
        {
            var gameScene = FindObjectOfType<GameScene>();
            var interAd = gameScene.GetComponent<InterstitialAds>();
            interAd.ShowAd();
        } 
                
        SceneManager.LoadScene(sceneToSave);
    }

    public void LoadMenu()
    {        
        SaveManager.Instance.Save();        
        SceneManager.LoadScene("Menu");        
    }   

    public void QuitApp()
    {
        SaveManager.Instance.Save();
        Application.Quit();
    }    
}
