using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanDepthMap : MonoBehaviour
{
    [Header("References")]
    public Material oceanMaterial; // Your material using the OceanShader
    
    [Header("Texture Settings")]
    public FilterMode filterMode = FilterMode.Bilinear;
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;

    [Header("Map Settings")]
    public int mapWidth = 512;
    public int mapHeight = 512;
    public float scale = 50f;
    public float maxDepth = 100f;
    
    [Header("Noise Settings")]
    public float noiseScale = 0.1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    
    private float[,] depthMap;
    private Texture2D depthTexture;
    
    // -----------------------------------------------
    void Start()
    {
        GenerateDepthMap();
        
        if (oceanMaterial == null)
        {
            oceanMaterial = GetComponent<Renderer>().material;
        }
        
        CreateAndAssignDepthTexture();

        // Apply to a sprite renderer or UI element
        GetComponent<SpriteRenderer>().sprite = Sprite.Create(depthTexture, 
            new Rect(0, 0, mapWidth, mapHeight), Vector2.one * 0.5f);
    }

    // -----------------------------------------------
    void GenerateDepthMap()
    {
        depthMap = new float[mapWidth, mapHeight];
        
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float depth = GenerateDepthAtPoint(x, y);
                depthMap[x, y] = depth;
            }
        }
    }

    float GenerateDepthAtPoint(int x, int y)
    {
        float amplitude = 1f;
        float frequency = noiseScale;
        float noiseHeight = 0f;
        float maxValue = 0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x * frequency;
            float sampleY = y * frequency;
            
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;
            maxValue += amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        
        noiseHeight /= maxValue;
        
        // Convert to depth (0 = surface, higher = deeper)
        return noiseHeight * maxDepth;
    }

    // -----------------------------------------------
    public void CreateAndAssignDepthTexture()
    {
        if (depthMap == null || oceanMaterial == null) return;
        
        // Get the depth map data
        float[,] depthData = depthMap;
        if (depthData == null)
        {
            Debug.LogError("Depth map is null! Make sure GenerateDepthMap() was called.");
            return;
        }
        int width = depthData.GetLength(0);
        int height = depthData.GetLength(1);
        
        // Create texture
        depthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
        depthTexture.filterMode = filterMode;
        depthTexture.wrapMode = wrapMode;
        
        // Convert depth array to texture data
        Color[] pixels = new Color[width * height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Normalize depth value (0-1 range)
                float normalizedDepth = depthData[x, y] / maxDepth;
                
                // Store depth in red channel (shader expects it there)
                Color pixelColor = new Color(normalizedDepth, normalizedDepth, normalizedDepth, 1f);
                pixels[y * width + x] = pixelColor;
            }
        }
        
        depthTexture.SetPixels(pixels);
        depthTexture.Apply();
        
        // Assign to material
        AssignToMaterial();
    }
    
    void AssignToMaterial()
    {
        if (oceanMaterial == null || depthTexture == null) return;
        
        // Assign the depth texture
        oceanMaterial.SetTexture("_HeightMap", depthTexture);
        oceanMaterial.SetTexture("HeightMap", depthTexture);  // Without underscore
        
        // Set world positioning (adjust these based on your world setup)
        Vector2 worldPos = new Vector2(transform.position.x, transform.position.y);
        oceanMaterial.SetVector("_CurrentWorldTexturePos", worldPos);
        
        // Set scale (how many world units the texture covers)
        float worldScale = scale; // Use your scale from depth map
        oceanMaterial.SetFloat("_CurrentWorldTextureScale", worldScale);
        
        Debug.Log("Depth texture assigned to material!");
    }
    
    // Call this if you regenerate the depth map at runtime
    public void UpdateDepthTexture()
    {
        CreateAndAssignDepthTexture();
    }
    
    void OnDestroy()
    {
        if (depthTexture != null)
        {
            DestroyImmediate(depthTexture);
        }
    }
}
