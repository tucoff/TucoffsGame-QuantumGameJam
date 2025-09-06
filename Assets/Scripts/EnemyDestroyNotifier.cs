using UnityEngine;
using System;
using System.Collections;

public class EnemyDestroyNotifier : MonoBehaviour
{
    public event Action OnEnemyConverted;
    
    private EnemyBeamRotation enemyBeam;
    private bool hasConverted = false;
    
    void Start()
    {
        enemyBeam = GetComponent<EnemyBeamRotation>();
        if (enemyBeam != null)
        {
            StartCoroutine(CheckForConversion());
        }
    }
    
    /// <summary>
    /// Check if the enemy has been converted to a player
    /// </summary>
    private IEnumerator CheckForConversion()
    {
        while (!hasConverted)
        {
            // Check if the tag has changed to "Player" (indicating conversion)
            if (gameObject.CompareTag("Player") && !hasConverted)
            {
                hasConverted = true;
                OnEnemyConverted?.Invoke();
                Debug.Log($"Enemy {gameObject.name} has been converted to a player!");
                break;
            }
            
            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }
    }
    
    void OnDestroy()
    {
        // Notify if destroyed before conversion
        if (!hasConverted)
        {
            OnEnemyConverted?.Invoke();
        }
    }
}
