using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatControls : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public bool eightDirectionalMovement = true;
    
    [Header("Physics Settings")]
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float maxSpeed = 8f;
    
    [Header("Beaching Settings")]
    public bool isBeached = false;
    public float beachedSpeedMultiplier = 0.2f;
    
    [Header("Optional Components")]
    public OceanDepthMap oceanDepthMap;
    public BoatType currentBoatType;
    
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private float currentSpeed;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // If no Rigidbody2D exists, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // No gravity for boats
            rb.drag = 2f; // Some water resistance
        }
    }
    
    void Update()
    {
        HandleInput();
        // CheckBeaching();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        // HandleRotation();
    }
    
    void HandleInput()
    {
        // Get input from WASD or arrow keys
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        
        // Normalize diagonal movement for 8-directional
        if (eightDirectionalMovement && movementInput.magnitude > 1f)
        {
            movementInput.Normalize();
        }
    }
    
    void HandleMovement()
    {
        // Calculate target velocity
        Vector2 targetVelocity = movementInput * moveSpeed;
        
        // Apply beaching penalty
        if (isBeached)
        {
            targetVelocity *= beachedSpeedMultiplier;
        }
        
        // Smooth acceleration/deceleration
        if (movementInput.magnitude > 0.1f)
        {
            // Accelerating
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerating
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, 
                deceleration * Time.fixedDeltaTime);
        }
        
        // Clamp to max speed
        currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxSpeed);
        
        // Apply velocity to rigidbody
        rb.velocity = currentVelocity;
        
        // Store current speed for other scripts
        currentSpeed = currentVelocity.magnitude;
    }
    
    // void HandleRotation()
    // {
    //     // Only rotate when moving
    //     if (movementInput.magnitude > 0.1f)
    //     {
    //         // Calculate target rotation based on movement direction
    //         float targetAngle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            
    //         // Adjust for sprite orientation (assuming sprite faces right by default)
    //         targetAngle -= 90f;
            
    //         // Smooth rotation
    //         float currentAngle = transform.eulerAngles.z;
    //         float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, 
    //             rotationSpeed * Time.fixedDeltaTime / 180f);
            
    //         transform.rotation = Quaternion.Euler(0, 0, newAngle);
    //     }
    // }
    
    // void CheckBeaching()
    // {
    //     // Only check if we have the depth map and boat type assigned
    //     if (oceanDepthMap != null && currentBoatType != null)
    //     {
    //         Vector2 currentPosition = transform.position;
    //         bool wasBeached = isBeached;
            
    //         isBeached = oceanDepthMap.IsBoatBeached(currentPosition, currentBoatType);
            
    //         // Trigger events when beaching status changes
    //         if (isBeached && !wasBeached)
    //         {
    //             OnBeached();
    //         }
    //         else if (!isBeached && wasBeached)
    //         {
    //             OnUnbeached();
    //         }
    //     }
    // }
    
    // Public methods for external control
    public void SetBeached(bool beached)
    {
        isBeached = beached;
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    public Vector2 GetMovementDirection()
    {
        return currentVelocity.normalized;
    }
    
    public bool IsMoving()
    {
        return currentSpeed > 0.1f;
    }
    
    // Events that can be overridden or connected to
    protected virtual void OnBeached()
    {
        Debug.Log($"{gameObject.name} has been beached!");
        // Add particle effects, sound, UI notifications, etc.
    }
    
    protected virtual void OnUnbeached()
    {
        Debug.Log($"{gameObject.name} is back in deep water!");
        // Remove beaching effects
    }
    
    // Alternative movement method without physics (if you prefer transform-based movement)
    // void HandleMovementTransform()
    // {
    //     Vector2 movement = movementInput * moveSpeed * Time.fixedDeltaTime;
        
    //     if (isBeached)
    //     {
    //         movement *= beachedSpeedMultiplier;
    //     }
        
    //     transform.Translate(movement, Space.World);
    // }
    
    // Method to check if boat can move to a specific position
    // public bool CanMoveTo(Vector2 targetPosition)
    // {
    //     if (oceanDepthMap != null && currentBoatType != null)
    //     {
    //         return oceanDepthMap.CanBoatNavigate(targetPosition, currentBoatType);
    //     }
    //     return true; // Allow movement if no depth checking is set up
    // }
    
    // // Method to get navigation warnings
    // public string GetNavigationWarning()
    // {
    //     if (isBeached)
    //     {
    //         return "BEACHED - Move to deeper water!";
    //     }
        
    //     if (oceanDepthMap != null && currentBoatType != null)
    //     {
    //         Vector2 currentPos = transform.position;
    //         float depth = oceanDepthMap.GetDepthAtWorldPosition(currentPos);
    //         float requiredDepth = currentBoatType.draft + currentBoatType.safetyMargin;
            
    //         if (depth < requiredDepth * 1.5f) // Warning zone
    //         {
    //             return "SHALLOW WATER - Proceed with caution!";
    //         }
    //     }
        
    //     return "";
    // }
}
