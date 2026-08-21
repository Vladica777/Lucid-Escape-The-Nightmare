using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Pune batranul in camera 6, pe marcajul SPAWN_OldMan de langa usa C.
///
/// GDD-ul il cere nemiscat langa una dintre usi, ca indiciu fals: jucatorul
/// vede o silueta si crede ca usa aia conteaza.
///
/// De ce unealta si nu editare directa in scena: modelul e un .fbx, iar
/// id-urile lui interne nu se pot ghici din fisier. Unity le rezolva singur
/// cand instantiaza modelul.
///
/// Clipul e importat ca Generic, deci are nevoie de Animator si de un
/// controller. Unealta il creeaza o data, langa scena, si il refoloseste.
///
/// Se poate rula de cate ori vrei: daca batranul e deja in scena, ii verifica
/// animatia si colliderul si le completeaza daca lipsesc.
///
/// Meniu: LUCID / Camera 6 - pune batranul langa usa C
public static class PuneBatranul
{
    const string Model = "Assets/Ch30_nonPBR@Old Man Idle.fbx";
    const string Controller = "Assets/Echipa/Vlad-camera6/vld_batran.controller";
    const string Marcaj = "SPAWN_OldMan";
    const string Nume = "Batran";

    [MenuItem("LUCID/Camera 6 - pune batranul langa usa C")]
    public static void Pune()
    {
        var scena = SceneManager.GetActiveScene();

        if (scena.name != "vld-room")
        {
            Debug.LogError($"Deschide intai vld-room. Acum e deschisa '{scena.name}'.");
            return;
        }

        var existent = Gaseste(Nume);

        if (existent != null)
        {
            // poate a ramas dintr-o rulare care a crapat: ii completam ce lipseste
            PornesteIdle(existent);
            PuneColliderul(existent);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("Batranul era deja in scena. I-am verificat animatia si " +
                      "colliderul. Salveaza cu Ctrl+S.", existent);
            return;
        }

        var marcaj = Gaseste(Marcaj);
        if (marcaj == null)
        {
            Debug.LogError($"Nu gasesc marcajul '{Marcaj}' in scena.");
            return;
        }

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
        if (model == null)
        {
            Debug.LogError($"Nu gasesc modelul la {Model}.");
            return;
        }

        var batran = (GameObject)PrefabUtility.InstantiatePrefab(model, scena);
        Undo.RegisterCreatedObjectUndo(batran, "Pune batranul");

        batran.name = Nume;

        // pe marcaj, cu fata spre camera - jucatorul vine dinspre sud
        batran.transform.position = marcaj.transform.position;
        batran.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        PornesteIdle(batran);
        PuneColliderul(batran);

        EditorSceneManager.MarkSceneDirty(scena);

        Debug.Log($"Batranul pus la {batran.transform.position}, cu fata spre " +
                  "camera. Salveaza cu Ctrl+S.", batran);
    }

    /// Clipul e Generic, deci ii trebuie un controller. Il facem o data si il
    /// refolosim la rulari urmatoare.
    static void PornesteIdle(GameObject batran)
    {
        var clip = AssetDatabase.LoadAllAssetsAtPath(Model)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));

        if (clip == null)
        {
            Debug.LogWarning("Modelul n-are niciun clip de animatie. " +
                             "Batranul ramane in pozitia de baza.", batran);
            return;
        }

        // Atentie: ?? nu se poate folosi cu obiecte Unity. GetComponent
        // intoarce un fals-null cand componenta lipseste, adica o referinta
        // care nu e null pentru C# dar e moarta pentru motor, iar ?? o lasa
        // sa treaca. Doar == null stie sa faca diferenta.
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);

        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(Controller, clip);

        var animator = batran.GetComponent<Animator>();

        if (animator == null) animator = batran.AddComponent<Animator>();

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        Debug.Log($"Animatie: '{clip.name}', prin {Controller}.");
    }

    /// Fara collider treci prin el ca prin ceata, ceea ce strica exact efectul
    /// pe care il vrem.
    static void PuneColliderul(GameObject batran)
    {
        if (batran.GetComponentInChildren<Collider>() != null) return;

        var col = batran.AddComponent<CapsuleCollider>();
        col.height = 1.8f;
        col.radius = 0.3f;
        col.center = new Vector3(0f, 0.9f, 0f);
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
