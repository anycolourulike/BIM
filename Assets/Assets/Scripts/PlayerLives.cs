using UnityEngine;
using System;

public class PlayerLives : MonoBehaviour
{
    [SerializeField] private int startingLives = 10;

    public int CurrentLives { get; set; }
    public bool IsAlive => CurrentLives > 0;

    // Event: broadcast when lives change
    public event Action<int> OnLivesChanged;

    // Event: broadcast when player dies
    public event Action OnPlayerDied;

    private void Awake()
    {
        CurrentLives = startingLives;
    }

    public void LoseLife()
    {
        if (CurrentLives <= 0)
            return;

        CurrentLives--;
        OnLivesChanged?.Invoke(CurrentLives);

        if (CurrentLives <= 0)
            OnPlayerDied?.Invoke();
    }

    public void AddLife(int amount = 1)
    {
        CurrentLives += amount;
        OnLivesChanged?.Invoke(CurrentLives);
    }
}