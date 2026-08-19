using UnityEditor;
using UnityEngine;

/// Ajutor de testare: deblocheaza usile din hol fara sa rezolvi camerele.
///
/// Usile 2 - 5 se descuie doar dupa ce camera dinainte e terminata, iar
/// camera 3 nu exista inca. Fara asta n-ai cum sa ajungi la camerele de la
/// capat ca sa le testezi.
///
/// Merge doar in Play mode, fiindca Progres traieste doar la runtime. Nu
/// modifica nicio scena si nu lasa nimic in urma: cand opresti Play, progresul
/// se sterge singur.
///
/// Meniu: LUCID / Test - marcheaza camerele 2-5 terminate
public static class TestProgres
{
    static readonly string[] Camere = { "camera2", "camera3", "camera4", "camera5" };

    [MenuItem("LUCID/Test - marcheaza camerele 2-5 terminate")]
    public static void DeblocheazaTot()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Porneste intai Play. Progres exista doar la runtime.");
            return;
        }

        foreach (string id in Camere) Progres.Termina(id);

        Debug.Log("Test: camerele 2-5 marcate terminate, toate usile din hol " +
                  "sunt deschise.");
    }
}
