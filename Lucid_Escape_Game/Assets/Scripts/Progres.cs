using System.Collections.Generic;
using UnityEngine;

/// Starea care trece dintr-o scena in alta: ce camere sunt terminate si cate
/// vieti au ramas.
///
/// E clasa statica, deci nu se pune pe niciun obiect si nu are nevoie de
/// DontDestroyOnLoad - supravietuieste automat schimbarii de scena.
///
/// Nu se salveaza pe disc. Cand opresti Play, o iei de la capat. Salvarea
/// intre sesiuni se adauga mai tarziu, cu PlayerPrefs, si nu schimba nimic
/// din API-ul de mai jos.
public static class Progres
{
    public const int VietiLaStart = 3;

    static readonly HashSet<string> terminate = new HashSet<string>();
    static int vieti = VietiLaStart;

    /// Se declanseaza la orice schimbare: camera terminata, viata pierduta,
    /// resetare. Usile din hol si HUD-ul se pot lega aici in loc sa intrebe
    /// in fiecare cadru.
    public static event System.Action Schimbat;

    public static int Vieti => vieti;
    public static bool MaiEViata => vieti > 0;
    public static int CateTerminate => terminate.Count;

    /// A fost terminata camera asta?
    public static bool ETerminata(string idCamera)
    {
        return !string.IsNullOrWhiteSpace(idCamera) && terminate.Contains(idCamera);
    }

    /// Conditia de deblocare a unei usi. Un id gol inseamna "fara conditie",
    /// deci usa e deschisa de la inceput. Asta e metoda pe care o cheama
    /// usile din hol.
    public static bool EIndeplinita(string idCamera)
    {
        return string.IsNullOrWhiteSpace(idCamera) || terminate.Contains(idCamera);
    }

    /// Marcheaza o camera ca rezolvata. Se poate chema de mai multe ori
    /// fara efecte secundare.
    public static void Termina(string idCamera)
    {
        if (string.IsNullOrWhiteSpace(idCamera))
        {
            Debug.LogWarning("Progres.Termina: id gol, nu marchez nimic.");
            return;
        }

        if (!terminate.Add(idCamera)) return;   // era deja terminata

        Debug.Log($"Progres: '{idCamera}' terminata. Total: {terminate.Count}.");
        Schimbat?.Invoke();
    }

    /// Scade o viata. Intoarce true daca jucatorul mai are cel putin una.
    public static bool PierdeOViata()
    {
        if (vieti > 0) vieti--;

        Debug.Log($"Progres: viata pierduta, au mai ramas {vieti}.");
        Schimbat?.Invoke();

        return vieti > 0;
    }

    /// Sterge tot: camere terminate si vieti. Pentru meniul de restart.
    public static void Reseteaza()
    {
        terminate.Clear();
        vieti = VietiLaStart;
        Schimbat?.Invoke();
    }

    /// Statica ramane in memorie intre sesiuni de Play daca cineva opreste
    /// Domain Reload din Project Settings. Asta o goleste la fiecare pornire,
    /// ca sa nu inceapa jocul cu jumatate de progres facut.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void LaPornire()
    {
        terminate.Clear();
        vieti = VietiLaStart;
        Schimbat = null;
    }
}
