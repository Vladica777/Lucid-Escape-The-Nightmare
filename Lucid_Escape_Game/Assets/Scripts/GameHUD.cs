using UnityEngine;
using UnityEngine.InputSystem;

/// Interfata jucatorului: crosshair, textul de interactiune, inventarul si foile citite.
///
/// NOTA: e desenata cu IMGUI (OnGUI) intentionat, ca sa functioneze fara Canvas,
/// fara prefaburi si fara TextMeshPro - care nu e importat in proiect.
/// E interfata de lucru, pentru testat mecanicile. Cand ajungeti la interfata
/// finala a jocului, se inlocuieste doar fisierul asta; restul sistemelor
/// comunica prin evenimente si nu se schimba.
public class GameHUD : MonoBehaviour
{
    [Header("Referinte (se completeaza singure daca le lasi goale)")]
    public PlayerInteractor interactor;
    public Inventory inventory;

    [Header("Aspect")]
    public int crosshairSize = 6;
    public Color crosshairColor = new Color(1f, 1f, 1f, 0.75f);

    static GameHUD instance;

    // starea panourilor
    bool inventarDeschis;
    string notaTitlu, notaText;
    bool notaDeschisa;
    int frameDeschidere = -1;

    // mesaj temporar ("Ai luat: cheie")
    string mesaj;
    float mesajPanaLa;

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
        instance.frameDeschidere = Time.frameCount;
        PlayerController.BlocheazaCursorul(false);
    }

    void InchideNota()
    {
        notaDeschisa = false;
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
        if (notaDeschisa) { DeseneazaNota(); return; }

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
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        st.normal.textColor = Color.white;

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

    void DeseneazaNota()
    {
        // fundal intunecat peste tot ecranul
        var vechi = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.82f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = vechi;

        float w = Mathf.Min(620f, Screen.width * 0.8f);
        float h = Mathf.Min(420f, Screen.height * 0.75f);
        var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

        GUI.Box(r, GUIContent.none);
        GUILayout.BeginArea(new Rect(r.x + 26f, r.y + 22f, r.width - 52f, r.height - 44f));

        GUILayout.Label(notaTitlu, new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        });
        GUILayout.Space(14);

        GUILayout.Label(notaText, new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            wordWrap = true
        });

        GUILayout.FlexibleSpace();
        GUILayout.Label("[E] sau [Esc] pentru a inchide", new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        });
        GUILayout.EndArea();
    }
}
