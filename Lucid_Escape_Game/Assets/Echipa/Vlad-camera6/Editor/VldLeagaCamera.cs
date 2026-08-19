using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Leaga camera 6 la Main Hall, adica pune peste ce exista deja in scena
/// cele doua piese cerute de contractul din PLAN-Legare-Camere.md:
///
///   1. PunctSpawn cu id "intrare", pe marcajul SPAWN_Player care exista deja
///   2. IesireCamera pe trapa, in modul LaDeschidere
///
/// Trapa e la 3 metri, in tavan, si in camera nu exista nici scara nici cutie
/// pe care sa te urci, deci nu se poate trece fizic prin ea. De asta iesirea
/// se declanseaza cand trapa se deschide, nu cand jucatorul trece prin trigger.
///
/// Se poate rula de cate ori vrei. Lucreaza pe scena deschisa in editor, deci
/// salvezi tu cu Ctrl+S dupa ce te uiti la rezultat. Ctrl+Z anuleaza.
///
/// Meniu: Camera 6 / Leaga camera la Main Hall
public static class VldLeagaCamera
{
    const string IdCamera = "camera6";
    const string IdCheieTrapa = "cheie_trapa";
    const string ScenaHol = "MainHall";
    const string SpawnInHol = "dupa_camera6";

    [MenuItem("Camera 6/Leaga camera la Main Hall")]
    public static void Leaga()
    {
        var scena = SceneManager.GetActiveScene();
        if (!scena.isLoaded)
        {
            Debug.LogError("Camera 6: nu e nicio scena deschisa.");
            return;
        }

        int schimbari = PunctulDeIntrare() + IesireaPrinTrapa();

        if (schimbari > 0)
        {
            EditorSceneManager.MarkSceneDirty(scena);
            Debug.Log($"Camera 6: {schimbari} lucruri legate. Salveaza cu Ctrl+S.");
        }
        else
        {
            Debug.Log("Camera 6: era deja legata, n-am schimbat nimic.");
        }
    }

    /// PunctSpawn pe marcajul care exista deja, ca sa nu apara doua obiecte
    /// care spun acelasi lucru in acelasi loc.
    static int PunctulDeIntrare()
    {
        var marcaj = Gaseste("SPAWN_Player");

        if (marcaj == null)
        {
            Debug.LogWarning("Camera 6: n-am gasit 'SPAWN_Player'. " +
                             "Pune manual un PunctSpawn cu id 'intrare'.");
            return 0;
        }

        if (marcaj.GetComponent<PunctSpawn>() != null) return 0;

        var p = Undo.AddComponent<PunctSpawn>(marcaj);
        p.id = Tranzitie.SpawnImplicit;
        p.seteazaOrientarea = true;
        EditorUtility.SetDirty(p);

        Debug.Log($"Camera 6: PunctSpawn '{p.id}' pus pe '{marcaj.name}'.", marcaj);
        return 1;
    }

    /// IesireCamera pe acelasi obiect ca SwingDoor-ul trapei, in modul
    /// LaDeschidere.
    static int IesireaPrinTrapa()
    {
        var trapa = UsaCu(IdCheieTrapa);

        if (trapa == null)
        {
            Debug.LogWarning($"Camera 6: n-am gasit usa care cere '{IdCheieTrapa}'. " +
                             "Trapa mai are componenta SwingDoor?");
            return 0;
        }

        if (trapa.GetComponent<IesireCamera>() != null) return 0;

        var ies = Undo.AddComponent<IesireCamera>(trapa.gameObject);
        ies.cand = IesireCamera.Declansare.LaDeschidere;
        ies.usa = trapa;
        ies.intarziere = 1.5f;
        ies.idCamera = IdCamera;
        ies.marcheazaTerminat = true;
        ies.spreScena = ScenaHol;
        ies.idSpawn = SpawnInHol;
        EditorUtility.SetDirty(ies);

        Debug.Log($"Camera 6: iesire pusa pe '{trapa.name}' -> {ScenaHol}, " +
                  $"spawn '{SpawnInHol}'. Se declanseaza cand se deschide trapa.",
                  trapa);
        return 1;
    }

    // ------------------------------------------------------------ ajutoare

    static SwingDoor UsaCu(string idCheie)
    {
        foreach (var u in Object.FindObjectsByType<SwingDoor>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (u.requiredItemId == idCheie) return u;
        return null;
    }

    static GameObject Gaseste(string nume)
    {
        var scena = SceneManager.GetActiveScene();
        foreach (var radacina in scena.GetRootGameObjects())
        {
            if (radacina.name == nume) return radacina;

            foreach (var t in radacina.GetComponentsInChildren<Transform>(true))
                if (t.name == nume) return t.gameObject;
        }
        return null;
    }
}
