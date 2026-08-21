using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Imbraca peretii camerei 6 cu panourile P_Wall_01, aceleasi pe care le
/// foloseste holul.
///
/// De ce unealta si nu pozitii scrise direct in scena: nu se poate sti din
/// fisier cat de lat si de gros e panoul, unde ii cade pivotul si incotro
/// priveste. Aici il instantiem o data, ii masuram cutia reala si asezam
/// restul dupa masuratoare, lipite de fata interioara a peretelui.
///
/// Sterge intai panourile puse anterior, deci se poate rula de cate ori vrei
/// si nu se aduna copii.
///
/// Meniu: LUCID / Camera 6 - imbraca peretii
public static class ImbracaPeretii
{
    const string Prefab = "Assets/Dnk_Dev/HospitalHorrorPack/Prefab/P_Wall_01.prefab";
    const string Prefix = "Panou-";

    // camera: pereti la x = +-4.5, grosime 0.2, deci fata interioara la +-4.4
    const float FataVest = -4.4f;
    const float FataEst = 4.4f;
    const float FataSud = -10.4f;
    const float ZMin = -10.5f;
    const float ZMax = 10.5f;

    [MenuItem("LUCID/Camera 6 - imbraca peretii")]
    public static void Imbraca()
    {
        var scena = SceneManager.GetActiveScene();

        if (scena.name != "vld-room")
        {
            Debug.LogError($"Deschide intai vld-room. Acum e '{scena.name}'.");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        if (prefab == null)
        {
            Debug.LogError($"Nu gasesc panoul la {Prefab}.");
            return;
        }

        int sterse = Curata(scena);

        // masuram panoul: instantiem unul, il rotim ca pentru peretele de vest
        var proba = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scena);
        proba.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 90f, 0f));

        if (!Cutie(proba, out Bounds b))
        {
            Debug.LogError("Panoul n-are niciun renderer, nu am ce masura.");
            Object.DestroyImmediate(proba);
            return;
        }

        Object.DestroyImmediate(proba);

        float latime = b.size.z;    // dupa rotatie, latimea cade pe axa Z
        float grosime = b.size.x;
        float inaltime = b.size.y;

        Debug.Log($"Panou masurat: latime {latime:0.##} m, inaltime {inaltime:0.##} m, " +
                  $"grosime {grosime:0.##} m.");

        if (latime < 0.1f)
        {
            Debug.LogError("Panoul are latime aproape zero. Ceva e in neregula cu prefabul.");
            return;
        }

        int puse = 0;
        var parinte = Gaseste("Decor");

        // peretii lungi, de la sud spre nord
        for (float z = ZMin; z < ZMax - 0.01f; z += latime)
        {
            float latimeAici = Mathf.Min(latime, ZMax - z);
            if (latimeAici < latime * 0.4f) continue;   // rest prea mic, il sarim

            puse += Pune(prefab, scena, parinte, $"{Prefix}V-{z:0.#}", 90f,
                         new Vector3(FataVest, 0f, z + latime / 2f), FataVest, true);
            puse += Pune(prefab, scena, parinte, $"{Prefix}E-{z:0.#}", -90f,
                         new Vector3(FataEst, 0f, z + latime / 2f), FataEst, false);
        }

        // peretele de sud, in spatele jucatorului
        for (float x = -4.5f; x < 4.5f - 0.01f; x += latime)
        {
            float latimeAici = Mathf.Min(latime, 4.5f - x);
            if (latimeAici < latime * 0.4f) continue;

            puse += PuneSud(prefab, scena, parinte, $"{Prefix}S-{x:0.#}",
                            new Vector3(x + latime / 2f, 0f, FataSud));
        }

        EditorSceneManager.MarkSceneDirty(scena);

        Debug.Log($"Peretii imbracati: {puse} panouri puse, {sterse} vechi sterse. " +
                  "Salveaza cu Ctrl+S.");
    }

    /// Aseaza un panou si il lipeste de fata peretelui, indiferent unde ii
    /// cade pivotul: masuram unde a ajuns si il impingem cat trebuie.
    static int Pune(GameObject prefab, Scene scena, GameObject parinte,
                    string nume, float yaw, Vector3 unde, float fata, bool spreEst)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scena);
        Undo.RegisterCreatedObjectUndo(go, "Panou de perete");

        go.name = nume;
        go.transform.SetPositionAndRotation(unde, Quaternion.Euler(0f, yaw, 0f));
        if (parinte != null) go.transform.SetParent(parinte.transform, true);

        if (Cutie(go, out Bounds b))
        {
            // fata dinspre camera trebuie sa cada pe planul peretelui
            float delta = spreEst ? fata - b.min.x : fata - b.max.x;
            go.transform.position += new Vector3(delta, -b.min.y, 0f);
        }

        return 1;
    }

    static int PuneSud(GameObject prefab, Scene scena, GameObject parinte,
                       string nume, Vector3 unde)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scena);
        Undo.RegisterCreatedObjectUndo(go, "Panou de perete");

        go.name = nume;
        go.transform.SetPositionAndRotation(unde, Quaternion.identity);
        if (parinte != null) go.transform.SetParent(parinte.transform, true);

        if (Cutie(go, out Bounds b))
            go.transform.position += new Vector3(0f, -b.min.y, FataSud - b.min.z);

        return 1;
    }

    static bool Cutie(GameObject go, out Bounds b)
    {
        var r = go.GetComponentsInChildren<Renderer>(true);
        b = new Bounds();

        if (r.Length == 0) return false;

        b = r[0].bounds;
        for (int i = 1; i < r.Length; i++) b.Encapsulate(r[i].bounds);
        return true;
    }

    /// Scoate panourile puse la rulari anterioare, ca sa nu se adune.
    static int Curata(Scene scena)
    {
        var vechi = new List<GameObject>();

        foreach (var radacina in scena.GetRootGameObjects())
            foreach (var t in radacina.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith(Prefix)) vechi.Add(t.gameObject);

        foreach (var go in vechi) Undo.DestroyObjectImmediate(go);
        return vechi.Count;
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
