using UnityEngine;
using UnityEngine.SceneManagement;

/// Schimbarea de scena, cu memorie despre unde trebuie sa apara jucatorul.
///
/// Cine pleaca dintr-o scena spune unde vrea sa ajunga:
///     Tranzitie.Incarca("MainHall", "dupa_camera6");
///
/// In scena noua, PunctSpawn cu id-ul "dupa_camera6" muta jucatorul la el.
/// Daca nimeni nu a cerut nimic - adica ai apasat Play direct in scena aia -
/// se foloseste punctul cu id-ul "intrare".
public static class Tranzitie
{
    public const string SpawnImplicit = "intrare";

    /// Unde trebuie sa apara jucatorul in scena care se incarca.
    public static string SpawnUrmator { get; private set; }

    /// Din ce scena s-a venit. Util pentru "inapoi de unde ai plecat".
    public static string ScenaAnterioara { get; private set; }

    /// Incarca o scena si retine unde sa apara jucatorul.
    /// Intoarce false daca scena nu poate fi incarcata, si spune de ce.
    public static bool Incarca(string scena, string idSpawn = SpawnImplicit)
    {
        if (string.IsNullOrWhiteSpace(scena))
        {
            Debug.LogError("Tranzitie.Incarca: nu mi-ai dat niciun nume de scena.");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(scena))
        {
            Debug.LogError(
                $"Tranzitie.Incarca: scena '{scena}' nu poate fi incarcata. " +
                "Cel mai probabil nu e in File > Build Profiles > Scene List. " +
                "Verifica si ca numele e scris exact ca fisierul, fara .unity.");
            return false;
        }

        SpawnUrmator = string.IsNullOrWhiteSpace(idSpawn) ? SpawnImplicit : idSpawn;
        ScenaAnterioara = SceneManager.GetActiveScene().name;

        Debug.Log($"Tranzitie: {ScenaAnterioara} -> {scena}, spawn '{SpawnUrmator}'.");
        SceneManager.LoadScene(scena);
        return true;
    }

    /// Punctul asta e tinta tranzitiei curente?
    /// Cand nimeni n-a cerut nimic, tinta e punctul de intrare.
    public static bool EsteTinta(string idPunct)
    {
        string cautat = string.IsNullOrWhiteSpace(SpawnUrmator) ? SpawnImplicit : SpawnUrmator;
        return !string.IsNullOrWhiteSpace(idPunct) && idPunct == cautat;
    }

    /// Chemat de punctul care a mutat jucatorul, ca sa nu il mai mute nimeni
    /// altcineva daca scena se reincarca.
    public static void MarcheazaFolosit()
    {
        SpawnUrmator = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void LaPornire()
    {
        SpawnUrmator = null;
        ScenaAnterioara = null;
    }
}
