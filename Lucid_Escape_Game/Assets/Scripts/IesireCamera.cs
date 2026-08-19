using System.Collections;
using UnityEngine;

/// Iesirea dintr-o camera: marcheaza camera terminata si duce jucatorul in
/// scena urmatoare, la punctul de spawn cerut.
///
/// Doua feluri de a se declansa:
///
///   LaTrecere        - obiectul are un collider bifat trigger si jucatorul
///                      trece prin el. Pentru praguri si usi la nivelul solului.
///
///   LaDeschidere     - se leaga de un SwingDoor si pleaca atunci cand acela
///                      se deschide. Pentru trape in tavan sau orice iesire
///                      prin care jucatorul nu poate trece fizic.
///
/// Exemplu, trapa din camera 6:
///   Cand      = LaDeschidere
///   Usa       = SwingDoor-ul de pe Capac-trapa
///   Id Camera = "camera6"
///   Spre Scena= "MainHall"
///   Id Spawn  = "dupa_camera6"
public class IesireCamera : MonoBehaviour
{
    public enum Declansare
    {
        LaTrecere,      // jucatorul intra in trigger
        LaDeschidere    // se deschide usa/trapa legata mai jos
    }

    [Header("Cand pleaca jucatorul")]
    public Declansare cand = Declansare.LaTrecere;

    [Tooltip("Doar pentru LaDeschidere: usa sau trapa care declanseaza " +
             "iesirea. Gol = o caut pe acelasi obiect.")]
    public SwingDoor usa;

    [Tooltip("Cate secunde se asteapta dupa declansare, ca sa se vada " +
             "trapa deschizandu-se inainte de schimbarea scenei.")]
    public float intarziere = 1.5f;

    [Header("Ce camera se termina aici")]
    [Tooltip("Id-ul camerei, asa cum il stie Progres: camera2 ... camera6.")]
    public string idCamera = "camera6";

    [Tooltip("Daca e debifat, jucatorul pleaca fara ca aceasta camera sa fie " +
             "marcata terminata. Pentru iesiri care doar te scot afara.")]
    public bool marcheazaTerminat = true;

    [Header("Unde se ajunge")]
    [Tooltip("Numele scenei, exact ca fisierul, fara .unity.")]
    public string spreScena = "MainHall";

    [Tooltip("Id-ul unui PunctSpawn din scena de destinatie.")]
    public string idSpawn = "dupa_camera6";

    [Header("Conditie optionala")]
    [Tooltip("Daca e completat, iesirea nu functioneaza pana cand jucatorul " +
             "nu are obiectul asta in inventar. Lasa gol daca nu vrei conditie.")]
    public string cereObiectulCuId = "";

    [Tooltip("Ce se scrie in consola cand iesirea e blocata.")]
    public string mesajBlocat = "Nu poti pleca inca.";

    bool plecat;

    void Reset()
    {
        // ca sa nu uite nimeni sa bifeze
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnEnable()
    {
        if (cand != Declansare.LaDeschidere) return;

        if (usa == null) usa = GetComponent<SwingDoor>();

        if (usa == null)
        {
            Debug.LogError($"IesireCamera pe '{name}': modul e LaDeschidere " +
                           "dar nu i-am gasit nicio usa. Completeaza campul Usa.", this);
            return;
        }

        usa.Deschisa += LaUsaDeschisa;
    }

    void OnDisable()
    {
        if (usa != null) usa.Deschisa -= LaUsaDeschisa;
    }

    void Awake()
    {
        if (cand != Declansare.LaTrecere) return;

        var col = GetComponent<Collider>();

        if (col == null)
            Debug.LogWarning($"IesireCamera pe '{name}': modul e LaTrecere dar " +
                             "obiectul n-are collider, deci nu se declanseaza.", this);
        else if (!col.isTrigger)
            Debug.LogWarning($"IesireCamera pe '{name}': colliderul nu e bifat " +
                             "Is Trigger, deci nu se declanseaza.", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (cand != Declansare.LaTrecere || plecat) return;

        var jucator = Jucatorul(other);
        if (jucator == null) return;

        if (!AreVoie(jucator))
        {
            Debug.Log($"IesireCamera: {mesajBlocat}");
            return;
        }

        Pleaca(0f);
    }

    void LaUsaDeschisa(SwingDoor _)
    {
        if (plecat) return;
        Pleaca(intarziere);
    }

    /// Se poate chema si din afara, daca vrei sa scoti jucatorul din alt motiv.
    public void Pleaca(float dupaSecunde)
    {
        if (plecat) return;
        plecat = true;

        if (dupaSecunde > 0f) StartCoroutine(PleacaPeste(dupaSecunde));
        else Schimba();
    }

    IEnumerator PleacaPeste(float secunde)
    {
        yield return new WaitForSeconds(secunde);
        Schimba();
    }

    void Schimba()
    {
        if (marcheazaTerminat) Progres.Termina(idCamera);

        if (!Tranzitie.Incarca(spreScena, idSpawn))
            plecat = false;   // n-a mers incarcarea, lasam iesirea activa
    }

    /// Colliderul care a intrat apartine jucatorului? Cautam in sus, fiindca
    /// modelele au de obicei colliderul pe un copil.
    static Transform Jucatorul(Collider other)
    {
        var cc = other.GetComponentInParent<CharacterController>();
        if (cc != null) return cc.transform;

        return other.CompareTag("Player") ? other.transform : null;
    }

    bool AreVoie(Transform jucator)
    {
        if (string.IsNullOrWhiteSpace(cereObiectulCuId)) return true;

        var inv = jucator.GetComponentInChildren<Inventory>();
        return inv != null && inv.Has(cereObiectulCuId);
    }

    void OnDrawGizmos()
    {
        if (cand != Declansare.LaTrecere) return;

        var col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.75f, 0.3f, 0.35f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
