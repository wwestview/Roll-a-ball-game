using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

/// <summary>
/// Procedurally builds extended level geometry at runtime:
/// - Ramps (inclined planes) connecting different height levels
/// - A raised second-level platform
/// - Moving obstacles that patrol between points
/// - A fall zone (gap in the floor) with death trigger
/// - Post-processing volume with Bloom and Color Grading
/// - PowerUps (Speed Boost, Speed Penalty)
/// - SkyboxController, GameAudioManager, PickupEffectSpawner singletons
/// - FallZone monitor
///
/// Attach this to an empty GameObject in the scene named "LevelBuilder".
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    [Header("References (auto-found if null)")]
    public GameObject player;
    public GameObject winTextObject;

    [Header("Level Settings")]
    [Tooltip("Вимкніть це, якщо ви скопіювали об'єкти в сцену вручну!")]
    public bool generateGeometry = true;
    public float rampWidth = 3f;
    public float rampLength = 6f;
    public float platformHeight = 3f;
    public float movingObstacleSpeed = 2.5f;

    [Header("Materials")]
    public Color rampColor = new Color(0.3f, 0.35f, 0.5f);
    public Color platformColor = new Color(0.25f, 0.3f, 0.45f);
    public Color obstacleColor = new Color(0.9f, 0.2f, 0.2f);
    public Color fallZoneEdgeColor = new Color(0.8f, 0.1f, 0.1f);

    void Start()
    {
        // Auto-find references if not assigned
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (winTextObject == null)
        {
            // Find by name in the scene
            GameObject winText = GameObject.Find("Win Text");
            if (winText != null) winTextObject = winText;
        }

        if (generateGeometry)
        {
            BuildLevel();
            SpawnPowerUps();
        }

        SetupManagers();
        SetupPostProcessing();

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.InitializePickupsCount();
        }
    }

    void BuildLevel()
    {
        BuildRamps();
        BuildRaisedPlatform();
        BuildMovingObstacles();
        BuildFallZone();
    }

    void SetupManagers()
    {
        // Setup SkyboxController
        if (FindFirstObjectByType<SkyboxController>() == null)
        {
            GameObject skyboxObj = new GameObject("SkyboxController");
            skyboxObj.AddComponent<SkyboxController>();
        }

        // Setup GameAudioManager
        if (GameAudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("GameAudioManager");
            audioObj.AddComponent<GameAudioManager>();
        }

        // Setup PickupEffectSpawner
        if (PickupEffectSpawner.Instance == null)
        {
            GameObject effectObj = new GameObject("PickupEffectSpawner");
            effectObj.AddComponent<PickupEffectSpawner>();
        }

        // Setup FallZone monitor
        if (FindFirstObjectByType<FallZone>() == null)
        {
            GameObject fallObj = new GameObject("FallZoneMonitor");
            FallZone fz = fallObj.AddComponent<FallZone>();
            fz.fallThreshold = -5f;
            fz.playerObject = player;
            fz.winTextObject = winTextObject;
        }

        // Setup Trail Renderer on player
        if (player != null)
        {
            SetupPlayerTrail(player);
        }
    }

    // ==================== POWERUPS ====================

    void SpawnPowerUps()
    {
        // Gold PowerUp (Speed Boost)
        CreatePowerUp(
            "GoldPowerUp",
            new Vector3(6f, 0.5f, 6f),
            PowerUpItem.PowerUpType.SpeedBoost,
            new Color(1f, 0.85f, 0f) // Gold
        );

        // Red PowerUp (Speed Penalty)
        CreatePowerUp(
            "RedPowerUp",
            new Vector3(-6f, 0.5f, -6f),
            PowerUpItem.PowerUpType.SpeedPenalty,
            new Color(1f, 0.2f, 0.2f) // Red
        );
    }

    void CreatePowerUp(string name, Vector3 position, PowerUpItem.PowerUpType type, Color color)
    {
        // We use a cylinder to distinguish from regular cube PickUps
        GameObject powerUpObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        powerUpObj.name = name;
        powerUpObj.transform.position = position;
        powerUpObj.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f); // Flattened cylinder (coin-like)
        powerUpObj.transform.eulerAngles = new Vector3(90f, 0f, 0f); // Stand it up

        // Setup Collider
        Collider col = powerUpObj.GetComponent<Collider>();
        if (col != null) Destroy(col);
        SphereCollider trigger = powerUpObj.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.5f;

        // Material (Emissive)
        Material mat = CreateMaterial(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 2f);
        powerUpObj.GetComponent<Renderer>().material = mat;

        // Add PowerUpItem script
        PowerUpItem pui = powerUpObj.AddComponent<PowerUpItem>();
        pui.type = type;

        // Add Rigidbody (needed for triggers sometimes depending on settings)
        Rigidbody rb = powerUpObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // ==================== RAMPS ====================

    void BuildRamps()
    {
        // Ramp 1: On the left side, going from ground level up to the raised platform
        CreateRamp(
            "Ramp_Left",
            new Vector3(-6f, platformHeight / 2f, 5f),
            new Vector3(20f, 0f, 0f), // Tilt on X axis
            new Vector3(rampWidth, 0.2f, rampLength)
        );

        // Ramp 2: On the right side, steeper ramp
        CreateRamp(
            "Ramp_Right",
            new Vector3(6f, platformHeight / 2f, -3f),
            new Vector3(25f, 180f, 0f), // Tilted and rotated
            new Vector3(rampWidth, 0.2f, rampLength * 0.8f)
        );
    }

    GameObject CreateRamp(string name, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = name;
        ramp.transform.position = position;
        ramp.transform.eulerAngles = rotation;
        ramp.transform.localScale = scale;

        // Apply material
        Renderer rend = ramp.GetComponent<Renderer>();
        rend.material = CreateMaterial(rampColor);

        // Ensure collider is present (CreatePrimitive adds one)
        return ramp;
    }

    // ==================== RAISED PLATFORM ====================

    void BuildRaisedPlatform()
    {
        // Create a raised platform (second level)
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "RaisedPlatform";
        platform.transform.position = new Vector3(-6f, platformHeight, 8f);
        platform.transform.localScale = new Vector3(8f, 0.3f, 6f);

        Renderer rend = platform.GetComponent<Renderer>();
        rend.material = CreateMaterial(platformColor);

        // Add guard rails (small walls) on the edges
        CreateGuardRail("Rail_Back", new Vector3(-6f, platformHeight + 0.4f, 10.8f), new Vector3(8f, 0.8f, 0.3f));
        CreateGuardRail("Rail_Left", new Vector3(-9.8f, platformHeight + 0.4f, 8f), new Vector3(0.3f, 0.8f, 6f));
        CreateGuardRail("Rail_Right", new Vector3(-2.2f, platformHeight + 0.4f, 8f), new Vector3(0.3f, 0.8f, 6f));

        // Place a bonus pickup on the raised platform
        CreateBonusPickup(new Vector3(-6f, platformHeight + 1f, 8f));
    }

    void CreateGuardRail(string name, Vector3 position, Vector3 scale)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.position = position;
        rail.transform.localScale = scale;

        Renderer rend = rail.GetComponent<Renderer>();
        Material mat = CreateMaterial(new Color(0.4f, 0.4f, 0.5f));
        mat.SetFloat("_Metallic", 0.6f);
        mat.SetFloat("_Smoothness", 0.7f);
        rend.material = mat;
    }

    void CreateBonusPickup(Vector3 position)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.name = "BonusPickup";
        pickup.tag = "PickUp";
        pickup.transform.position = position;
        pickup.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        pickup.transform.eulerAngles = new Vector3(45f, 45f, 45f);

        // Make it a trigger
        BoxCollider col = pickup.GetComponent<BoxCollider>();
        col.isTrigger = true;

        // Add Rigidbody (kinematic)
        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // Add Rotator
        pickup.AddComponent<Rotator>();

        // Emissive yellow material (will glow with Bloom)
        Material mat = CreateMaterial(new Color(1f, 0.85f, 0f));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0f) * 2f);
        pickup.GetComponent<Renderer>().material = mat;
    }

    // ==================== MOVING OBSTACLES ====================

    void BuildMovingObstacles()
    {
        // Moving obstacle 1: Patrols left-right across the arena center
        CreateMovingObstacle(
            "MovingBlock_1",
            new Vector3(-5f, 0.75f, 0f),
            new Vector3(5f, 0.75f, 0f),
            new Vector3(1.5f, 1.5f, 1.5f),
            movingObstacleSpeed
        );

        // Moving obstacle 2: Patrols forward-backward on the right side
        CreateMovingObstacle(
            "MovingBlock_2",
            new Vector3(4f, 0.75f, -6f),
            new Vector3(4f, 0.75f, 6f),
            new Vector3(1f, 1.5f, 1f),
            movingObstacleSpeed * 0.8f
        );

        // Moving obstacle 3: Near the ramp, smaller and faster
        CreateMovingObstacle(
            "MovingBlock_3",
            new Vector3(-8f, 0.5f, 2f),
            new Vector3(-4f, 0.5f, 2f),
            new Vector3(0.8f, 1f, 0.8f),
            movingObstacleSpeed * 1.3f
        );
    }

    void CreateMovingObstacle(string name, Vector3 pointA, Vector3 pointB, Vector3 scale, float speed)
    {
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.name = name;
        obs.transform.position = pointA;
        obs.transform.localScale = scale;

        // Material - red/orange warning color, emissive
        Material mat = CreateMaterial(obstacleColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", obstacleColor * 1.5f);
        obs.GetComponent<Renderer>().material = mat;

        // Add Rigidbody (kinematic so physics doesn't affect it, but it pushes the player)
        Rigidbody rb = obs.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Add MovingObstacle script
        MovingObstacle mo = obs.AddComponent<MovingObstacle>();
        mo.pointA = pointA;
        mo.pointB = pointB;
        mo.speed = speed;
        mo.useLocalSpace = false;
    }

    // ==================== FALL ZONE (GAP) ====================

    void BuildFallZone()
    {
        // Create a gap in the floor by placing "bridge" pieces around it
        // The gap is an area with no floor, so the player falls through

        // Visual edge markers (glowing red borders around the gap)
        float gapCenterX = 0f;
        float gapCenterZ = -6f;
        float gapWidth = 4f;
        float gapDepth = 3f;

        // North edge of gap
        CreateGapEdge("GapEdge_N", new Vector3(gapCenterX, 0.05f, gapCenterZ + gapDepth / 2f), new Vector3(gapWidth + 0.2f, 0.1f, 0.15f));
        // South edge
        CreateGapEdge("GapEdge_S", new Vector3(gapCenterX, 0.05f, gapCenterZ - gapDepth / 2f), new Vector3(gapWidth + 0.2f, 0.1f, 0.15f));
        // East edge
        CreateGapEdge("GapEdge_E", new Vector3(gapCenterX + gapWidth / 2f, 0.05f, gapCenterZ), new Vector3(0.15f, 0.1f, gapDepth + 0.2f));
        // West edge
        CreateGapEdge("GapEdge_W", new Vector3(gapCenterX - gapWidth / 2f, 0.05f, gapCenterZ), new Vector3(0.15f, 0.1f, gapDepth + 0.2f));

        // Create invisible floor-removal plane (a thin trigger zone inside the gap
        // that teleports/destroys the player if they somehow touch it at Y=0 level)
        // Actually, the fall detection is handled by FallZone.cs monitoring Y position.

        // Visual: place a semi-transparent dark plane at the gap to show the void
        GameObject voidPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        voidPlane.name = "GapVoid";
        voidPlane.transform.position = new Vector3(gapCenterX, -0.01f, gapCenterZ);
        voidPlane.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        voidPlane.transform.localScale = new Vector3(gapWidth, gapDepth, 1f);

        // Remove collider from the void visual
        Collider voidCol = voidPlane.GetComponent<Collider>();
        if (voidCol != null) Destroy(voidCol);

        // Dark transparent material
        Material voidMat = CreateTransparentMaterial(new Color(0.05f, 0f, 0.1f, 0.8f));
        voidPlane.GetComponent<Renderer>().material = voidMat;

        // Now create actual gap blockers — remove floor in that area
        // Since we can't easily cut a hole in the existing plane mesh,
        // we'll raise transparent walls around the gap to prevent accidental entry
        // and create thin floor sections that bridge around the gap.

        // The ground plane is at y=0 with scale (2,2,2) on a default plane (10x10 units = 20x20 with scale).
        // We need to place cover-up floor pieces everywhere EXCEPT the gap.
        // Simpler approach: place a thin invisible trigger box in the gap area
        // so when the ball rolls over it, it falls.

        // Create gap trigger — removes floor collision in this area
        GameObject gapTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gapTrigger.name = "GapTriggerZone";
        gapTrigger.transform.position = new Vector3(gapCenterX, 0.2f, gapCenterZ);
        gapTrigger.transform.localScale = new Vector3(gapWidth, 0.5f, gapDepth);

        // Make invisible
        Renderer gapRend = gapTrigger.GetComponent<Renderer>();
        gapRend.enabled = false;

        // Configure as trigger
        BoxCollider gapCol = gapTrigger.GetComponent<BoxCollider>();
        gapCol.isTrigger = true;

        // Add a script component to push the ball downward when entering
        gapTrigger.AddComponent<GapTrigger>();
    }

    void CreateGapEdge(string name, Vector3 position, Vector3 scale)
    {
        GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edge.name = name;
        edge.transform.position = position;
        edge.transform.localScale = scale;

        // Emissive red warning color
        Material mat = CreateMaterial(fallZoneEdgeColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", fallZoneEdgeColor * 3f);
        edge.GetComponent<Renderer>().material = mat;
    }

    // ==================== POST-PROCESSING ====================

    void SetupPostProcessing()
    {
        // Create a Global Volume for post-processing
        GameObject volumeObj = new GameObject("PostProcessVolume");
        Volume volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;

        // Bloom
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.8f);
        bloom.intensity.Override(1.2f);
        bloom.scatter.Override(0.7f);
        bloom.tint.Override(new Color(0.9f, 0.85f, 1f));

        // Color Adjustments (Color Grading)
        ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.postExposure.Override(0.3f);
        colorAdj.contrast.Override(12f);
        colorAdj.saturation.Override(15f);
        colorAdj.colorFilter.Override(new Color(0.95f, 0.9f, 1f)); // Slight cool tint

        // Tonemapping
        Tonemapping tonemap = profile.Add<Tonemapping>(true);
        tonemap.mode.Override(TonemappingMode.ACES);

        // Vignette
        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.3f);
        vignette.smoothness.Override(0.5f);
        vignette.color.Override(new Color(0.1f, 0.0f, 0.15f));
    }

    // ==================== PLAYER TRAIL ====================

    void SetupPlayerTrail(GameObject playerObj)
    {
        // Add Trail Renderer to the player ball
        TrailRenderer trail = playerObj.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = playerObj.AddComponent<TrailRenderer>();
        }

        trail.time = 0.5f;
        trail.startWidth = 0.4f;
        trail.endWidth = 0.0f;
        trail.minVertexDistance = 0.1f;

        // Create gradient for trail (cyan to transparent)
        Gradient trailGradient = new Gradient();
        trailGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0f, 0.8f, 1f), 0f),
                new GradientColorKey(new Color(0.2f, 0.4f, 1f), 0.5f),
                new GradientColorKey(new Color(0.5f, 0f, 1f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = trailGradient;

        // Trail material (URP compatible)
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material trailMat = new Material(shader);
        trailMat.color = new Color(0f, 0.8f, 1f, 0.8f);
        trailMat.SetFloat("_Surface", 1); // Transparent
        trailMat.renderQueue = 3000;

        trail.material = trailMat;

        // Disable shadow casting for trail
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
    }

    // ==================== MATERIAL HELPERS ====================

    Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    Material CreateTransparentMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.color = color;

        // Enable transparency
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        return mat;
    }
}

/// <summary>
/// Simple trigger component for the gap/pit area.
/// When the player ball enters, it disables the ground collider below
/// and applies a downward force to simulate falling.
/// </summary>
public class GapTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has a PlayerController
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            // Apply strong downward force to make the ball fall
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.AddForce(Vector3.down * 15f, ForceMode.Impulse);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.down * 30f, ForceMode.Force);
            }
        }
    }
}
