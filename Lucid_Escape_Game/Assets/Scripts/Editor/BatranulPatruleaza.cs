using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Pune batranul sa se plimbe stanga-dreapta prin fata celor trei usi.
///
/// Foloseste InamicPatrula, scriptul Karinei din camera 2, in loc sa scriu
/// inca unul la fel. El cere un Animator cu un parametru bool "Merge", care
/// comuta intre stat pe loc si mers.
///
/// Controllerul facut de unealta anterioara are o singura stare, deci il
/// rescriem aici cu doua: Idle si Mers, plus tranzitiile intre ele.
///
/// Se poate rula de cate ori vrei.
///
/// Meniu: LUCID / Camera 6 - batranul patruleaza prin fata usilor
public static class BatranulPatruleaza
{
    const string ModelIdle = "Assets/Ch30_nonPBR@Old Man Idle.fbx";
    const string ModelMers = "Assets/Ch30_nonPBR@Walking.fbx";
    const string Controller = "Assets/Echipa/Vlad-camera6/vld_batran.controller";

    // capetele traseului: prin fata usilor, care sunt la x = -3, 0, 3
    static readonly Vector3 Stanga = new Vector3(-3f, 0f, 9.5f);
    static readonly Vector3 Dreapta = new Vector3(3f, 0f, 9.5f);

    [MenuItem("LUCID/Camera 6 - batranul patruleaza prin fata usilor")]
    public static void Patruleaza()
    {
        var scena = SceneManager.GetActiveScene();

        if (scena.name != "vld-room")
        {
            Debug.LogError($"Deschide intai vld-room. Acum e '{scena.name}'.");
            return;
        }

        var batran = Gaseste("Batran");

        if (batran == null)
        {
            Debug.LogError("Nu gasesc 'Batran' in scena. Ruleaza intai " +
                           "LUCID / Camera 6 - pune batranul langa usa C.");
            return;
        }

        PregatesteModelele();

        var controller = FaControllerul();
        if (controller == null) return;

        var animator = batran.GetComponent<Animator>();
        if (animator == null) animator = batran.AddComponent<Animator>();

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;   // il misca scriptul, nu animatia

        // Fara avatar, un rig Generic nu poate primi un clip venit din alt
        // fbx: oasele nu se leaga, iar personajul ramane intepenit intr-o
        // pozitie in timp ce scriptul il plimba.
        if (animator.avatar == null)
        {
            var avatar = AssetDatabase.LoadAllAssetsAtPath(ModelIdle)
                .OfType<Avatar>()
                .FirstOrDefault();

            if (avatar == null)
                Debug.LogWarning("Modelul n-are avatar. Animatia de mers " +
                                 "n-o sa se lege de schelet.", batran);
            else if (!avatar.isValid)
                Debug.LogWarning($"Avatarul '{avatar.name}' nu e valid. " +
                                 "Rig-ul nu a fost recunoscut ca umanoid.", batran);
            else
            {
                animator.avatar = avatar;
                Debug.Log($"Avatar pus: '{avatar.name}'.");
            }
        }

        var a = Marcaj("PUNCT_Batran_Stanga", Stanga);
        var b = Marcaj("PUNCT_Batran_Dreapta", Dreapta);

        var patrula = batran.GetComponent<InamicPatrula>();
        if (patrula == null) patrula = Undo.AddComponent<InamicPatrula>(batran);

        patrula.punctulA = a.transform;
        patrula.punctulB = b.transform;
        patrula.viteza = 0.7f;          // pas de batran, nu de alergator
        patrula.timpAsteptare = 4f;     // se opreste si asculta

        EditorUtility.SetDirty(patrula);
        EditorSceneManager.MarkSceneDirty(scena);

        Debug.Log($"Batranul patruleaza intre {Stanga} si {Dreapta}, " +
                  $"cu {patrula.viteza} m/s si pauze de {patrula.timpAsteptare} s. " +
                  "Salveaza cu Ctrl+S.", batran);
    }

    /// Ambele modele sunt importate cu avatarSetup 0, adica fara avatar. Fara
    /// avatar nu exista schelet pentru motor, deci niciun clip nu se poate
    /// lega de personaj.
    ///
    /// Le trecem pe Humanoid: rig-urile Mixamo sunt umanoide, iar animatiile
    /// umanoide se retargheteaza, deci mersul dintr-un fbx merge pe scheletul
    /// din celalalt fara sa depinda de nume identice de oase.
    static void PregatesteModelele()
    {
        foreach (string cale in new[] { ModelIdle, ModelMers })
        {
            var imp = AssetImporter.GetAtPath(cale) as ModelImporter;

            if (imp == null)
            {
                Debug.LogWarning($"Nu pot citi setarile de import pentru {cale}.");
                continue;
            }

            if (imp.animationType == ModelImporterAnimationType.Human &&
                imp.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel)
                continue;

            imp.animationType = ModelImporterAnimationType.Human;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            imp.SaveAndReimport();

            Debug.Log($"Reimportat ca Humanoid, cu avatar: {cale}");
        }
    }

    /// Doua stari si un bool, exact ce asteapta InamicPatrula.
    static AnimatorController FaControllerul()
    {
        var idle = Clip(ModelIdle);
        var mers = Clip(ModelMers);

        if (idle == null || mers == null)
        {
            Debug.LogError("Nu gasesc clipurile de animatie in cele doua modele.");
            return null;
        }

        // il facem de la zero: cel vechi are o singura stare si niciun parametru
        AssetDatabase.DeleteAsset(Controller);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(Controller);

        ac.AddParameter("Merge", AnimatorControllerParameterType.Bool);

        var masina = ac.layers[0].stateMachine;

        var starePeLoc = masina.AddState("PeLoc");
        starePeLoc.motion = idle;

        var stareMers = masina.AddState("Merge");
        stareMers.motion = mers;

        masina.defaultState = starePeLoc;

        var pleaca = starePeLoc.AddTransition(stareMers);
        pleaca.AddCondition(AnimatorConditionMode.If, 0f, "Merge");
        pleaca.hasExitTime = false;
        pleaca.duration = 0.25f;

        var seOpreste = stareMers.AddTransition(starePeLoc);
        seOpreste.AddCondition(AnimatorConditionMode.IfNot, 0f, "Merge");
        seOpreste.hasExitTime = false;
        seOpreste.duration = 0.25f;

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();

        Debug.Log($"Controller refacut: '{idle.name}' pe loc, '{mers.name}' la mers.");
        return ac;
    }

    static AnimationClip Clip(string cale)
    {
        return AssetDatabase.LoadAllAssetsAtPath(cale)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    /// Capat de traseu. Daca exista deja, il lasam unde l-a mutat cineva.
    static GameObject Marcaj(string nume, Vector3 unde)
    {
        var existent = Gaseste(nume);
        if (existent != null) return existent;

        var go = new GameObject(nume);
        Undo.RegisterCreatedObjectUndo(go, "Capat de traseu");
        go.transform.position = unde;

        var marcaje = Gaseste("MARCAJE");
        if (marcaje != null) go.transform.SetParent(marcaje.transform, true);

        return go;
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
