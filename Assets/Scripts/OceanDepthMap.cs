using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanDepthMap : MonoBehaviour
{
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
    
    // -----------------------------------------------
    void Start()
    {
        GenerateDepthMap();
        CreateDepthTexture();
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
    // eventuallu will become redundant, use for error checking
    public void CreateDepthTexture()
    {
        Texture2D depthTexture = new Texture2D(mapWidth, mapHeight);
        
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float normalizedDepth = depthMap[x, y] / maxDepth;
                Color pixelColor = Color.Lerp(Color.white, Color.blue, normalizedDepth);
                depthTexture.SetPixel(x, y, pixelColor);
            }
        }
        
        depthTexture.Apply();
        
        // Apply to a sprite renderer or UI element
        GetComponent<SpriteRenderer>().sprite = Sprite.Create(depthTexture, 
            new Rect(0, 0, mapWidth, mapHeight), Vector2.one * 0.5f);
    }
}
