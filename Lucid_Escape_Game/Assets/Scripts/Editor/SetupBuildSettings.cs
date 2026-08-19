using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// Pune scenele jocului in Build Settings, in ordinea corecta.
///
/// Fara ele in lista, SceneManager.LoadScene nu functioneaza deloc, iar
/// Tranzitie.Incarca da eroare in consola.
///
/// Se poate rula de cate ori vrei. Daca lista e deja buna, nu face nimic.
/// Meniu: LUCID / Pune scenele in Build Settings
public static class SetupBuildSettings
{
    /// Ordinea conteaza doar pentru prima: aia e scena de start a jocului.
    static readonly string[] Scene =
    {
        "Assets/Echipa/Malvina_mainHall/MainHall.unity",
        "Assets/Echipa/Karina-camera2/findkey_room.unity",
        "Assets/Echipa/Ana-camera4/Camera_Ana.unity",
        "Assets/Echipa/Sinzina-camera5/cam5TEST.unity",
        "Assets/Echipa/Vlad-camera6/vld-room.unity",
    };

    [MenuItem("LUCID/Pune scenele in Build Settings")]
    public static void Aplica()
    {
        var lipsa = Scene.Where(c => AssetDatabase.LoadAssetAtPath<SceneAsset>(c) == null).ToList();

        foreach (var c in lipsa)
            Debug.LogWarning($"Build Settings: scena '{c}' nu exista, o sar.");

        var bune = Scene.Where(c => !lipsa.Contains(c)).ToList();

        if (bune.Count == 0)
        {
            Debug.LogError("Build Settings: n-am gasit nicio scena din lista. " +
                           "Ai facut pull?");
            return;
        }

        var acum = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToList();

        if (acum.SequenceEqual(bune))
        {
            Debug.Log("Build Settings: lista era deja buna, n-am schimbat nimic.");
            return;
        }

        EditorBuildSettings.scenes = bune
            .Select(c => new EditorBuildSettingsScene(c, true))
            .ToArray();

        Debug.Log($"Build Settings: {bune.Count} scene puse in lista. " +
                  $"Scena de start: {System.IO.Path.GetFileNameWithoutExtension(bune[0])}.");
    }

    /// Verificare rapida inainte de a testa tranzitiile.
    [MenuItem("LUCID/Verifica scenele din Build Settings")]
    public static void Verifica()
    {
        var linii = new List<string>();
        var lista = EditorBuildSettings.scenes;

        if (lista.Length == 0)
        {
            Debug.LogError("Build Settings e gol. Ruleaza LUCID / Pune scenele in Build Settings.");
            return;
        }

        for (int i = 0; i < lista.Length; i++)
        {
            string nume = System.IO.Path.GetFileNameWithoutExtension(lista[i].path);
            string stare = lista[i].enabled ? "activa" : "DEZACTIVATA";
            bool exista = AssetDatabase.LoadAssetAtPath<SceneAsset>(lista[i].path) != null;

            linii.Add($"  {i}. {nume}  ({stare}{(exista ? "" : ", FISIERUL LIPSESTE")})");
        }

        Debug.Log("Scene in Build Settings:\n" + string.Join("\n", linii) +
                  "\n\nNumele astea sunt exact ce se scrie in campul 'Spre Scena' " +
                  "de pe IesireCamera si in usile din hol.");
    }
}
