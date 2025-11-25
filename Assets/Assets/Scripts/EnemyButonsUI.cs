using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyButtonsUI : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyButtons;  // 4 buttons
    [SerializeField] private float radius = 150f;        // UI circle radius
    [SerializeField] private RectTransform canvasRect;   // parent canvas
    [SerializeField] private Transform player;           // world-space player

    private readonly List<Transform> activeEnemies = new List<Transform>();

    void Awake()
    {
        // Ensure all buttons are hidden at the start
        foreach (var btn in enemyButtons)
            btn.SetActive(false);
    }

    public void SetEnemies(List<Transform> enemies)
    {
        activeEnemies.Clear();
        activeEnemies.AddRange(enemies);

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        for (int i = 0; i < enemyButtons.Length; i++)
        {
            if (i < activeEnemies.Count)
            {
                enemyButtons[i].SetActive(true);
                PositionButton(enemyButtons[i].GetComponent<RectTransform>(), activeEnemies[i]);
            }
            else
            {
                enemyButtons[i].SetActive(false);
            }
        }
    }

    private void PositionButton(RectTransform button, Transform enemy)
    {
        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.position);

        // Convert screen → canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 canvasPos
        );

        // Convert canvas position → vector relative to player
        Vector2 direction = (canvasPos - (Vector2)player.position).normalized;

        // Place on circle
        button.anchoredPosition = (Vector2)player.position + direction * radius;
    }
}