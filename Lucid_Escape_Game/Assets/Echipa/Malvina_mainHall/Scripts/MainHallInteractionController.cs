using UnityEngine;
using UnityEngine.UI;

public class MainHallInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainHallFirstPersonController movement;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private ScreenFadeController fadeController;

    private Text promptText;
    private Text messageText;
    private Image fadeOverlay;

    private void Awake()
    {
        if (movement == null)
        {
            movement = FindFirstObjectByType<MainHallFirstPersonController>();
        }

        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        CreateUI();

        if (fadeController == null)
        {
            fadeController = gameObject.AddComponent<ScreenFadeController>();
        }

        fadeController.Configure(fadeOverlay, messageText);
    }

    public void SetPromptVisible(bool visible, string label)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.text = string.IsNullOrWhiteSpace(label) ? "[E] Interact" : label;
        promptText.enabled = visible;
    }

    public void PlayDoorTransition(DoorInteraction door, PlayerInteraction interactor)
    {
        if (door == null || fadeController == null || fadeController.IsRunning)
        {
            return;
        }

        SetPromptVisible(false, string.Empty);
        fadeController.PlayDoorPreview(door.SelectedMessage, movement, interactor ?? playerInteraction);
    }

    private void CreateUI()
    {
        GameObject canvasObject = new GameObject("MainHall UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        promptText = CreateText(canvasObject.transform, "[E] Interact", 28, TextAnchor.LowerCenter, new Vector2(0f, 96f));
        promptText.enabled = false;

        messageText = CreateText(canvasObject.transform, "You chose Door 01", 44, TextAnchor.MiddleCenter, Vector2.zero);
        messageText.enabled = false;

        GameObject fadeObject = new GameObject("Fade Overlay");
        fadeObject.transform.SetParent(canvasObject.transform, false);

        RectTransform fadeRect = fadeObject.AddComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;

        fadeOverlay = fadeObject.AddComponent<Image>();
        fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
        fadeOverlay.raycastTarget = false;
    }

    private static Text CreateText(Transform parent, string label, int fontSize, TextAnchor anchor, Vector2 anchoredPosition)
    {
        GameObject textObject = new GameObject(label + " Text");
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, anchor == TextAnchor.LowerCenter ? 0f : 0.5f);
        rect.anchorMax = new Vector2(0.5f, anchor == TextAnchor.LowerCenter ? 0f : 0.5f);
        rect.pivot = new Vector2(0.5f, anchor == TextAnchor.LowerCenter ? 0f : 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(800f, 120f);

        Text text = textObject.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = new Color(0.92f, 0.92f, 0.88f, 1f);

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(1f, -1f);

        return text;
    }
}
