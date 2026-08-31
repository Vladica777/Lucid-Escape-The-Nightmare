using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Finalul jocului, in hol: dupa ce te intorci din ultima camera, personajul
/// iese in fata ta, iar dupa cateva secunde apare ecranul final.
///
/// Ambele obiecte exista deja in scena, debifate: personajul are Animator-ul
/// legat de AnimatorJumpscare, iar Canvas-ul are panoul negru cu textul.
/// Lipsea doar ce le aprinde - scriptul asta.
///
/// Declansarea nu se uita la unde a aterizat jucatorul, ci la progres:
/// "camera6 e terminata si suntem in hol" e adevarat exact o data si nu
/// depinde de ordinea in care pornesc componentele. Cu punctele de spawn ar
/// fi fost o cursa: PunctSpawn consuma cererea la Start, iar daca el apuca
/// primul, aici nu mai ramane nimic de citit.
public class FinalDupaUltimaCamera : MonoBehaviour
{
    [Header("Ce se aprinde - gol = le caut dupa nume")]
    [Tooltip("Personajul care iese in fata.")]
    public GameObject personaj;

    [Tooltip("Canvas-ul cu ecranul final.")]
    public GameObject ecranFinal;

    public string numePersonaj = "Ch30_nonPBR@Zombie Attack";
    public string numeEcran = "Canvas";

    [Header("Cand")]
    [Tooltip("Camera dupa care se declanseaza finalul.")]
    public string idCamera = "camera6";

    [Tooltip("Secunde de la intrarea in hol pana iese personajul.")]
    public float intarziere = 1f;

    [Tooltip("Cat sta personajul pe ecran inainte sa apara panoul.")]
    public float pauzaPanaLaPanou = 2.5f;

    [Header("Corectii la rulare")]
    [Tooltip("Il intoarce cu fata spre jucator. In scena priveste spre +Z, " +
             "adica exact invers fata de cum vii tu, deci i-ai vedea ceafa.")]
    public bool seIntoarceSpreJucator = true;

    [Header("Jucator - gol = il caut singur")]
    public MainHallFirstPersonController miscare;
    public PlayerInteraction interactiune;

    [Header("Sunet, optional")]
    public AudioSource sunet;

    /// Finalul se joaca o singura data pe rulare. Steagul se reseteaza la
    /// pornirea jocului, la fel ca starea din Progres.
    static bool jucat;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void LaPornire() => jucat = false;

    IEnumerator Start()
    {
        if (jucat) yield break;
        if (!Progres.ETerminata(idCamera)) yield break;

        if (personaj == null) personaj = Gaseste(numePersonaj);
        if (ecranFinal == null) ecranFinal = Gaseste(numeEcran);

        if (personaj == null || ecranFinal == null)
        {
            Debug.LogWarning($"Finalul nu poate porni: personaj={personaj}, " +
                             $"ecran={ecranFinal}. Verifica numele in Inspector.", this);
            yield break;
        }

        if (miscare == null) miscare = FindFirstObjectByType<MainHallFirstPersonController>();
        if (interactiune == null) interactiune = FindFirstObjectByType<PlayerInteraction>();

        jucat = true;

        yield return new WaitForSeconds(intarziere);

        Sperie();

        yield return new WaitForSeconds(pauzaPanaLaPanou);

        AratatEcranul();
    }

    void Sperie()
    {
        if (seIntoarceSpreJucator)
        {
            var camera = Camera.main;

            if (camera != null)
            {
                // doar pe orizontala: altfel s-ar apleca dupa privirea jucatorului
                Vector3 spre = camera.transform.position - personaj.transform.position;
                spre.y = 0f;

                if (spre.sqrMagnitude > 0.01f)
                    personaj.transform.rotation = Quaternion.LookRotation(spre);
            }
        }

        personaj.SetActive(true);

        if (sunet != null) sunet.Play();
    }

    void AratatEcranul()
    {
        ecranFinal.SetActive(true);

        if (miscare != null) miscare.SetMovementEnabled(false);
        if (interactiune != null) interactiune.SetInteractionsEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// GameObject.Find nu vede obiectele dezactivate, iar astea doua sunt
    /// dezactivate tocmai pentru ca le aprindem noi.
    static GameObject Gaseste(string nume)
    {
        if (string.IsNullOrWhiteSpace(nume)) return null;

        foreach (var radacina in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (radacina.name == nume) return radacina;

            foreach (var t in radacina.GetComponentsInChildren<Transform>(true))
                if (t.name == nume) return t.gameObject;
        }
        return null;
    }
}
