using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Doua reparatii punctuale in scenele altora, cerute ca sa poata fi legate
/// la hol ca toate celelalte.
///
///   Camera 2 - n-are jucator deloc, doar o camera fixa la radacina. Ii punem
///              acelasi montaj ca in camera 6, in fata usii.
///
///   Camera 5 - are camera netaguita MainCamera, deci Camera.main da null
///              acolo. Doar bifa de tag, nimic altceva.
///
/// Amandoua deschid scena tinta, o modifica, o salveaza si te readuc unde
/// erai. Daca ai modificari nesalvate, refuza sa porneasca.
///
/// Meniu: LUCID / Camera 2 - pune jucatorul
///        LUCID / Camera 5 - taguieste camera ca MainCamera
public static class ReparaCamere
{
    const string Camera2 = "Assets/Echipa/Karina-camera2/findkey_room.unity";
    const string Camera5 = "Assets/Echipa/Sinzina-camera5/cam5TEST.unity";

    // acelasi montaj ca jucatorul din camera 6
    const float Inaltime = 1.8f;
    const float Raza = 0.3f;
    const float InaltimeaOchilor = 1.65f;

    // ------------------------------------------------------------ camera 2

    [MenuItem("LUCID/Camera 2 - pune jucatorul")]
    public static void JucatorInCamera2()
    {
        if (!PotSaLucrez(out string caleInitiala)) return;

        var scena = EditorSceneManager.OpenScene(Camera2, OpenSceneMode.Single);

        if (Object.FindFirstObjectByType<CharacterController>(FindObjectsInactive.Include) != null)
        {
            Debug.Log("Camera 2: are deja un jucator, nu ma ating.");
            Inapoi(caleInitiala);
            return;
        }

        if (!UndeIntram(scena, out Vector3 pozitie, out Quaternion rotatie))
        {
            Debug.LogError("Camera 2: n-am gasit podea sub niciun punct, " +
                           "nu stiu unde sa-l pun. Pune-l manual.");
            Inapoi(caleInitiala);
            return;
        }

        // camera veche, fixa la radacina, ar randa in paralel cu a jucatorului
        // si ar fura si tagul MainCamera. O stingem, n-o stergem.
        foreach (var c in Object.FindObjectsByType<Camera>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.transform.parent != null) continue;

            c.gameObject.SetActive(false);
            Debug.Log($"Camera 2: am stins camera fixa '{c.name}'. " +
                      "E doar dezactivata, o poti reaprinde oricand.", c);
        }

        var jucator = FaJucatorul(pozitie, rotatie);

        if (Object.FindObjectsByType<PunctSpawn>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).All(p => p.id != Tranzitie.SpawnImplicit))
        {
            var marcaj = new GameObject("SPAWN_intrare");
            marcaj.transform.SetPositionAndRotation(pozitie, rotatie);
            marcaj.AddComponent<PunctSpawn>().id = Tranzitie.SpawnImplicit;
        }

        EditorSceneManager.MarkSceneDirty(scena);
        EditorSceneManager.SaveScene(scena);

        Debug.Log($"Camera 2: jucator pus la {pozitie}, cu punctul de intrare " +
                  "peste el. Scena a fost salvata.", jucator);

        Inapoi(caleInitiala);
    }

    /// Intram pe unde intra si jucatorul: langa usa, cu fata spre camera.
    static bool UndeIntram(Scene scena, out Vector3 pozitie, out Quaternion rotatie)
    {
        pozitie = Vector3.zero;
        rotatie = Quaternion.identity;

        var randari = scena.GetRootGameObjects()
                           .SelectMany(g => g.GetComponentsInChildren<Renderer>(true))
                           .ToList();

        if (randari.Count == 0) return false;

        var cutie = randari[0].bounds;
        for (int i = 1; i < randari.Count; i++) cutie.Encapsulate(randari[i].bounds);

        // usa, daca exista; altfel plecam din mijloc
        var usa = scena.GetRootGameObjects()
                       .SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                       .FirstOrDefault(t => t.name.StartsWith("P_Door"));

        Vector3 deUnde = usa != null ? usa.position : cutie.center;

        // spre interiorul camerei, pe orizontala
        Vector3 spre = cutie.center - deUnde;
        spre.y = 0f;
        spre = spre.sqrMagnitude < 0.01f ? Vector3.forward : spre.normalized;

        Vector3 tinta = usa != null ? deUnde + spre * 2f : cutie.center;
        rotatie = Quaternion.LookRotation(spre, Vector3.up);

        // coboram pana la prima podea
        var deSus = new Vector3(tinta.x, cutie.max.y + 1f, tinta.z);

        if (Physics.Raycast(deSus, Vector3.down, out RaycastHit hit, cutie.size.y + 10f))
        {
            pozitie = hit.point + Vector3.up * 0.15f;
            return true;
        }

        pozitie = new Vector3(tinta.x, cutie.min.y + 0.15f, tinta.z);
        return true;
    }

    /// Acelasi montaj ca jucatorul din camera 6, ca sa se simta la fel.
    static GameObject FaJucatorul(Vector3 pozitie, Quaternion rotatie)
    {
        var go = new GameObject("Player");
        go.tag = "Player";
        go.transform.SetPositionAndRotation(pozitie, rotatie);

        var cc = go.AddComponent<CharacterController>();
        cc.height = Inaltime;
        cc.radius = Raza;
        cc.center = new Vector3(0f, Inaltime / 2f, 0f);
        cc.stepOffset = 0.35f;
        cc.skinWidth = 0.08f;

        var ochi = new GameObject("Camera_Jucator");
        ochi.tag = "MainCamera";
        ochi.transform.SetParent(go.transform, false);
        ochi.transform.localPosition = new Vector3(0f, InaltimeaOchilor, 0f);
        ochi.AddComponent<Camera>();
        ochi.AddComponent<AudioListener>();

        var ctrl = go.AddComponent<PlayerController>();
        ctrl.cameraPivot = ochi.transform;

        go.AddComponent<Inventory>();
        go.AddComponent<PlayerInteractor>();
        go.AddComponent<GameHUD>();

        return go;
    }

    // ------------------------------------------------------------ camera 5

    [MenuItem("LUCID/Camera 5 - taguieste camera ca MainCamera")]
    public static void TagInCamera5()
    {
        if (!PotSaLucrez(out string caleInitiala)) return;

        var scena = EditorSceneManager.OpenScene(Camera5, OpenSceneMode.Single);

        // camera de joc: cea activa, nu cea de zoom pe keypad
        var camera = Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c.gameObject.activeInHierarchy && c.enabled);

        if (camera == null)
        {
            Debug.LogError("Camera 5: n-am gasit nicio camera activa.");
            Inapoi(caleInitiala);
            return;
        }

        if (camera.CompareTag("MainCamera"))
        {
            Debug.Log($"Camera 5: '{camera.name}' era deja MainCamera.");
            Inapoi(caleInitiala);
            return;
        }

        Undo.RecordObject(camera.gameObject, "Tag MainCamera");
        camera.gameObject.tag = "MainCamera";
        EditorUtility.SetDirty(camera.gameObject);

        EditorSceneManager.MarkSceneDirty(scena);
        EditorSceneManager.SaveScene(scena);

        Debug.Log($"Camera 5: '{camera.name}' e acum MainCamera. " +
                  "Camera.main nu mai da null acolo. Scena a fost salvata.", camera);

        Inapoi(caleInitiala);
    }

    // ------------------------------------------------------------ ajutoare

    static bool PotSaLucrez(out string caleInitiala)
    {
        var deschisa = SceneManager.GetActiveScene();
        caleInitiala = deschisa.path;

        if (!deschisa.isDirty) return true;

        Debug.LogError("Ai modificari nesalvate in scena deschisa. Salveaza cu " +
                       "Ctrl+S intai - unealta deschide alta scena si le-ai pierde.");
        return false;
    }

    static void Inapoi(string cale)
    {
        if (!string.IsNullOrEmpty(cale))
            EditorSceneManager.OpenScene(cale, OpenSceneMode.Single);
    }
}
