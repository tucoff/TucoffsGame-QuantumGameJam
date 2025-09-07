using UnityEngine;
using System.Linq;

public class ConnectedBeam : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float growSpeed = 50f; // Very fast growth speed for almost instantaneous growth
    [SerializeField] private Transform parentPlayer;
    [SerializeField] private bool createRandomBeam = true; // Enable random beam creation
    [SerializeField] private float randomBeamDistance = 50f; // Minimum distance for random beam
    
    private Transform targetPlayer;
    private Vector3 randomTargetPoint; // For random beam targeting
    private bool useRandomTarget = false; // Flag to determine if using random target or player target
    private bool isGrowing = true;
    private float targetDistance;
    private Vector3 targetPosition;
    private Vector3 targetScale;
    
    void Start()
    {
        // If parent player is not assigned, try to find it from parent object
        if (parentPlayer == null)
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.CompareTag("Player"))
                {
                    parentPlayer = parent;
                    break;
                }
                parent = parent.parent;
            }
        }

        // Create duplicate beam that points to random position
        if (createRandomBeam)
        {
            CreateRandomBeam();
        }

        // Find the closest other player to connect to
        FindClosestPlayer();
        
        // Initialize the beam
        InitializeBeam();
    }
    
    void Update()
    {
        // Handle random target beam
        if (useRandomTarget && parentPlayer != null)
        {
            UpdateRandomTargetCalculations();
            
            // Handle beam growth
            if (isGrowing)
            {
                GrowBeam();
            }
            
            // Always keep beam positioned and oriented correctly
            UpdateBeamTransform();
            return;
        }
        
        // Handle regular player-to-player beam
        if (parentPlayer == null || targetPlayer == null)
            return;
            
        // Update target calculations in case players moved
        UpdateTargetCalculations();
        
        // Handle beam growth
        if (isGrowing)
        {
            GrowBeam();
        }
        
        // Always keep beam positioned and oriented correctly
        UpdateBeamTransform();
    }
    
    /// <summary>
    /// Find the closest player to connect the beam to
    /// </summary>
    private void FindClosestPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        if (allPlayers.Length < 2)
        {
            Debug.LogWarning("Need at least 2 players for beam connection!");
            return;
        }
        
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;
        
        foreach (GameObject player in allPlayers)
        {
            // Skip if it's the parent player
            if (player.transform == parentPlayer)
                continue;
                
            float distance = Vector3.Distance(parentPlayer.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }
        
        targetPlayer = closestPlayer;
        
        if (targetPlayer != null)
        {
            Debug.Log($"ConnectedBeam connecting {parentPlayer.name} to {targetPlayer.name}");
        }
    }

    /// <summary>
    /// Create a duplicate beam that points to a random distant position
    /// </summary>
    private void CreateRandomBeam()
    {
        if (parentPlayer == null) return;

        // Step 1: Generate random point with guaranteed 1000+ distance
        Vector3 randomPoint = GenerateRandomDistantPoint();

        // Create duplicate of this beam
        GameObject duplicateBeam = Instantiate(gameObject, transform.parent);
        duplicateBeam.name = gameObject.name + "_RandomBeam";
        
        ConnectedBeam duplicateScript = duplicateBeam.GetComponent<ConnectedBeam>();
        if (duplicateScript != null)
        {
            // Configure the duplicate to use random target
            duplicateScript.parentPlayer = parentPlayer;
            duplicateScript.randomTargetPoint = randomPoint;
            duplicateScript.useRandomTarget = true;
            duplicateScript.createRandomBeam = false; // Prevent recursive creation
            
            Debug.Log($"Created random beam from {parentPlayer.name} to point {randomPoint}");
        }
    }

    /// <summary>
    /// Generate a random point that is at least randomBeamDistance away from parentPlayer
    /// </summary>
    /// <returns>Random distant point</returns>
    private Vector3 GenerateRandomDistantPoint()
    {
        Vector3 playerPos = parentPlayer.position;
        Vector3 randomPoint;
        
        // Generate random direction
        Vector3 randomDirection = Random.onUnitSphere;
        // Allow both higher and lower Y positions (no restriction on Y axis)
        
        // Place point at guaranteed distance
        randomPoint = playerPos + (randomDirection * randomBeamDistance);
        
        Debug.Log($"Generated random point at distance {Vector3.Distance(playerPos, randomPoint):F1} from {parentPlayer.name}");
        return randomPoint;
    }
    
    /// <summary>
    /// Initialize beam properties
    /// </summary>
    private void InitializeBeam()
    {
        if (parentPlayer == null || targetPlayer == null)
            return;
            
        // Start with minimal scale
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 0.01f);
        
        // Add glow effect to make the beam bright
        BeamGlowEffect glowEffect = GetComponent<BeamGlowEffect>();
        if (glowEffect == null)
        {
            glowEffect = gameObject.AddComponent<BeamGlowEffect>();
        }
        
        UpdateTargetCalculations();
    }
    
    /// <summary>
    /// Update target position and distance calculations
    /// </summary>
    private void UpdateTargetCalculations()
    {
        if (parentPlayer == null || targetPlayer == null)
            return;
            
        // Calculate medium point between parent and target player
        targetPosition = (parentPlayer.position + targetPlayer.position) * 0.5f;
        
        // Calculate distance between players
        targetDistance = Vector3.Distance(parentPlayer.position, targetPlayer.position);
        
        // Target scale should match the distance
        targetScale = new Vector3(transform.localScale.x, transform.localScale.y, targetDistance);
    }

    /// <summary>
    /// Update target position and distance calculations for random target
    /// </summary>
    private void UpdateRandomTargetCalculations()
    {
        if (parentPlayer == null)
            return;
            
        // Step 2: Position beam at medium point between parentPlayer and randomTargetPoint
        targetPosition = (parentPlayer.position + randomTargetPoint) * 0.5f;
        
        // Step 3: Calculate distance for growing (GrowBeam will handle the scaling)
        targetDistance = Vector3.Distance(parentPlayer.position, randomTargetPoint);
        
        // Target scale should match the distance
        targetScale = new Vector3(transform.localScale.x, transform.localScale.y, targetDistance);
    }
    
    /// <summary>
    /// Handle beam growth animation
    /// </summary>
    private void GrowBeam()
    {
        // Grow the Z scale towards target distance
        float currentZScale = transform.localScale.z;
        float newZScale = Mathf.MoveTowards(currentZScale, targetDistance, growSpeed * Time.deltaTime);
        
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newZScale);
        
        // Check if growth is complete
        if (Mathf.Approximately(newZScale, targetDistance))
        {
            isGrowing = false;
            Debug.Log($"Beam growth complete. Final scale Z: {newZScale}");
        }
    }
    
    /// <summary>
    /// Update beam position and rotation
    /// </summary>
    private void UpdateBeamTransform()
    {
        if (parentPlayer == null)
            return;
            
        // Position at medium point
        transform.position = targetPosition;
        
        Vector3 direction;
        
        if (useRandomTarget)
        {
            // Rotate to face from parent to random point (Z axis pointing toward target)
            direction = (randomTargetPoint - parentPlayer.position).normalized;
        }
        else
        {
            if (targetPlayer == null) return;
            // Rotate to face from parent to target player (Z axis pointing toward target)
            direction = (targetPlayer.position - parentPlayer.position).normalized;
        }
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    /// <summary>
    /// Force immediate growth completion (for testing or instant setup)
    /// </summary>
    public void CompleteGrowthInstantly()
    {
        if (parentPlayer == null || targetPlayer == null)
            return;
            
        UpdateTargetCalculations();
        transform.localScale = targetScale;
        isGrowing = false;
        UpdateBeamTransform();
    }
    
    /// <summary>
    /// Set a specific parent player for this beam
    /// </summary>
    /// <param name="parent">The parent player transform</param>
    public void SetParentPlayer(Transform parent)
    {
        parentPlayer = parent;
        FindClosestPlayer();
        InitializeBeam();
    }
    
    /// <summary>
    /// Set a specific target player for this beam
    /// </summary>
    /// <param name="target">The target player transform</param>
    public void SetTargetPlayer(Transform target)
    {
        targetPlayer = target;
        InitializeBeam();
    }
}
