using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates and manages the Main Menu UI at runtime.
/// Auto-initializes when the MainMenu scene loads.
/// Builds a polished start screen with title, Start and Quit buttons.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    void Start()
    {
        BuildMenuUI();
    }

    void BuildMenuUI()
    {
        // Set camera background to dark cosmic color
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0.03f, 0.01f, 0.08f);

        // Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if not present
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ==================== BACKGROUND ====================

        // Dark gradient background panel
        GameObject bgPanel = CreatePanel(canvasObj.transform, "Background", new Color(0.03f, 0.01f, 0.08f, 1f));
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Decorative overlay with slight gradient
        GameObject overlayPanel = CreatePanel(bgPanel.transform, "Overlay", new Color(0.1f, 0.02f, 0.15f, 0.5f));
        RectTransform overlayRect = overlayPanel.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;

        // ==================== TITLE ====================

        // Game Title
        GameObject titleObj = CreateTextElement(bgPanel.transform, "GameTitle", "ROLL-A-BALL",
            72, new Color(0.4f, 0.8f, 1f), TextAlignmentOptions.Center);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 200);
        titleRect.sizeDelta = new Vector2(800, 120);

        // Subtitle
        GameObject subtitleObj = CreateTextElement(bgPanel.transform, "Subtitle", "COSMIC EDITION",
            28, new Color(0.7f, 0.5f, 1f), TextAlignmentOptions.Center);
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.anchoredPosition = new Vector2(0, 120);
        subtitleRect.sizeDelta = new Vector2(600, 50);

        // Decorative line under title
        GameObject lineObj = CreatePanel(bgPanel.transform, "DecoLine", new Color(0.4f, 0.8f, 1f, 0.6f));
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = new Vector2(0, 95);
        lineRect.sizeDelta = new Vector2(400, 2);

        // ==================== BUTTONS ====================

        // Start Button
        CreateMenuButton(bgPanel.transform, "StartButton", "START GAME",
            new Vector2(0, -20), new Color(0.1f, 0.6f, 0.4f), () =>
        {
            SceneManager.LoadScene("MiniGame");
        });

        // Quit Button
        CreateMenuButton(bgPanel.transform, "QuitButton", "QUIT",
            new Vector2(0, -110), new Color(0.6f, 0.15f, 0.15f), () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        // ==================== FOOTER ====================

        GameObject footerObj = CreateTextElement(bgPanel.transform, "Footer", "Use WASD or Arrow Keys to move the ball\nCollect all pickups to win!",
            18, new Color(0.5f, 0.5f, 0.6f), TextAlignmentOptions.Center);
        RectTransform footerRect = footerObj.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0.5f, 0);
        footerRect.anchorMax = new Vector2(0.5f, 0);
        footerRect.anchoredPosition = new Vector2(0, 60);
        footerRect.sizeDelta = new Vector2(600, 60);

        // ==================== ANIMATED PARTICLES (decorative floating cubes) ====================
        StartCoroutine(AnimateTitle(titleObj));
    }

    System.Collections.IEnumerator AnimateTitle(GameObject titleObj)
    {
        TMP_Text text = titleObj.GetComponent<TMP_Text>();
        float time = 0;
        while (titleObj != null)
        {
            time += Time.deltaTime;
            // Gentle pulsing glow effect on title color
            float pulse = 0.8f + 0.2f * Mathf.Sin(time * 2f);
            text.color = new Color(0.4f * pulse, 0.8f * pulse, 1f * pulse);
            yield return null;
        }
    }

    // ==================== UI HELPER METHODS ====================

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();

        Image img = panel.AddComponent<Image>();
        img.color = color;

        return panel;
    }

    GameObject CreateTextElement(Transform parent, string name, string text,
        int fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return textObj;
    }

    void CreateMenuButton(Transform parent, string name, string label,
        Vector2 position, Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        // Button container
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(320, 65);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Button hover colors
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        colors.selectedColor = bgColor * 1.1f;
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }
}
