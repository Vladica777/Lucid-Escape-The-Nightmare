using UnityEngine;

/// Spune pe ecran, in timp real, ce loveste crosshair-ul si de ce nu apare
/// promptul de interactiune.
///
/// De ce exista: cand promptul nu apare, cauzele arata toate la fel din joc -
/// esti prea departe, tintesti pe langa colider, obiectul n-are Interactable,
/// sau il are dar CanInteract e fals. Din exterior nu le poti deosebi, si
/// fiecare incercare de a ghici costa o repornire de Play.
///
/// Se pune pe jucator, langa PlayerInteractor. Se sterge cand nu mai trebuie.
///
/// Trage o raza mai lunga decat cea a jucatorului, ca sa poata spune "l-ai
/// gasit, dar e la 4.2 m si tu ajungi doar la 3".
[RequireComponent(typeof(PlayerInteractor))]
public class DiagnosticRaza : MonoBehaviour
{
    [Tooltip("Cat de departe se uita diagnosticul. Mai mult decat raza " +
             "jucatorului, ca sa vada si ce e prea departe.")]
    public float razaLunga = 25f;

    PlayerInteractor interactor;

    readonly RaycastHit[] lovituri = new RaycastHit[16];

    // ce s-a aflat in cadrul asta, pregatit pentru OnGUI
    string tinta, distanta, componenta, verdict;
    Color culoareVerdict;

    void Awake() => interactor = GetComponent<PlayerInteractor>();

    void Update()
    {
        var cam = interactor.cam;

        if (cam == null)
        {
            tinta = distanta = componenta = "-";
            verdict = "PlayerInteractor n-are camera. Nu se trage nicio raza.";
            culoareVerdict = Color.red;
            return;
        }

        var ray = new Ray(cam.transform.position, cam.transform.forward);

        // aceeasi regula ca la PlayerInteractor: sarim peste coliderele
        // jucatorului, altfel diagnosticul ar arata mereu propria capsula
        int n = Physics.RaycastNonAlloc(ray, lovituri, razaLunga,
                                        interactor.mask, QueryTriggerInteraction.Collide);

        var hit = new RaycastHit();
        float ceaMaiApropiata = float.MaxValue;
        bool gasit = false;

        for (int i = 0; i < n; i++)
        {
            if (interactor.EAlMeu(lovituri[i].collider)) continue;
            if (lovituri[i].distance >= ceaMaiApropiata) continue;

            ceaMaiApropiata = lovituri[i].distance;
            hit = lovituri[i];
            gasit = true;
        }

        if (!gasit)
        {
            tinta = distanta = componenta = "-";
            verdict = $"Raza nu loveste nimic pana la {razaLunga} m.";
            culoareVerdict = Color.yellow;
            return;
        }

        tinta = $"{hit.collider.name}  ({hit.collider.GetType().Name}, " +
                $"strat {LayerMask.LayerToName(hit.collider.gameObject.layer)})";
        distanta = $"{hit.distance:0.00} m   (jucatorul ajunge la {interactor.range} m)";

        var it = hit.collider.GetComponentInParent<Interactable>();

        componenta = it == null
            ? "niciun Interactable pe obiect sau pe parintii lui"
            : $"{it.GetType().Name}, CanInteract = {it.CanInteract}";

        // ordinea conteaza: prima conditie care cade e si cauza
        if (hit.distance > interactor.range)
        {
            verdict = "PREA DEPARTE. Apropie-te sau mareste range pe PlayerInteractor.";
            culoareVerdict = new Color(1f, 0.6f, 0.2f);
        }
        else if (it == null)
        {
            verdict = "Lovesti altceva decat obiectul cautat, sau obiectul " +
                      "n-are componenta de interactiune.";
            culoareVerdict = Color.red;
        }
        else if (!it.CanInteract)
        {
            verdict = "Il gasesti, dar el refuza: CanInteract e fals " +
                      "(deja luat, incuiat, dezactivat).";
            culoareVerdict = Color.red;
        }
        else
        {
            verdict = "TOTUL E BINE. Promptul ar trebui sa apara.";
            culoareVerdict = Color.green;
        }
    }

    void OnGUI()
    {
        var st = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        var fundal = new Rect(10, 10, 640, 108);

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(fundal, Texture2D.whiteTexture);
        GUI.color = Color.white;

        st.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 16, 620, 20), $"lovesc:    {tinta}", st);
        GUI.Label(new Rect(20, 38, 620, 20), $"distanta:  {distanta}", st);
        GUI.Label(new Rect(20, 60, 620, 20), $"componenta: {componenta}", st);

        st.normal.textColor = culoareVerdict;
        st.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(20, 86, 620, 20), verdict, st);
    }
}
