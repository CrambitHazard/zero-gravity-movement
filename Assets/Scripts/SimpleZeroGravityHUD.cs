using UnityEngine;

/// <summary>
/// Simplified HUD system for zero gravity movement that works without UI package dependencies.
/// Displays information via Debug.Log and on-screen GUI for maximum compatibility.
/// </summary>
public class SimpleZeroGravityHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZeroGravityPlayerController playerController;
    [SerializeField] private ZeroGravityPhysics physics;
    
    [Header("Display Settings")]
    [SerializeField] private bool showGUI = true;
    [SerializeField] private bool logToConsole = false;
    [SerializeField] private float updateInterval = 0.1f;
    
    private float nextUpdate = 0f;
    private GUIStyle textStyle;
    private bool isInitialized = false;
    
    void Awake()
    {
        // Auto-find components if not assigned
        if (playerController == null)
            playerController = FindFirstObjectByType<ZeroGravityPlayerController>();
        
        if (physics == null && playerController != null)
            physics = playerController.GetComponent<ZeroGravityPhysics>();
    }
    
    void Start()
    {
        InitializeGUI();
        isInitialized = true;
        
        if (playerController == null || physics == null)
        {
            Debug.LogWarning("SimpleZeroGravityHUD: Missing required components!");
            enabled = false;
            return;
        }
        
        Debug.Log("🚀 Zero Gravity HUD Active!");
        Debug.Log("Controls: WASD + Mouse = Move, Space/Shift = Up/Down, B = Brake, V = Stabilize");
    }
    
    void Update()
    {
        if (!isInitialized || Time.time < nextUpdate) return;
        
        nextUpdate = Time.time + updateInterval;
        
        if (logToConsole)
        {
            LogStats();
        }
    }
    
    void OnGUI()
    {
        if (!showGUI || !isInitialized || playerController == null || physics == null) return;
        
        // Set up GUI style
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 14;
            textStyle.normal.textColor = Color.white;
        }
        
        // Display HUD information
        float yPos = 10f;
        float lineHeight = 20f;
        
        // Velocity and speed
        Vector3 velocity = physics.Velocity;
        float speed = velocity.magnitude;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), $"Speed: {speed:F1} m/s", textStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 400, lineHeight), 
                 $"Velocity: ({velocity.x:F1}, {velocity.y:F1}, {velocity.z:F1})", textStyle);
        yPos += lineHeight;
        
        // Fuel status
        float fuelPercentage = playerController.FuelPercentage;
        Color fuelColor = fuelPercentage > 0.2f ? Color.green : Color.red;
        
        GUI.color = fuelColor;
        GUI.Label(new Rect(10, yPos, 200, lineHeight), $"Fuel: {fuelPercentage * 100:F0}%", textStyle);
        GUI.color = Color.white;
        yPos += lineHeight;
        
        // Orientation
        Vector3 euler = playerController.transform.eulerAngles;
        GUI.Label(new Rect(10, yPos, 400, lineHeight), 
                 $"Orientation: ({euler.x:F0}°, {euler.y:F0}°, {euler.z:F0}°)", textStyle);
        yPos += lineHeight;
        
        // Thruster status
        bool isThrusting = playerController.IsThrusting;
        GUI.color = isThrusting ? Color.cyan : Color.gray;
        GUI.Label(new Rect(10, yPos, 200, lineHeight), 
                 isThrusting ? "THRUSTERS ACTIVE" : "THRUSTERS IDLE", textStyle);
        GUI.color = Color.white;
        yPos += lineHeight;
        
        // Acceleration
        Vector3 acceleration = physics.GetAcceleration();
        GUI.Label(new Rect(10, yPos, 300, lineHeight), 
                 $"Acceleration: {acceleration.magnitude:F1} m/s²", textStyle);
        yPos += lineHeight;
        
        // Controls reminder
        yPos += 20f;
        GUI.Label(new Rect(10, yPos, 400, lineHeight), "--- CONTROLS ---", textStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 400, lineHeight), "WASD + Mouse: Move & Look", textStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 400, lineHeight), "Space/Shift: Up/Down", textStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 400, lineHeight), "B: Brake  |  V: Stabilize", textStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 400, lineHeight), "R: Refuel  |  H: Toggle HUD", textStyle);
        
        // Warning for low fuel
        if (fuelPercentage <= 0.2f && !playerController.HasFuel)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width / 2 - 100, 50, 200, 30), "⚠️ LOW FUEL WARNING ⚠️", textStyle);
            GUI.color = Color.white;
        }
        
        // Warning for high speed
        if (speed >= 20f)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(Screen.width / 2 - 100, 80, 200, 30), "⚠️ HIGH SPEED WARNING ⚠️", textStyle);
            GUI.color = Color.white;
        }
    }
    
    /// <summary>
    /// Initializes GUI styling.
    /// </summary>
    private void InitializeGUI()
    {
        textStyle = new GUIStyle();
        textStyle.fontSize = 14;
        textStyle.normal.textColor = Color.white;
    }
    
    /// <summary>
    /// Logs current stats to console.
    /// </summary>
    private void LogStats()
    {
        if (playerController == null || physics == null) return;
        
        Vector3 velocity = physics.Velocity;
        float speed = velocity.magnitude;
        float fuel = playerController.FuelPercentage * 100f;
        
        Debug.Log($"[Zero Gravity] Speed: {speed:F1} m/s | Fuel: {fuel:F0}% | " +
                 $"Velocity: ({velocity.x:F1}, {velocity.y:F1}, {velocity.z:F1})");
    }
    
    /// <summary>
    /// Toggles GUI display.
    /// </summary>
    public void ToggleGUI()
    {
        showGUI = !showGUI;
    }
    
    /// <summary>
    /// Toggles console logging.
    /// </summary>
    public void ToggleConsoleLogging()
    {
        logToConsole = !logToConsole;
    }
    
    /// <summary>
    /// Shows a temporary message.
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="duration">Duration to show message</param>
    public void ShowMessage(string message, float duration = 3f)
    {
        Debug.Log($"[Zero Gravity HUD] {message}");
        StartCoroutine(ShowTemporaryMessage(message, duration));
    }
    
    /// <summary>
    /// Coroutine for temporary message display.
    /// </summary>
    private System.Collections.IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        float startTime = Time.time;
        
        while (Time.time - startTime < duration)
        {
            yield return null;
        }
    }
}
