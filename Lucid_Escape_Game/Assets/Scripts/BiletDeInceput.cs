using System.Collections;
using UnityEngine;

/// Foaia care apare singura la cateva secunde dupa ce intri in camera, ca sa
/// spuna jucatorului ce are de facut.
///
/// Ideea e luata de la Karina, din camera 2, unde biletul de inceput apare
/// dupa doua secunde. Diferenta e ca aici foloseste panoul din GameHUD, deci
/// arata la fel cu restul biletelor din camera si se inchide tot cu Esc.
///
/// Panoul blocheaza miscarea cat e deschis, deci intarzierea conteaza: prea
/// mica si jucatorul nici nu apuca sa vada unde a nimerit; prea mare si a
/// pornit deja prin camera cand ii sare foaia in fata.
///
/// Textul se scrie FARA diacritice. Fontul Caveat e pe atlas dinamic: orice
/// gliful nou il genereaza la rulare si umfla fisierul fontului, care ajunge
/// apoi in git si se bate cu ce au ceilalti.
public class BiletDeInceput : MonoBehaviour
{
    [Header("Continut")]
    public string titlu = "Bilet";

    [TextArea(4, 14)]
    public string text =
        "Trei usi. Doar una duce afara.\n\n" +
        "Cauta indicii prin camera ca sa afli care.\n\n" +
        "Nu deschide la intamplare.";

    [Header("Cand apare")]
    [Tooltip("Cate secunde dupa intrarea in camera.")]
    public float intarziere = 1.5f;

    [Header("Cand nu apare")]
    [Tooltip("Camera pentru care se verifica progresul. Gol = apare mereu.")]
    public string idCamera = "camera6";

    [Tooltip("Daca e bifat, nu mai apare dupa ce camera a fost terminata o data.")]
    public bool doarInainteDeTerminare = true;

    IEnumerator Start()
    {
        if (doarInainteDeTerminare &&
            !string.IsNullOrWhiteSpace(idCamera) &&
            Progres.ETerminata(idCamera))
            yield break;

        yield return new WaitForSeconds(intarziere);

        GameHUD.DeschideNota(titlu, text);
    }
}
