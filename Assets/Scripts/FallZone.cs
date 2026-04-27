using UnityEngine;
using TMPro;

/// <summary>
/// Detects when the player falls below a certain Y threshold and triggers Game Over.
/// Attach to an empty GameObject or let LevelBuilder create it.
/// </summary>
public class FallZone : MonoBehaviour
{
    public float fallThreshold = -5f;
    public GameObject playerObject;
    public GameObject winTextObject;

    private bool triggered = false;

    void Update()
    {
        if (triggered || playerObject == null) return;

        if (playerObject.transform.position.y < fallThreshold)
        {
            triggered = true;
            OnPlayerFell();
        }
    }

    void OnPlayerFell()
    {
        // Use new End Game popup if available
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowLosePopup("YOU FELL!");
        }
        else if (winTextObject != null)
        {
            // Fallback to legacy UI
            winTextObject.SetActive(true);
            var tmp = winTextObject.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "You Fell! Game Over!";
                tmp.color = new Color(1f, 0.3f, 0.3f);
            }
        }

        // Destroy the player
        Destroy(playerObject);

        // Play fall sound
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayWallHitSound();
        }
    }
}
