using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Reincarca scena deschisa de pe disc.
///
/// Unity tine scena in memorie si nu observa cand fisierul .unity se schimba
/// din afara editorului. Daca cineva modifica scena direct in fisier - o
/// unealta, un merge din git, sau eu de aici - editorul lucreaza in
/// continuare cu versiunea lui veche, iar la salvare o scrie peste.
///
/// Asta face ce ai face cu File > Open Scene pe aceeasi scena, doar ca
/// dintr-un singur clic si cu o intrebare clara daca ai lucru nesalvat.
///
/// Meniu: LUCID / Reincarca scena de pe disc   (Ctrl+Alt+R)
public static class ReincarcaScena
{
    [MenuItem("LUCID/Reincarca scena de pe disc %&r")]
    public static void Reincarca()
    {
        var scena = SceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(scena.path))
        {
            Debug.LogError("Scena deschisa n-a fost salvata niciodata, " +
                           "deci n-am de unde s-o reincarc.");
            return;
        }

        if (Application.isPlaying)
        {
            Debug.LogWarning("Opreste intai Play. In timpul jocului scena nu " +
                             "se poate reincarca.");
            return;
        }

        if (scena.isDirty)
        {
            bool arunca = EditorUtility.DisplayDialog(
                "Reincarca scena",
                $"'{scena.name}' are modificari nesalvate in editor.\n\n" +
                "Reincarcarea le arunca si ia versiunea de pe disc.",
                "Arunca si reincarca",
                "Anuleaza");

            if (!arunca)
            {
                Debug.Log("Reincarcare anulata, n-am atins nimic.");
                return;
            }
        }

        string cale = scena.path;
        string nume = scena.name;

        EditorSceneManager.OpenScene(cale, OpenSceneMode.Single);

        Debug.Log($"'{nume}' reincarcata de pe disc. Ce vezi acum e exact ce e " +
                  "in fisier.");
    }

    /// Nu are rost sa apara activ cand nu se poate face nimic.
    [MenuItem("LUCID/Reincarca scena de pe disc %&r", true)]
    public static bool SePoate()
    {
        return !Application.isPlaying &&
               !string.IsNullOrEmpty(SceneManager.GetActiveScene().path);
    }
}
