using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] public PlayerLives playerLives;
    [SerializeField] public int levelsCompleted = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Hook events
        playerLives.OnLivesChanged += HandleLivesChanged;
        playerLives.OnPlayerDied += HandlePlayerDeath;

        // Initial UI sync
        UIManager.Instance.UpdateLivesUI(playerLives.CurrentLives);
    }

    private void HandleLivesChanged(int newLives)
    {
        UIManager.Instance.UpdateLivesUI(newLives);
    }

    private void HandlePlayerDeath()
    {
        UIManager.Instance.ShowDialogue("You Died", "Try again?");
        // Additional logic here (retry, game over menu, etc.)
    }
}

