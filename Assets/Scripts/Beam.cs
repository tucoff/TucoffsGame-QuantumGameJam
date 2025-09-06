using UnityEngine;

public class Beam : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float upwardForce = 10f;
    [SerializeField] private float rotationSpeed = 360f;
    
    [Header("Player Reference")]
    [SerializeField] private PlayerFollowController playerController;
    
    void Start()
    {
        // Auto-assign player controller if not set
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerFollowController>();
        }
    }

    void Update()
    {
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Enemy" tag
        if (other.CompareTag("Enemy"))
        {
            // Check if enemy already has rotation component and is rotating (already been hit)
            EnemyBeamRotation rotationComponent = other.GetComponent<EnemyBeamRotation>();
            
            // If enemy is already rotating, make it immune to upward force (can only levitate once)
            if (rotationComponent != null && (rotationComponent.IsDead() || rotationComponent.IsDefeated()))
            {
                Debug.Log($"Enemy {other.name} is already rotating - immune to upward force!");
                return; // Exit early, enemy is immune
            }
            
            // Get the Rigidbody component
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();
            if (enemyRigidbody != null)
            {
                // Disable gravity
                enemyRigidbody.useGravity = false;
                
                // Apply upward impulse (only for first hit)
                enemyRigidbody.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
                Debug.Log($"Applied upward force to {other.name} - first hit!");
            }
            
            // Activate all child objects (but they will be deactivated again by EnemyBeamRotation until defeat sequence completes)
            for (int i = 0; i < other.transform.childCount; i++)
            {
                other.transform.GetChild(i).gameObject.SetActive(true);
            }
            
            // Add rotation component if not present, or initialize existing one
            if (rotationComponent == null)
            {
                rotationComponent = other.gameObject.AddComponent<EnemyBeamRotation>();
            }
            rotationComponent.Initialize(rotationSpeed);
            
            // Notify player controller about successful hit to reset cooldown
            if (playerController != null)
            {
                playerController.OnSuccessfulHit();
                Debug.Log("Player shot cooldown reset due to successful hit!");
            }
            
            // Deactivate the beam object after successful hit
            gameObject.SetActive(false);
        }
    }
}
