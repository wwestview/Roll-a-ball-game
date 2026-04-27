using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the in-game HUD (score display), PowerUp status, and End Game popup.
/// Auto-created by GameBootstrapper in the MiniGame scene.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    // References to UI elements
    private TextMeshProUGUI hudScoreText;
    private GameObject endGamePanel;
    private TextMeshProUGUI endGameTitleText;
    private TextMeshProUGUI endGameScoreText;
    private Canvas hudCanvas;

    // PowerUp Status UI
    private GameObject powerUpPanel;
    private TextMeshProUGUI powerUpText;
    private Image powerUpBar;

    public int TotalPickups { get; private set; }
    private int currentScore = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        BuildHUD();
        BuildEndGamePopup();
        InitializePickupsCount(); // Initial check
    }

    /// <summary>
    /// Recalculates the total number of pickups. Called by LevelBuilder after generating bonus items.
    /// </summary>
    public void InitializePickupsCount()
    {
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("PickUp");
        TotalPickups = pickups.Length;
        UpdateScore(currentScore);
    }

    // ==================== PUBLIC API ====================

    /// <summary>
    /// Updates the HUD score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        currentScore = score;
        if (hudScoreText != null)
        {
            hudScoreText.text = $"\u2b50 {score} / {TotalPickups}";
        }
    }

    /// <summary>
    /// Shows the End Game popup with win message.
    /// </summary>
    public void ShowWinPopup()
    {
        ShowEndGamePopup("YOU WIN!", new Color(0.2f, 1f, 0.4f));
    }

    /// <summary>
    /// Shows the End Game popup with lose message.
    /// </summary>
    public void ShowLosePopup(string reason = "YOU LOSE!")
    {
        ShowEndGamePopup(reason, new Color(1f, 0.3f, 0.3f));
    }

    /// <summary>
    /// Shows the PowerUp status UI with a shrinking timer bar.
    /// </summary>
    public void ShowPowerUpStatus(string effectName, Color effectColor, float duration)
    {
        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(true);
            powerUpText.text = effectName;
            powerUpText.color = effectColor;
            powerUpBar.color = effectColor;

            // Stop any existing animation
            StopAllCoroutines();
            StartCoroutine(AnimatePowerUpBar(duration));
        }
    }

    /// <summary>
    /// Hides the PowerUp status UI.
    /// </summary>
    public void HidePowerUpStatus()
    {
        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(false);
            StopAllCoroutines();
        }
    }

    private System.Collections.IEnumerator AnimatePowerUpBar(float duration)
    {
        float timeRemaining = duration;
        Vector2 originalSize = powerUpBar.rectTransform.sizeDelta;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            float fillRatio = timeRemaining / duration;
            powerUpBar.rectTransform.sizeDelta = new Vector2(originalSize.x * fillRatio, originalSize.y);
            yield return null;
        }

        HidePowerUpStatus();
    }

    // ==================== HUD ====================

    void BuildHUD()
    {
        // Create HUD Canvas
        GameObject canvasObj = new GameObject("HUDCanvas");
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ---- Score Panel (top-left corner) ----
        GameObject scorePanel = new GameObject("ScorePanel");
        scorePanel.transform.SetParent(canvasObj.transform, false);

        RectTransform scorePanelRect = scorePanel.AddComponent<RectTransform>();
        scorePanelRect.anchorMin = new Vector2(0, 1);
        scorePanelRect.anchorMax = new Vector2(0, 1);
        scorePanelRect.pivot = new Vector2(0, 1);
        scorePanelRect.anchoredPosition = new Vector2(20, -15);
        scorePanelRect.sizeDelta = new Vector2(280, 60);

        Image scoreBg = scorePanel.AddComponent<Image>();
        scoreBg.color = new Color(0, 0, 0, 0.5f);

        // Score Text
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(scorePanel.transform, false);

        RectTransform scoreTextRect = scoreTextObj.AddComponent<RectTransform>();
        scoreTextRect.anchorMin = Vector2.zero;
        scoreTextRect.anchorMax = Vector2.one;
        scoreTextRect.offsetMin = new Vector2(15, 5);
        scoreTextRect.offsetMax = new Vector2(-15, -5);

        hudScoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        hudScoreText.text = $"\u2b50 0 / {TotalPickups}";
        hudScoreText.fontSize = 32;
        hudScoreText.color = new Color(1f, 0.9f, 0.3f);
        hudScoreText.fontStyle = FontStyles.Bold;
        hudScoreText.alignment = TextAlignmentOptions.MidlineLeft;

        // ---- PowerUp Status Panel (top-center) ----
        powerUpPanel = new GameObject("PowerUpPanel");
        powerUpPanel.transform.SetParent(canvasObj.transform, false);

        RectTransform powerUpRect = powerUpPanel.AddComponent<RectTransform>();
        powerUpRect.anchorMin = new Vector2(0.5f, 1);
        powerUpRect.anchorMax = new Vector2(0.5f, 1);
        powerUpRect.pivot = new Vector2(0.5f, 1);
        powerUpRect.anchoredPosition = new Vector2(0, -20);
        powerUpRect.sizeDelta = new Vector2(300, 60);

        Image powerUpBg = powerUpPanel.AddComponent<Image>();
        powerUpBg.color = new Color(0, 0, 0, 0.6f);

        // PowerUp Text
        GameObject pTextObj = new GameObject("PowerUpText");
        pTextObj.transform.SetParent(powerUpPanel.transform, false);
        RectTransform pTextRect = pTextObj.AddComponent<RectTransform>();
        pTextRect.anchorMin = Vector2.zero;
        pTextRect.anchorMax = Vector2.one;
        pTextRect.offsetMin = new Vector2(0, 10);
        pTextRect.offsetMax = new Vector2(0, -10);

        powerUpText = pTextObj.AddComponent<TextMeshProUGUI>();
        powerUpText.text = "SPEED BOOST!";
        powerUpText.fontSize = 24;
        powerUpText.alignment = TextAlignmentOptions.Center;
        powerUpText.fontStyle = FontStyles.Bold;

        // PowerUp Timer Bar
        GameObject pBarObj = new GameObject("PowerUpBar");
        pBarObj.transform.SetParent(powerUpPanel.transform, false);
        RectTransform pBarRect = pBarObj.AddComponent<RectTransform>();
        pBarRect.anchorMin = new Vector2(0, 0);
        pBarRect.anchorMax = new Vector2(0, 0);
        pBarRect.pivot = new Vector2(0, 0);
        pBarRect.anchoredPosition = new Vector2(0, 0);
        pBarRect.sizeDelta = new Vector2(300, 5);

        powerUpBar = pBarObj.AddComponent<Image>();
        powerUpBar.color = Color.white;

        powerUpPanel.SetActive(false); // Initially hidden
    }

    // ==================== END GAME POPUP ====================

    void BuildEndGamePopup()
    {
        // Create end game panel (initially hidden)
        endGamePanel = new GameObject("EndGamePanel");
        endGamePanel.transform.SetParent(hudCanvas.transform, false);

        RectTransform panelRect = endGamePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Semi-transparent dark overlay
        Image overlayImg = endGamePanel.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.75f);

        // ---- Center Card ----
        GameObject card = new GameObject("Card");
        card.transform.SetParent(endGamePanel.transform, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(500, 350);

        Image cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.08f, 0.05f, 0.15f, 0.95f);

        // Title text ("YOU WIN!" or "YOU LOSE!")
        GameObject titleObj = new GameObject("EndTitle");
        titleObj.transform.SetParent(card.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -60);
        titleRect.sizeDelta = new Vector2(450, 70);

        endGameTitleText = titleObj.AddComponent<TextMeshProUGUI>();
        endGameTitleText.text = "GAME OVER";
        endGameTitleText.fontSize = 52;
        endGameTitleText.color = Color.white;
        endGameTitleText.alignment = TextAlignmentOptions.Center;
        endGameTitleText.fontStyle = FontStyles.Bold;

        // Score text
        GameObject scoreObj = new GameObject("FinalScore");
        scoreObj.transform.SetParent(card.transform, false);

        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(0, 20);
        scoreRect.sizeDelta = new Vector2(400, 50);

        endGameScoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        endGameScoreText.text = "Score: 0";
        endGameScoreText.fontSize = 30;
        endGameScoreText.color = new Color(0.8f, 0.8f, 0.9f);
        endGameScoreText.alignment = TextAlignmentOptions.Center;

        // Decorative line
        GameObject line = new GameObject("Line");
        line.transform.SetParent(card.transform, false);
        RectTransform lineRect = line.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = new Vector2(0, -20);
        lineRect.sizeDelta = new Vector2(350, 2);
        Image lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(0.4f, 0.3f, 0.6f, 0.6f);

        // Restart Button
        CreatePopupButton(card.transform, "RestartButton", "RESTART",
            new Vector2(-100, -90), new Color(0.1f, 0.5f, 0.4f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // Main Menu Button
        CreatePopupButton(card.transform, "MenuButton", "MENU",
            new Vector2(100, -90), new Color(0.3f, 0.25f, 0.5f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        });

        // Hide initially
        endGamePanel.SetActive(false);
    }

    void ShowEndGamePopup(string title, Color titleColor)
    {
        if (endGamePanel == null) return;

        endGameTitleText.text = title;
        endGameTitleText.color = titleColor;
        endGameScoreText.text = $"Final Score: {currentScore} / {TotalPickups}";

        endGamePanel.SetActive(true);

        // Slow down time for dramatic effect
        Time.timeScale = 0.1f;

        // Create EventSystem if needed for button clicks
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Unfreeze time after a short delay so buttons work
        StartCoroutine(UnfreezeForUI());
    }

    System.Collections.IEnumerator UnfreezeForUI()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 0f; // Fully pause gameplay, UI still works with unscaledTime
    }

    void CreatePopupButton(Transform parent, string name, string label,
        Vector2 position, Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(170, 55);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;

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

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }
}
