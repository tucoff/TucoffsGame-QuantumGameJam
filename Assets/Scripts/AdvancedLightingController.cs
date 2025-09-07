using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Advanced lighting controller that provides more dramatic darkness effects
/// by controlling multiple Unity lighting systems simultaneously
/// </summary>
public class AdvancedLightingController : MonoBehaviour
{
    [Header("Lighting References")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light[] additionalLights; // Any other lights to control
    
    [Header("Timing Settings")]
    [SerializeField] private float lightSwitchTime = 120f;
    [SerializeField] private float transitionDuration = 3f;
    
    [Header("Player Lighting")]
    [SerializeField] private GameObject player;
    [SerializeField] private bool createPlayerLight = true;
    [SerializeField] private float playerLightIntensity = 10f;
    [SerializeField] private float playerLightRange = 20f;
    [SerializeField] private Color playerLightColor = Color.white;
    
    [Header("Dark Mode Settings")]
    [SerializeField] private bool disableAllLights = true; // Completely turn off all global lights
    [SerializeField] private float darkAmbientIntensity = 0.02f; // Very minimal ambient
    [SerializeField] private Color darkAmbientColor = new Color(0.01f, 0.01f, 0.05f, 1f); // Almost black
    [SerializeField] private float darkFogDensity = 0.05f; // Add fog for atmosphere
    [SerializeField] private Color darkFogColor = new Color(0.05f, 0.05f, 0.1f, 1f);
    
    // Original values storage
    private float originalDirectionalIntensity;
    private Color originalDirectionalColor;
    private Color originalAmbientSkyColor;
    private Color originalAmbientEquatorColor;
    private Color originalAmbientGroundColor;
    private float originalAmbientIntensity;
    private bool originalFogEnabled;
    private float originalFogDensity;
    private Color originalFogColor;
    private float[] originalLightIntensities;
    
    // Runtime state
    private bool hasTriggered = false;
    private bool isTransitioning = false;
    private float transitionStartTime;
    private Light playerPointLight;
    
    void Start()
    {
        InitializeLightingController();
    }
    
    void Update()
    {
        CheckForLightSwitch();
        HandleTransition();
        UpdatePlayerLight();
    }
    
    private void InitializeLightingController()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        
        // Find directional light if not assigned
        if (directionalLight == null)
        {
            FindDirectionalLight();
        }
        
        // Store all original lighting values
        StoreOriginalValues();
        
        // Create player light
        if (createPlayerLight && player != null)
        {
            CreatePlayerPointLight();
        }
        
        Debug.Log($"AdvancedLightingController initialized. Darkness begins in {lightSwitchTime} seconds.");
    }
    
    private void FindDirectionalLight()
    {
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                directionalLight = light;
                Debug.Log($"Found directional light: {light.name}");
                break;
            }
        }
        
        if (directionalLight == null)
        {
            Debug.LogWarning("No directional light found in scene!");
        }
    }
    
    private void StoreOriginalValues()
    {
        // Store directional light
        if (directionalLight != null)
        {
            originalDirectionalIntensity = directionalLight.intensity;
            originalDirectionalColor = directionalLight.color;
        }
        
        // Store ambient lighting
        originalAmbientSkyColor = RenderSettings.ambientSkyColor;
        originalAmbientEquatorColor = RenderSettings.ambientEquatorColor;
        originalAmbientGroundColor = RenderSettings.ambientGroundColor;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        
        // Store fog settings
        originalFogEnabled = RenderSettings.fog;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogColor = RenderSettings.fogColor;
        
        // Store additional lights
        if (additionalLights != null && additionalLights.Length > 0)
        {
            originalLightIntensities = new float[additionalLights.Length];
            for (int i = 0; i < additionalLights.Length; i++)
            {
                if (additionalLights[i] != null)
                {
                    originalLightIntensities[i] = additionalLights[i].intensity;
                }
            }
        }
        
        Debug.Log("All original lighting values stored.");
    }
    
    private void CreatePlayerPointLight()
    {
        GameObject lightObj = new GameObject("Player Survival Light");
        lightObj.transform.SetParent(player.transform);
        lightObj.transform.localPosition = Vector3.up * 2f; // 2 units above player
        
        playerPointLight = lightObj.AddComponent<Light>();
        playerPointLight.type = LightType.Point;
        playerPointLight.intensity = playerLightIntensity;
        playerPointLight.range = playerLightRange;
        playerPointLight.color = playerLightColor;
        playerPointLight.shadows = LightShadows.Soft;
        playerPointLight.enabled = false; // Start disabled
        
        Debug.Log("Player survival light created.");
    }
    
    private void CheckForLightSwitch()
    {
        if (hasTriggered || GameManager.Instance == null) return;
        
        float survivalTime = GameManager.Instance.GetSurvivalTime();
        if (survivalTime >= lightSwitchTime)
        {
            TriggerDarkMode();
        }
    }
    
    private void TriggerDarkMode()
    {
        if (hasTriggered) return;
        
        hasTriggered = true;
        isTransitioning = true;
        transitionStartTime = Time.time;
        
        // Enable player light immediately
        if (playerPointLight != null)
        {
            playerPointLight.enabled = true;
        }
        
        Debug.Log("🌙 DARKNESS FALLS! The world grows dark, only your light remains...");
    }
    
    private void HandleTransition()
    {
        if (!isTransitioning) return;
        
        float elapsed = Time.time - transitionStartTime;
        float progress = Mathf.Clamp01(elapsed / transitionDuration);
        
        // Smooth easing
        float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
        
        ApplyLightingTransition(easeProgress);
        
        if (progress >= 1f)
        {
            isTransitioning = false;
            Debug.Log("Darkness transition complete. The world is now shrouded in shadow.");
        }
    }
    
    private void ApplyLightingTransition(float progress)
    {
        // Fade out directional light
        if (directionalLight != null)
        {
            if (disableAllLights)
            {
                directionalLight.intensity = Mathf.Lerp(originalDirectionalIntensity, 0f, progress);
                if (progress >= 0.95f) // Almost complete
                {
                    directionalLight.enabled = false;
                }
            }
            else
            {
                directionalLight.intensity = Mathf.Lerp(originalDirectionalIntensity, 0.1f, progress);
            }
        }
        
        // Fade out additional lights
        if (additionalLights != null)
        {
            for (int i = 0; i < additionalLights.Length; i++)
            {
                if (additionalLights[i] != null && originalLightIntensities != null && i < originalLightIntensities.Length)
                {
                    if (disableAllLights)
                    {
                        additionalLights[i].intensity = Mathf.Lerp(originalLightIntensities[i], 0f, progress);
                        if (progress >= 0.95f)
                        {
                            additionalLights[i].enabled = false;
                        }
                    }
                    else
                    {
                        additionalLights[i].intensity = Mathf.Lerp(originalLightIntensities[i], originalLightIntensities[i] * 0.1f, progress);
                    }
                }
            }
        }
        
        // Transition ambient lighting
        RenderSettings.ambientIntensity = Mathf.Lerp(originalAmbientIntensity, darkAmbientIntensity, progress);
        RenderSettings.ambientSkyColor = Color.Lerp(originalAmbientSkyColor, darkAmbientColor, progress);
        RenderSettings.ambientEquatorColor = Color.Lerp(originalAmbientEquatorColor, darkAmbientColor * 0.5f, progress);
        RenderSettings.ambientGroundColor = Color.Lerp(originalAmbientGroundColor, darkAmbientColor * 0.2f, progress);
        
        // Add atmospheric fog
        RenderSettings.fog = true;
        RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, darkFogDensity, progress);
        RenderSettings.fogColor = Color.Lerp(originalFogColor, darkFogColor, progress);
    }
    
    private void UpdatePlayerLight()
    {
        if (!hasTriggered || playerPointLight == null || player == null) return;
        
        // The light is parented to player, so it follows automatically
        // You could add intensity flickering or other effects here
        
        // Example: Slight intensity variation for atmospheric effect
        float flicker = Mathf.Sin(Time.time * 4f) * 0.1f + 1f;
        playerPointLight.intensity = playerLightIntensity * flicker;
    }
    
    /// <summary>
    /// Manually trigger dark mode (for testing)
    /// </summary>
    [ContextMenu("Trigger Dark Mode")]
    public void TriggerDarkModeManually()
    {
        TriggerDarkMode();
    }
    
    /// <summary>
    /// Reset all lighting to original state
    /// </summary>
    [ContextMenu("Reset Lighting")]
    public void ResetToOriginal()
    {
        hasTriggered = false;
        isTransitioning = false;
        
        // Restore directional light
        if (directionalLight != null)
        {
            directionalLight.enabled = true;
            directionalLight.intensity = originalDirectionalIntensity;
            directionalLight.color = originalDirectionalColor;
        }
        
        // Restore additional lights
        if (additionalLights != null && originalLightIntensities != null)
        {
            for (int i = 0; i < additionalLights.Length && i < originalLightIntensities.Length; i++)
            {
                if (additionalLights[i] != null)
                {
                    additionalLights[i].enabled = true;
                    additionalLights[i].intensity = originalLightIntensities[i];
                }
            }
        }
        
        // Restore ambient lighting
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.ambientSkyColor = originalAmbientSkyColor;
        RenderSettings.ambientEquatorColor = originalAmbientEquatorColor;
        RenderSettings.ambientGroundColor = originalAmbientGroundColor;
        
        // Restore fog
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogColor = originalFogColor;
        
        // Disable player light
        if (playerPointLight != null)
        {
            playerPointLight.enabled = false;
        }
        
        Debug.Log("Lighting reset to original state.");
    }
    
    public bool IsInDarkMode() => hasTriggered && !isTransitioning;
    public bool IsTransitioning() => isTransitioning;
    public float GetDarkModeProgress() => hasTriggered ? (isTransitioning ? (Time.time - transitionStartTime) / transitionDuration : 1f) : 0f;
}
