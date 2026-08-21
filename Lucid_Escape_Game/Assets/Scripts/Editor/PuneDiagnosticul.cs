using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Pune sau scoate diagnosticul razei de pe jucatorul din scena deschisa.
///
/// Acelasi meniu face ambele: daca exista, il sterge. Merge in orice camera
/// care foloseste PlayerInteractor.
///
/// Meniu: LUCID / Diagnostic - ce loveste crosshair-ul
public static class PuneDiagnosticul
{
    [MenuItem("LUCID/Diagnostic - ce loveste crosshair-ul")]
    public static void Comuta()
    {
        var interactor = Object.FindFirstObjectByType<PlayerInteractor>();

        if (interactor == null)
        {
            Debug.LogError("Nu gasesc niciun PlayerInteractor in scena deschisa.");
            return;
        }

        var vechi = interactor.GetComponent<DiagnosticRaza>();

        if (vechi != null)
        {
            Undo.DestroyObjectImmediate(vechi);
            Debug.Log("Diagnostic scos.", interactor);
        }
        else
        {
            Undo.AddComponent<DiagnosticRaza>(interactor.gameObject);
            Debug.Log("Diagnostic pus pe " + interactor.name +
                      ". Intra in Play si uita-te la coltul din stanga sus.",
                      interactor);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
