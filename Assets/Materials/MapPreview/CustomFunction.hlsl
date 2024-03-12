#ifndef CUSTOM_INSTANCING
#define CUSTOM_INSTANCING

// This allows multiple Custom Function nodes to use the same file
// without trying to include the code multiple times (causing redefinition errors)
    
const static int maxLayerCount = 16;
const static float epsilon = 1E-4;

// Instancing Buffer
UNITY_INSTANCING_BUFFER_START(Props)

    UNITY_DEFINE_INSTANCED_PROP(int, layerCount)
    UNITY_DEFINE_INSTANCED_PROP(float4 baseColours[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float baseStartHeights[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float baseStartTemp[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float baseStartHumidity[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float baseBlends[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float baseColourStrength[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float baseTextureScales[maxLayerCount], [maxLayerCount])
    UNITY_DEFINE_INSTANCED_PROP(float, minHeight)
    UNITY_DEFINE_INSTANCED_PROP(float, maxHeight)
    UNITY_DEFINE_INSTANCED_PROP(sampler2D, testTexture)
    UNITY_DEFINE_INSTANCED_PROP(float, testScale)

UNITY_INSTANCING_BUFFER_END(Props)

float inverseLerp(float a, float b, float value)
{
    return saturate((value - a) / (b - a));
}

float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex, UnityTexture2DArray texArray, UnitySamplerState Sampler)
{
    float3 scaledWorldPos = worldPos / scale;
    float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(texArray, Sampler, float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex) * blendAxes.x;
    float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(texArray, Sampler, float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex) * blendAxes.y;
    float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(texArray, Sampler, float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex) * blendAxes.z;
    return xProjection + yProjection + zProjection;
}

void Grayscale_float(UnityTexture2DArray texArray, UnitySamplerState Sampler, float3 worldPos, float scale, float3 worldNormal, float4 uv, out float3 output)
{
    int count = UNITY_ACCESS_INSTANCED_PROP(Props, layerCount);
    float4 colours[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseColours);
    float startHeights[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseStartHeights);
    float blends[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseBlends);
    float colourStrength[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseColourStrength);
    float textureScales[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseTextureScales);
    float minH = UNITY_ACCESS_INSTANCED_PROP(Props, minHeight);
    float maxH = UNITY_ACCESS_INSTANCED_PROP(Props, maxHeight);
    float startTemp[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseStartTemp);
    float startHumidity[] = UNITY_ACCESS_INSTANCED_PROP(Props, baseStartHumidity);
    
    float heightPercent = inverseLerp(minH, maxH, worldPos.y);
    float tempPercentage = uv.x;
    float humidityPercentage = uv.y;
    
    float3 blendAxes = abs(worldNormal);
    blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
    
    float3 color = float3(0, 0, 0);
    
    for (int i = 0; i < count; i++)
    {
				
        float drawStrength = inverseLerp(-blends[i] / 2 - epsilon, blends[i] / 2, min((humidityPercentage - startHumidity[i]), min((heightPercent - startHeights[i]), (tempPercentage - startTemp[i]))));
        float3 baseColour = colours[i] * colourStrength[i];
        float3 textureColour = triplanar(worldPos, textureScales[i], blendAxes, i, texArray, Sampler) * (1 - colourStrength[i]);

        color = color * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
    }
    
    output = color;
}

#endif
