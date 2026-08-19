using UnityEngine;

/// Iesire pe care o apesi, pentru camerele care n-au PlayerInteractor.
///
/// Se pune pe usa. Isi trage singura raza din camera jucatorului si isi
/// deseneaza singura promptul, deci merge in orice scena, indiferent ce
/// controller sau ce sistem de interactiune are.
///
/// Foloseste Input-ul vechi, ca sa se potriveasca cu camerele care inca sunt
/// pe el. Merge si cu E, si cu clic stanga.
///
/// Nu stie unde se merge: scena, punctul de spawn si camera care se bifeaza
/// stau in IesireCamera, ca peste tot.
public class IesireLaClick : MonoBehaviour
{
    [Header("Legaturi")]
    [Tooltip("Cine stie in ce scena mergem. Gol = o caut in scena.")]
    public IesireCamera iesire;

    [Tooltip("Camera din care pleaca raza. Gol = Camera.main.")]
    public Camera cameraJucatorului;

    [Header("Interactiune")]
    [Tooltip("De la ce distanta se poate apasa, in metri.")]
    public float raza = 4f;

    [Tooltip("Cate secunde trec intre apasare si schimbarea scenei.")]
    public float intarziere = 1f;

    [Header("Text")]
    public string mesaj = "[E] Iesi din camera";

    bool pornit;
    bool privit;

    void Awake()
    {
        if (iesire == null) iesire = FindFirstObjectByType<IesireCamera>();
    }

    void Update()
    {
        privit = false;

        if (pornit) return;

        var cam = cameraJucatorului != null ? cameraJucatorului : Camera.main;
        if (cam == null) return;

        var raza2 = new Ray(cam.transform.position, cam.transform.forward);

        if (!Physics.Raycast(raza2, out RaycastHit hit, raza, ~0,
                             QueryTriggerInteraction.Ignore))
            return;

        // colliderul poate fi pe un copil al usii
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;

        privit = true;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            Porneste();
    }

    void Porneste()
    {
        if (pornit) return;

        if (iesire == null)
        {
            Debug.LogWarning("IesireLaClick: n-am gasit niciun IesireCamera in " +
                             "scena, deci nu stiu unde sa te trimit.", this);
            return;
        }

        pornit = true;
        iesire.Pleaca(intarziere);
    }

    void OnGUI()
    {
        if (!privit || pornit) return;

        var stil = new GUIStyle
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter
        };
        stil.normal.textColor = Color.white;

        GUI.Label(new Rect(0f, Screen.height * 0.58f, Screen.width, 40f), mesaj, stil);
    }
}
