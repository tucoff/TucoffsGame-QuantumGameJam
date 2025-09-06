using UnityEngine;

public class EnemyCollisionDetector : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private string playerTag = "Player";
    
    private EnemyBeamRotation enemyBeam;
    
    void Start()
    {
        enemyBeam = GetComponent<EnemyBeamRotation>();
        
        // Ensure this GameObject has a collider
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning($"EnemyCollisionDetector on {gameObject.name} needs a Collider component!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Only check collision if this enemy hasn't been converted to a player
        if (enemyBeam != null && enemyBeam.IsConverted())
        {
            return; // This enemy is now a player, don't trigger game over
        }
        
        // Check if collided with a player
        if (other.CompareTag(playerTag))
        {
            // Make sure the collided player is not a converted enemy
            EnemyBeamRotation otherEnemyBeam = other.GetComponent<EnemyBeamRotation>();
            
            // If the "player" was originally an enemy that got converted, don't trigger game over
            if (otherEnemyBeam != null && otherEnemyBeam.IsConverted())
            {
                Debug.Log($"Enemy {gameObject.name} collided with converted player {other.name} - No game over");
                return;
            }
            
            // This is a collision with an original player
            Debug.Log($"Enemy {gameObject.name} collided with original player {other.name} - Game Over!");
            
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
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Backup collision detection for non-trigger colliders
        if (enemyBeam != null && enemyBeam.IsConverted())
        {
            return; // This enemy is now a player, don't trigger game over
        }
        
        if (collision.gameObject.CompareTag(playerTag))
        {
            EnemyBeamRotation otherEnemyBeam = collision.gameObject.GetComponent<EnemyBeamRotation>();
            
            if (otherEnemyBeam != null && otherEnemyBeam.IsConverted())
            {
                return; // Collided with converted enemy, not original player
            }
            
            Debug.Log($"Enemy {gameObject.name} physically collided with original player {collision.gameObject.name} - Game Over!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}
