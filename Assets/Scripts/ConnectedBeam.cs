using UnityEngine;
using System.Linq;

public class ConnectedBeam : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float growSpeed = 50f; // Very fast growth speed for almost instantaneous growth
    [SerializeField] private Transform parentPlayer;
    
    private Transform targetPlayer;
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
        
        // Find the closest other player to connect to
        FindClosestPlayer();
        
        // Initialize the beam
        InitializeBeam();
    }
    
    void Update()
    {
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
    /// Initialize beam properties
    /// </summary>
    private void InitializeBeam()
    {
        if (parentPlayer == null || targetPlayer == null)
            return;
            
        // Start with minimal scale
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 0.01f);
        
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
        if (parentPlayer == null || targetPlayer == null)
            return;
            
        // Position at medium point between players
        transform.position = targetPosition;
        
        // Rotate to face from parent to target player (Z axis pointing toward target)
        Vector3 direction = (targetPlayer.position - parentPlayer.position).normalized;
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
