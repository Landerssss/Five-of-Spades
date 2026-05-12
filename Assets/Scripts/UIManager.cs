using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Auto-creates all UI at runtime. Attach to an empty GameObject.
/// </summary>
public class UIManager : MonoBehaviour
{
    private TextMeshProUGUI movesText;
    private List<TextMeshProUGUI> keySlots = new List<TextMeshProUGUI>();
    private GameObject gameOverPanel;
    private GameObject victoryPanel;
    private Canvas canvas;

    // Colors
    private readonly Color bgColor = new Color(0.1f, 0.1f, 0.14f, 0.85f);
    private readonly Color accentColor = new Color(0.96f, 0.65f, 0.14f);
    private readonly Color lockColor = new Color(0.4f, 0.4f, 0.45f);
    private readonly Color keyColor = new Color(0.96f, 0.65f, 0.14f);
    private readonly Color deathColor = new Color(0.82f, 0.12f, 0.12f);
    private readonly Color winColor = new Color(0.49f, 0.83f, 0.13f);

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // ---- Canvas ----
        GameObject canvasObj = new GameObject("GameCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();

        // ---- Moves Display (Top-Left) ----
        movesText = CreateText(canvasRT, "MovesText", "Actions: 5",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(300, 60), 32, TextAlignmentOptions.Left);

        // ---- Key Slots (Right side) ----
        // Will be rebuilt in UpdateKeys

        // ---- Game Over Panel ----
        gameOverPanel = CreatePanel(canvasRT, "GameOverPanel", deathColor);
        CreateText(gameOverPanel.GetComponent<RectTransform>(), "GameOverTitle",
            "GAME OVER", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 40), new Vector2(500, 80), 48, TextAlignmentOptions.Center);
        CreateText(gameOverPanel.GetComponent<RectTransform>(), "GameOverSub",
            "Action points exhausted", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -10), new Vector2(500, 40), 24, TextAlignmentOptions.Center);
        CreateButton(gameOverPanel.GetComponent<RectTransform>(), "RestartBtn",
            "RESTART LEVEL", new Vector2(0, -70), new Vector2(280, 55),
            () => { GameManager.Instance.RestartCurrentLevel(); });
        gameOverPanel.SetActive(false);

        // ---- Victory Panel ----
        victoryPanel = CreatePanel(canvasRT, "VictoryPanel", winColor);
        CreateText(victoryPanel.GetComponent<RectTransform>(), "VictoryTitle",
            "VICTORY!", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 40), new Vector2(500, 80), 48, TextAlignmentOptions.Center);
        CreateText(victoryPanel.GetComponent<RectTransform>(), "VictorySub",
            "All keys collected!", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -10), new Vector2(500, 40), 24, TextAlignmentOptions.Center);
        CreateButton(victoryPanel.GetComponent<RectTransform>(), "PlayAgainBtn",
            "PLAY AGAIN", new Vector2(0, -70), new Vector2(280, 55),
            () => { GameManager.Instance.RestartGame(); });
        victoryPanel.SetActive(false);
    }

    // ============ PUBLIC API ============

    public void UpdateMoves(int moves)
    {
        if (movesText != null)
        {
            string diamonds = "";
            for (int i = 0; i < moves; i++) diamonds += "<color=#F5A623>\u2666</color> ";
            movesText.text = "Actions: " + diamonds + $"({moves})";
        }
    }

    public void UpdateKeys(int collected, int total)
    {
        // Rebuild key slots
        foreach (var slot in keySlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        keySlots.Clear();

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        for (int i = 0; i < total; i++)
        {
            bool unlocked = i < collected;
            float yOff = -20f - (i * 55f);

            TextMeshProUGUI slotText = CreateText(canvasRT, $"KeySlot_{i}",
                unlocked ? "\u2605" : "\u25A0",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-30, yOff), new Vector2(50, 50),
                36, TextAlignmentOptions.Center);

            slotText.color = unlocked ? keyColor : lockColor;
            keySlots.Add(slotText);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // ============ UI BUILDER HELPERS ============

    private TextMeshProUGUI CreateText(RectTransform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;

        return tmp;
    }

    private GameObject CreatePanel(RectTransform parent, string name, Color tintColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(550, 250);

        Image img = panel.AddComponent<Image>();
        img.color = new Color(tintColor.r * 0.3f, tintColor.g * 0.3f, tintColor.b * 0.3f, 0.92f);

        return panel;
    }

    private void CreateButton(RectTransform parent, string name, string label,
        Vector2 anchoredPos, Vector2 sizeDelta, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.3f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.4f, 0.4f, 0.5f);
        cb.pressedColor = new Color(0.2f, 0.2f, 0.25f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}
