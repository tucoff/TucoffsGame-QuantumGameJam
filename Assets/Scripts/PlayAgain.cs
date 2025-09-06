using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayAgain : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Scene Settings")]
    [SerializeField] private string menuSceneName = "Menu";
    
    void Start()
    {
        UpdateScoreDisplay();
    }
    
    /// <summary>
    /// Updates the score and high score display texts
    /// </summary>
    private void UpdateScoreDisplay()
    {
        // Get the current score from GameManager if it exists
        float currentScore = 0f;
        if (GameManager.Instance != null)
        {
            currentScore = GameManager.Instance.GetCurrentScore();
        }
        
        // Get the high score from PlayerPrefs
        float highScore = PlayerPrefs.GetFloat("HighScore", 0f);
        
        // Update the UI texts if they are assigned
        if (scoreText != null)
        {
            scoreText.text = $" SCORE: {currentScore:F0}";
        }
        
        if (highScoreText != null)
        {
            highScoreText.text = $"HIGH SCORE: {highScore:F0}";
        }
    }
    
    /// <summary>
    /// Public function to load the menu scene - can be called from UI buttons
    /// </summary>
    public void LoadMenuScene()
    {
        try
        {
            // Reset the game state in GameManager if it exists
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGameState();
                Debug.Log("Game state reset before loading menu");
            }
            
            Time.timeScale = 1f; // Ensure time scale is normal
            SceneManager.LoadScene(menuSceneName);
            Debug.Log($"Loading menu scene: {menuSceneName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load menu scene '{menuSceneName}': {e.Message}");
        }
    }
}
