using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de gerenciamento de inimigos que se integra com o CameraEffectsController
/// </summary>
public class EnemyManager : MonoBehaviour
{
    
    [Header("Enemy Events")]
    public UnityEvent<int> OnEnemyKilled; // Event que passa o número total de kills
    public UnityEvent<int> OnKillMilestone; // Event para marcos específicos (a cada 10 kills, etc)
    
    [Header("Debug")]
    [SerializeField] private bool logEnemyDeaths = true;
    
    private int totalKills = 0;
    
    /// <summary>
    /// Chama quando um inimigo morre. Use este método de qualquer script de inimigo.
    /// </summary>
    public void RegisterEnemyDeath()
    {
        totalKills++;
        
        if (logEnemyDeaths)
        {
            Debug.Log($"Enemy died! Total kills: {totalKills}");
        }
        
        // Trigger events
        OnEnemyKilled?.Invoke(totalKills);
        
        // Check for milestones
        if (totalKills % 10 == 0)
        {
            OnKillMilestone?.Invoke(totalKills);
            if (logEnemyDeaths)
            {
                Debug.Log($"Kill milestone reached: {totalKills} enemies defeated!");
            }
        }
    }
    
    /// <summary>
    /// Registra múltiplas mortes de uma vez (útil para explosões, etc)
    /// </summary>
    public void RegisterMultipleEnemyDeaths(int count)
    {
        for (int i = 0; i < count; i++)
        {
            RegisterEnemyDeath();
        }
    }
    
    /// <summary>
    /// Reseta o contador de kills (útil para reiniciar o jogo)
    /// </summary>
    public void ResetKillCount()
    {
        totalKills = 0;
        if (logEnemyDeaths)
        {
            Debug.Log("Kill count reset to 0");
        }
    }
    
    /// <summary>
    /// Retorna o número total de kills
    /// </summary>
    public int GetTotalKills()
    {
        return totalKills;
    }
    
}

/// <summary>
/// Script simples para colocar em inimigos individuais
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float health = 100f;
    [SerializeField] private bool useEnemyManager = true;
    
    private EnemyManager enemyManager;
    
    void Start()
    {
        if (useEnemyManager)
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
        }
    }
    
    /// <summary>
    /// Causa dano ao inimigo
    /// </summary>
    public void TakeDamage(float damage)
    {
        health -= damage;
        
        if (health <= 0f)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Mata o inimigo instantaneamente
    /// </summary>
    public void Die()
    {
        if (useEnemyManager && enemyManager != null)
        {
            enemyManager.RegisterEnemyDeath();
        }
        
        // Aqui você pode adicionar efeitos de morte, som, etc.
        Debug.Log($"{gameObject.name} died!");
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Para testes - mata o inimigo quando clicado
    /// </summary>
    void OnMouseDown()
    {
        Die();
    }
}
