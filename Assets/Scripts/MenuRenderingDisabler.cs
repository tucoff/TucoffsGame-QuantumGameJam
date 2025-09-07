using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Completely disables retro rendering effects in menu/intro scenes
/// Multiple methods to ensure clean video playback
/// </summary>
public class MenuRenderingDisabler : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private Material retroEffectMaterial;
    [SerializeField] private bool findMaterialAutomatically = true;
    [SerializeField] private string materialName = "Shader Graphs_RetroCustomEffect";
    
    [Header("Disable Method")]
    [SerializeField] private DisableMethod disableMethod = DisableMethod.DisableMaterial;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool findCameraAutomatically = true;
    
    [Header("Post Processing")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private bool disablePostProcessing = true;
    
    private Material originalMaterial;
    private bool originalMaterialEnabled;
    private float originalVolumeWeight;
    
    public enum DisableMethod
    {
        DisableMaterial,        // Disable the material completely
        ReplaceWithStandard,    // Replace with standard material
        DisableRenderer,        // Disable the renderer using the material
        DisablePostProcess      // Only disable post-processing
    }
    
    void Start()
    {
        // Find components automatically
        if (findMaterialAutomatically && retroEffectMaterial == null)
        {
            retroEffectMaterial = FindMaterialByName();
        }
        
        if (findCameraAutomatically && targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
        }
        
        // Apply the chosen disable method
        ApplyDisableMethod();
    }
    
    void OnDestroy()
    {
        // Restore original state when leaving scene
        RestoreOriginalState();
    }
    
    private void ApplyDisableMethod()
    {
        switch (disableMethod)
        {
            case DisableMethod.DisableMaterial:
                DisableMaterialCompletely();
                break;
            case DisableMethod.ReplaceWithStandard:
                ReplaceWithStandardMaterial();
                break;
            case DisableMethod.DisableRenderer:
                DisableRenderersWithMaterial();
                break;
            case DisableMethod.DisablePostProcess:
                DisablePostProcessingOnly();
                break;
        }
        
        // Always disable post-processing if requested
        if (disablePostProcessing && postProcessVolume != null)
        {
            originalVolumeWeight = postProcessVolume.weight;
            postProcessVolume.weight = 0f;
            Debug.Log("Post-processing disabled for intro scene");
        }
    }
    
    private void DisableMaterialCompletely()
    {
        if (retroEffectMaterial == null) return;
        
        // Find all renderers using this material and disable them
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (renderer.materials[i] == retroEffectMaterial)
                {
                    renderer.enabled = false;
                    Debug.Log($"Disabled renderer on {renderer.name} to bypass retro effect");
                    break;
                }
            }
        }
    }
    
    private void ReplaceWithStandardMaterial()
    {
        if (retroEffectMaterial == null) return;
        
        // Create a standard unlit material
        Material standardMaterial = new Material(Shader.Find("Unlit/Texture"));
        
        // Find all renderers using the retro material and replace it
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == retroEffectMaterial)
                {
                    materials[i] = standardMaterial;
                    renderer.materials = materials;
                    Debug.Log($"Replaced retro material with standard on {renderer.name}");
                    break;
                }
            }
        }
    }
    
    private void DisableRenderersWithMaterial()
    {
        if (retroEffectMaterial == null) return;
        
        // Find and disable any objects using the retro effect material
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat == retroEffectMaterial)
                {
                    renderer.gameObject.SetActive(false);
                    Debug.Log($"Disabled GameObject {renderer.name} with retro effect");
                    break;
                }
            }
        }
    }
    
    private void DisablePostProcessingOnly()
    {
        // Only disable post-processing, leave materials alone
        Debug.Log("Only disabling post-processing effects");
    }
    
    private void RestoreOriginalState()
    {
        // Restore post-processing volume
        if (postProcessVolume != null && disablePostProcessing)
        {
            postProcessVolume.weight = originalVolumeWeight;
        }
    }
    
    /// <summary>
    /// Tries to find the material automatically
    /// </summary>
    private Material FindMaterialByName()
    {
        // Try Resources folder first
        Material mat = Resources.Load<Material>(materialName);
        if (mat != null) return mat;
        
        // Search in scene renderers
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material != null && material.name.Contains("RetroCustomEffect"))
                {
                    return material;
                }
            }
        }
        
        return null;
    }
}
