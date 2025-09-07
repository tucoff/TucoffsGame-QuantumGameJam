using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// Advanced Lighting Manager for dramatic darkness effects and camera visual enhancements.
/// This system provides comprehensive lighting control including:
/// - Extreme darkness mode with survival lighting
/// - Player light with atmospheric flickering
/// - Enhanced emissions for better visibility in darkness
/// - Integration with camera effects system
/// - Complete scene lighting management
/// 
/// Integrated features from AdvancedLightingController:
/// - Advanced player light creation and control
/// - Atmospheric flickering effects
/// - Enhanced fog settings for darkness
/// - Complete light disabling for maximum drama
/// </summary>
public class LightingManager : MonoBehaviour
{
    [Header("Lighting References")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light[] playerPointLights; // Point lights that should follow/illuminate the player
    [SerializeField] private Light[] additionalLights; // Any other lights to control
    [SerializeField] private float playerLightIntensity = 10f;
    [SerializeField] private float playerLightRange = 20f;
    [SerializeField] private Color playerLightColor = Color.white;
    [SerializeField] private bool enablePlayerLightFlicker = true;
    [SerializeField] private float flickerSpeed = 4f;
    [SerializeField] private float flickerIntensity = 0.1f;
    
    [Header("Advanced Dark Mode Settings")]
    [SerializeField] private bool disableAllLights = true; // Completely turn off all global lights
    [SerializeField] private float darkAmbientIntensity = 0.02f; // Very minimal ambient
    [SerializeField] private Color darkAmbientColor = new Color(0.01f, 0.01f, 0.05f, 1f); // Almost black
    [SerializeField] private float darkFogDensity = 0.05f; // Add fog for atmosphere
    [SerializeField] private Color darkFogColor = new Color(0.05f, 0.05f, 0.1f, 1f);
    
    [Header("Lighting Settings")]
    [SerializeField] private float lightSwitchTime = 120f; // Time in seconds when lights go out
    [SerializeField] private float transitionDuration = 1.5f; // Faster transition for dramatic effect
    
    [Header("Beam Enhancement")]
    [SerializeField] private bool enhanceBeamVisibility = true;
    [SerializeField] private float beamGlowIntensity = 15f; // MUCH higher intensity for extreme darkness
    [SerializeField] private float emissiveMaterialBoost = 5f; // Multiplier for emissive materials
    [SerializeField] private float particleSystemBoost = 3f; // Multiplier for particle system emissions
    
    [Header("Original Lighting Values")]
    private float originalDirectionalIntensity;
    private Color originalDirectionalColor;
    private Color originalAmbientLight;
    private float originalAmbientIntensity;
    private Color originalAmbientSkyColor;
    private Color originalAmbientEquatorColor;
    private Color originalAmbientGroundColor;
    private bool originalFogEnabled;
    private FogMode originalFogMode;
    private float originalFogDensity;
    private Color originalFogColor;
    private float originalReflectionIntensity;
    private int originalDefaultReflectionResolution;
    
    [Header("Dark Mode Values")]
    // Removed unused pointLightRange variable
    
    private bool hasTriggeredLightSwitch = false;
    private bool isTransitioning = false;
    private float transitionStartTime;
    private GameObject player;
    
    // Store all scene lights for restoration
    private Light[] allSceneLights;
    private bool[] originalLightStates;
    
    // Store original emission values for restoration
    private Renderer[] emissiveRenderers;
    private Color[] originalEmissionColors;
    private ParticleSystem[] enhancedParticleSystems;
    private Color[] originalParticleColors;
    private Light[] enhancedDecoLights;
    private float[] originalDecoLightIntensities;
    private float[] originalDecoLightRanges;
    
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
        originalAmbientSkyColor = RenderSettings.ambientSkyColor;
        originalAmbientEquatorColor = RenderSettings.ambientEquatorColor;
        originalAmbientGroundColor = RenderSettings.ambientGroundColor;
        
        // Store fog settings
        originalFogEnabled = RenderSettings.fog;
        originalFogMode = RenderSettings.fogMode;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogColor = RenderSettings.fogColor;
        
        // Store reflection settings
        originalReflectionIntensity = RenderSettings.reflectionIntensity;
        originalDefaultReflectionResolution = RenderSettings.defaultReflectionResolution;
        
        // Store all scene lights and their enabled states
        allSceneLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        originalLightStates = new bool[allSceneLights.Length];
        for (int i = 0; i < allSceneLights.Length; i++)
        {
            originalLightStates[i] = allSceneLights[i].enabled;
        }
        
        Debug.Log($"Original lighting stored - Directional: {originalDirectionalIntensity}, Ambient: {originalAmbientIntensity}, Fog: {originalFogEnabled}, Scene Lights: {allSceneLights.Length}");
    }
    
    private void SetupPlayerPointLights()
    {
        if (player == null) return;
        
        // Create point lights around the player
        playerPointLights = new Light[1]; // Start with one main light
        
        // Create main player light
        GameObject lightObj = new GameObject("Player Survival Light");
        lightObj.transform.SetParent(player.transform);
        lightObj.transform.localPosition = new Vector3(0, 2, 0); // Above the player
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.intensity = playerLightIntensity;
        pointLight.range = playerLightRange;
        pointLight.color = playerLightColor;
        pointLight.shadows = LightShadows.Soft;
        
        playerPointLights[0] = pointLight;
        
        Debug.Log("Player survival light created and configured with advanced settings.");
    }
    
    /// <summary>
    /// Disable all lights in the scene except player point lights for maximum darkness
    /// </summary>
    private void DisableAllNonPlayerLights()
    {
        if (allSceneLights == null) return;
        
        int disabledCount = 0;
        for (int i = 0; i < allSceneLights.Length; i++)
        {
            Light light = allSceneLights[i];
            if (light != null && light.enabled)
            {
                // Don't disable player point lights
                bool isPlayerLight = false;
                if (playerPointLights != null)
                {
                    foreach (Light playerLight in playerPointLights)
                    {
                        if (light == playerLight)
                        {
                            isPlayerLight = true;
                            break;
                        }
                    }
                }
                
                if (!isPlayerLight)
                {
                    light.enabled = false;
                    disabledCount++;
                }
            }
        }
        
        Debug.Log($"Disabled {disabledCount} scene lights for EXTREME darkness effect");
    }
    
    private void StartLightSwitch()
    {
        if (hasTriggeredLightSwitch) return;
        
        hasTriggeredLightSwitch = true;
        isTransitioning = true;
        transitionStartTime = Time.time;
        
        // Disable ALL other lights in the scene for maximum darkness
        DisableAllNonPlayerLights();
        
        // Enable player point lights
        SetPlayerPointLightsEnabled(true);
        
        // Enhance beam visibility in dark mode
        if (enhanceBeamVisibility)
        {
            EnhanceBeamVisibility();
        }
        
        // Switch music to dark mode with pitch change
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SwitchToDarkMode();
            Debug.Log("Music switched to dark mode with pitch change");
        }
        else
        {
            Debug.LogWarning("MusicManager not found - music pitch will not change");
        }
        
        // Modify enemy spawn distances to make them spawn closer
        EnemySpawn enemySpawn = FindFirstObjectByType<EnemySpawn>();
        if (enemySpawn != null)
        {
            enemySpawn.SetDarkModeSpawning(true);
            Debug.Log("Enemy spawn distances reduced for dark mode");
        }
        else
        {
            Debug.LogWarning("EnemySpawn not found - spawn distances will not change");
        }
        
        Debug.Log("EXTREME DARKNESS! All lights disabled, transitioning to pitch black mode...");
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
            Debug.Log("EXTREME dark mode transition complete!");
        }
        
        // Animate directional light to complete darkness
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
            directionalLight.color = Color.Lerp(originalDirectionalColor, Color.black, progress);
        }
        
        // Handle additional lights
        if (additionalLights != null)
        {
            for (int i = 0; i < additionalLights.Length; i++)
            {
                if (additionalLights[i] != null)
                {
                    if (disableAllLights)
                    {
                        additionalLights[i].intensity = Mathf.Lerp(additionalLights[i].intensity, 0f, progress);
                        if (progress >= 0.95f)
                        {
                            additionalLights[i].enabled = false;
                        }
                    }
                    else
                    {
                        additionalLights[i].intensity = Mathf.Lerp(additionalLights[i].intensity, additionalLights[i].intensity * 0.1f, progress);
                    }
                }
            }
        }
        
        // Animate ambient lighting to EXTREME darkness using both old and new settings
        RenderSettings.ambientIntensity = Mathf.Lerp(originalAmbientIntensity, darkAmbientIntensity, progress);
        RenderSettings.ambientLight = Color.Lerp(originalAmbientLight, darkAmbientColor, progress);
        
        // Make ALL ambient colors extremely dark for complete darkness effect
        RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, darkAmbientColor, progress);
        RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, darkAmbientColor * 0.5f, progress);
        RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, darkAmbientColor * 0.2f, progress);
        
        // Add dense fog to make distant objects invisible - use new fog settings
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, darkFogDensity, progress);
        RenderSettings.fogColor = Color.Lerp(originalFogColor, darkFogColor, progress);
        
        // Also reduce reflection intensity for even darker environment
        RenderSettings.reflectionIntensity = Mathf.Lerp(1f, 0.1f, progress);
        RenderSettings.defaultReflectionResolution = 16; // Lower quality reflections
        
        Debug.Log($"EXTREME Dark Mode Progress: {progress * 100:F0}% - Fog Density: {RenderSettings.fogDensity:F3}");
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
        // Add intensity flickering for atmospheric effect
        if (enablePlayerLightFlicker)
        {
            foreach (Light light in playerPointLights)
            {
                if (light != null)
                {
                    float flicker = Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity + 1f;
                    light.intensity = playerLightIntensity * flicker;
                }
            }
        }
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
        else
        {
            Debug.Log("Light switch has already been triggered!");
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
        RenderSettings.ambientSkyColor = originalAmbientSkyColor;
        RenderSettings.ambientEquatorColor = originalAmbientEquatorColor;
        RenderSettings.ambientGroundColor = originalAmbientGroundColor;
        
        // Restore fog settings
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogMode = originalFogMode;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogColor = originalFogColor;
        
        // Restore reflection settings
        RenderSettings.reflectionIntensity = originalReflectionIntensity;
        RenderSettings.defaultReflectionResolution = originalDefaultReflectionResolution;
        
        // Restore all scene lights to their original states
        if (allSceneLights != null && originalLightStates != null)
        {
            int restoredCount = 0;
            for (int i = 0; i < allSceneLights.Length && i < originalLightStates.Length; i++)
            {
                if (allSceneLights[i] != null)
                {
                    allSceneLights[i].enabled = originalLightStates[i];
                    restoredCount++;
                }
            }
            Debug.Log($"Restored {restoredCount} scene lights to original states");
        }
        
        // Disable player point lights
        SetPlayerPointLightsEnabled(false);
        
        // Restore all enhanced emissions
        RestoreAllEnhancedEmissions();
        
        // Reset music to normal mode
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SwitchToNormalMode();
            Debug.Log("Music reset to normal mode");
        }
        
        // Reset enemy spawn distances to normal
        EnemySpawn enemySpawn = FindFirstObjectByType<EnemySpawn>();
        if (enemySpawn != null)
        {
            enemySpawn.SetDarkModeSpawning(false);
            Debug.Log("Enemy spawn distances reset to normal");
        }
        
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
    /// Check if currently transitioning to dark mode
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    /// <summary>
    /// Get the current dark mode progress (0 to 1)
    /// </summary>
    public float GetDarkModeProgress()
    {
        return hasTriggeredLightSwitch ? (isTransitioning ? (Time.time - transitionStartTime) / transitionDuration : 1f) : 0f;
    }
    
    /// <summary>
    /// Enhance ALL emissions and visibility effects when lights go out
    /// </summary>
    private void EnhanceBeamVisibility()
    {
        int totalEnhanced = 0;
        
        // Find all beam objects and enhance their glow
        BeamGlowEffect[] beamEffects = FindObjectsByType<BeamGlowEffect>(FindObjectsSortMode.None);
        foreach (BeamGlowEffect beam in beamEffects)
        {
            if (beam != null)
            {
                // Increase glow intensity for better visibility in darkness
                beam.SetGlowIntensity(beamGlowIntensity);
                totalEnhanced++;
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
            totalEnhanced++;
        }
        
        EnemyBeamRotation[] enemyBeams = FindObjectsByType<EnemyBeamRotation>(FindObjectsSortMode.None);
        foreach (EnemyBeamRotation beam in enemyBeams)
        {
            BeamGlowEffect glowEffect = beam.GetComponent<BeamGlowEffect>();
            if (glowEffect == null)
            {
                glowEffect = beam.gameObject.AddComponent<BeamGlowEffect>();
                // Set enemy beam color to bright orange/red for maximum visibility
                glowEffect.SetGlowColor(new Color(2f, 0.6f, 0f, 1f)); // Much brighter orange
            }
            glowEffect.SetGlowIntensity(beamGlowIntensity);
            totalEnhanced++;
        }
        
        // Store and enhance ALL Renderers with emissive materials
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        List<Renderer> emissiveRenderersList = new List<Renderer>();
        List<Color> originalEmissionsList = new List<Color>();
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer.materials != null)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material != null && material.HasProperty("_EmissionColor"))
                    {
                        // Store original emission color
                        Color currentEmission = material.GetColor("_EmissionColor");
                        
                        // If there's any emission, boost it significantly
                        if (currentEmission.maxColorComponent > 0.01f)
                        {
                            emissiveRenderersList.Add(renderer);
                            originalEmissionsList.Add(currentEmission);
                            
                            Color boostedEmission = currentEmission * emissiveMaterialBoost;
                            material.SetColor("_EmissionColor", boostedEmission);
                            material.EnableKeyword("_EMISSION");
                            
                            Debug.Log($"Boosted emission on {renderer.gameObject.name} - {material.name}");
                        }
                        // If barely any emission, add some bright emission
                        else if (currentEmission.maxColorComponent <= 0.01f && currentEmission.maxColorComponent > 0f)
                        {
                            emissiveRenderersList.Add(renderer);
                            originalEmissionsList.Add(currentEmission);
                            
                            Color brightEmission = Color.white * 0.5f;
                            material.SetColor("_EmissionColor", brightEmission);
                            material.EnableKeyword("_EMISSION");
                            
                            Debug.Log($"Added bright emission to {renderer.gameObject.name} - {material.name}");
                        }
                    }
                }
            }
        }
        
        // Store emissive renderers for restoration
        emissiveRenderers = emissiveRenderersList.ToArray();
        originalEmissionColors = originalEmissionsList.ToArray();
        
        // Store and enhance ALL Particle Systems
        ParticleSystem[] allParticleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        List<ParticleSystem> enhancedParticlesList = new List<ParticleSystem>();
        List<Color> originalParticleColorsList = new List<Color>();
        
        foreach (ParticleSystem particles in allParticleSystems)
        {
            if (particles != null)
            {
                var main = particles.main;
                Color startColor = main.startColor.color;
                
                if (startColor.maxColorComponent > 0.1f)
                {
                    enhancedParticlesList.Add(particles);
                    originalParticleColorsList.Add(startColor);
                    
                    main.startColor = startColor * particleSystemBoost;
                    Debug.Log($"Enhanced particle system: {particles.gameObject.name}");
                }
            }
        }
        
        // Store enhanced particle systems for restoration
        enhancedParticleSystems = enhancedParticlesList.ToArray();
        originalParticleColors = originalParticleColorsList.ToArray();
        
        // Store and enhance decorative lights
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        List<Light> enhancedDecoLightsList = new List<Light>();
        List<float> originalDecoIntensitiesList = new List<float>();
        List<float> originalDecoRangesList = new List<float>();
        
        foreach (Light light in allLights)
        {
            if (light != null && light != directionalLight && !IsPlayerLight(light))
            {
                if (light.intensity > 0 && light.intensity < 5f)
                {
                    enhancedDecoLightsList.Add(light);
                    originalDecoIntensitiesList.Add(light.intensity);
                    originalDecoRangesList.Add(light.range);
                    
                    light.intensity *= 3f; // Triple the intensity
                    light.range *= 1.5f; // Increase range
                    Debug.Log($"Enhanced decorative light: {light.gameObject.name}");
                }
            }
        }
        
        // Store enhanced decorative lights for restoration
        enhancedDecoLights = enhancedDecoLightsList.ToArray();
        originalDecoLightIntensities = originalDecoIntensitiesList.ToArray();
        originalDecoLightRanges = originalDecoRangesList.ToArray();
        
        int materialEnhanced = emissiveRenderers.Length;
        int particleEnhanced = enhancedParticleSystems.Length;
        int lightEnhanced = enhancedDecoLights.Length;
        
        Debug.Log($"=== EMISSION ENHANCEMENT COMPLETE ===");
        Debug.Log($"Beam Effects Enhanced: {totalEnhanced}");
        Debug.Log($"Emissive Materials Enhanced: {materialEnhanced}");
        Debug.Log($"Particle Systems Enhanced: {particleEnhanced}");
        Debug.Log($"Decorative Lights Enhanced: {lightEnhanced}");
        Debug.Log($"Total Enhanced Objects: {totalEnhanced + materialEnhanced + particleEnhanced + lightEnhanced}");
    }
    
    /// <summary>
    /// Check if a light is one of the player lights
    /// </summary>
    private bool IsPlayerLight(Light light)
    {
        if (playerPointLights == null) return false;
        
        foreach (Light playerLight in playerPointLights)
        {
            if (light == playerLight) return true;
        }
        return false;
    }
    
    /// <summary>
    /// Restore all enhanced emissions to their original values
    /// </summary>
    private void RestoreAllEnhancedEmissions()
    {
        int totalRestored = 0;
        
        // Restore emissive materials
        if (emissiveRenderers != null && originalEmissionColors != null)
        {
            for (int i = 0; i < emissiveRenderers.Length && i < originalEmissionColors.Length; i++)
            {
                if (emissiveRenderers[i] != null && emissiveRenderers[i].materials != null)
                {
                    foreach (Material material in emissiveRenderers[i].materials)
                    {
                        if (material != null && material.HasProperty("_EmissionColor"))
                        {
                            material.SetColor("_EmissionColor", originalEmissionColors[i]);
                            totalRestored++;
                        }
                    }
                }
            }
            Debug.Log($"Restored {emissiveRenderers.Length} emissive materials");
        }
        
        // Restore particle systems
        if (enhancedParticleSystems != null && originalParticleColors != null)
        {
            for (int i = 0; i < enhancedParticleSystems.Length && i < originalParticleColors.Length; i++)
            {
                if (enhancedParticleSystems[i] != null)
                {
                    var main = enhancedParticleSystems[i].main;
                    main.startColor = originalParticleColors[i];
                    totalRestored++;
                }
            }
            Debug.Log($"Restored {enhancedParticleSystems.Length} particle systems");
        }
        
        // Restore decorative lights
        if (enhancedDecoLights != null && originalDecoLightIntensities != null && originalDecoLightRanges != null)
        {
            for (int i = 0; i < enhancedDecoLights.Length && i < originalDecoLightIntensities.Length && i < originalDecoLightRanges.Length; i++)
            {
                if (enhancedDecoLights[i] != null)
                {
                    enhancedDecoLights[i].intensity = originalDecoLightIntensities[i];
                    enhancedDecoLights[i].range = originalDecoLightRanges[i];
                    totalRestored++;
                }
            }
            Debug.Log($"Restored {enhancedDecoLights.Length} decorative lights");
        }
        
        Debug.Log($"Total emissions restored: {totalRestored}");
    }
}
