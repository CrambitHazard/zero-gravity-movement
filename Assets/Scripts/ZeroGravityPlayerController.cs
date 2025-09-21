using UnityEngine;

/// <summary>
/// Player controller for zero gravity movement.
/// Handles input, thruster controls, and movement in 3D space without gravity.
/// </summary>
[RequireComponent(typeof(ZeroGravityPhysics))]
public class ZeroGravityPlayerController : MonoBehaviour
{
    [Header("Thruster Settings")]
    [SerializeField] private float mainThrusterPower = 10f;
    [SerializeField] private float maneuvringThrusterPower = 5f;
    [SerializeField] private float rotationalThrusterPower = 8f;
    [SerializeField] private float brakingPower = 15f;
    
    [Header("Fuel System")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelConsumptionRate = 1f;
    [SerializeField] private float rotationalFuelConsumption = 0.8f;
    [SerializeField] private bool unlimitedFuel = false;
    
    [Header("Control Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private bool invertMouseY = false;
    [SerializeField] private float deadZone = 0.1f;
    [SerializeField] private bool relativeToCamera = true;
    
    [Header("Audio")]
    [SerializeField] private AudioSource thrusterAudio;
    [SerializeField] private AudioClip mainThrusterSound;
    [SerializeField] private AudioClip maneuvringThrusterSound;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem[] thrusterEffects;
    [SerializeField] private Light[] thrusterLights;
    
    private ZeroGravityPhysics zeroGravityPhysics;
    private Camera playerCamera;
    private float currentFuel;
    private bool isThrusting = false;
    private bool isRotating = false;
    
    // Input tracking
    private Vector2 mouseInput;
    private Vector3 movementInput;
    private bool brakingInput;
    private bool stabilizeInput;
    
    public float CurrentFuel => currentFuel;
    public float FuelPercentage => currentFuel / maxFuel;
    public bool HasFuel => unlimitedFuel || currentFuel > 0;
    public bool IsThrusting => isThrusting;
    public Vector3 Velocity => zeroGravityPhysics.Velocity;
    public float Speed => zeroGravityPhysics.Speed;
    
    void Awake()
    {
        zeroGravityPhysics = GetComponent<ZeroGravityPhysics>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        currentFuel = maxFuel;
        
        // Lock cursor for better control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleInput();
        UpdateAudio();
        UpdateVisualEffects();
    }
    
    void FixedUpdate()
    {
        ProcessMovement();
        ProcessRotation();
        ProcessBraking();
        ProcessStabilization();
    }
    
    /// <summary>
    /// Handles all player input for movement and rotation.
    /// </summary>
    private void HandleInput()
    {
        // Movement input (WASD + Space/Shift)
        movementInput = Vector3.zero;
        movementInput.x = Input.GetAxis("Horizontal"); // A/D
        movementInput.z = Input.GetAxis("Vertical");   // W/S
        
        // Vertical movement
        if (Input.GetKey(KeyCode.Space))
            movementInput.y += 1f;
        if (Input.GetKey(KeyCode.LeftShift))
            movementInput.y -= 1f;
        
        // Mouse look for rotation
        mouseInput.x = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseInput.y = Input.GetAxis("Mouse Y") * mouseSensitivity;
        if (invertMouseY)
            mouseInput.y = -mouseInput.y;
        
        // Braking and stabilization
        brakingInput = Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.X);
        stabilizeInput = Input.GetKey(KeyCode.V);
        
        // Toggle cursor lock
        if (Input.GetKeyDown(KeyCode.Escape))
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
        
        // Fuel debug (R to refuel)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefuelToMax();
        }
    }
    
    /// <summary>
    /// Processes movement input and applies thrust forces.
    /// </summary>
    private void ProcessMovement()
    {
        if (movementInput.magnitude < deadZone || !HasFuel)
        {
            isThrusting = false;
            return;
        }
        
        isThrusting = true;
        
        // Calculate thrust direction based on camera or world space
        Vector3 thrustDirection;
        if (relativeToCamera && playerCamera != null)
        {
            // Transform input relative to camera orientation
            thrustDirection = playerCamera.transform.TransformDirection(movementInput);
        }
        else
        {
            // Use world space
            thrustDirection = movementInput;
        }
        
        // Determine thrust power based on input type
        float thrustPower = mainThrusterPower;
        if (Mathf.Abs(movementInput.x) > 0 || Mathf.Abs(movementInput.y) > 0)
        {
            // Maneuvering thrusters for strafing and vertical movement
            thrustPower = maneuvringThrusterPower;
        }
        
        // Apply thrust
        zeroGravityPhysics.ApplyThrust(thrustDirection, thrustPower * movementInput.magnitude);
        
        // Consume fuel
        if (!unlimitedFuel)
        {
            float fuelToConsume = fuelConsumptionRate * movementInput.magnitude * Time.fixedDeltaTime;
            ConsumeFuel(fuelToConsume);
        }
    }
    
    /// <summary>
    /// Processes rotation input and applies rotational forces.
    /// </summary>
    private void ProcessRotation()
    {
        if (mouseInput.magnitude < deadZone || !HasFuel)
        {
            isRotating = false;
            return;
        }
        
        isRotating = true;
        
        // Calculate rotation axes
        Vector3 rotationAxis = Vector3.zero;
        
        // Yaw (horizontal mouse movement)
        rotationAxis += transform.up * mouseInput.x;
        
        // Pitch (vertical mouse movement)
        rotationAxis += transform.right * -mouseInput.y;
        
        // Apply rotational thrust
        zeroGravityPhysics.ApplyRotationalThrust(rotationAxis, rotationalThrusterPower);
        
        // Consume fuel for rotation
        if (!unlimitedFuel)
        {
            float fuelToConsume = rotationalFuelConsumption * mouseInput.magnitude * Time.fixedDeltaTime;
            ConsumeFuel(fuelToConsume);
        }
    }
    
    /// <summary>
    /// Processes braking input to reduce velocity.
    /// </summary>
    private void ProcessBraking()
    {
        if (!brakingInput || !HasFuel) return;
        
        // Apply linear braking
        zeroGravityPhysics.ApplyBraking(brakingPower);
        
        // Apply rotational braking
        zeroGravityPhysics.ApplyRotationalBraking(brakingPower * 0.5f);
        
        // Consume fuel for braking
        if (!unlimitedFuel)
        {
            float fuelToConsume = fuelConsumptionRate * 1.5f * Time.fixedDeltaTime;
            ConsumeFuel(fuelToConsume);
        }
    }
    
    /// <summary>
    /// Processes stabilization input to stop rotation.
    /// </summary>
    private void ProcessStabilization()
    {
        if (!stabilizeInput || !HasFuel) return;
        
        // Only apply rotational braking for stabilization
        zeroGravityPhysics.ApplyRotationalBraking(brakingPower);
        
        // Consume fuel for stabilization
        if (!unlimitedFuel)
        {
            float fuelToConsume = rotationalFuelConsumption * Time.fixedDeltaTime;
            ConsumeFuel(fuelToConsume);
        }
    }
    
    /// <summary>
    /// Updates thruster audio based on current state.
    /// </summary>
    private void UpdateAudio()
    {
        if (thrusterAudio == null) return;
        
        if (isThrusting || isRotating)
        {
            if (!thrusterAudio.isPlaying)
            {
                thrusterAudio.clip = isThrusting ? mainThrusterSound : maneuvringThrusterSound;
                thrusterAudio.Play();
            }
            
            // Adjust volume based on thrust intensity
            float targetVolume = Mathf.Max(movementInput.magnitude, mouseInput.magnitude * 0.5f);
            thrusterAudio.volume = Mathf.Lerp(thrusterAudio.volume, targetVolume, Time.deltaTime * 5f);
        }
        else
        {
            if (thrusterAudio.isPlaying)
            {
                thrusterAudio.volume = Mathf.Lerp(thrusterAudio.volume, 0f, Time.deltaTime * 10f);
                if (thrusterAudio.volume < 0.01f)
                {
                    thrusterAudio.Stop();
                }
            }
        }
    }
    
    /// <summary>
    /// Updates visual effects for thrusters.
    /// </summary>
    private void UpdateVisualEffects()
    {
        // Update particle effects
        if (thrusterEffects != null)
        {
            for (int i = 0; i < thrusterEffects.Length; i++)
            {
                if (thrusterEffects[i] != null)
                {
                    var emission = thrusterEffects[i].emission;
                    emission.enabled = isThrusting || isRotating;
                    
                    if (emission.enabled)
                    {
                        var rateOverTime = emission.rateOverTime;
                        float thrustIntensity = Mathf.Max(movementInput.magnitude, mouseInput.magnitude);
                        rateOverTime.constant = 50f * thrustIntensity;
                        emission.rateOverTime = rateOverTime;
                    }
                }
            }
        }
        
        // Update thruster lights
        if (thrusterLights != null)
        {
            for (int i = 0; i < thrusterLights.Length; i++)
            {
                if (thrusterLights[i] != null)
                {
                    thrusterLights[i].enabled = isThrusting || isRotating;
                    if (thrusterLights[i].enabled)
                    {
                        float thrustIntensity = Mathf.Max(movementInput.magnitude, mouseInput.magnitude);
                        thrusterLights[i].intensity = thrustIntensity * 2f;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Consumes fuel by the specified amount.
    /// </summary>
    /// <param name="amount">Amount of fuel to consume</param>
    private void ConsumeFuel(float amount)
    {
        if (unlimitedFuel) return;
        
        currentFuel = Mathf.Max(0, currentFuel - amount);
    }
    
    /// <summary>
    /// Refuels to maximum capacity.
    /// </summary>
    public void RefuelToMax()
    {
        currentFuel = maxFuel;
    }
    
    /// <summary>
    /// Adds fuel to current amount.
    /// </summary>
    /// <param name="amount">Amount of fuel to add</param>
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }
    
    /// <summary>
    /// Emergency stop - stops all movement immediately.
    /// </summary>
    public void EmergencyStop()
    {
        zeroGravityPhysics.EmergencyStop();
    }
    
    /// <summary>
    /// Sets unlimited fuel mode.
    /// </summary>
    /// <param name="unlimited">Whether fuel should be unlimited</param>
    public void SetUnlimitedFuel(bool unlimited)
    {
        unlimitedFuel = unlimited;
        if (unlimited)
        {
            currentFuel = maxFuel;
        }
    }
}
