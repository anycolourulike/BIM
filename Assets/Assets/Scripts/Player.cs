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
    [SerializeField] PlayerFollow PlayerCam;
    [SerializeField] Animation pickupAnim; // optional: assign the pickup Animation directly
    GameObject pauseButton;
    Animation PUAnim;

    private bool _isDead;
    private int  _textToken; // bumped whenever new on-screen text is shown, so stale coroutines don't hide it


    public RectTransform enemyUI; // Reference to the Enemy UI element
    public RectTransform centerUI; // Reference to the Center UI element
    public float distanceFromCenter = 250f; // Fixed distance from the center    

    public float doorOpenTimer;
    public delegate void PlayerDead();
    public static event PlayerDead playerHasDied;

    private void OnEnable()
    {
        playerHasDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        playerHasDied -= HandlePlayerDeath;
    }

    private void Start()
    {
        PUAnim = pickupAnim != null ? pickupAnim : FindObjectOfType<Animation>();
        pauseButton = GameObject.FindWithTag("Pause");
    }
    
    
    //Add PickUp
    //Add Draw Weapon
    //Add Ranged Weapon
    //Inventory
    //Add Dive

    //Find and add enemies. Find Enemy manager Object. Called By a trigger. On Exit the Fight area the list clears
    //Populate Buttons with correct UI and rotation.

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
        yield return ShowTimedText("+15 EXTRA SECONDS", 7f);
    }

    public IEnumerator ShowTEXT30()
    {
        yield return ShowTimedText("+30 EXTRA SECONDS", 7f);
    }

    private IEnumerator ShowTimedText(string message, float seconds)
    {
        if (text == null) yield break;

        int token = ++_textToken;
        text.enabled = true;
        text.SetText(message);

        yield return new WaitForSeconds(seconds);

        if (_textToken == token) // a newer message hasn't replaced ours
            text.enabled = false;
    }

    public IEnumerator DoorOpenedTimer()
    {
        if (text == null) yield break;

        int token = ++_textToken;
        text.enabled = true;
        text.SetText("Door Open");
        yield return new WaitForSeconds(Mathf.Max(0f, doorOpenTimer));

        if (PUAnim != null) PUAnim.Play("PUAnim");
        yield return new WaitForSeconds(5f);
        if (PUAnim != null) PUAnim.Stop("PUAnim");

        if (_textToken == token)
            text.enabled = false;
    }

    public void DisableText()
    {
        _textToken++; // invalidate any running text coroutine
        if (text != null)
            text.enabled = false;
    }

    public void OutOfTime()
    {
        onDeath?.Invoke(); //play audio
        playerHasDied?.Invoke(); // Pause Time & handle Player Death

        if (PlayerDeath != null && player != null)
            Instantiate(PlayerDeath, player.transform.position, Quaternion.identity);
    }

    public void PlayerComplete()
    {
        if (PlayerCompleteLevel != null && player != null)
            Instantiate(PlayerCompleteLevel, player.transform.position, Quaternion.identity);
    }

    public void HandlePlayerDeath()
    {
        if (_isDead) return; // several call sites + the static event can all fire this
        _isDead = true;

        gameObject.tag = "Respawn";

        if (player != null)
        {
            var rend = player.GetComponentInChildren<Renderer>(); // Renderer covers Skinned meshes too
            if (rend != null) rend.enabled = false;

            var col = player.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnPlayerDeath();
            SaveManager.Instance.Save();
        }

        if (pauseButton != null)
            pauseButton.SetActive(false);

        int livesLeft = -1;
        if (GameManager.Instance != null && GameManager.Instance.playerLives != null)
            livesLeft = GameManager.Instance.playerLives.CurrentLives;

        ShowDeathDialog(livesLeft);
    }

    private void ShowDeathDialog(int livesLeft)
    {
        if (DialogUI.Instance == null)
        {
            Debug.Log("Player Died");
            return;
        }

        if (livesLeft == 0)
        {
            DialogUI.Instance
                .SetTitle("Game Over")
                .SetMessage("Puny Human!")
                .OnClose(LevelManager.loadMenu)
                .Show();
        }
        else if (livesLeft > 0 && livesLeft % 3 == 0)
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



