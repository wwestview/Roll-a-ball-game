using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Creates a procedural sunset/cosmic skybox and configures atmospheric lighting at runtime.
/// Attach this to an empty GameObject in the scene.
/// </summary>
public class SkyboxController : MonoBehaviour
{
    [Header("Skybox Colors")]
    public Color skyTopColor = new Color(0.05f, 0.02f, 0.15f);      // Deep cosmic purple
    public Color skyHorizonColor = new Color(0.8f, 0.25f, 0.1f);    // Sunset orange
    public Color groundColor = new Color(0.1f, 0.05f, 0.02f);       // Dark ground

    [Header("Lighting")]
    public Color ambientSkyColor = new Color(0.15f, 0.08f, 0.25f);    // Purple ambient
    public Color ambientEquatorColor = new Color(0.3f, 0.15f, 0.1f);  // Warm equator
    public Color ambientGroundColor = new Color(0.05f, 0.03f, 0.02f); // Dark ground

    [Header("Directional Light Settings")]
    public Color sunColor = new Color(1f, 0.7f, 0.4f);   // Warm sunset sun
    public float sunIntensity = 1.2f;

    [Header("Stars")]
    [Range(0, 500)]
    public int starCount = 200;

    void Awake()
    {
        SetupSkybox();
        SetupLighting();
        SetupDirectionalLight();
    }

    void SetupSkybox()
    {
        // Create a procedural gradient skybox material
        // Unity built-in shader: "Skybox/Procedural" doesn't support custom gradients easily,
        // so we create a cubemap-style texture approach using a simple gradient texture on
        // a "Skybox/Panoramic" or fallback to tinting the procedural skybox.

        // Use the built-in Procedural skybox with atmosphere settings
        Material skyMat = new Material(Shader.Find("Skybox/Procedural"));
        if (skyMat != null)
        {
            // _SunDisk: 0=None, 1=Simple, 2=HighQuality
            skyMat.SetFloat("_SunDisk", 2);
            skyMat.SetFloat("_SunSize", 0.06f);
            skyMat.SetFloat("_SunSizeConvergence", 8f);
            skyMat.SetFloat("_AtmosphereThickness", 2.5f);
            skyMat.SetFloat("_Exposure", 0.8f);
            skyMat.SetColor("_SkyTint", new Color(0.3f, 0.15f, 0.5f));    // Purple tint
            skyMat.SetColor("_GroundColor", groundColor);

            RenderSettings.skybox = skyMat;
        }
        else
        {
            // Fallback: create a 6-sided skybox with gradient textures
            CreateGradientSkybox();
        }

        // Force reflection probe update
        DynamicGI.UpdateEnvironment();
    }

    void CreateGradientSkybox()
    {
        // Create gradient textures for each face
        int size = 512;
        Texture2D skyTex = CreateGradientTexture(size, skyTopColor, skyHorizonColor, groundColor);

        Material skyMat = new Material(Shader.Find("Skybox/Panoramic"));
        if (skyMat != null)
        {
            skyMat.SetTexture("_MainTex", skyTex);
            skyMat.SetFloat("_Exposure", 1.0f);
            skyMat.SetFloat("_Rotation", 0);
            RenderSettings.skybox = skyMat;
        }
    }

    Texture2D CreateGradientTexture(int size, Color top, Color horizon, Color bottom)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        // Add some stars in the upper portion
        System.Random rng = new System.Random(42);

        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            Color color;

            if (t > 0.5f)
            {
                // Upper half: horizon to top
                float localT = (t - 0.5f) * 2f;
                color = Color.Lerp(horizon, top, localT * localT);
            }
            else
            {
                // Lower half: bottom to horizon
                float localT = t * 2f;
                color = Color.Lerp(bottom, horizon, localT);
            }

            for (int x = 0; x < size; x++)
            {
                Color pixelColor = color;

                // Add procedural stars in the upper sky
                if (t > 0.6f && rng.NextDouble() < 0.002 * (t - 0.6f))
                {
                    float brightness = 0.5f + (float)rng.NextDouble() * 0.5f;
                    pixelColor = Color.Lerp(color, Color.white, brightness);
                }

                tex.SetPixel(x, y, pixelColor);
            }
        }

        tex.Apply();
        return tex;
    }

    void SetupLighting()
    {
        // Configure ambient lighting for sunset atmosphere
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;

        // Enable fog for depth atmosphere
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.fogColor = new Color(0.15f, 0.08f, 0.12f);
    }

    void SetupDirectionalLight()
    {
        // Find the directional light in the scene
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.color = sunColor;
                light.intensity = sunIntensity;
                // Angle the light like a sunset (low angle)
                light.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
                break;
            }
        }
    }
}
