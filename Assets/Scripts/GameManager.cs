using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private string gameOverScene = "SceneManager"; // Scene to load on game over
    [SerializeField] private float scoreMultiplier = 10f; // Points per second
    
    private float gameStartTime;
    private float currentScore;
    private bool isGameOver = false;
    
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        gameStartTime = Time.time;
        Debug.Log("Game started! Score tracking begins.");
    }
    
    void Update()
    {
        if (!isGameOver)
        {
            // Update score based on survival time
            currentScore = (Time.time - gameStartTime) * scoreMultiplier;
        }
    }
    
    /// <summary>
    /// Call this when player collides with enemy - triggers game over
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return; // Prevent multiple game overs
        
        isGameOver = true;
        float finalScore = currentScore;
        
        Debug.Log($"Game Over! Final Score: {finalScore:F0}");
        
        // Save high score
        SaveHighScore(finalScore);
        
        // Load SceneManager scene
        Debug.Log($"Loading scene: {gameOverScene}");
        Invoke(nameof(LoadGameOverScene), 2f); // Wait 2 seconds then load scene
    }
    
    /// <summary>
    /// Save the score if it's a new high score
    /// </summary>
    /// <param name="score">The score to potentially save</param>
    private void SaveHighScore(float score)
    {
        float currentHighScore = PlayerPrefs.GetFloat("HighScore", 0f);
        
        if (score > currentHighScore)
        {
            PlayerPrefs.SetFloat("HighScore", score);
            PlayerPrefs.Save();
            Debug.Log($"New High Score! {score:F0} (Previous: {currentHighScore:F0})");
        }
        else
        {
            Debug.Log($"Score: {score:F0} (High Score: {currentHighScore:F0})");
        }
    }
    
    /// <summary>
    /// Get the current high score
    /// </summary>
    /// <returns>The high score from PlayerPrefs</returns>
    public float GetHighScore()
    {
        return PlayerPrefs.GetFloat("HighScore", 0f);
    }
    
    /// <summary>
    /// Get the current score
    /// </summary>
    /// <returns>Current game score</returns>
    public float GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// Get the game start time
    /// </summary>
    /// <returns>Time when the game started</returns>
    public float GetGameStartTime()
    {
        return gameStartTime;
    }
    
    /// <summary>
    /// Get the current survival time in seconds
    /// </summary>
    /// <returns>How long the player has survived</returns>
    public float GetSurvivalTime()
    {
        return Time.time - gameStartTime;
    }
    
    /// <summary>
    /// Load the game over scene (SceneManager)
    /// </summary>
    private void LoadGameOverScene()
    {
        Time.timeScale = 1f; // Ensure time scale is normal
        
        try
        {
            SceneManager.LoadScene(gameOverScene);
            Debug.Log($"Successfully loaded scene: {gameOverScene}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{gameOverScene}': {e.Message}");
            Debug.LogWarning("Attempting to restart current scene instead...");
            RestartGame();
        }
    }
    
    /// <summary>
    /// Restart the current scene (fallback option)
    /// </summary>
    private void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time scale is normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Clear the high score (for testing)
    /// </summary>
    [ContextMenu("Clear High Score")]
    public void ClearHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        Debug.Log("High score cleared!");
    }
    
    /// <summary>
    /// Reset the game state - used when returning to menu
    /// </summary>
    public void ResetGameState()
    {
        currentScore = 0f;
        isGameOver = false;
        gameStartTime = Time.time;
        Time.timeScale = 1f; // Ensure time scale is normal
        
        Debug.Log("Game state reset!");
    }
}
