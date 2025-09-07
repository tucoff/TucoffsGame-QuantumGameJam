using UnityEngine;
using System.Collections;

public class EnemyBeamRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    private Vector3 randomRotationAxis;
    private float rotationSpeed;
    private bool isRotating = false;
    
    [Header("Player Following Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Defeat Settings")]
    [SerializeField] private float defeatWaitTime = 3f;
    [SerializeField] private Transform childToActivate; // The child object to activate after defeat
    
    [Header("Death/Conversion Visual Settings")]
    [SerializeField] private Material happyMaterial; // Material to apply when enemy dies/converts
    [SerializeField] private string planeChildName = "Plane"; // Name of the plane child to find
    
    private Transform player;
    private bool isDead = false;
    private bool isDefeated = false;
    private bool isConverted = false; // New flag to stop all movement after conversion
    private Coroutine defeatCoroutine;
    
    void Start()
    {
        // Find the player object (this will be overridden if SetTargetPlayer is called)
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            // Fallback: try to find by name
            playerObject = GameObject.Find("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }
        
        // Fix mesh collider issues on this enemy
        FixMeshColliderIssues();
        
        // Ensure child objects start inactive
        EnsureChildrenAreInactive();
    }
    
    /// <summary>
    /// Ensure all child objects are inactive at start
    /// </summary>
    private void EnsureChildrenAreInactive()
    {
        if (childToActivate != null)
        {
            childToActivate.gameObject.SetActive(false);
        }
        else
        {
            // First, find and keep reference to the Plane child
            Transform planeChild = transform.Find("Plane");
            
            // Deactivate all children to ensure they're not active before defeat sequence
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            
            // Reactivate the Plane child and orient it towards the player
            if (planeChild != null)
            {
                planeChild.gameObject.SetActive(true);
                if (player != null)
                {
                    Vector3 directionToPlayer = (player.position - planeChild.position).normalized;
                    if (directionToPlayer != Vector3.zero)
                    {
                        planeChild.rotation = Quaternion.LookRotation(directionToPlayer);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Fix mesh collider issues on this enemy and its children (specifically for Plane/PlaneFace)
    /// </summary>
    private void FixMeshColliderIssues()
    {
        // Fix mesh colliders on this object and all children recursively
        FixMeshCollidersRecursive(transform);
    }
    
    /// <summary>
    /// Recursively fix mesh collider issues
    /// </summary>
    /// <param name="parent">Parent transform to check</param>
    private void FixMeshCollidersRecursive(Transform parent)
    {
        // Check current object for mesh collider issues
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
    /// Set a specific player as the target for this enemy
    /// </summary>
    /// <param name="targetPlayer">The transform of the target player</param>
    public void SetTargetPlayer(Transform targetPlayer)
    {
        player = targetPlayer;
        Debug.Log($"Enemy {gameObject.name} now targeting player: {targetPlayer.name}");
    }
    
    /// <summary>
    /// Set the movement speed of this enemy
    /// </summary>
    /// <param name="speed">The new movement speed</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void Initialize(float speed)
    {
        rotationSpeed = speed;
        // Generate a random rotation axis
        randomRotationAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        isRotating = true;
        isDead = true; // Enemy is considered dead when rotating
        isDefeated = true; // Mark as defeated
        
        // Activate visual effects when enemy is hit
        ActivateDeathEffects();
        
        // Start the defeat sequence
        if (defeatCoroutine != null)
        {
            StopCoroutine(defeatCoroutine);
        }
        defeatCoroutine = StartCoroutine(HandleDefeatSequence());
    }
    
    /// <summary>
    /// Handles the defeat sequence: rotate for 3 seconds, then stop ALL movement and activate child
    /// </summary>
    private IEnumerator HandleDefeatSequence()
    {
        Debug.Log($"Enemy {gameObject.name} defeated! Starting 3-second defeat sequence...");
        
        // IMPORTANT: Child remains INACTIVE during the 3-second timer
        // Enemy continues flying/moving during rotation until timer completes
        
        // Wait for the specified time while rotating and potentially moving
        yield return new WaitForSeconds(defeatWaitTime);
        
        Debug.Log($"3 seconds completed! Stopping ALL movement and activating child for {gameObject.name}");
        
        // AFTER 3 seconds: Stop ALL movement and rotation
        isRotating = false;
        isDead = false;
        isConverted = true; // This COMPLETELY stops all movement, flying, following, etc.
        
        // Stop any physics or rigidbody movement if present
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Prevent any physics movement
        }
        
        // Look at the player (final orientation)
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            if (directionToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
        }
        
        // ONLY NOW activate child object (after 3 seconds have passed)
        if (childToActivate != null)
        {
            childToActivate.gameObject.SetActive(true);
            Debug.Log($"Child object '{childToActivate.name}' activated AFTER 3 seconds");
        }
        else
        {
            // If no specific child is assigned, activate the first inactive child
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"Child object '{child.name}' activated AFTER 3 seconds");
                    break;
                }
            }
        }
        
        // Change tag to "Player"
        gameObject.tag = "Player";
        Debug.Log($"Enemy {gameObject.name} is now a Player and has COMPLETELY stopped all movement!");
        
        isDefeated = false; // Reset defeated state
    }
    
    void Update()
    {
        // If converted to player, COMPLETELY stop all movement, rotation, and physics
        if (isConverted)
            return; // No movement, no rotation, no following, no flying - NOTHING
            
        // Handle rotation (defeat state) - enemy can still move/fly during this phase
        if (isRotating && isDefeated)
        {
            // Rotate around the random axis at the specified speed
            transform.Rotate(randomRotationAxis * rotationSpeed * Time.deltaTime, Space.World);
        }
        
        // Handle player following (only when alive - not rotating and not defeated)
        // During defeat sequence (isDefeated = true), enemy can still move/fly until conversion
        if (!isDead && !isDefeated && player != null)
        {
            // Match player's Y position
            Vector3 targetPosition = transform.position;
            targetPosition.y = player.position.y;
            
            // Move towards player's position (keeping current Y)
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0; // Don't move vertically towards player, only match Y position
            
            // Apply movement
            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(player.position.x, player.position.y, player.position.z), 
                moveSpeed * Time.deltaTime);
        }
        
        // Note: Any other movement scripts (flying, floating, etc.) will also be stopped
        // when isConverted = true, as this Update method returns early
    }
    
    public void StopRotation()
    {
        isRotating = false;
        isDead = false; // Enemy becomes alive when not rotating
        isDefeated = false; // Reset defeated state
        
        // Stop defeat coroutine if running
        if (defeatCoroutine != null)
        {
            StopCoroutine(defeatCoroutine);
            defeatCoroutine = null;
        }
    }
    
    public void StartRotation()
    {
        isRotating = true;
        isDead = true; // Enemy dies when rotation starts
        // Note: This doesn't start the defeat sequence - only Initialize() does that
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public bool IsDefeated()
    {
        return isDefeated;
    }
    
    public bool IsConverted()
    {
        return isConverted;
    }
    
    /// <summary>
    /// Set the child object that should be activated when the enemy is defeated
    /// </summary>
    /// <param name="child">The child transform to activate</param>
    public void SetChildToActivate(Transform child)
    {
        childToActivate = child;
    }
    
    /// <summary>
    /// Activate visual effects when enemy is hit/dies
    /// </summary>
    private void ActivateDeathEffects()
    {
        // 1. Activate the Light component within this enemy
        Light enemyLight = GetComponentInChildren<Light>();
        if (enemyLight != null)
        {
            enemyLight.enabled = true;
            Debug.Log($"Activated light on enemy: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"No Light component found in children of {gameObject.name}");
        }
        
        // 2. Find the grandchild "Plane" and change its texture
        ChangeGrandchildPlaneTexture();
    }
    
    /// <summary>
    /// Find the plane grandchild and change its material to the happy material
    /// </summary>
    private void ChangeGrandchildPlaneTexture()
    {
        if (happyMaterial == null)
        {
            Debug.LogWarning($"Happy material not assigned for enemy: {gameObject.name}");
            return;
        }
        
        // Search through all children and their children to find the plane
        Transform planeTransform = FindGrandchildByName(transform, planeChildName);
        
        if (planeTransform != null)
        {
            // Get the renderer component and change the material
            Renderer planeRenderer = planeTransform.GetComponent<Renderer>();
            if (planeRenderer != null)
            {
                // Change the material to happy material
                planeRenderer.material = happyMaterial;
                Debug.Log($"Changed material of {planeTransform.name} to happy material on enemy: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"No Renderer found on plane {planeTransform.name} in enemy: {gameObject.name}");
                
                // Try to find renderer in children of the plane
                Renderer childRenderer = planeTransform.GetComponentInChildren<Renderer>();
                if (childRenderer != null)
                {
                    childRenderer.material = happyMaterial;
                    Debug.Log($"Changed material of child renderer in {planeTransform.name} to happy material on enemy: {gameObject.name}");
                }
                else
                {
                    Debug.LogError($"No Renderer found in {planeTransform.name} or its children in enemy: {gameObject.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Plane child named '{planeChildName}' not found in grandchildren of enemy: {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Recursively search for a grandchild with the specified name
    /// </summary>
    /// <param name="parent">Parent transform to search in</param>
    /// <param name="childName">Name of the child to find</param>
    /// <returns>Transform of the found child, or null if not found</returns>
    private Transform FindGrandchildByName(Transform parent, string childName)
    {
        // Search through all direct children
        foreach (Transform child in parent)
        {
            // Check if this child has the name we're looking for
            if (child.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            
            // Recursively search in this child's children
            Transform found = FindGrandchildByName(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        
        return null; // Not found
    }
}
