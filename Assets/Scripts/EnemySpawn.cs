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
    
    [Header("Enemy Movement Speed Settings")]
    [SerializeField] private float baseMoveSpeed = 5f; // Base movement speed for enemies
    [SerializeField] private float moveSpeedIncreaseInterval = 20f; // Every 20 seconds
    [SerializeField] private float moveSpeedIncreaseAmount = 0.5f; // Add 0.5 speed each time
    [SerializeField] private float maxMoveSpeed = 15f; // Maximum movement speed
    
    [Header("Target Selection")]
    private GameObject[] players;
    private GameObject selectedTargetPlayer;
    
    private int currentEnemyCount = 0;
    private float currentSpawnInterval;
    private float currentMoveSpeed;
    private float gameStartTime;
    private bool cutsceneSkipped = false;
    private bool spawningActive = false;
    private Coroutine spawnRoutineCoroutine;
    private Coroutine delayCoroutine;
    
    void Start()
    {
        // Initialize spawn settings
        currentSpawnInterval = initialSpawnInterval;
        currentMoveSpeed = baseMoveSpeed;
        gameStartTime = Time.time;
        
        // Find all players with "Player" tag
        players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players.Length == 0)
        {
            Debug.LogWarning("No players found with 'Player' tag!");
            return;
        }
        
        // Start delay routine and speed increase systems
        delayCoroutine = StartCoroutine(InitialDelayRoutine());
        StartCoroutine(SpeedIncreaseRoutine());
        StartCoroutine(MoveSpeedIncreaseRoutine());
    }
    
    /// <summary>
    /// Initial delay routine - handles the 30 second wait
    /// </summary>
    IEnumerator InitialDelayRoutine()
    {
        Debug.Log($"Waiting {initialDelay} seconds before starting enemy spawning...");
        yield return new WaitForSeconds(initialDelay);
        
        // Only start spawning if cutscene wasn't skipped
        if (!cutsceneSkipped)
        {
            StartSpawning();
        }
    }
    
    /// <summary>
    /// Start the enemy spawning system
    /// </summary>
    void StartSpawning()
    {
        if (!spawningActive)
        {
            spawningActive = true;
            spawnRoutineCoroutine = StartCoroutine(SpawnEnemyLoop());
            Debug.Log("Enemy spawning begins!");
        }
    }
    
    /// <summary>
    /// Public function to skip cutscene and start spawning enemies immediately
    /// </summary>
    public void SkipCutscene()
    {
        if (!cutsceneSkipped && !spawningActive)
        {
            cutsceneSkipped = true;
            
            // Stop the delay routine
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
                Debug.Log("Initial delay stopped!");
            }
            
            // Reset game start time for speed increase system
            gameStartTime = Time.time;
            
            // Start spawning immediately
            StartSpawning();
            Debug.Log("Cutscene skipped! Enemy spawning starts immediately.");
        }
        else if (spawningActive)
        {
            Debug.Log("Spawning is already active!");
        }
        else
        {
            Debug.Log("Cutscene already skipped!");
        }
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
    
    /// <summary>
    /// Coroutine that increases enemy movement speed over time
    /// </summary>
    IEnumerator MoveSpeedIncreaseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveSpeedIncreaseInterval);
            
            // Increase movement speed
            float previousSpeed = currentMoveSpeed;
            currentMoveSpeed += moveSpeedIncreaseAmount;
            
            // Don't exceed maximum
            currentMoveSpeed = Mathf.Min(currentMoveSpeed, maxMoveSpeed);
            
            Debug.Log($"Enemy movement speed increased! Speed: {previousSpeed:F1} → {currentMoveSpeed:F1} (after {Time.time - gameStartTime:F0} seconds)");
            
            // Update speed of all existing enemies
            UpdateExistingEnemySpeeds();
        }
    }
    
    /// <summary>
    /// Main enemy spawning loop - runs continuously once started
    /// </summary>
    IEnumerator SpawnEnemyLoop()
    {
        // Spawn first enemy immediately
        if (currentEnemyCount < maxEnemies && players.Length > 0)
        {
            Debug.Log("Spawning first enemy immediately...");
            SpawnEnemy();
        }
        
        // Continue spawning at intervals
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            
            Debug.Log($"Spawn check: Enemies {currentEnemyCount}/{maxEnemies}, Players: {players.Length}, Interval: {currentSpawnInterval:F2}s");
            
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
        // - Maintain minimum distance from player (using spawnDistance as minimum)
        // - Y same as player
        
        // Use spawnDistance as the minimum distance (respects inspector setting)
        float minDistance = spawnDistance; // Now respects the configured spawnDistance
        float maxDistance = spawnDistance + 20f; // Add some variation (20 units beyond minimum)
        
        // Generate a random angle for circular spawning (0 to 360 degrees)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Generate a random radius between minimum distance and max distance
        float randomRadius = Random.Range(minDistance, maxDistance);
        
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
            
            // Fix concave mesh collider issues on spawned enemy
            FixEnemyMeshColliders(spawnedEnemy);
            
            // Get the EnemyBeamRotation component and set the target player + movement speed
            EnemyBeamRotation enemyBeam = spawnedEnemy.GetComponent<EnemyBeamRotation>();
            if (enemyBeam != null)
            {
                // Set the enemy to follow the selected target player
                enemyBeam.SetTargetPlayer(selectedTargetPlayer.transform);
                
                // Set current movement speed
                enemyBeam.SetMoveSpeed(currentMoveSpeed);
                Debug.Log($"Enemy spawned with move speed: {currentMoveSpeed:F1}");
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
    
    /// <summary>
    /// Fix concave mesh collider issues on enemy and its children
    /// </summary>
    /// <param name="enemy">The enemy GameObject to fix</param>
    private void FixEnemyMeshColliders(GameObject enemy)
    {
        // Fix mesh colliders recursively on enemy and all children
        FixMeshCollidersRecursive(enemy.transform);
    }
    
    /// <summary>
    /// Recursively fix mesh collider issues
    /// </summary>
    /// <param name="parent">Parent transform to check</param>
    private void FixMeshCollidersRecursive(Transform parent)
    {
        // Check current object
        MeshCollider meshCollider = parent.GetComponent<MeshCollider>();
        Rigidbody rigidbody = parent.GetComponent<Rigidbody>();
        
        if (meshCollider != null && rigidbody != null && !rigidbody.isKinematic)
        {
            // If there's a non-kinematic rigidbody and a mesh collider, make the mesh collider convex
            if (!meshCollider.convex)
            {
                meshCollider.convex = true;
                Debug.Log($"Fixed concave mesh collider on {parent.name} - made convex for dynamic rigidbody");
            }
        }
        
        // Recursively check all children
        for (int i = 0; i < parent.childCount; i++)
        {
            FixMeshCollidersRecursive(parent.GetChild(i));
        }
    }
    
    /// <summary>
    /// Update movement speed of all existing enemies in the scene
    /// </summary>
    private void UpdateExistingEnemySpeeds()
    {
        // Find all enemies in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int updatedCount = 0;
        
        foreach (GameObject enemy in enemies)
        {
            EnemyBeamRotation enemyBeam = enemy.GetComponent<EnemyBeamRotation>();
            if (enemyBeam != null)
            {
                enemyBeam.SetMoveSpeed(currentMoveSpeed);
                updatedCount++;
            }
        }
        
        Debug.Log($"Updated movement speed for {updatedCount} existing enemies to {currentMoveSpeed:F1}");
    }
    
    void Update()
    {
        // Refresh players list periodically in case new players are added/removed
        if (Time.frameCount % 120 == 0) // Check every 2 seconds at 60 FPS
        {
            players = GameObject.FindGameObjectsWithTag("Player");
        }
    }
    
    /// <summary>
    /// Debug method to check spawn system status including movement speed
    /// </summary>
    [ContextMenu("Debug Spawn Status")]
    public void DebugSpawnStatus()
    {
        Debug.Log($"=== ENEMY SPAWN DEBUG ===");
        Debug.Log($"Cutscene Skipped: {cutsceneSkipped}");
        Debug.Log($"Spawning Active: {spawningActive}");
        Debug.Log($"Current Enemy Count: {currentEnemyCount}/{maxEnemies}");
        Debug.Log($"Players Found: {players.Length}");
        Debug.Log($"Spawn Interval: {currentSpawnInterval:F2}s");
        Debug.Log($"Enemy Move Speed: {currentMoveSpeed:F1} (Base: {baseMoveSpeed:F1}, Max: {maxMoveSpeed:F1})");
        Debug.Log($"Delay Routine Running: {delayCoroutine != null}");
        Debug.Log($"Spawn Routine Running: {spawnRoutineCoroutine != null}");
        Debug.Log($"Enemy Prefab Assigned: {enemyPrefab != null}");
        Debug.Log($"Initial Delay: {initialDelay}s");
        Debug.Log($"Time since game start: {Time.time - gameStartTime:F1}s");
        if (players.Length > 0)
        {
            Debug.Log($"First Player Position: {players[0].transform.position}");
        }
        Debug.Log($"=========================");
    }
    
    /// <summary>
    /// Force spawn an enemy for testing
    /// </summary>
    [ContextMenu("Force Spawn Enemy")]
    public void ForceSpawnEnemy()
    {
        if (players.Length > 0)
        {
            SpawnEnemy();
        }
        else
        {
            Debug.LogWarning("Cannot force spawn - no players found!");
        }
    }
    
}
