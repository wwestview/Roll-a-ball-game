using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    private int count;

    private float movementX;
    private float movementY;

    public float speed = 0;

    public TextMeshProUGUI countText;

    public GameObject winTextObject;

    // ==================== JUMP ====================
    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.6f;
    private bool isGrounded = false;

    // ==================== SPEED MODIFIER ====================
    private float baseSpeed;
    private float speedModifier = 1f;
    private Coroutine activeSpeedCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        count = 0;
        baseSpeed = speed;

        SetCountText();

        winTextObject.SetActive(false);
    }

    void Update()
    {
        // Ground check via raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        // Jump input (Space key)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();

        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        rb.AddForce(movement * speed);
    }

    // ==================== SPEED MODIFIER SYSTEM ====================

    /// <summary>
    /// Applies a temporary speed modifier (boost or penalty).
    /// Called by PowerUpItem when collected.
    /// </summary>
    public void ApplySpeedModifier(float multiplier, float duration, string effectName, Color effectColor)
    {
        // Cancel previous modifier if active
        if (activeSpeedCoroutine != null)
        {
            StopCoroutine(activeSpeedCoroutine);
            speed = baseSpeed; // Reset before applying new
        }

        activeSpeedCoroutine = StartCoroutine(SpeedModifierCoroutine(multiplier, duration, effectName, effectColor));
    }

    IEnumerator SpeedModifierCoroutine(float multiplier, float duration, string effectName, Color effectColor)
    {
        // Apply modifier
        speedModifier = multiplier;
        speed = baseSpeed * speedModifier;

        // Show HUD notification
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowPowerUpStatus(effectName, effectColor, duration);
        }

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Reset speed
        speedModifier = 1f;
        speed = baseSpeed;
        activeSpeedCoroutine = null;

        // Clear HUD notification
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.HidePowerUpStatus();
        }
    }

    // ==================== PICKUPS ====================

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            // Skip if it's a PowerUpItem (handled by its own script)
            if (other.gameObject.GetComponent<PowerUpItem>() != null)
                return;

            // Spawn pickup particle effect
            if (PickupEffectSpawner.Instance != null)
            {
                Color effectColor = new Color(1f, 0.85f, 0f);
                Renderer pickupRenderer = other.gameObject.GetComponent<Renderer>();
                if (pickupRenderer != null)
                {
                    effectColor = pickupRenderer.material.color;
                }
                PickupEffectSpawner.Instance.SpawnPickupEffect(other.transform.position, effectColor);
            }

            // Play pickup "ding" sound
            if (GameAudioManager.Instance != null)
            {
                GameAudioManager.Instance.PlayPickupSound();
            }

            other.gameObject.SetActive(false);

            count = count + 1;

            // Update HUD score FIRST
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateScore(count);
            }

            SetCountText();
        }
    }

    void SetCountText()
    {
        // Hide legacy UI text to avoid double count on screen
        if (countText != null && countText.gameObject.activeSelf)
        {
            countText.gameObject.SetActive(false);
        }

        int targetToWin = 12;
        if (GameUIManager.Instance != null)
        {
            targetToWin = GameUIManager.Instance.TotalPickups;
        }

        if (count >= targetToWin && targetToWin > 0)
        {
            if (winTextObject != null)
            {
                winTextObject.SetActive(false);
            }

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowWinPopup();
            }
            else
            {
                winTextObject.SetActive(true);
            }

            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowLosePopup("YOU LOSE!");
            }
            else
            {
                winTextObject.gameObject.SetActive(true);
                winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            }
        }
        else
        {
            if (collision.relativeVelocity.magnitude > 2f)
            {
                if (GameAudioManager.Instance != null)
                {
                    GameAudioManager.Instance.PlayWallHitSound();
                }
            }
        }
    }
}