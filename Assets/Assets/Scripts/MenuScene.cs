using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    private CanvasGroup fadeGroup;
    private float fadeInSpeed = 1f;

    public Transform LevelPanel;
    private Vector3 desiredMenuPosition;
    public RectTransform menuContainer;
    private MenuCamera menuCam;

    public AnimationCurve enteringLevelZoomCurve; 
    private float zoomTransition;

    private void Start()
    {
        fadeGroup = FindObjectOfType<CanvasGroup>();
        fadeGroup.alpha = 1;      
    }

    private void Update()
    {
        fadeGroup.alpha = 1 - Time.time * fadeInSpeed;
    }

    private void InitLevel()
    {
        if (LevelPanel == null)
        {
            Debug.Log("Did not assign the level in inspector");
        }          
    }

    private void SetCameraTo(int menuIndex)
    {
        NavigateTo(menuIndex);
        menuContainer.anchoredPosition3D = desiredMenuPosition;
    }

    private void NavigateTo(int menuIndex)
    {
        switch (menuIndex)
        {
            default:            
            case 0:
                desiredMenuPosition = Vector3.zero;
                menuCam.BackToMainMenu();
                break;
            case 1:
                desiredMenuPosition = Vector3.right * -1980;
                menuCam.MoveToLevel();
                break;          
        }
    }

    public void OnLevelSelect(int currentIndex)
    {
        GameManager.Instance.levelsCompleted = currentIndex;
        if (currentIndex < 2)
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            SceneManager.LoadScene(currentIndex);
        }
        Debug.Log("Selecting level :" + currentIndex);
    }

    //Buttons
    public void OnBeginClick()
    {     
        SaveManager.Instance.ResetSave();
        OnLevelSelect(2);
    }
           
    public void OnQuitClick()
    {
       SaveManager.Instance.Save();
       Application.Quit();
    }  
}
