using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Doua reglaje marunte pentru camera 6, amandoua de rulat o data.
///
/// Meniu: LUCID / Camera 6 - coboara luminile de la usi
/// Meniu: LUCID / Camera 6 - pune biletul de inceput
public static class ReglajeCamera6
{
    const string NumeBilet = "Bilet-inceput";

    // fata de jos a tavanului. Placile au centrul la y = 3 si grosime 0.2.
    const float FataTavanului = 2.9f;
    const float InaltimeNoua = 2.3f;

    [MenuItem("LUCID/Camera 6 - coboara luminile de la usi")]
    public static void CoboaraLuminile()
    {
        if (!EScena()) return;

        int mutate = 0;

        foreach (string nume in new[] { "L-UsaA", "L-UsaB", "L-UsaC" })
        {
            var go = Gaseste(nume);

            if (go == null) { Debug.LogWarning($"Nu gasesc '{nume}'."); continue; }

            var t = go.transform;
            float vechi = t.position.y;

            if (Mathf.Approximately(vechi, InaltimeNoua)) continue;

            Undo.RecordObject(t, "Coboara " + nume);
            t.position = new Vector3(t.position.x, InaltimeNoua, t.position.z);
            EditorUtility.SetDirty(t);
            mutate++;

            Debug.Log($"{nume}: y {vechi:0.##} -> {InaltimeNoua:0.##}. " +
                      $"Distanta pana la tavan {FataTavanului - vechi:0.##} m -> " +
                      $"{FataTavanului - InaltimeNoua:0.##} m.", go);
        }

        // intensitatea pe o suprafata scade cu patratul distantei, deci de la
        // 20 cm la 60 cm pata de pe tavan se stinge de vreo noua ori
        if (mutate > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"{mutate} lumini coborate. Salveaza cu Ctrl+S.");
        }
        else Debug.Log("Luminile erau deja la inaltimea buna.");
    }

    [MenuItem("LUCID/Camera 6 - pune biletul de inceput")]
    public static void PuneBiletul()
    {
        if (!EScena()) return;

        var go = Gaseste(NumeBilet);

        if (go == null)
        {
            go = new GameObject(NumeBilet);
            Undo.RegisterCreatedObjectUndo(go, "Bilet de inceput");
            Debug.Log($"'{NumeBilet}' creat.", go);
        }

        if (go.GetComponent<BiletDeInceput>() == null)
            Undo.AddComponent<BiletDeInceput>(go);

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("Biletul de inceput e pe '" + NumeBilet +
                  "'. Textul si intarzierea se schimba din Inspector. " +
                  "Salveaza cu Ctrl+S.", go);
    }

    static bool EScena()
    {
        var scena = SceneManager.GetActiveScene();
        if (scena.name == "vld-room") return true;

        Debug.LogError($"Deschide intai vld-room. Acum e '{scena.name}'.");
        return false;
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
