using UnityEngine;

/// <summary>
/// Runtime demo setup that works without editor dependencies.
/// Add this to an empty GameObject and it will create the complete demo automatically.
/// </summary>
public class RuntimeDemoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool destroyAfterSetup = true;
    
    [Header("Player Settings")]
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;
    [SerializeField] private bool unlimitedFuel = true;
    
    [Header("Environment Settings")]
    [SerializeField] private bool generateEnvironment = true;
    [SerializeField] private bool setupLighting = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupCompleteDemo();
        }
    }
    
    /// <summary>
    /// Sets up the complete zero gravity demo at runtime.
    /// </summary>
    [ContextMenu("Setup Demo Now")]
    public void SetupCompleteDemo()
    {
        Debug.Log("Setting up Zero Gravity Demo...");
        
        // Check if demo already exists
        if (GameObject.Find("Zero Gravity Player") != null)
        {
            Debug.Log("Demo already exists! Use ClearDemo() first if you want to recreate it.");
            return;
        }
        
        // Create player
        GameObject player = CreatePlayer();
        
        // Create camera
        CreateCamera(player);
        
        // Create environment
        if (generateEnvironment)
        {
            CreateEnvironment();
        }
        
        // Setup lighting
        if (setupLighting)
        {
            SetupSceneLighting();
        }
        
        // Create simple HUD
        CreateSimpleHUD(player);
        
        Debug.Log("✅ Zero Gravity Demo setup complete!");
        Debug.Log("🎮 Controls: WASD + Mouse to move, Space/Shift for up/down, B to brake");
        
        // Destroy this setup object if requested
        if (destroyAfterSetup)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Clears the demo objects.
    /// </summary>
    [ContextMenu("Clear Demo")]
    public void ClearDemo()
    {
        string[] demoObjectNames = {
            "Zero Gravity Player",
            "Zero Gravity Camera",
            "Demo Environment", 
            "Environment Generator",
            "Basic UI Canvas"
        };
        
        foreach (string objName in demoObjectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        
        Debug.Log("Demo cleared.");
    }
    
    /// <summary>
    /// Creates the player with all necessary components.
    /// </summary>
    private GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Zero Gravity Player");
        player.transform.position = playerStartPosition;
        
        // Add visual representation
        GameObject playerMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerMesh.transform.SetParent(player.transform);
        playerMesh.transform.localPosition = Vector3.zero;
        playerMesh.transform.localScale = new Vector3(1, 1.5f, 1);
        playerMesh.name = "Player Visual";
        
        // Style the player
        Renderer renderer = playerMesh.GetComponent<Renderer>();
        Material playerMat = new Material(Shader.Find("Standard"));
        playerMat.color = new Color(0.2f, 0.6f, 1f);
        playerMat.SetFloat("_Metallic", 0.5f);
        playerMat.SetFloat("_Smoothness", 0.7f);
        renderer.material = playerMat;
        
        // Add physics
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 75f;
        rb.useGravity = false;
        
        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 3f;
        collider.radius = 0.5f;
        
        // Add zero gravity components
        player.AddComponent<ZeroGravityPhysics>();
        ZeroGravityPlayerController controller = player.AddComponent<ZeroGravityPlayerController>();
        
        if (unlimitedFuel)
        {
            controller.SetUnlimitedFuel(true);
        }
        
        // Add basic thruster effects
        CreateBasicThrusterEffects(player);
        
        return player;
    }
    
    /// <summary>
    /// Creates basic thruster particle effects.
    /// </summary>
    private void CreateBasicThrusterEffects(GameObject player)
    {
        GameObject thrusterParent = new GameObject("Thruster Effects");
        thrusterParent.transform.SetParent(player.transform);
        thrusterParent.transform.localPosition = Vector3.zero;
        
        // Create main thruster
        CreateThruster(thrusterParent.transform, Vector3.back * 1.5f, "Main Thruster");
        
        // Create maneuvering thrusters
        CreateThruster(thrusterParent.transform, Vector3.left * 0.5f, "Left Thruster");
        CreateThruster(thrusterParent.transform, Vector3.right * 0.5f, "Right Thruster");
        CreateThruster(thrusterParent.transform, Vector3.down * 1f, "Bottom Thruster");
    }
    
    /// <summary>
    /// Creates a single thruster effect.
    /// </summary>
    private void CreateThruster(Transform parent, Vector3 position, string name)
    {
        GameObject thruster = new GameObject(name);
        thruster.transform.SetParent(parent);
        thruster.transform.localPosition = position;
        
        // Add particle system
        ParticleSystem particles = thruster.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.1f;
        main.startColor = Color.cyan;
        main.maxParticles = 30;
        
        var emission = particles.emission;
        emission.enabled = false;
        emission.rateOverTime = 30f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
    }
    
    /// <summary>
    /// Creates the camera system.
    /// </summary>
    private void CreateCamera(GameObject player)
    {
        GameObject cameraObj = new GameObject("Zero Gravity Camera");
        cameraObj.tag = "MainCamera";
        
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.farClipPlane = 1000f;
        cam.fieldOfView = 75f;
        
        cameraObj.AddComponent<AudioListener>();
        
        ZeroGravityCameraController cameraController = cameraObj.AddComponent<ZeroGravityCameraController>();
        cameraController.SetTarget(player.transform);
    }
    
    /// <summary>
    /// Creates the environment.
    /// </summary>
    private void CreateEnvironment()
    {
        GameObject envGen = new GameObject("Environment Generator");
        DemoEnvironmentGenerator generator = envGen.AddComponent<DemoEnvironmentGenerator>();
        generator.GenerateEnvironment();
    }
    
    /// <summary>
    /// Sets up basic lighting.
    /// </summary>
    private void SetupSceneLighting()
    {
        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f);
        
        // Create sun light
        GameObject sunLight = new GameObject("Sun Light");
        Light sun = sunLight.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = Color.white;
        sun.intensity = 1.5f;
        sun.shadows = LightShadows.Soft;
        sunLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
    
    /// <summary>
    /// Creates simple HUD that works without UI package dependencies.
    /// </summary>
    private void CreateSimpleHUD(GameObject player)
    {
        GameObject hudObj = new GameObject("Simple HUD");
        SimpleZeroGravityHUD hud = hudObj.AddComponent<SimpleZeroGravityHUD>();
        
        // The HUD will automatically find the player controller
        Debug.Log("📊 Simple HUD created - displays via OnGUI (no UI package required)");
    }
}
