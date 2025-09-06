using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnDistance = 10f;
    [SerializeField] private float initialSpawnInterval = 3f;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float initialDelay = 30f; // Wait 30 seconds before starting to spawn
    
    [Header("Speed Increase Settings")]
    [SerializeField] private float speedIncreaseInterval = 30f; // Every 30 seconds
    [SerializeField] private float speedMultiplier = 0.8f; // Makes spawning 20% faster each time
    [SerializeField] private float minimumSpawnInterval = 0.5f; // Don't go faster than this
    
    [Header("Target Selection")]
    private GameObject[] players;
    private GameObject selectedTargetPlayer;
    
    private int currentEnemyCount = 0;
    private float currentSpawnInterval;
    private float gameStartTime;
    
    void Start()
    {
        // Initialize spawn settings
        currentSpawnInterval = initialSpawnInterval;
        gameStartTime = Time.time;
        
        // Find all players with "Player" tag
        players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players.Length == 0)
        {
            Debug.LogWarning("No players found with 'Player' tag!");
            return;
        }
        
        // Start spawning enemies and speed increase system
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpeedIncreaseRoutine());
    }
    
    /// <summary>
    /// Coroutine that increases spawn speed every 30 seconds
    /// </summary>
    IEnumerator SpeedIncreaseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(speedIncreaseInterval);
            
            // Increase spawn speed (reduce interval)
            float previousInterval = currentSpawnInterval;
            currentSpawnInterval *= speedMultiplier;
            
            // Don't go below minimum
            currentSpawnInterval = Mathf.Max(currentSpawnInterval, minimumSpawnInterval);
            
            Debug.Log($"Spawn speed increased! Interval: {previousInterval:F2}s → {currentSpawnInterval:F2}s (after {Time.time - gameStartTime:F0} seconds)");
        }
    }
    
    IEnumerator SpawnEnemyRoutine()
    {
        // Wait for initial delay before starting to spawn enemies
        Debug.Log($"Waiting {initialDelay} seconds before starting enemy spawning...");
        yield return new WaitForSeconds(initialDelay);
        Debug.Log("Enemy spawning begins!");
        
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval); // Use dynamic interval
            
            if (currentEnemyCount < maxEnemies && players.Length > 0)
            {
                SpawnEnemy();
            }
        }
    }
    
    void SpawnEnemy()
    {
        // Randomly select a target player
        selectedTargetPlayer = players[Random.Range(0, players.Length)];
        
        if (selectedTargetPlayer == null)
        {
            Debug.LogWarning("Selected target player is null!");
            return;
        }
        
        Vector3 playerPosition = selectedTargetPlayer.transform.position;
        
        // Generate random spawn position with constraints:
        // - Spawn at any position around player within a circular radius
        // - Maintain minimum distance from player (radius)
        // - Y same as player
        
        float minDistance = 30f;
        float effectiveSpawnDistance = Mathf.Max(spawnDistance, minDistance);
        
        // Generate a random angle for circular spawning (0 to 360 degrees)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Generate a random radius between minimum distance and effective spawn distance
        float randomRadius = Random.Range(minDistance, effectiveSpawnDistance);
        
        // Calculate X and Z offsets using circular coordinates
        float xOffset = Mathf.Cos(randomAngle) * randomRadius;
        float zOffset = Mathf.Sin(randomAngle) * randomRadius;
        
        Vector3 spawnPosition = new Vector3(
            playerPosition.x + xOffset,
            playerPosition.y,
            playerPosition.z + zOffset // Z can now be any direction (positive or negative)
        );
        
        Debug.Log($"Spawn calculation: Angle={randomAngle * Mathf.Rad2Deg:F0}°, Radius={randomRadius:F2}, X offset={xOffset:F2}, Z offset={zOffset:F2}");
        
        // Instantiate the enemy
        if (enemyPrefab != null)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Get the EnemyBeamRotation component and set the target player
            EnemyBeamRotation enemyBeam = spawnedEnemy.GetComponent<EnemyBeamRotation>();
            if (enemyBeam != null)
            {
                // Set the enemy to follow the selected target player
                enemyBeam.SetTargetPlayer(selectedTargetPlayer.transform);
            }
            
            // Track enemy count
            currentEnemyCount++;
            
            // Subscribe to enemy defeat/conversion to update count
            EnemyDestroyNotifier destroyNotifier = spawnedEnemy.GetComponent<EnemyDestroyNotifier>();
            if (destroyNotifier == null)
            {
                destroyNotifier = spawnedEnemy.AddComponent<EnemyDestroyNotifier>();
            }
            destroyNotifier.OnEnemyConverted += OnEnemyConverted;
            
            Debug.Log($"Enemy spawned at {spawnPosition} targeting player at {playerPosition}");
        }
        else
        {
            Debug.LogError("Enemy prefab is not assigned!");
        }
    }
    
    private void OnEnemyConverted()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount); // Ensure it doesn't go below 0
        Debug.Log($"Enemy converted to player. Current enemy count: {currentEnemyCount}");
    }
    
    void Update()
    {
        // Refresh players list periodically in case new players are added/removed
        if (Time.frameCount % 120 == 0) // Check every 2 seconds at 60 FPS
        {
            players = GameObject.FindGameObjectsWithTag("Player");
        }
    }
}
