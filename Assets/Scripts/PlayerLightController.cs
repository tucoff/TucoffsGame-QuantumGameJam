using UnityEngine;

/// <summary>
/// Manages the player's personal light during dark mode
/// Provides smooth following, flickering effects, and intensity based on movement
/// </summary>
public class PlayerLightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light playerLight;
    [SerializeField] private bool createLightAutomatically = true;
    
    [Header("Light Properties")]
    [SerializeField] private float baseIntensity = 8f;
    [SerializeField] private float lightRange = 15f;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private Vector3 lightOffset = Vector3.up * 2f; // Position relative to player
    
    [Header("Effects")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float flickerSpeed = 5f;
    [SerializeField] private float flickerAmount = 0.15f; // Max variation (0-1)
    [SerializeField] private bool intensityBasedOnMovement = false;
    [SerializeField] private float movementIntensityMultiplier = 1.2f;
    
    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 10f;
    [SerializeField] private bool smoothFollowing = true;
    
    private Vector3 lastPosition;
    private float currentMovementSpeed;
    private bool isMoving;
    private Transform lightTransform;
    private GameObject lightObject;
    
    void Start()
    {
        SetupPlayerLight();
        lastPosition = transform.position;
    }
    
    void Update()
    {
        UpdateMovementTracking();
        UpdateLightEffects();
        UpdateLightPosition();
    }
    
    private void SetupPlayerLight()
    {
        // Try to find existing light first
        if (playerLight == null)
        {
            playerLight = GetComponentInChildren<Light>();
        }
        
        // Create light if needed and enabled
        if (playerLight == null && createLightAutomatically)
        {
            CreatePlayerLight();
        }
        
        if (playerLight != null)
        {
            lightTransform = playerLight.transform;
            lightObject = playerLight.gameObject;
            
            // Configure the light
            ConfigureLight();
            
            // Initially disable it (will be enabled by lighting manager)
            playerLight.enabled = false;
            
            Debug.Log($"Player light controller setup complete on {gameObject.name}");
        }
    }
    
    private void CreatePlayerLight()
    {
        lightObject = new GameObject("PlayerLight");
        lightObject.transform.SetParent(transform);
        lightObject.transform.localPosition = lightOffset;
        
        playerLight = lightObject.AddComponent<Light>();
        lightTransform = lightObject.transform;
        
        Debug.Log("Player light created automatically.");
    }
    
    private void ConfigureLight()
    {
        if (playerLight == null) return;
        
        playerLight.type = LightType.Point;
        playerLight.intensity = baseIntensity;
        playerLight.range = lightRange;
        playerLight.color = lightColor;
        playerLight.shadows = LightShadows.Soft;
        
        // Optional: Add a light cookie for more interesting shadows
        // playerLight.cookie = yourLightCookieTexture;
    }
    
    private void UpdateMovementTracking()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        currentMovementSpeed = movement.magnitude / Time.deltaTime;
        
        isMoving = currentMovementSpeed > 0.1f; // Threshold for considering "moving"
        
        lastPosition = currentPosition;
    }
    
    private void UpdateLightEffects()
    {
        if (playerLight == null || !playerLight.enabled) return;
        
        float targetIntensity = baseIntensity;
        
        // Movement-based intensity
        if (intensityBasedOnMovement && isMoving)
        {
            targetIntensity *= movementIntensityMultiplier;
        }
        
        // Flickering effect
        if (enableFlicker)
        {
            float flicker = 1f + Mathf.Sin(Time.time * flickerSpeed) * flickerAmount * 0.5f;
            flicker += Mathf.Sin(Time.time * flickerSpeed * 1.7f) * flickerAmount * 0.3f; // Secondary flicker
            targetIntensity *= flicker;
        }
        
        // Apply the intensity
        playerLight.intensity = targetIntensity;
    }
    
    private void UpdateLightPosition()
    {
        if (lightTransform == null) return;
        
        Vector3 targetPosition = transform.position + lightOffset;
        
        if (smoothFollowing)
        {
            lightTransform.position = Vector3.Lerp(lightTransform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);
        }
        else
        {
            lightTransform.position = targetPosition;
        }
    }
    
    /// <summary>
    /// Enable or disable the player light
    /// </summary>
    public void SetLightEnabled(bool enabled)
    {
        if (playerLight != null)
        {
            playerLight.enabled = enabled;
            Debug.Log($"Player light {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Get the player's light component
    /// </summary>
    public Light GetPlayerLight()
    {
        return playerLight;
    }
    
    /// <summary>
    /// Adjust light intensity (useful for gameplay effects)
    /// </summary>
    public void SetIntensityMultiplier(float multiplier)
    {
        baseIntensity *= multiplier;
        if (playerLight != null)
        {
            ConfigureLight();
        }
    }
    
    /// <summary>
    /// Temporarily boost light intensity (e.g., when player is in danger)
    /// </summary>
    public void BoostLight(float boostAmount, float duration)
    {
        if (playerLight != null)
        {
            StartCoroutine(LightBoostCoroutine(boostAmount, duration));
        }
    }
    
    private System.Collections.IEnumerator LightBoostCoroutine(float boostAmount, float duration)
    {
        float originalIntensity = baseIntensity;
        baseIntensity += boostAmount;
        
        yield return new WaitForSeconds(duration);
        
        baseIntensity = originalIntensity;
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize light range in editor
        if (playerLight != null)
        {
            Gizmos.color = lightColor;
            Gizmos.DrawWireSphere(transform.position + lightOffset, lightRange);
        }
    }
}
