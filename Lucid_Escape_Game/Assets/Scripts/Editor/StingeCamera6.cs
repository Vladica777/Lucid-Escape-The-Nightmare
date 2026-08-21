using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Stinge camera 6 si pune lanterna pe jos, langa masa.
///
/// Ideea: te trezesti in intuneric, vezi departe pata calda a lumanarii, te
/// duci acolo si gasesti biletul si lanterna. De acolo incolo, cauti cu ea.
///
/// Ce ramane aprins: lumanarea de pe masa si cele trei lampi de deasupra
/// usilor, slabe, ca sa se vada ce te ademeneste. Restul se stinge, inclusiv
/// lumina de umplere si cea de deasupra dulapului - dulapul trebuie gasit,
/// nu servit.
///
/// Lampile ale caror lumini se sting se ascund si ele: altfel ar ramane niste
/// dreptunghiuri aprinse pe tavan, fara sa lumineze nimic.
///
/// Meniu: LUCID / Camera 6 - stinge lumina si pune lanterna
public static class StingeCamera6
{
    const string PrefabLanterna = "Assets/Flashlight/Flashlight_Gold/Flashlight.prefab";
    const string NumeLanterna = "Lanterna";

    // pe jos, langa masa, in bataia lumanarii
    static readonly Vector3 UndeSta = new Vector3(-0.85f, 0.12f, -0.55f);

    [MenuItem("LUCID/Camera 6 - stinge lumina si pune lanterna")]
    public static void Stinge()
    {
        var scena = SceneManager.GetActiveScene();

        if (scena.name != "vld-room")
        {
            Debug.LogError($"Deschide intai vld-room. Acum e '{scena.name}'.");
            return;
        }

        Lumini();
        Lanterna(scena);

        EditorSceneManager.MarkSceneDirty(scena);
        Debug.Log("Camera 6 stinsa. Salveaza cu Ctrl+S.");
    }

    static void Lumini()
    {
        // ce se stinge cu totul, impreuna cu corpul de lampa de deasupra
        Stinge("L-Umplere", null);
        Stinge("L-Intrare", "Lampa-Intrare");
        Stinge("L-Dulap", "Lampa-Dulap");

        // lumanarea: pata mica si calda peste carte
        Regleaza("L-Masa", 1.1f, 3.2f);

        // usile: cat sa se ghiceasca, nu cat sa se citeasca
        foreach (string usa in new[] { "L-UsaA", "L-UsaB", "L-UsaC" })
            Regleaza(usa, 0.75f, 4f);
    }

    static void Stinge(string numeLumina, string numeLampa)
    {
        var go = Gaseste(numeLumina);

        if (go == null)
            Debug.LogWarning($"Nu gasesc '{numeLumina}'.");
        else
        {
            Undo.RecordObject(go, "Stinge " + numeLumina);
            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }

        if (numeLampa == null) return;

        var lampa = Gaseste(numeLampa);

        if (lampa != null)
        {
            Undo.RecordObject(lampa, "Ascunde " + numeLampa);
            lampa.SetActive(false);
            EditorUtility.SetDirty(lampa);
        }
    }

    static void Regleaza(string nume, float intensitate, float raza)
    {
        var go = Gaseste(nume);
        if (go == null) { Debug.LogWarning($"Nu gasesc '{nume}'."); return; }

        var lumina = go.GetComponent<Light>();
        if (lumina == null) return;

        Undo.RecordObject(lumina, "Regleaza " + nume);
        lumina.intensity = intensitate;
        lumina.range = raza;
        EditorUtility.SetDirty(lumina);
    }

    static void Lanterna(Scene scena)
    {
        if (Gaseste(NumeLanterna) != null)
        {
            Debug.Log("Lanterna e deja in scena.");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabLanterna);

        if (prefab == null)
        {
            Debug.LogError($"Nu gasesc lanterna la {PrefabLanterna}.");
            return;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scena);
        Undo.RegisterCreatedObjectUndo(go, "Pune lanterna");

        go.name = NumeLanterna;
        go.transform.SetPositionAndRotation(UndeSta, Quaternion.Euler(0f, 35f, 90f));

        // trebuie sa poata fi tintita cu raza din centrul ecranului
        if (go.GetComponentInChildren<Collider>(true) == null)
        {
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(0.22f, 0.22f, 0.3f);
        }

        var lant = go.GetComponent<Lanterna>();
        if (lant == null) lant = go.AddComponent<Lanterna>();

        lant.bec = go.GetComponentInChildren<Light>(true);

        Debug.Log($"Lanterna pusa pe jos la {UndeSta}, langa masa.", go);
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
