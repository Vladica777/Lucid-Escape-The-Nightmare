using UnityEngine;

/// Iesirea din camera 5, legata de usa finala.
///
/// Se pune pe acelasi obiect ca FinalDoor. Cand CodePanel cheama
/// FinalDoor.Unlock(), usa incepe sa se roteasca si de aici pleaca si
/// numaratoarea: dupa cateva secunde, cat sa se vada usa deschizandu-se,
/// jucatorul e dus in hol.
///
/// La fel ca trapa din camera 6, care porneste tranzitia in momentul in care
/// se deschide. Nu depinde de raycast, de collidere sau de unde stai in
/// camera - doar de starea usii.
///
/// Apasarea pe usa merge si ea, ca scurtatura, daca nu vrei sa astepti.
///
/// Nu stie unde se merge: scena, punctul de spawn si camera care se bifeaza
/// stau in IesireCamera, ca in toate celelalte camere.
[RequireComponent(typeof(FinalDoor))]
public class IesireUsaFinala : Interactable
{
    [Header("Legaturi (se completeaza singure daca le lasi goale)")]
    [Tooltip("Usa care se deschide la codul corect.")]
    public FinalDoor usa;

    [Tooltip("Cine stie in ce scena mergem. Gol = o caut in scena.")]
    public IesireCamera iesire;

    [Header("Cand pleaca jucatorul")]
    [Tooltip("Daca e bifat, pleaca singur dupa ce usa s-a deschis, fara sa " +
             "fie nevoie sa apesi pe ea.")]
    public bool plecaSingur = true;

    [Tooltip("Cate secunde trec intre descuierea usii si schimbarea scenei.")]
    public float intarziere = 2f;

    [Header("Texte")]
    public string mesajIncuiat = "E incuiata. Gaseste codul.";
    public string mesajIesire = "Iesi din camera";

    bool pornit;

    void Awake()
    {
        if (usa == null) usa = GetComponent<FinalDoor>();
        if (iesire == null) iesire = FindFirstObjectByType<IesireCamera>();
    }

    void Update()
    {
        if (pornit || !plecaSingur) return;
        if (usa == null || !usa.isOpen) return;

        Porneste(intarziere);
    }

    public override bool CanInteract => enabled && usa != null && usa.isOpen;

    public override string GetPrompt()
    {
        return usa != null && usa.isOpen ? mesajIesire : mesajIncuiat;
    }

    /// Apasarea sare peste asteptare.
    public override void Interact(PlayerInteractor player)
    {
        if (usa == null || !usa.isOpen)
        {
            GameHUD.Mesaj(mesajIncuiat);
            return;
        }

        Porneste(0.2f);
    }

    void Porneste(float dupaSecunde)
    {
        if (pornit) return;

        if (iesire == null)
        {
            Debug.LogWarning("IesireUsaFinala: n-am gasit niciun IesireCamera " +
                             "in scena, deci nu stiu unde sa te trimit.", this);
            return;
        }

        pornit = true;
        iesire.Pleaca(dupaSecunde);
    }
}
