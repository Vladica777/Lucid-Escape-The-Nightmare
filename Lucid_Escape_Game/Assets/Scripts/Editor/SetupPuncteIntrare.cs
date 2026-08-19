using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Pune in fiecare scena din Build Settings punctul de intrare cerut de
/// contract: un obiect gol cu PunctSpawn si id-ul "intrare".
///
/// Acolo apare jucatorul cand intra in nivel dinspre hol. Si tot ala se
/// foloseste cand apesi Play direct in scena aia, deci fiecare camera ramane
/// jucabila si separat.
///
/// Unde il pune:
///   - daca scena are deja un jucator, exact peste el, cu aceeasi orientare
///   - daca nu are, in mijlocul camerei, coborat pana la prima podea de sub el
///
/// A doua varianta e o aproximare, nu o decizie de design: cine detine camera
/// muta marcajul unde vrea. Gizmoul arata capsula jucatorului si incotro
/// priveste, deci se vede din Scene view daca e prost pus.
///
/// Se poate rula de cate ori vrei: scenele care au deja punctul sunt sarite.
///
/// Meniu: LUCID / Pune punctele de intrare in toate scenele
public static class SetupPuncteIntrare
{
    const string Versiune = "v1";
    const string NumeMarcaj = "SPAWN_intrare";

    [MenuItem("LUCID/Pune punctele de intrare in toate scenele")]
    public static void Pune()
    {
        var deschisa = SceneManager.GetActiveScene();

        if (deschisa.isDirty)
        {
            Debug.LogError("Ai modificari nesalvate in scena deschisa. " +
                           "Salveaza cu Ctrl+S intai - unealta deschide alte " +
                           "scene si le-ai pierde.");
            return;
        }

        var scene = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();

        if (scene.Count == 0)
        {
            Debug.LogError("Build Settings e gol. Ruleaza intai " +
                           "LUCID / Pune scenele in Build Settings.");
            return;
        }

        string caleInitiala = deschisa.path;
        var raport = new List<string>();
        int puse = 0;

        foreach (string cale in scene)
        {
            var scena = EditorSceneManager.OpenScene(cale, OpenSceneMode.Single);
            string nume = scena.name;

            var existent = Object.FindObjectsByType<PunctSpawn>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(p => p.id == Tranzitie.SpawnImplicit);

            if (existent != null)
            {
                raport.Add($"  {nume}: avea deja punct de intrare ({existent.name})");
                continue;
            }

            Vector3 pozitie;
            Quaternion rotatie;
            string cum;

            var jucator = Object.FindFirstObjectByType<CharacterController>(
                FindObjectsInactive.Include);

            if (jucator != null)
            {
                pozitie = jucator.transform.position;
                rotatie = Quaternion.Euler(0f, jucator.transform.eulerAngles.y, 0f);
                cum = $"peste jucatorul '{jucator.name}'";
            }
            else if (MijloculCamerei(scena, out pozitie))
            {
                rotatie = Quaternion.identity;
                cum = "in mijlocul camerei, DE AJUSTAT";
            }
            else
            {
                raport.Add($"  {nume}: n-are jucator si n-am putut gasi o podea. SARIT");
                continue;
            }

            var go = new GameObject(NumeMarcaj);
            go.transform.SetPositionAndRotation(pozitie, rotatie);

            var punct = go.AddComponent<PunctSpawn>();
            punct.id = Tranzitie.SpawnImplicit;
            punct.seteazaOrientarea = true;

            EditorSceneManager.MarkSceneDirty(scena);
            EditorSceneManager.SaveScene(scena);

            raport.Add($"  {nume}: pus la {Rotunjit(pozitie)}, {cum}");
            puse++;
        }

        // ne intoarcem la scena de la care ai plecat
        if (!string.IsNullOrEmpty(caleInitiala))
            EditorSceneManager.OpenScene(caleInitiala, OpenSceneMode.Single);

        Debug.Log($"Puncte de intrare [{Versiune}]: {puse} puse, " +
                  $"{scene.Count} scene verificate.\n" + string.Join("\n", raport) +
                  "\n\nScenele modificate au fost salvate automat.");
    }

    /// Fara jucator in scena nu exista un raspuns corect, doar unul rezonabil:
    /// centrul a tot ce se vede, coborat pana la prima suprafata de sub el.
    static bool MijloculCamerei(Scene scena, out Vector3 pozitie)
    {
        pozitie = Vector3.zero;

        var randari = scena.GetRootGameObjects()
                           .SelectMany(g => g.GetComponentsInChildren<Renderer>(true))
                           .ToList();

        if (randari.Count == 0) return false;

        var cutie = randari[0].bounds;
        for (int i = 1; i < randari.Count; i++) cutie.Encapsulate(randari[i].bounds);

        var deSus = new Vector3(cutie.center.x, cutie.max.y + 1f, cutie.center.z);

        if (Physics.Raycast(deSus, Vector3.down, out RaycastHit hit, cutie.size.y + 5f))
        {
            pozitie = hit.point + Vector3.up * 0.1f;
            return true;
        }

        // fara collider sub noi: macar il punem la nivelul de jos al camerei
        pozitie = new Vector3(cutie.center.x, cutie.min.y + 0.1f, cutie.center.z);
        return true;
    }

    static string Rotunjit(Vector3 v) =>
        $"({v.x:0.##}, {v.y:0.##}, {v.z:0.##})";
}
