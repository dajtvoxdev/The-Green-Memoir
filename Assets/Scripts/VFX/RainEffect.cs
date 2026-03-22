using UnityEngine;

/// <summary>
/// Rain particle effect that follows the camera.
/// Subscribes to WeatherManager to auto-show/hide rain based on weather state.
///
/// Usage: Attach to a GameObject in PlayScene. No configuration needed —
/// creates particle system at runtime and follows Main Camera.
/// </summary>
public class RainEffect : MonoBehaviour
{
    [Header("Rain Settings")]
    [Tooltip("Number of rain particles emitted per second.")]
    public int emissionRate = 300;

    [Tooltip("Rain drop fall speed.")]
    public float fallSpeed = 8f;

    [Tooltip("Horizontal spread area of rain.")]
    public float spreadX = 20f;

    [Tooltip("Vertical spawn height above camera.")]
    public float spawnHeight = 10f;

    [Tooltip("Rain drop color.")]
    public Color rainColor = new Color(0.6f, 0.75f, 1f, 0.35f);

    [Tooltip("Rain drop size.")]
    public float dropSize = 0.06f;

    [Tooltip("Rain drop length (stretch).")]
    public float dropLength = 3f;

    private ParticleSystem rainPS;
    private ParticleSystemRenderer psRenderer;

    void Start()
    {
        CreateRainParticleSystem();

        // Subscribe to weather changes
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherChanged += OnWeatherChanged;
            // Set initial state
            SetRainActive(WeatherManager.Instance.IsRainingToday);
        }
        else
        {
            SetRainActive(false);
        }
    }

    void LateUpdate()
    {
        // Follow the main camera
        if (Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y + spawnHeight, camPos.z);
        }
    }

    private void OnWeatherChanged(WeatherManager.WeatherType weather)
    {
        SetRainActive(weather == WeatherManager.WeatherType.Rainy);
    }

    private void SetRainActive(bool active)
    {
        if (rainPS == null) return;

        if (active)
        {
            if (!rainPS.isPlaying) rainPS.Play();
            Debug.Log("[RainEffect] Rain started");
        }
        else
        {
            if (rainPS.isPlaying) rainPS.Stop();
            Debug.Log("[RainEffect] Rain stopped");
        }
    }

    private void CreateRainParticleSystem()
    {
        // Create child GameObject with ParticleSystem
        var rainGO = new GameObject("RainParticles");
        rainGO.transform.SetParent(transform, false);

        rainPS = rainGO.AddComponent<ParticleSystem>();
        psRenderer = rainGO.GetComponent<ParticleSystemRenderer>();

        // Stop default playback to configure
        rainPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main module
        var main = rainPS.main;
        main.loop = true;
        main.startLifetime = 2f;
        main.startSpeed = fallSpeed;
        main.startSize = dropSize;
        main.startColor = rainColor;
        main.maxParticles = 2000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;

        // Emission
        var emission = rainPS.emission;
        emission.rateOverTime = emissionRate;

        // Shape — box spread above camera
        var shape = rainPS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spreadX, 0.5f, 1f);
        shape.rotation = new Vector3(0f, 0f, 0f);

        // Velocity over lifetime — rain falls downward
        // All axes must use the same MinMaxCurve mode (Random Between Two Constants)
        var vel = rainPS.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); // slight wind
        vel.y = new ParticleSystem.MinMaxCurve(-fallSpeed * 0.5f, -fallSpeed * 0.5f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        // Renderer — stretched billboard for raindrop look
        psRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        psRenderer.lengthScale = dropLength;
        psRenderer.velocityScale = 0.1f;

        // Use default particle material
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psRenderer.material.color = rainColor;
        psRenderer.sortingOrder = 50; // Above most things

        // Color over lifetime — fade out at end
        var col = rainPS.colorOverLifetime;
        col.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 0.8f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = gradient;
    }

    void OnDestroy()
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherChanged -= OnWeatherChanged;
        }
    }
}
