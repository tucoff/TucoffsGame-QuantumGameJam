using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private float pitchIncreaseInterval = 10f; // Every 10 seconds
    [SerializeField] private float pitchIncreaseAmount = 0.1f; // 0.1 more pitch each time
    [SerializeField] private float maxPitch = 3f; // Maximum pitch limit
    
    private float gameStartTime;
    private float basePitch = 1f; // Starting pitch
    
    // Singleton pattern
    public static MusicManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Auto-assign AudioSource if not set
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
        }
        
        if (musicAudioSource == null)
        {
            Debug.LogWarning("MusicManager: No AudioSource found! Please assign one in the inspector or add one to this GameObject.");
            return;
        }
        
        // Store the original pitch and start time
        basePitch = musicAudioSource.pitch;
        gameStartTime = Time.time;
        
        // Start the pitch increase routine
        StartCoroutine(PitchIncreaseRoutine());
        
        Debug.Log("MusicManager started! Pitch will increase every 10 seconds.");
    }
    
    /// <summary>
    /// Coroutine that increases music pitch every 10 seconds
    /// </summary>
    IEnumerator PitchIncreaseRoutine()
    {
        while (true)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(pitchIncreaseInterval);
            
            // Check if we have an AudioSource
            if (musicAudioSource != null)
            {
                // Calculate new pitch
                float newPitch = musicAudioSource.pitch + pitchIncreaseAmount;
                
                // Clamp to maximum pitch
                newPitch = Mathf.Min(newPitch, maxPitch);
                
                // Apply new pitch
                musicAudioSource.pitch = newPitch;
                
                float timeElapsed = Time.time - gameStartTime;
                Debug.Log($"Music pitch increased to {newPitch:F2} after {timeElapsed:F1} seconds");
                
                // Stop if we've reached maximum pitch
                if (newPitch >= maxPitch)
                {
                    Debug.Log("Maximum music pitch reached!");
                    break;
                }
            }
            else
            {
                Debug.LogWarning("MusicManager: AudioSource is null, stopping pitch increase routine.");
                break;
            }
        }
    }
    
    /// <summary>
    /// Reset the music pitch to base value - used when returning to menu
    /// </summary>
    public void ResetMusicPitch()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.pitch = basePitch;
            gameStartTime = Time.time;
            Debug.Log("Music pitch reset to base value!");
            
            // Restart the pitch increase routine
            StopAllCoroutines();
            StartCoroutine(PitchIncreaseRoutine());
        }
    }
    
    /// <summary>
    /// Get the current music pitch
    /// </summary>
    /// <returns>Current pitch value</returns>
    public float GetCurrentPitch()
    {
        return musicAudioSource != null ? musicAudioSource.pitch : basePitch;
    }
}
