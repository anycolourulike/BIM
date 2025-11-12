using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemies;
    [SerializeField] ParticleSystem PlayerDeath;
    [SerializeField] ParticleSystem PlayerCompleteLevel;
    [SerializeField] UnityEvent onDeath;
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject invBtn;
    [SerializeField] GameObject equip;
    [SerializeField] GameObject pickUp;
    [SerializeField] GameObject dive;
    [SerializeField] GameObject enemyUI1;
    [SerializeField] GameObject enemyUI2;
    [SerializeField] GameObject enemyUI3;
    [SerializeField] GameObject enemyUI4;
    Animation PUAnim;
    PlayerFollow PlayerCam;
    GameObject pauseButton;

    public RectTransform enemyUI; // Reference to the Enemy UI element
    public RectTransform centerUI; // Reference to the Center UI element
    public float distanceFromCenter = 250f; // Fixed distance from the center    

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

    //Add PickUp
    //Add Draw Weapon
    //Add Ranged Weapon
    //Inventory
    //Add Dive

    //Find and add enemies. Find Enemy manager Object. Called By a trigger. On Exit the Fight area the list clears
    //Populate Buttons with correct UI and rotation.

    private void Start()
    {
        PUAnim = FindObjectOfType<Animation>();
        pauseButton = GameObject.FindWithTag("Pause");
    }

    // Positions the Enemy UI object at a specified angle relative to the center UI object.
    public void PositionEnemyUIAtAngle(float angle)
    {
        // Convert angle to radians, as Unity's trigonometric functions use radians
        float angleInRadians = angle * Mathf.Deg2Rad;

        // Calculate the new position based on the angle and fixed distance
        float xOffset = Mathf.Cos(angleInRadians) * distanceFromCenter;
        float yOffset = Mathf.Sin(angleInRadians) * distanceFromCenter;

        // Set the position of the Enemy UI relative to the Center UI's position
        enemyUI.anchoredPosition = new Vector2(centerUI.anchoredPosition.x + xOffset, centerUI.anchoredPosition.y + yOffset);
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
        Instantiate(PlayerDeath, player.transform.position, Quaternion.identity);
    }

    public void OutOfTime()
    {
        onDeath.Invoke(); //play audio
        playerHasDied?.Invoke(); // Pause Time & handle Player Death
        Instantiate(PlayerDeath, player.transform.position, Quaternion.identity);
    }

    public void PlayerComlpete()
    {
        Instantiate(PlayerCompleteLevel, player.transform.position, Quaternion.identity);
    }

    void HandlePlayerDeath()
    {
        this.gameObject.tag = "Respawn";
        player.GetComponentInParent<MeshRenderer>().enabled = false;
        player.GetComponent<BoxCollider>().enabled = false;
        PlayerCam.GetComponent<PlayerFollow>().enabled = false;
      

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


    float GetAngleBetweenObjects(Transform player, Transform enemy)
    {
        ////// Direction vector from player to enemy
        Vector3 directionToEnemy = enemy.position - player.position;

        // Project the direction onto the horizontal plane to ignore vertical differences
        directionToEnemy.y = 0;

        // Find the forward vector of the player in the horizontal plane
        Vector3 playerForward = player.forward;
        playerForward.y = 0;

        // Calculate the angle between the player's forward direction and the direction to the enemy
        float angle = Vector3.Angle(playerForward, directionToEnemy);

        // Determine if the enemy is to the left or right of the player
        float crossProductY = Vector3.Cross(playerForward, directionToEnemy).y;
        if (crossProductY < 0)
        {
            angle = -angle;
        }

        return angle;
    }
}



