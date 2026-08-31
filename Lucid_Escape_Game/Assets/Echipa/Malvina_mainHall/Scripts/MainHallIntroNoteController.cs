using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MainHallIntroNoteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainHallFirstPersonController movement;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Sprite notePaper;
    [SerializeField] private TMP_FontAsset storyFont;

    [Header("Timing")]
    [SerializeField] private float openingDelay = 3f;

    private const string StoryTitle = "\u00ceNC\u0102 VISEZI";

    private const string StoryBody =
        "Te-ai trezit\u2026 sau cel pu\u021bin a\u0219a crezi.\n\n" +
        "Coridoarele din jurul t\u0103u nu sunt reale. E\u0219ti prins \u00een propriul t\u0103u co\u0219mar, iar fiecare camer\u0103 ascunde o parte din drumul spre trezire.\n\n" +
        "Cinci u\u0219i stau \u00eentre tine \u0219i libertate.\n\n" +
        "\u00cen spatele fiec\u0103reia te a\u0219teapt\u0103 o alt\u0103 \u00eencercare. Trebuie s\u0103 intri \u00een fiecare camer\u0103, s\u0103 termini fiecare misiune \u0219i s\u0103 supravie\u021buie\u0219ti lucrurilor pe care visul le-a creat pentru tine.\n\n" +
        "Doar dup\u0103 ce toate misiunile sunt \u00eendeplinite, co\u0219marul te va l\u0103sa s\u0103 pleci.\n\n" +
        "P\u00e2n\u0103 atunci\u2026\n\n" +
        "nu e\u0219ti treaz.";

    private GameObject noteRoot;

    // Steagul e static, nu de instanta: holul se reincarca de fiecare data
    // cand te intorci dintr-o camera, deci un camp de instanta ar porni fals
    // la fiecare intrare si nota ar aparea din nou. Asa apare o singura data
    // pe rulare, la primul spawn.
    private static bool wasShown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        wasShown = false;
    }

    private IEnumerator Start()
    {
        if (wasShown)
        {
            yield break;
        }

        if (movement == null)
        {
            movement = FindFirstObjectByType<MainHallFirstPersonController>();
        }

        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        BuildNoteUI();
        noteRoot.SetActive(false);

        yield return new WaitForSeconds(openingDelay);

        if (!wasShown)
        {
            ShowNote();
        }
    }

    private void BuildNoteUI()
    {
        Canvas canvas = FindMainHallCanvas();
        EnsureEventSystem();

        noteRoot = new GameObject("IntroStoryNote");
        noteRoot.transform.SetParent(canvas.transform, false);

        RectTransform noteRect = noteRoot.AddComponent<RectTransform>();
        noteRect.anchorMin = new Vector2(0.5f, 0.5f);
        noteRect.anchorMax = new Vector2(0.5f, 0.5f);
        noteRect.pivot = new Vector2(0.5f, 0.5f);
        noteRect.anchoredPosition = Vector2.zero;
        noteRect.sizeDelta = new Vector2(1180f, 820f);

        if (notePaper != null && notePaper.rect.height > 0f)
        {
            float paperAspect = notePaper.rect.width / notePaper.rect.height;
            float maxWidth = 1180f;
            float maxHeight = 820f;
            float width = Mathf.Min(maxWidth, maxHeight * paperAspect);
            float height = width / paperAspect;
            noteRect.sizeDelta = new Vector2(width, height);
        }

        CanvasGroup canvasGroup = noteRoot.AddComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        GameObject backgroundObject = new GameObject("NoteBackground");
        backgroundObject.transform.SetParent(noteRoot.transform, false);

        RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.AddComponent<Image>();
        background.sprite = notePaper;
        background.color = notePaper != null ? Color.white : new Color(0.86f, 0.8f, 0.66f, 0.98f);
        background.preserveAspect = true;
        background.raycastTarget = true;

        RectMask2D mask = backgroundObject.AddComponent<RectMask2D>();
        mask.padding = new Vector4(-54f, -72f, -54f, -72f);

        TextMeshProUGUI title = CreateStoryText(backgroundObject.transform, "StoryTitle");
        title.text = StoryTitle;
        title.fontSize = 58f;
        title.fontStyle = FontStyles.Bold;
        title.lineSpacing = -12f;

        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.12f, 0.75f);
        titleRect.anchorMax = new Vector2(0.88f, 0.88f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI body = CreateStoryText(backgroundObject.transform, "StoryText");
        body.text = StoryBody;
        body.fontSize = 31f;
        body.fontStyle = FontStyles.Bold;
        body.lineSpacing = -13f;
        body.paragraphSpacing = -8f;

        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0.1f, 0.13f);
        bodyRect.anchorMax = new Vector2(0.9f, 0.75f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        GameObject closeObject = new GameObject("CloseButton");
        closeObject.transform.SetParent(noteRoot.transform, false);

        RectTransform closeRect = closeObject.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-86f, -96f);
        closeRect.sizeDelta = new Vector2(58f, 58f);

        Image closeImage = closeObject.AddComponent<Image>();
        closeImage.color = new Color(0.13f, 0.08f, 0.05f, 0.9f);

        Button closeButton = closeObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(CloseNote);

        GameObject closeLabelObject = new GameObject("X");
        closeLabelObject.transform.SetParent(closeObject.transform, false);

        RectTransform closeLabelRect = closeLabelObject.AddComponent<RectTransform>();
        closeLabelRect.anchorMin = Vector2.zero;
        closeLabelRect.anchorMax = Vector2.one;
        closeLabelRect.offsetMin = Vector2.zero;
        closeLabelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI closeLabel = closeLabelObject.AddComponent<TextMeshProUGUI>();
        closeLabel.text = "X";
        closeLabel.font = storyFont;
        closeLabel.fontSize = 42f;
        closeLabel.color = new Color(0.96f, 0.9f, 0.78f, 1f);
        closeLabel.alignment = TextAlignmentOptions.Center;
        closeLabel.raycastTarget = false;
    }

    private TextMeshProUGUI CreateStoryText(Transform parent, string objectName)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = storyFont;
        text.color = new Color(0.055f, 0.032f, 0.018f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        StrengthenTextMaterial(text);
        return text;
    }

    private static void StrengthenTextMaterial(TMP_Text text)
    {
        if (text.fontSharedMaterial == null)
        {
            return;
        }

        Material material = Instantiate(text.fontSharedMaterial);
        material.name = text.fontSharedMaterial.name + " Intro Readable";

        if (material.HasProperty(ShaderUtilities.ID_FaceDilate))
        {
            material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.12f);
        }

        if (material.HasProperty(ShaderUtilities.ID_OutlineWidth))
        {
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.035f);
        }

        if (material.HasProperty(ShaderUtilities.ID_OutlineColor))
        {
            material.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.055f, 0.032f, 0.018f, 0.55f));
        }

        text.fontSharedMaterial = material;
    }

    private Canvas FindMainHallCanvas()
    {
        GameObject existingCanvasObject = GameObject.Find("MainHall UI");
        if (existingCanvasObject != null && existingCanvasObject.TryGetComponent(out Canvas existingCanvas))
        {
            return existingCanvas;
        }

        GameObject canvasObject = new GameObject("MainHall UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private void ShowNote()
    {
        wasShown = true;
        noteRoot.SetActive(true);

        if (movement != null)
        {
            movement.SetMovementEnabled(false);
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionsEnabled(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseNote()
    {
        noteRoot.SetActive(false);

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionsEnabled(true);
        }

        if (movement != null)
        {
            movement.SetMovementEnabled(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
