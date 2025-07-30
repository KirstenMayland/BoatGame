void GaussianBlur_float(UnityTexture2D MainTex, UnitySamplerState Sample, float2 UV, float BlurAmount, float2 TexelSize, out float3 out_RGB, out float out_Alpha)
{
    float4 result = float4(0, 0, 0, 0);
    
    // 3x3 kernel - much faster     
    float weights[9] =
    {
        0.0625, 0.125, 0.0625,
        0.125, 0.25, 0.125,
        0.0625, 0.125, 0.0625
    };
    
    int index = 0;
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            float2 offset = float2(x, y) * TexelSize * BlurAmount;
            float2 sampleUV = UV + offset;
            result += SAMPLE_TEXTURE2D(MainTex, Sample, sampleUV) * weights[index];
            index++;
        }
    }
    
    out_RGB = result.rgb;
    out_Alpha = result.a;
}