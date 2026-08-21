using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Aseaza lanterna pe jos exact ca in camera 2 a Karinei si verifica daca
/// modelul chiar se vede.
///
/// De ce a fost nevoie: prefabul lanternei isi cauta plasa dupa un guid care
/// nu mai exista in proiect - fbx-ul a fost reimportat si i s-a schimbat
/// guid-ul, deci referinta s-a rupt. Se vedea doar lumina, obiectul era gol.
/// Guid-ul e pus la loc in Flashlight.FBX.meta; unealta asta verifica
/// rezultatul si se plange daca plasa tot lipseste.
///
/// Setarile sunt copiate de la Karina: scara 2 si pozitia culcata din prefab.
///
/// Se poate rula de cate ori vrei.
///
/// Meniu: LUCID / Camera 6 - repara lanterna
public static class ReparaLanterna
{
    const string Prefab = "Assets/Flashlight/Flashlight_Gold/Flashlight.prefab";
    const string Nume = "Lanterna";

    // pe podea, la ~2.8 m in fata spawn-ului (0, 1, -9) si putin la dreapta:
    // destul de aproape cat sa intre in con, destul de departe cat sa nu cada
    // sub marginea de jos a ecranului
    static readonly Vector3 Unde = new Vector3(0.8f, 0f, -6.2f);

    // culcata, cum e in prefab si cum a lasat-o Karina
    static readonly Quaternion Cum = new Quaternion(-0.70710677f, 0f, 0f, 0.70710677f);

    const float Scara = 2f;      // la scara 1 e cat o bricheta
    const float Podea = 0.02f;   // cat sta talpa deasupra podelei

    // cutia de interactiune, in metri de lume. Se calculeaza din plasa
    // masurata, nu se scrie de mana: doar Unity stie cat e lanterna.
    const float Margine = 0.08f;     // cat depaseste obiectul de fiecare parte
    const float MinimCutie = 0.3f;   // sub atat nu coboara pe nicio axa

    // cum sta in mana. Pozitia e de la Karina; rotatia e calculata, nu
    // copiata - a ei trimite fasciculul in stanga, pe langa ecran. Vezi
    // comentariul lung din Lanterna.cs.
    static readonly Vector3 InMana = new Vector3(0.4f, -0.3f, 0.6f);
    static readonly Vector3 RotitaInMana = new Vector3(97f, -8f, 0f);

    [MenuItem("LUCID/Camera 6 - repara lanterna")]
    public static void Repara()
    {
        var scena = SceneManager.GetActiveScene();

        if (scena.name != "vld-room")
        {
            Debug.LogError($"Deschide intai vld-room. Acum e '{scena.name}'.");
            return;
        }

        var go = Gaseste(Nume);

        if (go == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);

            if (prefab == null)
            {
                Debug.LogError($"Nu gasesc prefabul la {Prefab}.");
                return;
            }

            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scena);
            Undo.RegisterCreatedObjectUndo(go, "Pune lanterna");
            go.name = Nume;
        }

        Undo.RecordObject(go.transform, "Aseaza lanterna");

        go.transform.SetPositionAndRotation(Unde, Cum);
        go.transform.localScale = Vector3.one * Scara;

        var mr = go.GetComponentInChildren<MeshRenderer>(true);
        var mf = go.GetComponentInChildren<MeshFilter>(true);

        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("Lanterna n-are plasa. Referinta din prefab e tot " +
                           "rupta: verifica guid-ul din " +
                           "Assets/Flashlight/Misc/Flashlight.FBX.meta, " +
                           "trebuie sa fie 9a3b57f0a1248be419dd9c4d54748512.", go);
            return;
        }

        // acum stim ca exista ceva de vazut: il masuram si il asezam pe podea
        Bounds b = mr.bounds;
        go.transform.position += Vector3.up * (Podea - b.min.y);

        Debug.Log($"Plasa: '{mf.sharedMesh.name}', {mr.sharedMaterials.Length} materiale. " +
                  $"Lanterna masoara {b.size.x:0.##} x {b.size.y:0.##} x {b.size.z:0.##} m.", go);

        Colider(go, mr);
        Componenta(go);

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(scena);

        Debug.Log($"Lanterna asezata la {go.transform.position}, scara {Scara}. " +
                  "Salveaza cu Ctrl+S.", go);
    }

    /// Cutia de interactiune: plasa masurata, plus o margine mica.
    ///
    /// Nu se scrie de mana. Prefabul e importat la 1/100, deci nimeni nu poate
    /// spune din fisier cat e lanterna - unealta o masoara si construieste
    /// cutia in jurul masuratorii.
    ///
    /// Marginea exista fiindca lanterna e o tija subtire culcata pe podea, iar
    /// raza pleaca dintr-un singur punct din centrul ecranului. Fara ea ar
    /// trebui sa nimeresti cativa centimetri. Cu 8 cm de fiecare parte ai o
    /// tinta lejera care tot arata a lanterna, nu a cutie invizibila.
    ///
    /// Cat timp raza jucatorului se lovea de propria lui capsula, cutia asta
    /// a fost umflata pana la 1.86 m ca sa compenseze ceva ce n-avea legatura
    /// cu ea. Acea problema e reparata in PlayerInteractor, deci cutia poate
    /// sa se stranga la loc pe obiect.
    static void Colider(GameObject go, MeshRenderer mr)
    {
        var box = go.GetComponent<BoxCollider>();
        if (box == null) box = Undo.AddComponent<BoxCollider>(go);

        // in local, fiindca BoxCollider lucreaza in local, iar radacina e la scara 2
        float margine = Margine / Scara;
        float minim = MinimCutie / Scara;

        Bounds local = mr.localBounds;

        var marime = new Vector3(
            Mathf.Max(local.size.x + margine * 2f, minim),
            Mathf.Max(local.size.y + margine * 2f, minim),
            Mathf.Max(local.size.z + margine * 2f, minim));

        Undo.RecordObject(box, "Cutie lanterna");
        box.center = local.center;
        box.size = marime;
        box.isTrigger = false;
        EditorUtility.SetDirty(box);

        // colidere in plus ar fura raza inaintea cutiei celei bune
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            if (c != box) Undo.DestroyObjectImmediate(c);

        Debug.Log($"Cutie de interactiune: {marime.x * Scara:0.##} x " +
                  $"{marime.y * Scara:0.##} x {marime.z * Scara:0.##} m, " +
                  $"in jurul unui obiect de {local.size.x * Scara:0.##} x " +
                  $"{local.size.y * Scara:0.##} x {local.size.z * Scara:0.##} m.", go);
    }

    /// Valorile se scriu explicit, nu se lasa pe seama celor din cod: obiectul
    /// exista deja in scena cu campurile serializate de la o rulare veche, iar
    /// alea nu se schimba doar fiindca s-a schimbat valoarea implicita.
    static void Componenta(GameObject go)
    {
        var lant = go.GetComponent<Lanterna>();
        if (lant == null) lant = Undo.AddComponent<Lanterna>(go);

        Undo.RecordObject(lant, "Setari lanterna");

        lant.bec = go.GetComponentInChildren<Light>(true);
        lant.pozitieInMana = InMana;
        lant.rotatieInMana = RotitaInMana;

        EditorUtility.SetDirty(lant);

        if (lant.bec == null)
        {
            Debug.LogWarning("Lanterna n-are bec in ea, F n-o sa faca nimic.", go);
            return;
        }

        // becul ramane exact cum vine din prefab. Il raportam doar ca sa se
        // vada daca l-a modificat cineva in scena: fasciculul din camera 2 e
        // spot, 2.1, raza 10, unghi 62.
        Debug.Log($"In mana: pozitie {InMana}, rotatie {RotitaInMana}. " +
                  $"Bec: {lant.bec.type}, intensitate {lant.bec.intensity}, " +
                  $"raza {lant.bec.range}, unghi {lant.bec.spotAngle}, " +
                  $"aprins {lant.bec.enabled}.", go);
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
