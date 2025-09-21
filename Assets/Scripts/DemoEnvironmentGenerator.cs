using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a demo environment for testing zero gravity movement.
/// Creates space station modules, obstacles, and interactive objects.
/// </summary>
public class DemoEnvironmentGenerator : MonoBehaviour
{
    [Header("Environment Settings")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private Vector3 environmentSize = new Vector3(100, 50, 100);
    [SerializeField] private Material defaultMaterial;
    
    [Header("Space Station")]
    [SerializeField] private int stationModules = 8;
    [SerializeField] private Vector3 moduleSize = new Vector3(10, 4, 10);
    [SerializeField] private float moduleSpacing = 15f;
    [SerializeField] private Material stationMaterial;
    
    [Header("Obstacles")]
    [SerializeField] private int asteroidCount = 20;
    [SerializeField] private Vector3 asteroidSizeRange = new Vector3(1, 3, 1);
    [SerializeField] private Material asteroidMaterial;
    
    [Header("Interactive Objects")]
    [SerializeField] private int cargoContainers = 10;
    [SerializeField] private Vector3 containerSize = new Vector3(2, 2, 2);
    [SerializeField] private Material containerMaterial;
    
    [Header("Lighting")]
    [SerializeField] private bool createLighting = true;
    [SerializeField] private Color ambientColor = new Color(0.1f, 0.1f, 0.2f);
    [SerializeField] private int stationLights = 6;
    
    [Header("Boundaries")]
    [SerializeField] private bool createBoundaries = true;
    [SerializeField] private Material boundaryMaterial;
    
    private List<GameObject> generatedObjects = new List<GameObject>();
    private Transform environmentParent;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateEnvironment();
        }
    }
    
    /// <summary>
    /// Generates the complete demo environment.
    /// </summary>
    public void GenerateEnvironment()
    {
        ClearEnvironment();
        CreateEnvironmentParent();
        SetupLighting();
        GenerateSpaceStation();
        GenerateAsteroids();
        GenerateCargoContainers();
        GenerateBoundaries();
        
        Debug.Log("Zero Gravity Demo Environment Generated!");
    }
    
    /// <summary>
    /// Clears any previously generated environment objects.
    /// </summary>
    public void ClearEnvironment()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        generatedObjects.Clear();
        
        if (environmentParent != null)
        {
            DestroyImmediate(environmentParent.gameObject);
        }
    }
    
    /// <summary>
    /// Creates the parent object for organizing environment elements.
    /// </summary>
    private void CreateEnvironmentParent()
    {
        GameObject parentObj = new GameObject("Demo Environment");
        environmentParent = parentObj.transform;
        generatedObjects.Add(parentObj);
    }
    
    /// <summary>
    /// Sets up lighting for the space environment.
    /// </summary>
    private void SetupLighting()
    {
        if (!createLighting) return;
        
        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        
        // Create main directional light (sun)
        GameObject sunLight = new GameObject("Directional Light (Sun)");
        sunLight.transform.SetParent(environmentParent);
        Light sun = sunLight.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = Color.white;
        sun.intensity = 1.5f;
        sun.shadows = LightShadows.Soft;
        sunLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        generatedObjects.Add(sunLight);
    }
    
    /// <summary>
    /// Generates the space station with multiple modules.
    /// </summary>
    private void GenerateSpaceStation()
    {
        GameObject stationParent = new GameObject("Space Station");
        stationParent.transform.SetParent(environmentParent);
        generatedObjects.Add(stationParent);
        
        for (int i = 0; i < stationModules; i++)
        {
            Vector3 position = GetStationModulePosition(i);
            GameObject module = CreateStationModule(position, i);
            module.transform.SetParent(stationParent.transform);
            
            // Add station lights
            if (createLighting && i < stationLights)
            {
                CreateStationLight(module.transform);
            }
        }
    }
    
    /// <summary>
    /// Calculates position for a station module.
    /// </summary>
    /// <param name="index">Module index</param>
    /// <returns>World position for the module</returns>
    private Vector3 GetStationModulePosition(int index)
    {
        // Create a circular arrangement with some vertical variation
        float angle = (index / (float)stationModules) * 2f * Mathf.PI;
        float radius = moduleSpacing;
        
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        float y = Mathf.Sin(angle * 2f) * 5f; // Vertical variation
        
        return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// Creates a single station module.
    /// </summary>
    /// <param name="position">Position for the module</param>
    /// <param name="index">Module index for variation</param>
    /// <returns>Created module GameObject</returns>
    private GameObject CreateStationModule(Vector3 position, int index)
    {
        GameObject module = GameObject.CreatePrimitive(PrimitiveType.Cube);
        module.name = $"Station Module {index + 1}";
        module.transform.position = position;
        module.transform.localScale = moduleSize;
        
        // Random rotation for variety
        module.transform.rotation = Quaternion.Euler(
            Random.Range(-15f, 15f),
            Random.Range(0f, 360f),
            Random.Range(-15f, 15f)
        );
        
        // Apply material
        if (stationMaterial != null)
        {
            module.GetComponent<Renderer>().material = stationMaterial;
        }
        else
        {
            // Create a metallic-looking material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.7f, 0.7f, 0.8f);
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.6f);
            module.GetComponent<Renderer>().material = mat;
        }
        
        // Add some detail cubes
        for (int i = 0; i < 3; i++)
        {
            GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            detail.name = "Detail";
            detail.transform.SetParent(module.transform);
            detail.transform.localPosition = Random.insideUnitSphere * 0.4f;
            detail.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            detail.GetComponent<Renderer>().material = module.GetComponent<Renderer>().material;
        }
        
        return module;
    }
    
    /// <summary>
    /// Creates a light for a station module.
    /// </summary>
    /// <param name="moduleTransform">Transform of the module to light</param>
    private void CreateStationLight(Transform moduleTransform)
    {
        GameObject lightObj = new GameObject("Station Light");
        lightObj.transform.SetParent(moduleTransform);
        lightObj.transform.localPosition = Vector3.up * 3f;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.9f, 0.7f);
        light.intensity = 2f;
        light.range = 20f;
        light.shadows = LightShadows.Soft;
    }
    
    /// <summary>
    /// Generates asteroids throughout the environment.
    /// </summary>
    private void GenerateAsteroids()
    {
        GameObject asteroidParent = new GameObject("Asteroids");
        asteroidParent.transform.SetParent(environmentParent);
        generatedObjects.Add(asteroidParent);
        
        for (int i = 0; i < asteroidCount; i++)
        {
            Vector3 position = GetRandomPosition();
            GameObject asteroid = CreateAsteroid(position);
            asteroid.transform.SetParent(asteroidParent.transform);
        }
    }
    
    /// <summary>
    /// Creates a single asteroid with random properties.
    /// </summary>
    /// <param name="position">Position for the asteroid</param>
    /// <returns>Created asteroid GameObject</returns>
    private GameObject CreateAsteroid(Vector3 position)
    {
        GameObject asteroid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        asteroid.name = "Asteroid";
        asteroid.transform.position = position;
        
        // Random size and rotation
        Vector3 scale = new Vector3(
            Random.Range(asteroidSizeRange.x, asteroidSizeRange.y),
            Random.Range(asteroidSizeRange.x, asteroidSizeRange.y),
            Random.Range(asteroidSizeRange.x, asteroidSizeRange.y)
        );
        asteroid.transform.localScale = scale;
        asteroid.transform.rotation = Random.rotation;
        
        // Apply material
        if (asteroidMaterial != null)
        {
            asteroid.GetComponent<Renderer>().material = asteroidMaterial;
        }
        else
        {
            // Create a rocky material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.4f, 0.3f, 0.2f);
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.2f);
            asteroid.GetComponent<Renderer>().material = mat;
        }
        
        // Add rigidbody for physics interactions
        Rigidbody rb = asteroid.AddComponent<Rigidbody>();
        rb.mass = scale.x * scale.y * scale.z; // Mass based on size
        rb.useGravity = false;
        
        // Add some initial rotation
        rb.angularVelocity = Random.insideUnitSphere * 0.5f;
        
        return asteroid;
    }
    
    /// <summary>
    /// Generates cargo containers that can be pushed around.
    /// </summary>
    private void GenerateCargoContainers()
    {
        GameObject containerParent = new GameObject("Cargo Containers");
        containerParent.transform.SetParent(environmentParent);
        generatedObjects.Add(containerParent);
        
        for (int i = 0; i < cargoContainers; i++)
        {
            Vector3 position = GetRandomPosition();
            GameObject container = CreateCargoContainer(position);
            container.transform.SetParent(containerParent.transform);
        }
    }
    
    /// <summary>
    /// Creates a cargo container with physics.
    /// </summary>
    /// <param name="position">Position for the container</param>
    /// <returns>Created container GameObject</returns>
    private GameObject CreateCargoContainer(Vector3 position)
    {
        GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);
        container.name = "Cargo Container";
        container.transform.position = position;
        container.transform.localScale = containerSize;
        container.transform.rotation = Random.rotation;
        
        // Apply material
        if (containerMaterial != null)
        {
            container.GetComponent<Renderer>().material = containerMaterial;
        }
        else
        {
            // Create a metallic container material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.7f, 0.8f);
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Smoothness", 0.8f);
            container.GetComponent<Renderer>().material = mat;
        }
        
        // Add physics
        Rigidbody rb = container.AddComponent<Rigidbody>();
        rb.mass = 5f;
        rb.useGravity = false;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
        
        return container;
    }
    
    /// <summary>
    /// Creates invisible boundaries to contain the play area.
    /// </summary>
    private void GenerateBoundaries()
    {
        if (!createBoundaries) return;
        
        GameObject boundaryParent = new GameObject("Boundaries");
        boundaryParent.transform.SetParent(environmentParent);
        generatedObjects.Add(boundaryParent);
        
        // Create 6 boundary walls
        CreateBoundaryWall(Vector3.forward * environmentSize.z / 2, Vector3.back, boundaryParent.transform);
        CreateBoundaryWall(Vector3.back * environmentSize.z / 2, Vector3.forward, boundaryParent.transform);
        CreateBoundaryWall(Vector3.right * environmentSize.x / 2, Vector3.left, boundaryParent.transform);
        CreateBoundaryWall(Vector3.left * environmentSize.x / 2, Vector3.right, boundaryParent.transform);
        CreateBoundaryWall(Vector3.up * environmentSize.y / 2, Vector3.down, boundaryParent.transform);
        CreateBoundaryWall(Vector3.down * environmentSize.y / 2, Vector3.up, boundaryParent.transform);
    }
    
    /// <summary>
    /// Creates a single boundary wall.
    /// </summary>
    /// <param name="position">Position of the wall</param>
    /// <param name="normal">Normal direction of the wall</param>
    /// <param name="parent">Parent transform</param>
    private void CreateBoundaryWall(Vector3 position, Vector3 normal, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Boundary Wall";
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.LookAt(position + normal);
        
        // Scale based on environment size
        Vector3 scale = environmentSize;
        scale[GetLargestComponent(normal)] = 1f; // Thin in the normal direction
        wall.transform.localScale = scale;
        
        // Make it semi-transparent
        if (boundaryMaterial != null)
        {
            wall.GetComponent<Renderer>().material = boundaryMaterial;
        }
        else
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0f, 0f, 0.3f);
            mat.SetFloat("_Mode", 3f); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            wall.GetComponent<Renderer>().material = mat;
        }
    }
    
    /// <summary>
    /// Gets a random position within the environment bounds.
    /// </summary>
    /// <returns>Random position vector</returns>
    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-environmentSize.x / 2, environmentSize.x / 2),
            Random.Range(-environmentSize.y / 2, environmentSize.y / 2),
            Random.Range(-environmentSize.z / 2, environmentSize.z / 2)
        );
    }
    
    /// <summary>
    /// Gets the index of the largest component in a vector.
    /// </summary>
    /// <param name="vector">Input vector</param>
    /// <returns>Index of largest component (0=x, 1=y, 2=z)</returns>
    private int GetLargestComponent(Vector3 vector)
    {
        vector = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        
        if (vector.x >= vector.y && vector.x >= vector.z)
            return 0;
        else if (vector.y >= vector.z)
            return 1;
        else
            return 2;
    }
}
