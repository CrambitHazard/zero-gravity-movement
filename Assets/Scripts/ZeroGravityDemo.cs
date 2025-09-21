using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main demo controller for the zero gravity movement system.
/// Sets up the complete demo scene with player, environment, and UI.
/// </summary>
public class ZeroGravityDemo : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;
    [SerializeField] private Vector3 playerStartRotation = Vector3.zero;
    
    [Header("Camera Setup")]
    [SerializeField] private bool createCamera = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);
    
    [Header("Environment")]
    [SerializeField] private bool generateEnvironment = true;
    [SerializeField] private DemoEnvironmentGenerator environmentGenerator;
    
    [Header("Audio")]
    [SerializeField] private AudioClip ambientSpaceSound;
    [SerializeField] private AudioClip thrusterSound;
    [SerializeField] private float ambientVolume = 0.3f;
    
    [Header("Demo Settings")]
    [SerializeField] private bool showWelcomeMessage = true;
    [SerializeField] private bool unlimitedFuel = true;
    [SerializeField] private float messageDisplayTime = 5f;
    
    private GameObject playerInstance;
    private Camera demoCamera;
    private ZeroGravityPlayerController playerController;
    private SimpleZeroGravityHUD hud;
    private AudioSource ambientAudio;
    
    public ZeroGravityPlayerController PlayerController => playerController;
    public Camera DemoCamera => demoCamera;
    
    void Start()
    {
        SetupDemo();
    }
    
    void Update()
    {
        HandleDemoInput();
    }
    
    /// <summary>
    /// Sets up the complete zero gravity demo.
    /// </summary>
    public void SetupDemo()
    {
        Debug.Log("Setting up Zero Gravity Movement Demo...");
        
        // Generate environment first
        if (generateEnvironment)
        {
            SetupEnvironment();
        }
        
        // Create player
        SetupPlayer();
        
        // Setup camera
        if (createCamera)
        {
            SetupCamera();
        }
        
        // Setup audio
        SetupAudio();
        
        // Setup UI
        SetupUI();
        
        // Configure demo settings
        ConfigureDemo();
        
        // Show welcome message
        if (showWelcomeMessage)
        {
            ShowWelcomeMessage();
        }
        
        Debug.Log("Zero Gravity Demo Setup Complete!");
        Debug.Log("Controls: WASD + Mouse to move, Space/Shift for up/down, B to brake, V to stabilize");
    }
    
    /// <summary>
    /// Sets up the demo environment.
    /// </summary>
    private void SetupEnvironment()
    {
        if (environmentGenerator == null)
        {
            GameObject envGenObj = new GameObject("Environment Generator");
            environmentGenerator = envGenObj.AddComponent<DemoEnvironmentGenerator>();
        }
        
        environmentGenerator.GenerateEnvironment();
    }
    
    /// <summary>
    /// Creates and configures the player object.
    /// </summary>
    private void SetupPlayer()
    {
        // Create player if prefab is provided, otherwise create from scratch
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, playerStartPosition, 
                                       Quaternion.Euler(playerStartRotation));
        }
        else
        {
            playerInstance = CreatePlayerFromScratch();
        }
        
        // Get player controller component
        playerController = playerInstance.GetComponent<ZeroGravityPlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("Player object missing ZeroGravityPlayerController!");
            return;
        }
        
        playerInstance.name = "Zero Gravity Player";
    }
    
    /// <summary>
    /// Creates a basic player object with all necessary components.
    /// </summary>
    /// <returns>Created player GameObject</returns>
    private GameObject CreatePlayerFromScratch()
    {
        // Create main player object
        GameObject player = new GameObject("Player");
        player.transform.position = playerStartPosition;
        player.transform.rotation = Quaternion.Euler(playerStartRotation);
        
        // Add visual representation
        GameObject playerMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerMesh.transform.SetParent(player.transform);
        playerMesh.transform.localPosition = Vector3.zero;
        playerMesh.transform.localScale = new Vector3(1, 1.5f, 1);
        playerMesh.name = "Player Mesh";
        
        // Style the player
        Material playerMat = new Material(Shader.Find("Standard"));
        playerMat.color = new Color(0.2f, 0.6f, 1f); // Blue color
        playerMat.SetFloat("_Metallic", 0.5f);
        playerMat.SetFloat("_Smoothness", 0.7f);
        playerMesh.GetComponent<Renderer>().material = playerMat;
        
        // Add physics components
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 75f; // Typical astronaut mass
        rb.useGravity = false;
        
        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 3f;
        collider.radius = 0.5f;
        
        // Add movement components
        player.AddComponent<ZeroGravityPhysics>();
        player.AddComponent<ZeroGravityPlayerController>();
        
        // Add thruster effects
        CreateThrusterEffects(player);
        
        return player;
    }
    
    /// <summary>
    /// Creates thruster visual and audio effects for the player.
    /// </summary>
    /// <param name="player">Player GameObject to add effects to</param>
    private void CreateThrusterEffects(GameObject player)
    {
        // Create thruster audio source
        AudioSource thrusterAudio = player.AddComponent<AudioSource>();
        thrusterAudio.clip = thrusterSound;
        thrusterAudio.loop = true;
        thrusterAudio.volume = 0f;
        thrusterAudio.pitch = 1.2f;
        
        // Create thruster particle effects
        GameObject thrusterEffectsParent = new GameObject("Thruster Effects");
        thrusterEffectsParent.transform.SetParent(player.transform);
        thrusterEffectsParent.transform.localPosition = Vector3.zero;
        
        // Main thruster (back)
        CreateThrusterEffect(thrusterEffectsParent.transform, Vector3.back * 1.5f, Vector3.forward);
        
        // Maneuvering thrusters
        CreateThrusterEffect(thrusterEffectsParent.transform, Vector3.left * 0.5f, Vector3.right);
        CreateThrusterEffect(thrusterEffectsParent.transform, Vector3.right * 0.5f, Vector3.left);
        CreateThrusterEffect(thrusterEffectsParent.transform, Vector3.down * 1f, Vector3.up);
    }
    
    /// <summary>
    /// Creates a single thruster effect.
    /// </summary>
    /// <param name="parent">Parent transform</param>
    /// <param name="position">Local position</param>
    /// <param name="direction">Thrust direction</param>
    private void CreateThrusterEffect(Transform parent, Vector3 position, Vector3 direction)
    {
        GameObject thrusterObj = new GameObject("Thruster");
        thrusterObj.transform.SetParent(parent);
        thrusterObj.transform.localPosition = position;
        thrusterObj.transform.LookAt(thrusterObj.transform.position + direction);
        
        // Add particle system
        ParticleSystem particles = thrusterObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.1f;
        main.startColor = Color.cyan;
        main.maxParticles = 50;
        
        var emission = particles.emission;
        emission.enabled = false; // Will be controlled by player controller
        emission.rateOverTime = 50f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        
        // Add light
        GameObject lightObj = new GameObject("Light");
        lightObj.transform.SetParent(thrusterObj.transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.cyan;
        light.intensity = 0f; // Will be controlled by player controller
        light.range = 5f;
        light.enabled = false;
    }
    
    /// <summary>
    /// Sets up the camera system.
    /// </summary>
    private void SetupCamera()
    {
        // Find existing camera or create new one
        demoCamera = Camera.main;
        if (demoCamera == null)
        {
            GameObject cameraObj = new GameObject("Demo Camera");
            demoCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        
        // Add camera controller
        ZeroGravityCameraController cameraController = demoCamera.GetComponent<ZeroGravityCameraController>();
        if (cameraController == null)
        {
            cameraController = demoCamera.gameObject.AddComponent<ZeroGravityCameraController>();
        }
        
        // Configure camera for space environment
        demoCamera.backgroundColor = Color.black;
        demoCamera.farClipPlane = 1000f;
        demoCamera.fieldOfView = 75f;
        
        // Set target to player
        if (playerInstance != null)
        {
            cameraController.SetTarget(playerInstance.transform);
            cameraController.SetOffset(cameraOffset);
        }
    }
    
    /// <summary>
    /// Sets up ambient audio for the space environment.
    /// </summary>
    private void SetupAudio()
    {
        if (ambientSpaceSound != null)
        {
            GameObject audioObj = new GameObject("Ambient Audio");
            ambientAudio = audioObj.AddComponent<AudioSource>();
            ambientAudio.clip = ambientSpaceSound;
            ambientAudio.loop = true;
            ambientAudio.volume = ambientVolume;
            ambientAudio.spatialBlend = 0f; // 2D sound
            ambientAudio.Play();
        }
    }
    
    /// <summary>
    /// Sets up the HUD and UI elements.
    /// </summary>
    private void SetupUI()
    {
        // Create simple HUD component (no UI package dependencies)
        GameObject hudObj = new GameObject("Simple HUD");
        hud = hudObj.AddComponent<SimpleZeroGravityHUD>();
        
        Debug.Log("📊 Simple HUD created - uses OnGUI for compatibility");
    }
    
    
    /// <summary>
    /// Configures demo-specific settings.
    /// </summary>
    private void ConfigureDemo()
    {
        if (playerController != null)
        {
            playerController.SetUnlimitedFuel(unlimitedFuel);
        }
        
        // Set time scale to normal
        Time.timeScale = 1f;
        
        // Configure cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Shows welcome message to the player.
    /// </summary>
    private void ShowWelcomeMessage()
    {
        string message = "Welcome to Zero Gravity Movement Demo!\n" +
                        "Controls:\n" +
                        "WASD + Mouse - Move and look\n" +
                        "Space/Shift - Up/Down\n" +
                        "B - Brake\n" +
                        "V - Stabilize rotation\n" +
                        "Tab - Toggle controls\n" +
                        "H - Toggle HUD\n" +
                        "ESC - Toggle cursor";
        
        Debug.Log(message);
        
        if (hud != null)
        {
            hud.ShowMessage(message, messageDisplayTime);
        }
    }
    
    /// <summary>
    /// Handles demo-specific input.
    /// </summary>
    private void HandleDemoInput()
    {
        // Restart demo
        if (Input.GetKeyDown(KeyCode.F5))
        {
            RestartDemo();
        }
        
        // Toggle time scale
        if (Input.GetKeyDown(KeyCode.T))
        {
            Time.timeScale = Time.timeScale > 0.5f ? 0.1f : 1f;
        }
        
        // Emergency stop
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playerController != null)
            {
                playerController.EmergencyStop();
            }
        }
    }
    
    /// <summary>
    /// Restarts the demo scene.
    /// </summary>
    public void RestartDemo()
    {
        Debug.Log("Restarting Zero Gravity Demo...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Teleports player to a specific position.
    /// </summary>
    /// <param name="position">Target position</param>
    public void TeleportPlayer(Vector3 position)
    {
        if (playerInstance != null)
        {
            playerInstance.transform.position = position;
            
            // Stop all movement
            if (playerController != null)
            {
                playerController.EmergencyStop();
            }
            
            // Snap camera to new position
            if (demoCamera != null)
            {
                ZeroGravityCameraController cameraController = 
                    demoCamera.GetComponent<ZeroGravityCameraController>();
                if (cameraController != null)
                {
                    cameraController.SnapToTarget();
                }
            }
        }
    }
    
    /// <summary>
    /// Gets demo statistics for debugging.
    /// </summary>
    /// <returns>Demo stats string</returns>
    public string GetDemoStats()
    {
        if (playerController == null) return "No player controller found";
        
        return $"Speed: {playerController.Speed:F1} m/s\n" +
               $"Fuel: {playerController.FuelPercentage * 100:F0}%\n" +
               $"Position: {playerInstance.transform.position}\n" +
               $"Velocity: {playerController.Velocity}";
    }
}
