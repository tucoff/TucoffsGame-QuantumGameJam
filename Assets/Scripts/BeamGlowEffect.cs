using UnityEngine;

public class BeamGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = new Color(1f, 0.2f, 1f, 1f); // Bright magenta
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float glowRange = 5f;
    [SerializeField] private bool addPointLight = true;
    
    [Header("Material Enhancement")]
    [SerializeField] private float emissionMultiplier = 2f;
    
    private Light glowLight;
    private Renderer beamRenderer;
    private Material beamMaterial;
    
    void Start()
    {
        // Get the renderer component
        beamRenderer = GetComponent<Renderer>();
        
        if (beamRenderer != null)
        {
            // Get a copy of the material to avoid modifying the original asset
            beamMaterial = beamRenderer.material;
            
            // Enhance the emission color
            EnhanceEmission();
        }
        
        // Add a point light for additional glow effect
        if (addPointLight)
        {
            AddGlowLight();
        }
    }
    
    void Update()
    {
        // Optionally add pulsing effect
        if (beamMaterial != null)
        {
            // Create a subtle pulsing effect
            float pulse = Mathf.Sin(Time.time * 3f) * 0.2f + 1f;
            Color currentEmission = beamMaterial.GetColor("_EmissionColor");
            beamMaterial.SetColor("_EmissionColor", currentEmission * pulse);
        }
        
        // Update light intensity with pulse
        if (glowLight != null)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.3f + 1f;
            glowLight.intensity = glowIntensity * pulse;
        }
    }
    
    private void EnhanceEmission()
    {
        if (beamMaterial == null) return;
        
        // Enable emission
        beamMaterial.EnableKeyword("_EMISSION");
        
        // Set bright emission color
        Vector3 emissionColor = new Vector3(glowColor.r, glowColor.g, glowColor.b) * emissionMultiplier;
        beamMaterial.SetColor("_EmissionColor", new Color(emissionColor.x, emissionColor.y, emissionColor.z, 1f));
        
        // Make the base color brighter
        beamMaterial.SetColor("_BaseColor", glowColor);
        beamMaterial.SetColor("_Color", glowColor);
        
        Debug.Log($"Enhanced beam material emission with color: {emissionColor}");
    }
    
    private void AddGlowLight()
    {
        // Validate that the gameObject exists
        if (gameObject == null)
        {
            Debug.LogError("BeamGlowEffect: gameObject is null, cannot add light");
            return;
        }
        
        // Check if there's already a Light component
        glowLight = GetComponent<Light>();
        if (glowLight != null)
        {
            // Use existing light component
            Debug.Log($"Using existing Light component on beam: {gameObject.name}");
        }
        else
        {
            // Create a new light component for additional glow
            glowLight = gameObject.AddComponent<Light>();
            Debug.Log($"Added new glow light to beam: {gameObject.name}");
        }
        
        // Configure the light properties
        if (glowLight != null)
        {
            glowLight.type = LightType.Point;
            glowLight.color = glowColor;
            glowLight.intensity = glowIntensity;
            glowLight.range = glowRange;
            glowLight.shadows = LightShadows.None; // No shadows for performance
        }
        else
        {
            Debug.LogError($"Failed to get or create Light component on: {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Set custom glow color
    /// </summary>
    /// <param name="color">The glow color</param>
    public void SetGlowColor(Color color)
    {
        glowColor = color;
        
        if (beamMaterial != null)
        {
            Vector3 emissionColor = new Vector3(color.r, color.g, color.b) * emissionMultiplier;
            beamMaterial.SetColor("_EmissionColor", new Color(emissionColor.x, emissionColor.y, emissionColor.z, 1f));
            beamMaterial.SetColor("_BaseColor", color);
            beamMaterial.SetColor("_Color", color);
        }
        
        if (glowLight != null)
        {
            glowLight.color = color;
        }
    }
    
    /// <summary>
    /// Set glow intensity
    /// </summary>
    /// <param name="intensity">Intensity value</param>
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        
        if (glowLight != null)
        {
            glowLight.intensity = intensity;
        }
    }
    
    void OnDestroy()
    {
        // Clean up the material copy
        if (beamMaterial != null)
        {
            DestroyImmediate(beamMaterial);
        }
    }
}
