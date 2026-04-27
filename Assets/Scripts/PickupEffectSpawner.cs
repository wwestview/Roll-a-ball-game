using UnityEngine;

/// <summary>
/// Procedurally creates particle effects for pickup collection.
/// Provides a static method to spawn spark bursts at any position.
/// </summary>
public class PickupEffectSpawner : MonoBehaviour
{
    public static PickupEffectSpawner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawns a burst of sparks at the given world position with the specified color.
    /// </summary>
    public void SpawnPickupEffect(Vector3 position, Color color)
    {
        GameObject effectObj = new GameObject("PickupEffect");
        effectObj.transform.position = position;

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();

        // Stop the auto-play to configure first
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main module
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
        main.maxParticles = 50;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;

        // Emission - single burst
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 30, 50)
        });

        // Shape - sphere
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // Color over lifetime - fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(Color.yellow, 0.3f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // Size over lifetime - shrink
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Renderer setup for URP
        var renderer = effectObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(color);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // Light module for glow effect
        var lights = ps.lights;
        lights.enabled = false; // Keep disabled for performance

        // Play and destroy after completion
        ps.Play();
        Destroy(effectObj, 2f);
    }

    private Material CreateParticleMaterial(Color color)
    {
        // Try to use URP particle shader first, fallback to standard
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = color;

        // Enable additive blending for glow
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 1);   // Additive
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;

        return mat;
    }
}
