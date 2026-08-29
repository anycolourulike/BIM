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

    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Assign (not +=) so destroyed duplicates or fast-enter-playmode
        // can never stack stale handlers onto these static delegates.
        reloadLevel = ReloadLevel;
        loadMenu    = LoadMenu;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance    = null;
        reloadLevel = null;
        loadMenu    = null;
    }

    // Resume from the menu: jump to the furthest level reached.
    public void Resume()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Load();

        int target = GameManager.Instance != null ? GameManager.Instance.levelsCompleted : 0;
        LoadSceneByIndex(target);
    }

    // Death / retry: restart the level currently being played.
    public void ReloadLevel()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Load();

        LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        int sceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;

        if (SaveManager.Instance != null)
            SaveManager.Instance.CompleteLevel(sceneToLoad);

        if (sceneToLoad > 2 && sceneToLoad % 2 == 0)
        {
            var gameScene = FindObjectOfType<GameScene>();
            var interAd   = gameScene != null ? gameScene.GetComponent<InterstitialAds>() : null;
            //interAd?.ShowAd();
        }

        LoadSceneByIndex(sceneToLoad);
    }

    public void LoadMenu()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Save();
        SceneManager.LoadScene("Menu");
    }

    public void QuitApp()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Save();
        Application.Quit();
    }

    private static void LoadSceneByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"LevelManager: scene build index {buildIndex} is out of range " +
                           $"(0..{SceneManager.sceneCountInBuildSettings - 1}); loading Menu instead.");
            SceneManager.LoadScene("Menu");
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }
}
