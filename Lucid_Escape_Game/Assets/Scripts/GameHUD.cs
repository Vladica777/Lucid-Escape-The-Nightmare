using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Interfata jucatorului: crosshair, textul de interactiune, inventarul si foile citite.
///
/// Crosshair-ul, promptul, mesajele si inventarul sunt inca desenate cu IMGUI,
/// interfata de lucru de la inceput.
///
/// Biletul, in schimb, e panou adevarat: Canvas cu sprite de hartie si text
/// TextMeshPro, construit din cod la prima deschidere. Arata ca biletul din
/// camera 2, ca sa fie la fel peste tot in joc.
public class GameHUD : MonoBehaviour
{
    [Header("Referinte (se completeaza singure daca le lasi goale)")]
    public PlayerInteractor interactor;
    public Inventory inventory;

    [Header("Aspect")]
    public int crosshairSize = 6;
    public Color crosshairColor = new Color(1f, 1f, 1f, 0.75f);

    [Header("Biletul")]
    [Tooltip("Imaginea de hartie pe care se scrie textul. Gol = dreptunghi simplu.")]
    public Sprite fundalBilet;

    [Tooltip("Cat din ecran ocupa foaia. 0.75 inseamna trei sferturi.")]
    [Range(0.3f, 1f)]
    public float parteDinEcran = 0.75f;

    [Tooltip("Fontul textului. Gol = fontul implicit TextMeshPro.")]
    public TMP_FontAsset fontBilet;

    [Tooltip("Cat spatiu gol ramane pe margini, ca fractie din foaie.")]
    public Vector2 margineText = new Vector2(0.12f, 0.12f);

    [Tooltip("Marimea maxima a literei. Scade singura daca textul nu incape.")]
    public float marimeLitera = 40f;

    public Color culoareText = new Color(0.12f, 0.1f, 0.09f);

    static GameHUD instance;

    const string NL = "\n";

    // starea panourilor
    bool inventarDeschis;
    string notaTitlu, notaText;
    bool notaDeschisa;
    int frameDeschidere = -1;

    // mesaj temporar ("Ai luat: cheie")
    string mesaj;
    float mesajPanaLa;

    // panoul de bilet, construit la prima deschidere
    GameObject panouBilet;
    TextMeshProUGUI textBilet;
    RectTransform panouRect, cutieRect, foaieRect, textRect, hintRect;

    /// Cat timp e deschisa o foaie, jucatorul nu se misca.
    public static bool Blocking => instance != null && instance.notaDeschisa;

    void Awake()
    {
        instance = this;
        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
        if (inventory == null) inventory = GetComponent<Inventory>();
    }

    void OnDestroy() { if (instance == this) instance = null; }

    public static void Mesaj(string text, float durata = 2f)
    {
        if (instance == null) { Debug.Log(text); return; }
        instance.mesaj = text;
        instance.mesajPanaLa = Time.time + durata;
    }

    public static void DeschideNota(string titlu, string text)
    {
        if (instance == null) return;
        instance.notaTitlu = titlu;
        instance.notaText = text;
        instance.notaDeschisa = true;
        instance.DeschidePanoul(titlu, text);
        instance.frameDeschidere = Time.frameCount;
        PlayerController.BlocheazaCursorul(false);
    }

    void InchideNota()
    {
        notaDeschisa = false;
        if (panouBilet != null) panouBilet.SetActive(false);
        PlayerController.BlocheazaCursorul(true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (notaDeschisa)
        {
            // ignoram frame-ul in care s-a deschis, altfel acelasi E o inchide instant
            if (Time.frameCount != frameDeschidere &&
                (kb.eKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                InchideNota();
            return;
        }

        if (kb.tabKey.wasPressedThisFrame) inventarDeschis = !inventarDeschis;
    }

    void OnGUI()
    {
        if (notaDeschisa) return;   // biletul e panou pe Canvas, nu IMGUI

        DeseneazaCrosshair();
        DeseneazaPrompt();
        DeseneazaMesaj();
        if (inventarDeschis) DeseneazaInventar();
    }

    void DeseneazaCrosshair()
    {
        float s = crosshairSize;
        var r = new Rect((Screen.width - s) / 2f, (Screen.height - s) / 2f, s, s);
        var vechi = GUI.color;
        GUI.color = crosshairColor;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = vechi;
    }

    void DeseneazaPrompt()
    {
        var tinta = interactor != null ? interactor.Current : null;
        if (tinta == null) return;

        var st = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            // fontSize = 18,
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        st.normal.textColor = new Color(1f, 0.92f, 0.6f);
        // st.normal.textColor = Color.white;

        var r = new Rect(0, Screen.height * 0.62f, Screen.width, 30);
        GUI.Label(r, $"[E]  {tinta.GetPrompt()}", st);
    }

    void DeseneazaMesaj()
    {
        if (Time.time > mesajPanaLa || string.IsNullOrEmpty(mesaj)) return;

        var st = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16
        };
        st.normal.textColor = new Color(1f, 0.92f, 0.6f);
        GUI.Label(new Rect(0, Screen.height * 0.72f, Screen.width, 26), mesaj, st);
    }

    void DeseneazaInventar()
    {
        const float W = 260f;
        float h = 60f + (inventory != null ? inventory.Count : 0) * 24f;
        var r = new Rect(Screen.width - W - 16f, 16f, W, Mathf.Max(h, 90f));

        GUI.Box(r, GUIContent.none);
        GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));

        int n = inventory != null ? inventory.Count : 0;
        int cap = inventory != null ? inventory.capacity : 0;
        GUILayout.Label($"<b>INVENTAR</b>  {n}/{(cap > 0 ? cap.ToString() : "∞")}",
                        new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 });

        if (n == 0) GUILayout.Label("(gol)");
        else foreach (var it in inventory.Items) GUILayout.Label("• " + it.displayName);

        GUILayout.FlexibleSpace();
        GUILayout.Label("<i>Tab inchide</i>",
                        new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 });
        GUILayout.EndArea();
    }

    // ---------------------------------------------------------- biletul

    /// Construieste panoul la prima deschidere si il reface la fiecare
    /// citire cu textul nou. Canvas propriu, ca sa nu depinda de nimic din
    /// scena si sa mearga la fel in orice camera.
    void DeschidePanoul(string titlu, string text)
    {
        if (panouBilet == null) FaPanoul();

        // reasezam la fiecare deschidere: rezolutia se poate schimba intre timp
        Aseaza();

        string continut = string.IsNullOrWhiteSpace(titlu)
            ? text
            : "<b>" + titlu + "</b>" + NL + NL + text;

        if (textBilet != null) textBilet.text = continut;

        panouBilet.SetActive(true);

        // TextMeshPro isi calculeaza incadrarea la activare, iar ancorele
        // hartiei nu sunt inca aplicate in cadrul asta. Fara reconstructia
        // fortata, textul se aseaza pe dreptunghiul vechi si iese din foaie.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panouRect);

        Potriveste(continut);

        if (textBilet != null) textBilet.ForceMeshUpdate();
    }

    void FaPanoul()
    {
        var canvasGo = new GameObject("Panou bilet");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        panouRect = canvasGo.GetComponent<RectTransform>();

        // fundal intunecat, ca sa nu se piarda hartia peste decor
        var umbra = new GameObject("Umbra");
        umbra.transform.SetParent(canvasGo.transform, false);
        Intinde(umbra.AddComponent<RectTransform>(), 0f);
        var umbraImg = umbra.AddComponent<Image>();
        umbraImg.color = new Color(0f, 0f, 0f, 0.75f);
        umbraImg.raycastTarget = false;

        // cutia in care incape foaia: o fractie din ecran, prin ancore.
        // Ancorele sunt procente din parinte, deci nu trebuie masurat nimic
        // si nu conteaza rezolutia sau cadrul in care se deschide panoul.
        var cutie = new GameObject("Cutie");
        cutie.transform.SetParent(canvasGo.transform, false);
        cutieRect = cutie.AddComponent<RectTransform>();

        var foaie = new GameObject("Foaie");
        foaie.transform.SetParent(cutie.transform, false);
        foaieRect = foaie.AddComponent<RectTransform>();
        Intinde(foaieRect, 0f);

        var foaieImg = foaie.AddComponent<Image>();
        foaieImg.raycastTarget = false;

        // plasa de siguranta: orice iese din hartie nu se mai deseneaza
        foaie.AddComponent<RectMask2D>();

        if (fundalBilet != null)
        {
            foaieImg.sprite = fundalBilet;
            foaieImg.color = Color.white;

            // pastreaza proportiile hartiei in interiorul cutiei
            if (fundalBilet.rect.height > 0f)
            {
                var potrivire = foaie.AddComponent<AspectRatioFitter>();
                potrivire.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                potrivire.aspectRatio = fundalBilet.rect.width / fundalBilet.rect.height;
            }
        }
        else
        {
            foaieImg.color = new Color(0.88f, 0.85f, 0.76f, 0.97f);
        }

        // textul, intins peste hartie cu margini procentuale
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(foaie.transform, false);
        textRect = textGo.AddComponent<RectTransform>();

        textBilet = textGo.AddComponent<TextMeshProUGUI>();
        if (fontBilet != null) textBilet.font = fontBilet;
        textBilet.color = culoareText;
        textBilet.alignment = TextAlignmentOptions.Center;
        textBilet.textWrappingMode = TextWrappingModes.Normal;
        textBilet.raycastTarget = false;

        // Auto-scalarea lui TMP nu lucreaza impreuna cu Truncate: taie in loc
        // sa micsoreze. Calculam noi marimea, cu masuratoarea lui TMP, si o
        // aplicam inainte de desenare. RectMask2D de pe hartie ramane plasa
        // de siguranta daca tot nu incape.
        textBilet.enableAutoSizing = false;
        textBilet.overflowMode = TextOverflowModes.Overflow;

        // randul de jos, prins de marginea de jos a ecranului
        var hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(canvasGo.transform, false);
        hintRect = hintGo.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.offsetMin = new Vector2(0f, 24f);
        hintRect.offsetMax = new Vector2(0f, 64f);

        var hint = hintGo.AddComponent<TextMeshProUGUI>();
        hint.text = "[E] sau [Esc] pentru a inchide";
        hint.fontSize = 22f;
        hint.color = new Color(1f, 1f, 1f, 0.7f);
        hint.alignment = TextAlignmentOptions.Center;
        hint.raycastTarget = false;

        panouBilet = canvasGo;
        panouBilet.SetActive(false);
    }

    /// Ancore procentuale, aplicate la fiecare deschidere ca sa se vada
    /// imediat daca schimbi valorile din Inspector in timpul jocului.
    void Aseaza()
    {
        if (cutieRect == null) return;

        float parte = Mathf.Clamp(parteDinEcran, 0.3f, 1f);
        float margine = (1f - parte) / 2f;

        cutieRect.anchorMin = new Vector2(margine, margine);
        cutieRect.anchorMax = new Vector2(1f - margine, 1f - margine);
        cutieRect.offsetMin = Vector2.zero;
        cutieRect.offsetMax = Vector2.zero;

        if (textRect != null)
        {
            textRect.anchorMin = new Vector2(Mathf.Clamp01(margineText.x),
                                             Mathf.Clamp01(margineText.y));
            textRect.anchorMax = new Vector2(1f - Mathf.Clamp01(margineText.x),
                                             1f - Mathf.Clamp01(margineText.y));
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        if (textBilet != null)
        {
            textBilet.fontSizeMax = marimeLitera;
            textBilet.color = culoareText;
        }
    }

    /// Alege cea mai mare litera cu care tot textul incape in zona de scris.
    ///
    /// Intreaba TMP cat loc i-ar trebui la marimea maxima, apoi micsoreaza
    /// proportional. Doua treceri, fiindca la litera mai mica randurile se
    /// rup altfel si de obicei ocupa ceva mai putin decat estimarea liniara.
    void Potriveste(string continut)
    {
        if (textBilet == null || textRect == null) return;

        float latime = textRect.rect.width;
        float inaltime = textRect.rect.height;

        if (latime < 1f || inaltime < 1f) return;

        float litera = Mathf.Max(8f, marimeLitera);

        for (int i = 0; i < 2; i++)
        {
            textBilet.fontSize = litera;
            Vector2 nevoie = textBilet.GetPreferredValues(continut, latime, 0f);

            if (nevoie.y <= inaltime) break;

            litera = Mathf.Max(8f, litera * (inaltime / nevoie.y) * 0.98f);
        }

        textBilet.fontSize = litera;
    }

    /// Intinde un dreptunghi peste tot parintele, cu o margine in pixeli.
    static void Intinde(RectTransform r, float margine)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(margine, margine);
        r.offsetMax = new Vector2(-margine, -margine);
    }
}
