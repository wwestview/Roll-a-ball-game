using UnityEngine;

/// <summary>
/// Power-up / penalty pickup item.
/// Gold objects give speed boost for 5 seconds.
/// Red objects reduce speed for 5 seconds.
/// Attach to a GameObject with a trigger collider and tag "PickUp".
/// Created procedurally by LevelBuilder.
/// </summary>
public class PowerUpItem : MonoBehaviour
{
    public enum PowerUpType
    {
        SpeedBoost,   // Gold — increases speed
        SpeedPenalty  // Red — decreases speed
    }

    public PowerUpType type = PowerUpType.SpeedBoost;
    public float speedMultiplier = 1.5f;    // Boost: 1.5x speed
    public float penaltyMultiplier = 0.5f;  // Penalty: 0.5x speed
    public float duration = 5f;             // Duration in seconds

    [Header("Visual Pulse")]
    public float pulseSpeed = 3f;
    public float pulseIntensity = 0.3f;

    private Renderer rend;
    private Color baseEmissionColor;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            if (mat.HasProperty("_EmissionColor"))
            {
                baseEmissionColor = mat.GetColor("_EmissionColor");
            }
        }
    }

    void Update()
    {
        // Rotate like a pickup
        transform.Rotate(new Vector3(0, 60, 0) * Time.deltaTime);

        // Pulse emission for visual feedback
        if (mat != null && mat.HasProperty("_EmissionColor"))
        {
            float pulse = 1f + pulseIntensity * Mathf.Sin(Time.time * pulseSpeed);
            mat.SetColor("_EmissionColor", baseEmissionColor * pulse);
        }

        // Bob up and down
        float bob = Mathf.Sin(Time.time * 2f) * 0.15f;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + bob * Time.deltaTime,
            transform.position.z
        );
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            // Apply effect
            float multiplier = (type == PowerUpType.SpeedBoost) ? speedMultiplier : penaltyMultiplier;
            string effectName = (type == PowerUpType.SpeedBoost) ? "SPEED BOOST!" : "SPEED DOWN!";
            Color effectColor = (type == PowerUpType.SpeedBoost) ? new Color(1f, 0.85f, 0f) : new Color(1f, 0.2f, 0.2f);

            player.ApplySpeedModifier(multiplier, duration, effectName, effectColor);

            // Spawn particle effect
            if (PickupEffectSpawner.Instance != null)
            {
                PickupEffectSpawner.Instance.SpawnPickupEffect(transform.position, effectColor);
            }

            // Play sound
            if (GameAudioManager.Instance != null)
            {
                GameAudioManager.Instance.PlayPickupSound();
            }

            // Destroy this power-up
            Destroy(gameObject);
        }
    }
}
