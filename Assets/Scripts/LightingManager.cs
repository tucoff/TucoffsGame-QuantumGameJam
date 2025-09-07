using UnityEngine;
using UnityEngine.Rendering;

public class LightingManager : MonoBehaviour
{
    [Header("Lighting References")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light[] playerPointLights; // Point lights that should follow/illuminate the player
    
    [Header("Lighting Settings")]
    [SerializeField] private float lightSwitchTime = 120f; // Time in seconds when lights go out
    [SerializeField] private float transitionDuration = 2f; // How long the transition takes
    
    [Header("Beam Enhancement")]
    [SerializeField] private bool enhanceBeamVisibility = true;
    [SerializeField] private float beamGlowIntensity = 3f;
    
    [Header("Original Lighting Values")]
    private float originalDirectionalIntensity;
    private Color originalDirectionalColor;
    private Color originalAmbientLight;
    private float originalAmbientIntensity;
    
    [Header("Dark Mode Values")]
    [SerializeField] private float darkAmbientIntensity = 0.1f; // Very dim ambient light
    [SerializeField] private Color darkAmbientColor = new Color(0.05f, 0.05f, 0.1f, 1f); // Very dark blue tint
    [SerializeField] private float pointLightIntensity = 8f; // Intensity for player point lights
    [SerializeField] private float pointLightRange = 15f; // Range for player point lights
    
    private bool hasTriggeredLightSwitch = false;
    private bool isTransitioning = false;
    private float transitionStartTime;
    private GameObject player;
    
    void Start()
    {
        // Find the player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj;
        }
        
        // Auto-find directional light if not assigned
        if (directionalLight == null)
        {
            directionalLight = FindFirstObjectByType<Light>();
            if (directionalLight != null && directionalLight.type != LightType.Directional)
            {
                // If the found light isn't directional, look for a directional light specifically
                Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (Light light in allLights)
                {
                    if (light.type == LightType.Directional)
                    {
                        directionalLight = light;
                        break;
                    }
                }
            }
        }
        
        // Store original lighting values
        StoreOriginalLightingValues();
        
        // Set up player point lights if none assigned
        if (playerPointLights == null || playerPointLights.Length == 0)
        {
            SetupPlayerPointLights();
        }
        
        // Initially disable player point lights (they'll be enabled when it gets dark)
        SetPlayerPointLightsEnabled(false);
        
        Debug.Log($"LightingManager initialized. Light switch will occur at {lightSwitchTime} seconds.");
    }
    
    void Update()
    {
        // Check if we should switch to dark mode
        if (!hasTriggeredLightSwitch && GameManager.Instance != null)
        {
            float survivalTime = GameManager.Instance.GetSurvivalTime();
            
            if (survivalTime >= lightSwitchTime)
            {
                StartLightSwitch();
                Debug.Log($"Light switch triggered! Player survived {survivalTime:F1} seconds.");
            }
        }
        
        // Handle lighting transition
        if (isTransitioning)
        {
            UpdateLightingTransition();
        }
        
        // Update player point light positions
        if (hasTriggeredLightSwitch && player != null)
        {
            UpdatePlayerPointLights();
        }
    }
    
    private void StoreOriginalLightingValues()
    {
        // Store directional light values
        if (directionalLight != null)
        {
            originalDirectionalIntensity = directionalLight.intensity;
            originalDirectionalColor = directionalLight.color;
        }
        
        // Store ambient lighting values
        originalAmbientLight = RenderSettings.ambientLight;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        
        Debug.Log($"Original lighting stored - Directional: {originalDirectionalIntensity}, Ambient: {originalAmbientIntensity}");
    }
    
    private void SetupPlayerPointLights()
    {
        if (player == null) return;
        
        // Create point lights around the player
        playerPointLights = new Light[1]; // Start with one main light
        
        // Create main player light
        GameObject lightObj = new GameObject("Player Point Light");
        lightObj.transform.SetParent(player.transform);
        lightObj.transform.localPosition = new Vector3(0, 2, 0); // Above the player
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.intensity = pointLightIntensity;
        pointLight.range = pointLightRange;
        pointLight.color = Color.white;
        pointLight.shadows = LightShadows.Soft;
        
        playerPointLights[0] = pointLight;
        
        Debug.Log("Player point lights created and configured.");
    }
    
    private void StartLightSwitch()
    {
        if (hasTriggeredLightSwitch) return;
        
        hasTriggeredLightSwitch = true;
        isTransitioning = true;
        transitionStartTime = Time.time;
        
        // Enable player point lights
        SetPlayerPointLightsEnabled(true);
        
        // Enhance beam visibility in dark mode
        if (enhanceBeamVisibility)
        {
            EnhanceBeamVisibility();
        }
        
        Debug.Log("LIGHTS OUT! Transitioning to dark mode...");
    }
    
    private void UpdateLightingTransition()
    {
        float elapsed = Time.time - transitionStartTime;
        float progress = elapsed / transitionDuration;
        
        if (progress >= 1f)
        {
            // Transition complete
            progress = 1f;
            isTransitioning = false;
            Debug.Log("Dark mode transition complete!");
        }
        
        // Animate directional light
        if (directionalLight != null)
        {
            directionalLight.intensity = Mathf.Lerp(originalDirectionalIntensity, 0f, progress);
            directionalLight.color = Color.Lerp(originalDirectionalColor, Color.black, progress);
        }
        
        // Animate ambient lighting
        RenderSettings.ambientIntensity = Mathf.Lerp(originalAmbientIntensity, darkAmbientIntensity, progress);
        RenderSettings.ambientLight = Color.Lerp(originalAmbientLight, darkAmbientColor, progress);
        
        // Also reduce ambient sky color if using gradient ambient mode
        RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, darkAmbientColor, progress);
        RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, darkAmbientColor * 0.5f, progress);
        RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, Color.black, progress);
    }
    
    private void SetPlayerPointLightsEnabled(bool enabled)
    {
        if (playerPointLights == null) return;
        
        foreach (Light light in playerPointLights)
        {
            if (light != null)
            {
                light.enabled = enabled;
            }
        }
    }
    
    private void UpdatePlayerPointLights()
    {
        if (playerPointLights == null || player == null) return;
        
        // The lights are parented to the player, so they move automatically
        // But we could add additional logic here if needed (like intensity changes based on movement, etc.)
    }
    
    /// <summary>
    /// Public method to manually trigger the light switch (for testing)
    /// </summary>
    [ContextMenu("Trigger Light Switch")]
    public void TriggerLightSwitchManually()
    {
        if (!hasTriggeredLightSwitch)
        {
            StartLightSwitch();
        }
    }
    
    /// <summary>
    /// Reset lighting to original values (for testing)
    /// </summary>
    [ContextMenu("Reset Lighting")]
    public void ResetLighting()
    {
        hasTriggeredLightSwitch = false;
        isTransitioning = false;
        
        // Restore directional light
        if (directionalLight != null)
        {
            directionalLight.intensity = originalDirectionalIntensity;
            directionalLight.color = originalDirectionalColor;
        }
        
        // Restore ambient lighting
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.ambientLight = originalAmbientLight;
        
        // Disable player point lights
        SetPlayerPointLightsEnabled(false);
        
        Debug.Log("Lighting reset to original values.");
    }
    
    /// <summary>
    /// Get whether the lights have switched to dark mode
    /// </summary>
    public bool IsInDarkMode()
    {
        return hasTriggeredLightSwitch && !isTransitioning;
    }
    
    /// <summary>
    /// Get the current lighting transition progress (0 to 1)
    /// </summary>
    public float GetTransitionProgress()
    {
        if (!isTransitioning) return hasTriggeredLightSwitch ? 1f : 0f;
        
        float elapsed = Time.time - transitionStartTime;
        return Mathf.Clamp01(elapsed / transitionDuration);
    }
    
    /// <summary>
    /// Enhance beam visibility when lights go out
    /// </summary>
    private void EnhanceBeamVisibility()
    {
        // Find all beam objects and enhance their glow
        BeamGlowEffect[] beamEffects = FindObjectsByType<BeamGlowEffect>(FindObjectsSortMode.None);
        
        foreach (BeamGlowEffect beam in beamEffects)
        {
            if (beam != null)
            {
                // Increase glow intensity for better visibility in darkness
                beam.SetGlowIntensity(beamGlowIntensity);
                
                Debug.Log($"Enhanced glow for beam: {beam.gameObject.name}");
            }
        }
        
        // Also look for objects with ConnectedBeam or EnemyBeamRotation components
        ConnectedBeam[] connectedBeams = FindObjectsByType<ConnectedBeam>(FindObjectsSortMode.None);
        foreach (ConnectedBeam beam in connectedBeams)
        {
            BeamGlowEffect glowEffect = beam.GetComponent<BeamGlowEffect>();
            if (glowEffect == null)
            {
                glowEffect = beam.gameObject.AddComponent<BeamGlowEffect>();
            }
            glowEffect.SetGlowIntensity(beamGlowIntensity);
        }
        
        EnemyBeamRotation[] enemyBeams = FindObjectsByType<EnemyBeamRotation>(FindObjectsSortMode.None);
        foreach (EnemyBeamRotation beam in enemyBeams)
        {
            BeamGlowEffect glowEffect = beam.GetComponent<BeamGlowEffect>();
            if (glowEffect == null)
            {
                glowEffect = beam.gameObject.AddComponent<BeamGlowEffect>();
                // Set enemy beam color to orange/red
                glowEffect.SetGlowColor(new Color(1f, 0.3f, 0f, 1f));
            }
            glowEffect.SetGlowIntensity(beamGlowIntensity);
        }
        
        Debug.Log($"Enhanced visibility for {beamEffects.Length + connectedBeams.Length + enemyBeams.Length} beam objects");
    }
}
