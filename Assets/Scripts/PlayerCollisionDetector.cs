using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private string enemyTag = "Enemy";
    
    void Start()
    {
        // Ensure this GameObject has a collider
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning($"PlayerCollisionDetector on {gameObject.name} needs a Collider component!");
        }
        else
        {
            // Make sure it's set as a trigger for collision detection
            if (!collider.isTrigger)
            {
                Debug.Log($"Setting collider on {gameObject.name} as trigger for collision detection");
                collider.isTrigger = true;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if collided with an enemy
        if (other.CompareTag(enemyTag))
        {
            // Check if the enemy is not converted to player yet
            EnemyBeamRotation enemyBeam = other.GetComponent<EnemyBeamRotation>();
            
            // Only trigger game over if enemy is still an actual enemy (not converted)
            if (enemyBeam == null || !enemyBeam.IsConverted())
            {
                Debug.Log($"Player {gameObject.name} collided with enemy {other.name} - Game Over!");
                
                // Trigger game over
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogError("GameManager instance not found! Cannot trigger game over.");
                }
            }
            else
            {
                Debug.Log($"Player {gameObject.name} collided with converted player {other.name} - No game over");
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Backup collision detection for non-trigger colliders
        if (collision.gameObject.CompareTag(enemyTag))
        {
            EnemyBeamRotation enemyBeam = collision.gameObject.GetComponent<EnemyBeamRotation>();
            
            if (enemyBeam == null || !enemyBeam.IsConverted())
            {
                Debug.Log($"Player {gameObject.name} physically collided with enemy {collision.gameObject.name} - Game Over!");
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }
    }
}
