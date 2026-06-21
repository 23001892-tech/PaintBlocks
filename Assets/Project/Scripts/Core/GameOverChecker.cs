using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverChecker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public bool HasAnyValidMove(List<BlockData> remainingBlocks)
    {
        if (isGameOver)
            return false;

        if (gridManager == null)
        {
            Debug.LogError("GameOverChecker: Chưa có GridManager.");
            return false;
        }

        if (remainingBlocks == null || remainingBlocks.Count == 0)
        {
            return true;
        }

        foreach (BlockData blockData in remainingBlocks)
        {
            if (blockData == null)
                continue;

            if (gridManager.HasValidPlacement(blockData))
            {
                return true;
            }
        }

        return false;
    }

    public void CheckGameOver(List<BlockData> remainingBlocks)
    {
        if (isGameOver)
            return;

        bool hasMove = HasAnyValidMove(remainingBlocks);

        if (!hasMove)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("GameOverChecker: GAME OVER.");
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}