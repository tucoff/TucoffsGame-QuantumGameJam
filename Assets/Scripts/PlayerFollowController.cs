using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerFollowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerSphere;
    [SerializeField] private Camera followCamera;
    [SerializeField] private GameObject shootObject;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 3f;
    [SerializeField] private float smoothSpeed = 5f;
    
    [Header("Shooting Settings")]
    [SerializeField] private float shootDuration = 0.5f;
    [SerializeField] private float shootCooldown = 2f;
    
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    
    // Private variables
    private Vector2 lookInput;
    private float currentCameraAngleY;
    private Vector3 targetCameraPosition;
    private Vector3 currentCameraPosition;
    private bool canShoot = true;
    private Coroutine currentShootCoroutine;
    
    // Input Actions
    private InputAction lookAction;
    private InputAction attackAction;
    
    void Start()
    {
        // Auto-assign references if not set
        if (playerSphere == null)
        {
            GameObject sphere = GameObject.FindWithTag("Player");
            if (sphere == null)
                sphere = GameObject.Find("Player");
            if (sphere != null)
                playerSphere = sphere.transform;
        }
        
        if (followCamera == null)
            followCamera = Camera.main;
        
        // Setup Input Actions
        if (inputActions == null)
        {
            // Try to find the InputSystem_Actions asset
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActions == null)
            {
                Debug.LogError("InputSystem_Actions asset not found! Please assign it in the inspector or place it in a Resources folder.");
                return;
            }
        }
        
        // Get specific actions
        lookAction = inputActions.FindActionMap("Player").FindAction("Look");
        attackAction = inputActions.FindActionMap("Player").FindAction("Attack");
        
        // Subscribe to attack action
        if (attackAction != null)
            attackAction.performed += OnAttackPerformed;
        
        // Enable input actions
        inputActions.Enable();
        
        // Free cursor (unlocked and visible)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Initialize camera position
        if (playerSphere != null && followCamera != null)
        {
            currentCameraPosition = playerSphere.position + new Vector3(0, cameraHeight, cameraDistance);
            followCamera.transform.position = currentCameraPosition;
            followCamera.transform.LookAt(playerSphere);
        }
    }
    
    void Update()
    {
        if (playerSphere == null || followCamera == null) return;
        
        HandleInput();
        HandleCameraRotation();
        UpdateCameraPosition();
        
        // Toggle cursor lock with Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    void HandleInput()
    {
        // Get look input from Input System - only need mouse for camera rotation
        if (lookAction != null)
            lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity;
    }
    
    void HandleCameraRotation()
    {
        // Update camera rotation angles - only horizontal rotation
        currentCameraAngleY += lookInput.x;
        
        // Rotate the sphere to look in the same direction as the camera
        if (playerSphere != null)
        {
            Vector3 sphereRotation = playerSphere.eulerAngles;
            sphereRotation.y = currentCameraAngleY;
            playerSphere.eulerAngles = sphereRotation;
        }
    }
    
    void UpdateCameraPosition()
    {
        // Calculate target camera position based on horizontal rotation only
        float radianY = currentCameraAngleY * Mathf.Deg2Rad;
        
        // Calculate position relative to player - fixed height, rotating around horizontally
        Vector3 offset = new Vector3(
            Mathf.Sin(radianY) * cameraDistance,
            cameraHeight,
            Mathf.Cos(radianY) * cameraDistance
        );
        
        targetCameraPosition = playerSphere.position + offset;
        
        // Smooth camera movement
        currentCameraPosition = Vector3.Lerp(currentCameraPosition, targetCameraPosition, smoothSpeed * Time.deltaTime);
        followCamera.transform.position = currentCameraPosition;
        
        // Make camera look at player
        followCamera.transform.LookAt(playerSphere.position + Vector3.up * 0.5f);
    }
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (canShoot && shootObject != null)
        {
            currentShootCoroutine = StartCoroutine(ShootCoroutine());
        }
    }
    
    private IEnumerator ShootCoroutine()
    {
        // Prevent shooting during cooldown
        canShoot = false;
        
        // Turn on shoot object
        shootObject.SetActive(true);
        
        // Wait for shoot duration (0.5 seconds)
        yield return new WaitForSeconds(shootDuration);
        
        // Turn off shoot object
        shootObject.SetActive(false);
        
        // Wait for cooldown (2 seconds)
        yield return new WaitForSeconds(shootCooldown);
        
        // Allow shooting again
        canShoot = true;
        currentShootCoroutine = null;
    }
    
    /// <summary>
    /// Called when the player successfully hits an enemy - resets shooting cooldown
    /// </summary>
    public void OnSuccessfulHit()
    {
        // If we're currently in a shooting cooldown, reset it
        if (!canShoot && currentShootCoroutine != null)
        {
            StopCoroutine(currentShootCoroutine);
            currentShootCoroutine = null;
            canShoot = true;
            Debug.Log("Shot cooldown reset due to successful hit!");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from input actions
        if (attackAction != null)
            attackAction.performed -= OnAttackPerformed;
            
        // Disable input actions when the object is destroyed
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerSphere == null) return;
        
        // Draw camera orbit visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerSphere.position, cameraDistance);
        
        if (followCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerSphere.position, followCamera.transform.position);
        }
    }
}
