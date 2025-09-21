using UnityEngine;

/// <summary>
/// Quick setup utility for the Zero Gravity Movement System.
/// Provides easy one-click setup for the complete demo.
/// </summary>
public class QuickSetup : MonoBehaviour
{
    [Header("Quick Setup Options")]
    [SerializeField] private bool createCompleteDemo = true;
    [SerializeField] private bool addAudioSources = true;
    [SerializeField] private bool generateEnvironment = true;
    [SerializeField] private bool setupLighting = true;
    
    [Header("Player Configuration")]
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;
    [SerializeField] private bool unlimitedFuel = true;
    [SerializeField] private float thrusterPower = 10f;
    
    [Header("Environment Configuration")]
    [SerializeField] private int numberOfStationModules = 8;
    [SerializeField] private int numberOfAsteroids = 20;
    [SerializeField] private Vector3 environmentSize = new Vector3(100, 50, 100);
    
    [ContextMenu("Setup Complete Zero Gravity Demo")]
    public void SetupDemo()
    {
        if (createCompleteDemo)
        {
            CreateCompleteDemo();
        }
        
        Debug.Log("Zero Gravity Movement System setup complete!");
        Debug.Log("Press Play to test the demo. Use WASD + Mouse to move around.");
    }
    
    /// <summary>
    /// Creates a complete zero gravity demo from scratch.
    /// </summary>
    private void CreateCompleteDemo()
    {
        // Clear existing setup
        ClearExistingDemo();
        
        // Create player
        GameObject player = CreatePlayer();
        
        // Create camera
        GameObject camera = CreateCamera(player);
        
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
        
        // Create UI
        CreateUI(player);
        
        // Create demo controller
        CreateDemoController(player, camera);
        
        Debug.Log("Complete Zero Gravity Demo created successfully!");
    }
    
    /// <summary>
    /// Clears any existing demo objects.
    /// </summary>
    private void ClearExistingDemo()
    {
        // Find and destroy existing demo objects
        GameObject[] existingDemoObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in existingDemoObjects)
        {
            DestroyImmediate(obj);
        }
        
        // Remove existing environment
        GameObject existingEnv = GameObject.Find("Demo Environment");
        if (existingEnv != null)
        {
            DestroyImmediate(existingEnv);
        }
        
        // Remove existing UI
        GameObject existingUI = GameObject.Find("Demo HUD Canvas");
        if (existingUI != null)
        {
            DestroyImmediate(existingUI);
        }
    }
    
    /// <summary>
    /// Creates the player object with all necessary components.
    /// </summary>
    /// <returns>Created player GameObject</returns>
    private GameObject CreatePlayer()
    {
        // Create main player object
        GameObject player = new GameObject("Zero Gravity Player");
        player.tag = "Player";
        player.transform.position = playerStartPosition;
        
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
        rb.mass = 75f;
        rb.useGravity = false;
        
        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 3f;
        collider.radius = 0.5f;
        
        // Add movement components
        ZeroGravityPhysics physics = player.AddComponent<ZeroGravityPhysics>();
        ZeroGravityPlayerController controller = player.AddComponent<ZeroGravityPlayerController>();
        
        // Configure controller
        if (unlimitedFuel)
        {
            controller.SetUnlimitedFuel(true);
        }
        
        // Add audio if requested
        if (addAudioSources)
        {
            AudioSource audioSource = player.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        
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
        GameObject thrusterEffectsParent = new GameObject("Thruster Effects");
        thrusterEffectsParent.transform.SetParent(player.transform);
        thrusterEffectsParent.transform.localPosition = Vector3.zero;
        
        // Create multiple thruster positions
        Vector3[] thrusterPositions = {
            Vector3.back * 1.5f,      // Main thruster
            Vector3.left * 0.5f,      // Left maneuvering
            Vector3.right * 0.5f,     // Right maneuvering
            Vector3.down * 1f         // Bottom maneuvering
        };
        
        Vector3[] thrusterDirections = {
            Vector3.forward,   // Main thrust direction
            Vector3.right,     // Left thrust direction
            Vector3.left,      // Right thrust direction
            Vector3.up         // Bottom thrust direction
        };
        
        for (int i = 0; i < thrusterPositions.Length; i++)
        {
            CreateSingleThruster(thrusterEffectsParent.transform, 
                               thrusterPositions[i], 
                               thrusterDirections[i]);
        }
    }
    
    /// <summary>
    /// Creates a single thruster effect.
    /// </summary>
    /// <param name="parent">Parent transform</param>
    /// <param name="position">Local position</param>
    /// <param name="direction">Thrust direction</param>
    private void CreateSingleThruster(Transform parent, Vector3 position, Vector3 direction)
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
        emission.enabled = false;
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
        light.intensity = 0f;
        light.range = 5f;
        light.enabled = false;
    }
    
    /// <summary>
    /// Creates the camera system.
    /// </summary>
    /// <param name="player">Player object to follow</param>
    /// <returns>Created camera GameObject</returns>
    private GameObject CreateCamera(GameObject player)
    {
        GameObject cameraObj = new GameObject("Zero Gravity Camera");
        cameraObj.tag = "MainCamera";
        
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.farClipPlane = 1000f;
        cam.fieldOfView = 75f;
        
        AudioListener listener = cameraObj.AddComponent<AudioListener>();
        
        ZeroGravityCameraController cameraController = cameraObj.AddComponent<ZeroGravityCameraController>();
        cameraController.SetTarget(player.transform);
        
        return cameraObj;
    }
    
    /// <summary>
    /// Creates the environment using the generator.
    /// </summary>
    private void CreateEnvironment()
    {
        GameObject envGenObj = new GameObject("Environment Generator");
        DemoEnvironmentGenerator envGen = envGenObj.AddComponent<DemoEnvironmentGenerator>();
        envGen.GenerateEnvironment();
    }
    
    /// <summary>
    /// Sets up scene lighting for space environment.
    /// </summary>
    private void SetupSceneLighting()
    {
        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f);
        
        // Create main directional light (sun)
        GameObject sunLight = new GameObject("Directional Light (Sun)");
        Light sun = sunLight.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = Color.white;
        sun.intensity = 1.5f;
        sun.shadows = LightShadows.Soft;
        sunLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
    
    /// <summary>
    /// Creates the simple HUD system without UI dependencies.
    /// </summary>
    /// <param name="player">Player object for HUD reference</param>
    private void CreateUI(GameObject player)
    {
        GameObject hudObj = new GameObject("Simple HUD");
        SimpleZeroGravityHUD hud = hudObj.AddComponent<SimpleZeroGravityHUD>();
        
        Debug.Log("📊 Simple HUD created - displays via OnGUI (no UI dependencies)");
    }
    
    
    /// <summary>
    /// Creates the main demo controller.
    /// </summary>
    /// <param name="player">Player GameObject</param>
    /// <param name="camera">Camera GameObject</param>
    private void CreateDemoController(GameObject player, GameObject camera)
    {
        GameObject demoControllerObj = new GameObject("Zero Gravity Demo Controller");
        ZeroGravityDemo demoController = demoControllerObj.AddComponent<ZeroGravityDemo>();
        
        // The demo controller will automatically find and configure components
    }
    
/* Editor menu items - uncomment when using Unity Editor
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Zero Gravity/Quick Setup Demo")]
    public static void QuickSetupMenu()
    {
        GameObject setupObj = new GameObject("Quick Setup");
        QuickSetup setup = setupObj.AddComponent<QuickSetup>();
        setup.SetupDemo();
        
        DestroyImmediate(setupObj);
        Debug.Log("Zero Gravity Demo setup complete! Press Play to test.");
    }
    
    [UnityEditor.MenuItem("Zero Gravity/Clear Demo")]
    public static void ClearDemoMenu()
    {
        string[] demoObjectNames = {
            "Zero Gravity Player", "Zero Gravity Camera", 
            "Demo Environment", "Demo HUD Canvas",
            "Zero Gravity Demo Controller", "Environment Generator"
        };
        
        foreach (string objName in demoObjectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null) DestroyImmediate(obj);
        }
        
        Debug.Log("Zero Gravity Demo cleared.");
    }
#endif
*/
}
