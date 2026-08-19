using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Leaga cele cinci usi din Main Hall la nivelurile lor si pune punctele de
/// intoarcere pe ancore.
///
/// Lanturi: usa 1 e deschisa de la inceput, fiecare urmatoare cere camera
/// dinainte terminata. Toate valorile ajung in Inspector, deci ordinea se
/// poate rearanja fara cod.
///
/// Se poate rula de cate ori vrei. Lucreaza pe scena deschisa, deci salvezi
/// tu cu Ctrl+S. Ctrl+Z anuleaza.
///
/// Meniu: LUCID / Leaga usile din Main Hall
public static class SetupMainHall
{
    /// Apare in fiecare mesaj din consola, ca sa se vada imediat daca Unity a
    /// recompilat sau daca a rulat o versiune veche a uneltei.
    const string Versiune = "v3 - punct in coridor la 2.7 m";

    /// doorNumber -> (id camera, scena, spawn in nivel, camera ceruta)
    struct Legatura
    {
        public int usa;
        public string idCamera;
        public string scena;
        public string cere;
    }

    static readonly Legatura[] Legaturi =
    {
        new Legatura { usa = 1, idCamera = "camera2", scena = "findkey_room", cere = ""        },
        new Legatura { usa = 2, idCamera = "camera3", scena = "",             cere = "camera2" },
        new Legatura { usa = 3, idCamera = "camera4", scena = "Camera_Ana",   cere = "camera3" },
        new Legatura { usa = 4, idCamera = "camera5", scena = "cam5TEST",     cere = "camera4" },
        new Legatura { usa = 5, idCamera = "camera6", scena = "vld-room",     cere = "camera5" },
    };

    [MenuItem("LUCID/Leaga usile din Main Hall")]
    public static void Leaga()
    {
        var scena = SceneManager.GetActiveScene();

        if (!scena.isLoaded || scena.name != "MainHall")
        {
            Debug.LogError("Deschide intai MainHall.unity. Acum e deschisa " +
                           $"'{scena.name}'.");
            return;
        }

        int schimbari = Usile() + Ancorele();

        if (schimbari > 0)
        {
            EditorSceneManager.MarkSceneDirty(scena);
            Debug.Log($"Main Hall [{Versiune}]: {schimbari} lucruri legate. " +
                      "Salveaza cu Ctrl+S.");
        }
        else
        {
            Debug.Log($"Main Hall [{Versiune}]: era deja legat, n-am schimbat nimic.");
        }
    }

    static int Usile()
    {
        var usi = Object.FindObjectsByType<DoorInteraction>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (usi.Length == 0)
        {
            Debug.LogWarning("Main Hall: n-am gasit nicio usa cu DoorInteraction.");
            return 0;
        }

        int schimbate = 0;

        foreach (var leg in Legaturi)
        {
            var usa = usi.FirstOrDefault(u => u.DoorNumber == leg.usa);

            if (usa == null)
            {
                Debug.LogWarning($"Main Hall: nu exista usa cu Door Number {leg.usa}.");
                continue;
            }

            var so = new SerializedObject(usa);
            bool schimbat = false;

            schimbat |= Pune(so, "sceneName", leg.scena);
            schimbat |= Pune(so, "spawnId", Tranzitie.SpawnImplicit);
            schimbat |= Pune(so, "requiresRoom", leg.cere);
            schimbat |= Pune(so, "lockedPrompt", MesajIncuiat(leg));

            if (!schimbat) continue;

            Undo.RecordObject(usa, "Leaga usa " + leg.usa);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(usa);

            string unde = string.IsNullOrEmpty(leg.scena) ? "(nicio scena inca)" : leg.scena;
            string cheie = string.IsNullOrEmpty(leg.cere) ? "deschisa de la inceput" : "cere " + leg.cere;
            Debug.Log($"Main Hall: usa {leg.usa} -> {leg.idCamera} / {unde}, {cheie}.", usa);
            schimbate++;
        }

        return schimbate;
    }

    /// Punctele de intoarcere, ca sa apari in fata usii pe care ai intrat,
    /// nu in dormitor.
    ///
    /// RoomAnchor_0N e punctul de montaj al usii in perete, la x = +-4.05, si
    /// priveste spre camera, nu spre coridor. Coridorul are podeaua lata de
    /// 4.5 m, deci interiorul lui e x intre -2.2 si 2.2, iar pereti cu usi
    /// sunt la x = +-2.25. Intre ancora si coridor mai e si vestibulul, la
    /// x = +-3.15.
    ///
    /// De aceea punctul de intoarcere nu se pune pe ancora si nici la un metru
    /// de ea - ai aparea in toc, cu camera in geometrie, si vezi ecran negru.
    /// Se pune cu 2.7 m in spatele ei, adica la x = +-1.35: in coridor, la
    /// vreo 85 cm in fata usii.
    ///
    /// Offsetul merge pe axele locale ale ancorei, deci e valabil si pentru
    /// usile de pe peretele opus.
    const float DistantaInCoridor = 2.7f;

    static int Ancorele()
    {
        var offsetLocal = new Vector3(0f, 0.1f, -DistantaInCoridor);

        int puse = 0;

        foreach (var leg in Legaturi)
        {
            string numeAncora = $"RoomAnchor_{leg.usa:00}";
            var ancora = Gaseste(numeAncora);

            if (ancora == null)
            {
                Debug.LogWarning($"Main Hall: n-am gasit '{numeAncora}'.");
                continue;
            }

            string idDorit = "dupa_" + leg.idCamera;
            string numePunct = "SPAWN_" + idDorit;

            // O versiune mai veche a uneltei punea PunctSpawn direct pe ancora,
            // adica la x = +-4.05, in perete. Il scoatem, altfel jucatorul
            // ajunge cu camera in geometrie si vede ecran negru.
            var greselaVeche = ancora.GetComponent<PunctSpawn>();
            if (greselaVeche != null && greselaVeche.id == idDorit)
            {
                Debug.Log($"Main Hall: scot PunctSpawn '{idDorit}' de pe " +
                          $"{numeAncora}, era in perete.", ancora);
                Undo.DestroyObjectImmediate(greselaVeche);
                puse++;
            }

            var alPunct = ancora.transform.Find(numePunct);

            if (alPunct != null)
            {
                // punct facut de unealta: il corectam, ca sa se poata repara
                // o pozitie gresita ruland din nou
                if (alPunct.localPosition == offsetLocal) continue;

                Undo.RecordObject(alPunct, "Corecteaza punctul " + leg.usa);
                alPunct.localPosition = offsetLocal;
                alPunct.rotation = Quaternion.Euler(0f, 180f, 0f);
                EditorUtility.SetDirty(alPunct);

                Debug.Log($"Main Hall: '{numePunct}' mutat in coridor, " +
                          $"la {alPunct.position}.", alPunct);
                puse++;
                continue;
            }

            // punct cu alt nume, facut de mana: nu ne atingem de el.
            // p.gameObject != ancora conteaza: GetComponentsInChildren include
            // si obiectul insusi, iar fara filtrul asta unealta refuza sa-si
            // repare propria greseala de mai sus.
            var altul = ancora.GetComponentsInChildren<PunctSpawn>(true)
                              .FirstOrDefault(p => p.id == idDorit && p.gameObject != ancora);
            if (altul != null) continue;

            var go = new GameObject(numePunct);
            Undo.RegisterCreatedObjectUndo(go, "Punct de intoarcere " + leg.usa);

            go.transform.SetParent(ancora.transform, false);
            go.transform.localPosition = offsetLocal;
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var punct = go.AddComponent<PunctSpawn>();
            punct.id = idDorit;
            punct.seteazaOrientarea = true;
            EditorUtility.SetDirty(punct);

            Debug.Log($"Main Hall: '{numePunct}' pus sub {numeAncora}, " +
                      $"la {go.transform.position}.", go);
            puse++;
        }

        return puse;
    }

    /// Mesajul de pe usa incuiata spune care nivel il deblocheaza, nu doar
    /// ca e incuiata.
    static string MesajIncuiat(Legatura leg)
    {
        if (string.IsNullOrEmpty(leg.scena))
            return "[E] Usa asta nu duce nicaieri inca";

        if (string.IsNullOrEmpty(leg.cere))
            return "[E] Intra";

        // derivat din conditia reala, nu din numarul usii
        return $"[E] Incuiata. Termina mai intai nivelul {leg.cere.Replace("camera", "")}";
    }

    static bool Pune(SerializedObject so, string camp, string valoare)
    {
        var p = so.FindProperty(camp);

        if (p == null)
        {
            Debug.LogWarning($"DoorInteraction n-are campul '{camp}'. " +
                             "Scriptul e cel actualizat?");
            return false;
        }

        if (p.stringValue == valoare) return false;

        p.stringValue = valoare;
        return true;
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

    // ------------------------------------------------------- ajutor de test

    /// Deblocheaza tot, ca sa poti ajunge la camera 6 fara sa rezolvi intai
    /// camerele care inca n-au iesire. Merge doar in Play mode, fiindca
    /// Progres exista doar la runtime. Nu modifica nicio scena.
    [MenuItem("LUCID/Test - marcheaza camerele 2-5 terminate")]
    public static void DeblocheazaTot()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Porneste intai Play. Progres exista doar la runtime.");
            return;
        }

        foreach (var leg in Legaturi)
            if (leg.idCamera != "camera6") Progres.Termina(leg.idCamera);

        Debug.Log("Test: camerele 2-5 marcate terminate, toate usile sunt deschise.");
    }
}
