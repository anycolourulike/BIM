using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] GameObject Biffo;      
    [SerializeField] ParticleSystem PlayerDeath;
    [SerializeField] ParticleSystem PlayerCompleteLevel;
    [SerializeField] UnityEvent onDeath;
    [SerializeField] TMP_Text text;
    Animation PUAnim;
    PlayerFollow PlayerCam;
    GameObject pauseButton;   

    public float doorOpenTimer;
    public delegate void PlayerDead();
    public static event PlayerDead playerHasDied;

    private void OnEnable()
    {
        PlayerCam = FindObjectOfType<PlayerFollow>();
        playerHasDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        playerHasDied -= HandlePlayerDeath;
    }

    private void Start()
    {
        PUAnim = FindObjectOfType<Animation>();
        pauseButton = GameObject.FindWithTag("Pause");
    }         

    public IEnumerator Bounce()
    {
        Biffo.transform.gameObject.tag = "Bounce";        
        text.enabled = true;
        text.SetText("BOUNCE OFF WALLS");
        yield return new WaitForSeconds(15f);

        PUAnim.Play("PUAnim");
        yield return new WaitForSeconds(5f);
        PUAnim.Stop("PUAnim");
        text.enabled = false;

        Biffo.transform.gameObject.tag = "Player";    
    } 

    public IEnumerator Shrink()
    {
        Biffo.transform.localScale = new Vector3(0.5f, 0.356f, 0.5f);
        text.enabled = true;
        text.SetText("SHRUNK");
        yield return new WaitForSecondsRealtime(15f);
        
        PUAnim.Play("PUAnim");
        yield return new WaitForSeconds(5f);
        PUAnim.Stop("PUAnim");
        text.enabled = false;

        Biffo.transform.localScale = new Vector3(1f, 0.356f, 1f); 
    }     

    public IEnumerator ShowTEXT15()
    {
        text.enabled = true;
        text.SetText("+15 EXTRA SECONDS");
        yield return new WaitForSeconds(7f);
        text.enabled = false;
    }

    public IEnumerator ShowTEXT30()
    {
        text.enabled = true;
        text.SetText("+30 EXTRA SECONDS");
        yield return new WaitForSeconds(7f);
        text.enabled = false;
    }

    public IEnumerator DoorOpenedTimer()
    {
        text.enabled = true;
        text.SetText("Door Open");
        yield return new WaitForSeconds(doorOpenTimer);
        
        PUAnim.Play("PUAnim");
        yield return new WaitForSeconds(5f);
        PUAnim.Stop("PUAnim");
        text.enabled = false;        
    }

    public void DisableText()
    {
        text.enabled = false;
    }

    public void PlayerCrash()
    {        
        this.gameObject.tag = "Respawn";
        onDeath.Invoke(); //play audio
        playerHasDied?.Invoke(); // Pause Time & handle Player Death
        Instantiate(PlayerDeath, Biffo.transform.position, Quaternion.identity);
    }

    public void OutOfTime()
    {
        onDeath.Invoke(); //play audio
        playerHasDied?.Invoke(); // Pause Time & handle Player Death
        Instantiate(PlayerDeath, Biffo.transform.position, Quaternion.identity);
    }

    public void PlayerComlpete()
    {
        Instantiate(PlayerCompleteLevel, Biffo.transform.position, Quaternion.identity);
    }

    void HandlePlayerDeath()
    {   
        this.gameObject.tag = "Respawn";    
        Biffo.GetComponent<MeshRenderer>().enabled = false;
        Biffo.GetComponent<BoxCollider>().enabled = false;
        PlayerCam.GetComponent<PlayerFollow>().enabled = false;
        Biffo.GetComponent<BiffoMover>().enabled = false;
        
        SaveManager.Instance.OnPlayerDeath();
        var playerLivesLeft = GameManager.Instance.playerLives;
        SaveManager.Instance.Save();        
        pauseButton.SetActive(false);        

        if (playerLivesLeft == 0)
        {
            DialogUI.Instance
             .SetTitle("Game Over")
             .SetMessage("Puny Human!")             
             .OnClose(LevelManager.loadMenu)
             .Show();
        }
        if (playerLivesLeft % 3 == 0) 
        {
             DialogUI.Instance
              .SetTitle("Ouch!")
              .SetMessage("Poor Blaze!")                 
              .OnClose(LevelManager.reloadLevel)
              .Show();
        }
        else
        {
             DialogUI.Instance
              .SetTitle("You Died!")
              .SetMessage("One Life Lost!")
              .OnClose(LevelManager.reloadLevel)
              .Show();            
        }
    }
}
