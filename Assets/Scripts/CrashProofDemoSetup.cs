using UnityEngine;

/// <summary>
/// Crash-proof demo setup that creates a minimal working zero gravity demo.
/// Uses simplified components to avoid Unity crashes.
/// </summary>
public class CrashProofDemoSetup : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupCrashProofDemo();
        }
    }
    
    [ContextMenu("Setup Crash-Proof Demo")]
    public void SetupCrashProofDemo()
    {
        Debug.Log("🔧 Setting up crash-proof zero gravity demo...");
        
        try
        {
            // Clear existing objects first
            ClearExistingDemo();
            
            // Create simple player
            GameObject player = CreateSimplePlayer();
            
            // Create simple camera
            CreateSimpleCamera(player);
            
            // Create basic environment
            CreateBasicEnvironment();
            
            // Setup lighting
            SetupBasicLighting();
            
            Debug.Log("✅ Crash-proof demo setup complete!");
            Debug.Log("🎮 Controls: WASD + Mouse to move, Space/Shift for up/down, B to brake");
            
            // Destroy this setup object
            Destroy(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error during demo setup: {e.Message}");
        }
    }
    
    private void ClearExistingDemo()
    {
        // Safe cleanup
        string[] demoObjectNames = {
            "Simple Player", "Simple Camera", "Basic Environment"
        };
        
        foreach (string objName in demoObjectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
    }
    
    private GameObject CreateSimplePlayer()
    {
        // Create basic player
        GameObject player = new GameObject("Simple Player");
        player.transform.position = playerStartPosition;
        
        // Add visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.SetParent(player.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(1, 1.5f, 1);
        visual.name = "Player Visual";
        
        // Style the player
        Renderer renderer = visual.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.6f, 1f); // Blue
        renderer.material = mat;
        
        // Add physics (safe setup)
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = false;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
        
        // Add simple collider
        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 3f;
        collider.radius = 0.5f;
        
        // Add simple controller
        player.AddComponent<SimplePlayerController>();
        
        Debug.Log("✅ Simple player created");
        return player;
    }
    
    private void CreateSimpleCamera(GameObject player)
    {
        // Create basic camera
        GameObject cameraObj = new GameObject("Simple Camera");
        cameraObj.tag = "MainCamera";
        
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.farClipPlane = 1000f;
        cam.fieldOfView = 75f;
        
        // Add audio listener
        cameraObj.AddComponent<AudioListener>();
        
        // Simple camera following
        SimpleCameraFollow follow = cameraObj.AddComponent<SimpleCameraFollow>();
        follow.target = player.transform;
        
        Debug.Log("✅ Simple camera created");
    }
    
    private void CreateBasicEnvironment()
    {
        GameObject envParent = new GameObject("Basic Environment");
        
        // Create a few simple objects to interact with
        for (int i = 0; i < 5; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Floating Cube {i + 1}";
            cube.transform.SetParent(envParent.transform);
            cube.transform.position = Random.insideUnitSphere * 20f;
            cube.transform.rotation = Random.rotation;
            
            // Add physics
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 0.5f;
            
            // Random color
            Renderer renderer = cube.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Random.ColorHSV();
            renderer.material = mat;
        }
        
        Debug.Log("✅ Basic environment created");
    }
    
    private void SetupBasicLighting()
    {
        // Simple lighting setup
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.3f);
        
        // Create simple directional light
        GameObject lightObj = new GameObject("Simple Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        Debug.Log("✅ Basic lighting setup");
    }
}

/// <summary>
/// Simple camera follow script that won't crash.
/// </summary>
public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 2f;
    
    void LateUpdate()
    {
        if (target == null) return;
        
        try
        {
            Vector3 targetPosition = target.position + target.TransformDirection(offset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            
            Vector3 lookDirection = target.position - transform.position;
            if (lookDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Camera follow error: {e.Message}");
            enabled = false;
        }
    }
}
