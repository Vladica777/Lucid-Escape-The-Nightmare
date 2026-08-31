using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Pune in hol obiectul care declanseaza finalul si il leaga de personaj si
/// de ecranul final.
///
/// Le cauta si dezactivate - GameObject.Find nu le-ar vedea, iar amandoua
/// stau debifate tocmai fiindca scriptul le aprinde.
///
/// Meniu: LUCID / Main Hall - pune finalul dupa camera 6
public static class PuneFinalul
{
    const string Nume = "FINAL-dupa-camera6";
    const string Personaj = "Ch30_nonPBR@Zombie Attack";
    const string Ecran = "Canvas";

    [MenuItem("LUCID/Main Hall - pune finalul dupa camera 6")]
    public static void Pune()
    {
        var scena = SceneManager.GetActiveScene();

        if (scena.name != "MainHall")
        {
            Debug.LogError($"Deschide intai MainHall. Acum e '{scena.name}'.");
            return;
        }

        var go = Gaseste(Nume);

        if (go == null)
        {
            go = new GameObject(Nume);
            Undo.RegisterCreatedObjectUndo(go, "Finalul jocului");
        }

        var final = go.GetComponent<FinalDupaUltimaCamera>();
        if (final == null) final = Undo.AddComponent<FinalDupaUltimaCamera>(go);

        Undo.RecordObject(final, "Leaga finalul");

        final.personaj = Gaseste(Personaj);
        final.ecranFinal = Gaseste(Ecran);
        final.idCamera = "camera6";

        EditorUtility.SetDirty(final);

        if (final.personaj == null) Debug.LogWarning($"Nu gasesc '{Personaj}' in scena.");
        if (final.ecranFinal == null) Debug.LogWarning($"Nu gasesc '{Ecran}' in scena.");

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(scena);

        Debug.Log($"Finalul e pe '{Nume}': personaj={final.personaj}, " +
                  $"ecran={final.ecranFinal}. Cele doua raman debifate in scena, " +
                  "scriptul le aprinde. Salveaza cu Ctrl+S.", go);
    }

    static GameObject Gaseste(string nume)
    {
        foreach (var radacina in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (radacina.name == nume) return radacina;

            foreach (var t in radacina.GetComponentsInChildren<Transform>(true))
                if (t.name == nume) return t.gameObject;
        }
        return null;
    }
}
