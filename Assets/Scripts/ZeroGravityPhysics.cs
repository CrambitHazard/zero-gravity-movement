using UnityEngine;

/// <summary>
/// Core physics system for zero gravity environments.
/// Handles momentum conservation, drift mechanics, and realistic space movement.
/// </summary>
public class ZeroGravityPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float dragCoefficient = 0.01f;
    [SerializeField] private float angularDrag = 0.05f;
    [SerializeField] private bool useRealisticPhysics = true;
    [SerializeField] private float maxVelocity = 20f;
    [SerializeField] private float maxAngularVelocity = 10f;
    
    [Header("Environmental Forces")]
    [SerializeField] private Vector3 ambientForce = Vector3.zero;
    [SerializeField] private float gravitationalConstant = 0f;
    
    private Rigidbody rb;
    private Vector3 lastVelocity;
    private Vector3 lastAngularVelocity;
    
    public Vector3 Velocity => rb.linearVelocity;
    public Vector3 AngularVelocity => rb.angularVelocity;
    public float Speed => rb.linearVelocity.magnitude;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        InitializePhysics();
    }
    
    void Start()
    {
        lastVelocity = rb.linearVelocity;
        lastAngularVelocity = rb.angularVelocity;
    }
    
    void FixedUpdate()
    {
        ApplyZeroGravityPhysics();
        ApplyEnvironmentalForces();
        EnforceVelocityLimits();
        UpdateLastKnownValues();
    }
    
    /// <summary>
    /// Initializes physics settings for zero gravity environment.
    /// </summary>
    private void InitializePhysics()
    {
        rb.useGravity = false;
        rb.linearDamping = dragCoefficient;
        rb.angularDamping = angularDrag;
        
        // Ensure continuous collision detection for fast moving objects
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    /// <summary>
    /// Applies zero gravity physics principles including momentum conservation.
    /// </summary>
    private void ApplyZeroGravityPhysics()
    {
        if (!useRealisticPhysics) return;
        
        // In zero gravity, objects maintain their momentum unless acted upon by external forces
        // Apply minimal drag to simulate very thin atmosphere or micro-particles
        Vector3 dragForce = -rb.linearVelocity * dragCoefficient * rb.mass;
        Vector3 angularDragTorque = -rb.angularVelocity * angularDrag * rb.mass;
        
        rb.AddForce(dragForce, ForceMode.Force);
        rb.AddTorque(angularDragTorque, ForceMode.Force);
    }
    
    /// <summary>
    /// Applies environmental forces like solar wind, magnetic fields, etc.
    /// </summary>
    private void ApplyEnvironmentalForces()
    {
        if (ambientForce != Vector3.zero)
        {
            rb.AddForce(ambientForce, ForceMode.Force);
        }
        
        // Apply minimal gravitational effects if needed
        if (gravitationalConstant > 0)
        {
            Vector3 gravitationalForce = Vector3.down * gravitationalConstant * rb.mass;
            rb.AddForce(gravitationalForce, ForceMode.Force);
        }
    }
    
    /// <summary>
    /// Enforces maximum velocity limits to prevent physics instabilities.
    /// </summary>
    private void EnforceVelocityLimits()
    {
        // Clamp linear velocity
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
        
        // Clamp angular velocity
        if (rb.angularVelocity.magnitude > maxAngularVelocity)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularVelocity;
        }
    }
    
    /// <summary>
    /// Updates last known velocity values for delta calculations.
    /// </summary>
    private void UpdateLastKnownValues()
    {
        lastVelocity = rb.linearVelocity;
        lastAngularVelocity = rb.angularVelocity;
    }
    
    /// <summary>
    /// Applies thrust force in the specified direction.
    /// </summary>
    /// <param name="thrustDirection">Normalized direction vector</param>
    /// <param name="thrustPower">Thrust force magnitude</param>
    public void ApplyThrust(Vector3 thrustDirection, float thrustPower)
    {
        Vector3 force = thrustDirection.normalized * thrustPower;
        rb.AddForce(force, ForceMode.Force);
    }
    
    /// <summary>
    /// Applies rotational thrust around the specified axis.
    /// </summary>
    /// <param name="axis">Rotation axis</param>
    /// <param name="torquePower">Torque magnitude</param>
    public void ApplyRotationalThrust(Vector3 axis, float torquePower)
    {
        Vector3 torque = axis.normalized * torquePower;
        rb.AddTorque(torque, ForceMode.Force);
    }
    
    /// <summary>
    /// Applies a braking force opposite to current velocity.
    /// </summary>
    /// <param name="brakingPower">Braking force magnitude</param>
    public void ApplyBraking(float brakingPower)
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector3 brakingForce = -rb.linearVelocity.normalized * brakingPower;
            rb.AddForce(brakingForce, ForceMode.Force);
        }
    }
    
    /// <summary>
    /// Applies rotational braking to reduce angular velocity.
    /// </summary>
    /// <param name="rotationalBrakingPower">Rotational braking torque magnitude</param>
    public void ApplyRotationalBraking(float rotationalBrakingPower)
    {
        if (rb.angularVelocity.magnitude > 0.1f)
        {
            Vector3 brakingTorque = -rb.angularVelocity.normalized * rotationalBrakingPower;
            rb.AddTorque(brakingTorque, ForceMode.Force);
        }
    }
    
    /// <summary>
    /// Gets the acceleration based on velocity change.
    /// </summary>
    /// <returns>Current acceleration vector</returns>
    public Vector3 GetAcceleration()
    {
        return (rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime;
    }
    
    /// <summary>
    /// Gets the angular acceleration based on angular velocity change.
    /// </summary>
    /// <returns>Current angular acceleration vector</returns>
    public Vector3 GetAngularAcceleration()
    {
        return (rb.angularVelocity - lastAngularVelocity) / Time.fixedDeltaTime;
    }
    
    /// <summary>
    /// Stops all movement immediately (emergency stop).
    /// </summary>
    public void EmergencyStop()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
