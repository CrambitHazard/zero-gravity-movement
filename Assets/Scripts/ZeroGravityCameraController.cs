using UnityEngine;

/// <summary>
/// Camera controller optimized for zero gravity environments.
/// Provides smooth following, orientation tracking, and stabilization options.
/// </summary>
public class ZeroGravityCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2, -5);
    [SerializeField] private bool useLocalOffset = true;
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float maxFollowDistance = 20f;
    [SerializeField] private bool smoothFollow = true;
    
    [Header("Look Settings")]
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private Vector3 lookAtOffset = Vector3.zero;
    [SerializeField] private float lookSmoothness = 2f;
    
    [Header("Stabilization")]
    [SerializeField] private bool stabilizeHorizon = false;
    [SerializeField] private Vector3 upVector = Vector3.up;
    [SerializeField] private float stabilizationSpeed = 1f;
    
    [Header("Velocity Prediction")]
    [SerializeField] private bool predictMovement = true;
    [SerializeField] private float predictionDistance = 5f;
    [SerializeField] private float predictionSmoothness = 2f;
    
    [Header("Collision Avoidance")]
    [SerializeField] private bool avoidCollisions = true;
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float collisionBuffer = 1f;
    
    [Header("Zoom Settings")]
    [SerializeField] private bool dynamicZoom = false;
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 90f;
    [SerializeField] private float zoomSpeed = 2f;
    
    private Camera cam;
    private ZeroGravityPlayerController playerController;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private Vector3 predictedPosition;
    private float baseFOV;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
        
        baseFOV = cam.fieldOfView;
        
        // Auto-find target if not set
        if (target == null)
        {
            playerController = FindFirstObjectByType<ZeroGravityPlayerController>();
            if (playerController != null)
            {
                target = playerController.transform;
            }
        }
        else
        {
            playerController = target.GetComponent<ZeroGravityPlayerController>();
        }
    }
    
    void Start()
    {
        if (target != null)
        {
            // Initialize position
            UpdateTargetPosition();
            transform.position = targetPosition;
            
            if (lookAtTarget)
            {
                transform.LookAt(target.position + lookAtOffset);
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        UpdateTargetPosition();
        UpdateCameraPosition();
        UpdateCameraRotation();
        UpdateDynamicZoom();
    }
    
    /// <summary>
    /// Updates the target position for the camera based on movement prediction.
    /// </summary>
    private void UpdateTargetPosition()
    {
        Vector3 basePosition;
        
        if (useLocalOffset)
        {
            basePosition = target.position + target.TransformDirection(offsetPosition);
        }
        else
        {
            basePosition = target.position + offsetPosition;
        }
        
        // Apply movement prediction
        if (predictMovement && playerController != null)
        {
            Vector3 velocity = playerController.Velocity;
            predictedPosition = target.position + velocity.normalized * 
                               Mathf.Min(velocity.magnitude * predictionDistance, maxFollowDistance * 0.5f);
            
            // Blend predicted position with base position
            Vector3 predictedOffset = Vector3.Lerp(target.position, predictedPosition, 
                                                  velocity.magnitude / 10f);
            basePosition = Vector3.Lerp(basePosition, 
                                       predictedOffset + (useLocalOffset ? target.TransformDirection(offsetPosition) : offsetPosition),
                                       Time.deltaTime * predictionSmoothness);
        }
        
        targetPosition = basePosition;
        
        // Apply collision avoidance
        if (avoidCollisions)
        {
            targetPosition = AvoidCollisions(targetPosition);
        }
        
        // Enforce maximum distance
        Vector3 direction = targetPosition - target.position;
        if (direction.magnitude > maxFollowDistance)
        {
            targetPosition = target.position + direction.normalized * maxFollowDistance;
        }
    }
    
    /// <summary>
    /// Updates the camera position with smooth following.
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (smoothFollow)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, 
                                                   ref currentVelocity, 1f / followSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 
                                             followSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Updates camera rotation to look at target with optional stabilization.
    /// </summary>
    private void UpdateCameraRotation()
    {
        if (!lookAtTarget) return;
        
        Vector3 lookPosition = target.position + lookAtOffset;
        Vector3 direction = lookPosition - transform.position;
        
        if (direction.magnitude < 0.1f) return;
        
        Quaternion targetRotation;
        
        if (stabilizeHorizon)
        {
            // Create rotation with stabilized horizon
            targetRotation = Quaternion.LookRotation(direction, upVector);
        }
        else
        {
            // Free rotation following target orientation
            Vector3 targetUp = target.up;
            if (playerController != null)
            {
                // Use a blend of target up and world up for smoother experience
                targetUp = Vector3.Slerp(upVector, target.up, 0.7f);
            }
            targetRotation = Quaternion.LookRotation(direction, targetUp);
        }
        
        // Apply rotation smoothly
        float rotSpeed = stabilizeHorizon ? stabilizationSpeed : rotationSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                             rotSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Adjusts field of view based on movement speed.
    /// </summary>
    private void UpdateDynamicZoom()
    {
        if (!dynamicZoom || playerController == null) return;
        
        float speed = playerController.Speed;
        float normalizedSpeed = Mathf.Clamp01(speed / 20f); // Normalize to 0-1 based on max expected speed
        
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, normalizedSpeed);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Performs collision avoidance for camera positioning.
    /// </summary>
    /// <param name="desiredPosition">The desired camera position</param>
    /// <returns>Adjusted position to avoid collisions</returns>
    private Vector3 AvoidCollisions(Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;
        
        if (distance < 0.1f) return desiredPosition;
        
        // Raycast from target to desired position
        RaycastHit hit;
        if (Physics.Raycast(target.position, direction.normalized, out hit, distance, collisionLayers))
        {
            // Move camera closer to avoid collision
            float safeDistance = Mathf.Max(hit.distance - collisionBuffer, 1f);
            return target.position + direction.normalized * safeDistance;
        }
        
        return desiredPosition;
    }
    
    /// <summary>
    /// Sets a new target for the camera to follow.
    /// </summary>
    /// <param name="newTarget">The new target transform</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (newTarget != null)
        {
            playerController = newTarget.GetComponent<ZeroGravityPlayerController>();
        }
    }
    
    /// <summary>
    /// Sets the camera offset position.
    /// </summary>
    /// <param name="newOffset">New offset position</param>
    public void SetOffset(Vector3 newOffset)
    {
        offsetPosition = newOffset;
    }
    
    /// <summary>
    /// Toggles stabilization mode.
    /// </summary>
    /// <param name="stabilize">Whether to stabilize the horizon</param>
    public void SetStabilization(bool stabilize)
    {
        stabilizeHorizon = stabilize;
    }
    
    /// <summary>
    /// Instantly moves camera to target position (useful for teleporting).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        
        UpdateTargetPosition();
        transform.position = targetPosition;
        
        if (lookAtTarget)
        {
            Vector3 lookPosition = target.position + lookAtOffset;
            transform.LookAt(lookPosition);
        }
    }
    
    /// <summary>
    /// Shakes the camera (useful for impacts or explosions).
    /// </summary>
    /// <param name="intensity">Shake intensity</param>
    /// <param name="duration">Shake duration</param>
    public void Shake(float intensity, float duration)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }
    
    /// <summary>
    /// Coroutine for camera shake effect.
    /// </summary>
    private System.Collections.IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            float z = Random.Range(-1f, 1f) * intensity;
            
            transform.localPosition = originalPosition + new Vector3(x, y, z);
            
            elapsed += Time.deltaTime;
            intensity = Mathf.Lerp(intensity, 0f, elapsed / duration);
            
            yield return null;
        }
        
        transform.localPosition = originalPosition;
    }
}
