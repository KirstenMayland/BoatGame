using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanDepthMap : MonoBehaviour
{
    [Header("References")]
    public Material spriteMaterial; // Your material using the OceanShader
    
    [Header("Texture Settings")]
    public FilterMode filterMode = FilterMode.Bilinear;
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;

    [Header("Map Settings")]
    public int mapWidth = 512;
    public int mapHeight = 512;
    public float scale = 50f;
    public float minDepth = 0f;
    public float maxDepth = 100f;

    [Header("Noise Settings")]
    public float noiseScale = 0.1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Debug")]
    public bool logValues = true;
    public bool showDebugColors = true;
    
    private SpriteRenderer spriteRenderer;
    private float[,] depthMap;
    private Texture2D depthTexture;
    
    // -----------------------------------------------
    void Awake()
    {
        // Only generate if we don't already have data
        if (depthMap == null)
        {
            GenerateDepthMap();
            Debug.Log("generated depth map");
        }
    }

    // -----------------------------------------------
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found! This script needs a SpriteRenderer.");
            return;
        }

        if (depthTexture == null && depthMap != null)
        {
            Debug.Log("creating texture");
            CreateAndAssignDepthTexture();
        }
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
        
        // Convert to depth (0 = surface, lower = deeper)
        return noiseHeight * maxDepth * -1;
    }

    // -----------------------------------------------
    public void CreateAndAssignDepthTexture()
    {
        // Add this check at the start
        if (Application.isPlaying == false)
        {
            Debug.LogWarning("Cannot create texture outside of play mode");
            return;
        }
        
        if (depthMap == null || spriteMaterial == null)
        {
            Debug.LogError("Depth map or sprite material is null.");
            return;
        }
        
        // Get the depth map data
        float[,] depthData = (float[,])depthMap.Clone();
        if (depthData == null)
        {
            Debug.LogError("Depth map is null! Make sure GenerateDepthMap() was called.");
            return;
        }

        int width = depthData.GetLength(0);
        int height = depthData.GetLength(1);
        
        // Create texture
        depthTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        depthTexture.filterMode = filterMode;
        depthTexture.wrapMode = wrapMode;
        
        // Convert depth array to texture data
        Color[] pixels = new Color[width * height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Normalize depth value (0-1 range)
                float depth = depthData[x, y];
                float normalizedDepth = (depth - minDepth) / (maxDepth - minDepth);
                
                // Store depth in red channel (shader expects it there)
                Color pixelColor = new Color(normalizedDepth, normalizedDepth, normalizedDepth, 1f);
                
                int pixelIndex = (height - 1 - y) * width + x;
                pixels[pixelIndex] = pixelColor;
            }
        }
        
        depthTexture.SetPixels(pixels);
        depthTexture.Apply();
        
        // Assign to material
        Debug.Log("Attempting to assign to material");
        AssignToMaterial();
    }
    
    void AssignToMaterial()
    {
        // Shader graph values: _MainTex, HeightMap, DepthGradient, CurrentWorldTexturePos, CurrentWorldTextureScale

        if (spriteMaterial == null || spriteRenderer.material.name == "Default Sprite Material" || depthTexture == null)
        {
            Debug.LogError("Can't assign to material");
            return;
        }
        
        // Assign the depth texture
        if (spriteMaterial.HasProperty("_HeightMap"))
        {
            spriteMaterial.SetTexture("_HeightMap", depthTexture);
            Debug.Log("Applied texture to _HeightMap property");
        }

        // Set positioning if properties exist
        if (spriteMaterial.HasProperty("_CurrentWorldTexturePos"))
        {
            Vector2 bottomLeft = GetBottomLeftWorldPosition();
            spriteMaterial.SetVector("_CurrentWorldTexturePos", bottomLeft);
            Debug.Log($"Applied vector to _CurrentWorldTexturePos: {bottomLeft}");
        }
        
        if (spriteMaterial.HasProperty("_CurrentWorldTextureScale"))
        {
            float textureScale = GetTextureScale();
            spriteMaterial.SetFloat("_CurrentWorldTextureScale", textureScale);
            Debug.Log($"Applied scale to _CurrentWorldTextureScale: {textureScale}");
        }
        
        spriteRenderer.material = spriteMaterial;
        Debug.Log("Depth texture assigned to material!");
    }

    Vector2 GetBottomLeftWorldPosition()
    {
        Bounds bounds = spriteRenderer.bounds;
        return new Vector2(bounds.min.x, bounds.min.y);
    }
    
    float GetTextureScale()
    {
        Bounds bounds = spriteRenderer.bounds;
        // Return 1/size so we can multiply in shader to normalize coordinates
        return 1.0f / Mathf.Max(bounds.size.x, bounds.size.y);
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
