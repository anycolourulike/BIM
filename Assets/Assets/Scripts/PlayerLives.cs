using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerLives : MonoBehaviour
{        
    [SerializeField] TextMeshProUGUI livesText;
    public int playerLives;
    bool playerIsAlive = false;    

    private void Update()
    {
        playerIsAlive = true;
        playerLives = GameManager.Instance.playerLives;
        PlayerIsAlive();        
    }  

    public void PlayerIsAlive()
    {   
            if (playerIsAlive)
            {
                if (playerLives > 0)
                {
                    DisplayLives(playerLives);
                }
                else
                {
                    playerIsAlive = false;
                }
            }        
    }

    public void DisplayLives(int livesToDisplay)
    {        
        int livesLeft = Mathf.FloorToInt(livesToDisplay);
        livesText.text = string.Format("{000}", livesLeft);
    }
}
