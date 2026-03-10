using UnityEngine;
using UnityEditor;

/// <summary>
/// Adds ambient particle effects to the Vietnamese farm scene:
///   - Pond mist (sương mù ao) — soft fog over the water
///   - Falling leaves (lá rơi) — gentle leaf particles from tree areas
///   - Fireflies at dusk (đom đóm) — small glowing dots near vegetation
/// </summary>
public static class AddFarmAmbience
{
    [MenuItem("Tools/Add Farm Ambience Effects")]
    public static void AddAmbience()
    {
        GameObject parent = GameObject.Find("FarmAmbience");
        if (parent != null) Object.DestroyImmediate(parent);
        parent = new GameObject("FarmAmbience");

        // ─── Pond Mist (sương mù ao) ───
        CreatePondMist(parent);

        // ─── Falling Leaves (lá rơi) ───
        CreateFallingLeaves(parent);

        // ─── Fireflies (đom đóm) ───
        CreateFireflies(parent);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("AddFarmAmbience: 3 ambient effects added (mist, leaves, fireflies)!");
    }

    static void CreatePondMist(GameObject parent)
    {
        GameObject mist = new GameObject("PondMist");
        mist.transform.parent = parent.transform;
        mist.transform.position = new Vector3(0f, 2f, -1f); // Over pond, z=-1 in front of tilemap

        var ps = mist.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 5f;
        main.startSpeed = 0.02f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f); // Small wisps, not blanket
        main.startColor = new Color(0.85f, 0.9f, 0.95f, 0.06f);     // Very subtle alpha
        main.maxParticles = 10;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 1.5f; // Sparse, not thick fog

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(5f, 3f, 0.1f); // Tighter around pond only

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.85f, 0.9f, 0.95f), 0f),
                new GradientColorKey(new Color(0.85f, 0.9f, 0.95f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.05f, 0.3f),  // Very faint peak
                new GradientAlphaKey(0.05f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var renderer = mist.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 5;
        renderer.material = GetParticleMaterial();

        Debug.Log("AddFarmAmbience: Pond mist created at (0, 2)");
    }

    static void CreateFallingLeaves(GameObject parent)
    {
        GameObject leaves = new GameObject("FallingLeaves");
        leaves.transform.parent = parent.transform;
        leaves.transform.position = new Vector3(-5f, 6f, -1f); // Above tree line, z=-1 in front

        var ps = leaves.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.7f, 0.55f, 0.12f, 0.7f),  // Golden brown
            new Color(0.4f, 0.6f, 0.15f, 0.7f)     // Olive green
        );
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.02f;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

        var emission = ps.emission;
        emission.rateOverTime = 2f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(25f, 3f, 0.1f); // Wide area across map

        // Wind drift via gravity + start velocity (avoids VelocityOverLifetime mode conflicts)
        main.gravityModifier = 0.03f;

        // Tumble via random start rotation (avoids RotationOverLifetime mode conflicts)
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.6f, 0.5f, 0.15f), 0f),
                new GradientColorKey(new Color(0.5f, 0.35f, 0.1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.7f, 0.15f),
                new GradientAlphaKey(0.6f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var renderer = leaves.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 10;
        renderer.material = GetParticleMaterial();

        Debug.Log("AddFarmAmbience: Falling leaves created above tree line");
    }

    static void CreateFireflies(GameObject parent)
    {
        GameObject flies = new GameObject("Fireflies");
        flies.transform.parent = parent.transform;
        flies.transform.position = new Vector3(-3f, 0f, -1f); // Near pond/vegetation, z=-1 in front

        var ps = flies.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
        main.startColor = new Color(0.9f, 1f, 0.4f, 0.8f); // Warm yellow-green glow
        main.maxParticles = 25;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 4f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 12f, 0.1f); // Across whole map

        // Random wandering motion
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;

        // Pulsing glow effect
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.9f, 1f, 0.4f), 0f),
                new GradientColorKey(new Color(0.7f, 0.9f, 0.2f), 0.5f),
                new GradientColorKey(new Color(0.9f, 1f, 0.4f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.2f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0.8f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // Size pulsing
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.5f),
            new Keyframe(0.3f, 1f),
            new Keyframe(0.6f, 0.6f),
            new Keyframe(1f, 0f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = flies.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 12;
        renderer.material = GetParticleMaterial();

        Debug.Log("AddFarmAmbience: Fireflies created for evening ambience");
    }

    /// <summary>
    /// Creates a particle material that works reliably in 2D.
    /// Falls back through multiple shader names to find one that exists.
    /// </summary>
    static Material GetParticleMaterial()
    {
        // Try built-in particle additive shader first (best for glowing effects in 2D)
        string[] shaderNames = {
            "Particles/Standard Unlit",
            "Mobile/Particles/Additive",
            "Sprites/Default",
            "Particles/Alpha Blended"
        };

        foreach (var name in shaderNames)
        {
            var shader = Shader.Find(name);
            if (shader != null)
            {
                var mat = new Material(shader);
                // Enable transparency
                mat.SetFloat("_Mode", 2); // Fade mode
                mat.color = Color.white;
                Debug.Log($"AddFarmAmbience: Using shader '{name}' for particles");
                return mat;
            }
        }

        // Absolute fallback: use Unity's default particle material
        Debug.LogWarning("AddFarmAmbience: No suitable shader found, using default material");
        return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
    }
}
