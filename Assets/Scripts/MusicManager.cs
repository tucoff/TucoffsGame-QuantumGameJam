using UnityEngine;

/// <summary>
/// Manages background music and audio effects for the game
/// Handles pitch changes when lighting conditions change
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("Pitch Settings")]
    [SerializeField] private float normalPitch = 1.0f;
    [SerializeField] private float darkModePitch = -1.5f;
    [SerializeField] private float pitchTransitionDuration = 2f;
    
    private float originalPitch;
    private bool isDarkMode = false;
    private bool isTransitioning = false;
    private float transitionStartTime;
    private float targetPitch;
    private float startPitch;
    
    public static MusicManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
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
        // Set up music audio source if not assigned
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure audio source
        ConfigureAudioSource();
        
        // Store original pitch
        originalPitch = normalPitch;
        musicAudioSource.pitch = normalPitch;
        
        // Start playing background music if assigned
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
        
        Debug.Log($"MusicManager initialized. Normal pitch: {normalPitch}, Dark mode pitch: {darkModePitch}");
    }
    
    void Update()
    {
        // Handle pitch transition
        if (isTransitioning)
        {
            UpdatePitchTransition();
        }
    }
    
    private void ConfigureAudioSource()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.loop = true;
            musicAudioSource.playOnAwake = false;
            musicAudioSource.volume = 0.5f; // Adjust as needed
        }
    }
    
    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (musicAudioSource != null && backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.Play();
            Debug.Log("Background music started");
        }
    }
    
    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            Debug.Log("Background music stopped");
        }
    }
    
    /// <summary>
    /// Change music pitch to dark mode (called when lights switch)
    /// </summary>
    public void SwitchToDarkMode()
    {
        if (!isDarkMode)
        {
            isDarkMode = true;
            StartPitchTransition(darkModePitch);
            Debug.Log($"Switching music to dark mode - pitch changing to {darkModePitch}");
        }
    }
    
    /// <summary>
    /// Reset music pitch to normal (for testing or game reset)
    /// </summary>
    public void SwitchToNormalMode()
    {
        if (isDarkMode)
        {
            isDarkMode = false;
            StartPitchTransition(normalPitch);
            Debug.Log($"Switching music to normal mode - pitch changing to {normalPitch}");
        }
    }
    
    /// <summary>
    /// Start a pitch transition
    /// </summary>
    private void StartPitchTransition(float newPitch)
    {
        startPitch = musicAudioSource.pitch;
        targetPitch = newPitch;
        transitionStartTime = Time.time;
        isTransitioning = true;
    }
    
    /// <summary>
    /// Update pitch transition over time
    /// </summary>
    private void UpdatePitchTransition()
    {
        float elapsed = Time.time - transitionStartTime;
        float progress = elapsed / pitchTransitionDuration;
        
        if (progress >= 1f)
        {
            // Transition complete
            progress = 1f;
            isTransitioning = false;
            Debug.Log($"Pitch transition complete - final pitch: {targetPitch}");
        }
        
        // Interpolate pitch
        float currentPitch = Mathf.Lerp(startPitch, targetPitch, progress);
        if (musicAudioSource != null)
        {
            musicAudioSource.pitch = currentPitch;
        }
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetVolume(float volume)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"Music volume set to {musicAudioSource.volume}");
        }
    }
    
    /// <summary>
    /// Get current pitch
    /// </summary>
    public float GetCurrentPitch()
    {
        return musicAudioSource != null ? musicAudioSource.pitch : 0f;
    }
    
    /// <summary>
    /// Check if currently in dark mode
    /// </summary>
    public bool IsInDarkMode()
    {
        return isDarkMode;
    }
    
    /// <summary>
    /// Set background music clip
    /// </summary>
    public void SetBackgroundMusic(AudioClip newMusic)
    {
        backgroundMusic = newMusic;
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            PlayBackgroundMusic();
        }
    }
    
    /// <summary>
    /// Debug method to test dark mode switch
    /// </summary>
    [ContextMenu("Test Dark Mode Switch")]
    public void TestDarkModeSwitch()
    {
        if (isDarkMode)
        {
            SwitchToNormalMode();
        }
        else
        {
            SwitchToDarkMode();
        }
    }
}
