using UnityEngine;

/// Usa sau usita de dulap care se roteste in jurul unei balamale.
/// Merge si pe canaturi al caror pivot e la mijloc (calculeaza singura balamaua),
/// si pe modele care au deja un grup pivotat corect (dulapurile metalice).
public class SwingDoor : Interactable
{
    public enum Hinge
    {
        PivotPropriu,   // obiectul are deja pivotul pe balama (ex: DoorLeftGrp)
        MargineStanga,  // balamaua pe muchia din stanga canatului
        MargineDreapta,
        MargineFata,    // pentru trape: muchia dinspre +Z a obiectului
        MargineSpate    // ... dinspre -Z
    }

    [Header("Rotatie")]
    [Tooltip("Ce se roteste. Gol = obiectul asta.")]
    public Transform target;

    public Hinge hinge = Hinge.PivotPropriu;

    [Tooltip("Axa in jurul careia se roteste. Sus pentru usi obisnuite, " +
             "orizontala pentru trape care se rabat.")]
    public Vector3 hingeAxis = Vector3.up;

    [Tooltip("Unghiul de deschidere. Negativ = in sens invers.")]
    public float openAngle = 95f;

    [Tooltip("Grade pe secunda.")]
    public float speed = 220f;

    public bool startOpen;

    [Header("Incuiere")]
    public bool locked;

    [Tooltip("Id-ul obiectului din inventar care descuie. Gol = nu poate fi descuiata.")]
    public string requiredItemId = "";

    [TextArea(1, 3)]
    public string lockedMessage = "E incuiata.";

    [Header("Texte")]
    public string promptOpen = "Deschide";
    public string promptClose = "Inchide";

    bool open;

    /// E deschisa acum? Util pentru puzzle-uri si pentru IesireCamera.
    public bool EDeschisa => open;

    /// Se declanseaza in momentul in care usa incepe sa se deschida.
    /// IesireCamera se leaga aici ca sa scoata jucatorul din camera cand
    /// deschide trapa - nu poti trece printr-o trapa de la 3 metri.
    public event System.Action<SwingDoor> Deschisa;
    float unghi, unghiTinta;

    Vector3 startPos;
    Quaternion startRot;
    Vector3 hingeWorld;

    void Awake()
    {
        if (target == null) target = transform;

        startPos = target.position;
        startRot = target.rotation;
        hingeWorld = CalculeazaBalamaua();

        open = startOpen;
        unghi = unghiTinta = open ? openAngle : 0f;
        Aplica();
    }

    Vector3 CalculeazaBalamaua()
    {
        if (hinge == Hinge.PivotPropriu) return startPos;

        var rs = target.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return startPos;

        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);

        // directia in care se afla muchia cu balamaua, in spatiul obiectului
        Vector3 dir = hinge switch
        {
            Hinge.MargineStanga => -target.right,
            Hinge.MargineDreapta => target.right,
            Hinge.MargineFata => target.forward,
            _ => -target.forward
        };

        // jumatatea cutiei masurata pe directia aia
        float jumatate = Mathf.Abs(Vector3.Dot(b.extents,
            new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z))));

        return b.center + dir * jumatate;
    }

    public override string GetPrompt()
    {
        if (locked) return lockedMessage;
        return open ? promptClose : promptOpen;
    }

    public override void Interact(PlayerInteractor player)
    {
        if (locked)
        {
            var inv = player.inventory;
            if (!string.IsNullOrEmpty(requiredItemId) && inv != null && inv.Has(requiredItemId))
            {
                locked = false;
                GameHUD.Mesaj("Ai descuiat usa.");
                return;   // prima apasare descuie, a doua deschide
            }

            GameHUD.Mesaj(lockedMessage);
            return;
        }

        open = !open;
        unghiTinta = open ? openAngle : 0f;

        if (open) Deschisa?.Invoke(this);
    }

    void Update()
    {
        if (Mathf.Approximately(unghi, unghiTinta)) return;
        unghi = Mathf.MoveTowards(unghi, unghiTinta, speed * Time.deltaTime);
        Aplica();
    }

    void Aplica()
    {
        // componentele vechi din scena nu au campul salvat; cadem inapoi pe verticala
        Vector3 axa = hingeAxis.sqrMagnitude < 0.0001f ? Vector3.up : hingeAxis.normalized;

        Quaternion q = Quaternion.AngleAxis(unghi, axa);
        target.rotation = q * startRot;
        target.position = hingeWorld + q * (startPos - hingeWorld);
    }
}
