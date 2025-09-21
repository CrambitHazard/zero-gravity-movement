using UnityEngine;

/// <summary>
/// Simplified, crash-proof player controller for zero gravity movement.
/// Minimal implementation to avoid Unity crashes.
/// </summary>
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float thrustPower = 5f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float maxSpeed = 10f;
    
    private Rigidbody rb;
    private bool isInitialized = false;
    
    void Start()
    {
        // Safe initialization
        try
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            // Safe physics setup
            rb.useGravity = false;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.1f;
            rb.mass = 1f;
            
            isInitialized = true;
            
            Debug.Log("✅ Simple Player Controller initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error initializing player: {e.Message}");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (!isInitialized || rb == null) return;
        
        try
        {
            HandleInput();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error in input handling: {e.Message}");
            enabled = false;
        }
    }
    
    private void HandleInput()
    {
        // Simple, safe input handling
        Vector3 moveInput = Vector3.zero;
        
        // Movement input
        if (Input.GetKey(KeyCode.W)) moveInput += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveInput -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveInput -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveInput += transform.right;
        if (Input.GetKey(KeyCode.Space)) moveInput += transform.up;
        if (Input.GetKey(KeyCode.LeftShift)) moveInput -= transform.up;
        
        // Apply movement
        if (moveInput.magnitude > 0.1f)
        {
            Vector3 force = moveInput.normalized * thrustPower;
            rb.AddForce(force, ForceMode.Force);
            
            // Speed limit
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
        
        // Simple rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        
        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            transform.Rotate(-mouseY, mouseX, 0, Space.Self);
        }
        
        // Emergency stop
        if (Input.GetKey(KeyCode.B))
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
        }
    }
    
    void OnGUI()
    {
        if (!isInitialized) return;
        
        // Simple, safe GUI display
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        
        string info = $"Speed: {(rb != null ? rb.linearVelocity.magnitude.ToString("F1") : "0")} m/s\n";
        info += "Controls: WASD = Move, Mouse = Look, Space/Shift = Up/Down, B = Brake";
        
        GUI.Label(new Rect(10, 10, 400, 60), info, style);
    }
}
