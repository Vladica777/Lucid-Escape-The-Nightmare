using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Unealta temporara pentru camera 6.
///
/// Rezolva golul de design in care nimic nu-i spunea jucatorului sa caute
/// in dulapul metalic: adauga strofa care trimite acolo, schimba mesajul
/// trapei incuiate, pune o lumina peste dulap si mareste colliderul cheii.
///
/// Se poate rula de cate ori vrei: verifica intai ce e deja facut si nu
/// repeta nimic. Merge pe scena deschisa in editor, deci salvezi tu, cu
/// Ctrl+S, dupa ce te uiti la rezultat. Se poate anula cu Ctrl+Z.
///
/// Dupa ce si-a facut treaba, fisierul asta se poate sterge - tot ce
/// conteaza ramane in scena.
public static class VldIndiciiDulap
{
    // ------------------------------------------------------------ constante

    const string MARCA_STROFA = "dinte de alama";

    const string STROFA_NOUA =
        "\nGura e zavorata, iar zavorul cere un dinte de alama." +
        "\n\nDintele doarme in burta unui fier ruginit," +
        "\nla dreapta ta cand stai cu fata la cele trei minciuni.";

    const string MESAJ_TRAPA =
        "Trapa e incuiata. Zavorul cere o cheie mica, de alama.";

    const string ID_CHEIE_TRAPA = "cheie_trapa";

    const string CALE_ALAMA =
        "Assets/Echipa/Vlad-camera6/materiale-vld/vld_alama.mat";

    // cat de mare sa fie cutia de nimerit a cheii, in metri
    static readonly Vector3 CUTIE_CHEIE = new Vector3(0.25f, 0.18f, 0.25f);

    // ------------------------------------------------------------ meniu

    [MenuItem("Camera 6/Aplica indiciile pentru dulap")]
    public static void Aplica()
    {
        var scena = SceneManager.GetActiveScene();
        if (!scena.isLoaded)
        {
            Debug.LogError("Camera 6: nu e nicio scena deschisa.");
            return;
        }

        int schimbari = 0;

        schimbari += StrofaDinBilet();
        schimbari += MesajulTrapei();
        schimbari += CutiaCheii();
        schimbari += AlamaPeCheie();
        schimbari += LuminaDulapului();
        schimbari += LuminiCazuteDeasupraTavanului();

        if (schimbari > 0)
        {
            EditorSceneManager.MarkSceneDirty(scena);
            Debug.Log($"Camera 6: {schimbari} lucruri schimbate. Salveaza scena cu Ctrl+S.");
        }
        else
        {
            Debug.Log("Camera 6: totul era deja aplicat, n-am schimbat nimic.");
        }
    }

    [MenuItem("Camera 6/Adauga luminile de pe usi (optional)")]
    public static void LuminiUsi()
    {
        var parinte = GrupulLumini();
        if (parinte == null) return;

        int facute = 0;
        float[] x = { -3f, 0f, 3f };
        string[] nume = { "L-UsaA", "L-UsaB", "L-UsaC" };

        for (int i = 0; i < 3; i++)
        {
            if (Gaseste(nume[i]) != null) continue;

            FaLumina(nume[i], parinte,
                     new Vector3(x[i], 2.6f, 9.8f),
                     new Color(0.85f, 0.9f, 1f),
                     2f, 5f);
            facute++;
        }

        if (facute > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"Camera 6: {facute} lumini adaugate pe usi. Salveaza cu Ctrl+S.");
        }
        else
        {
            Debug.Log("Camera 6: luminile de pe usi existau deja.");
        }
    }

    // ------------------------------------------------------------ pasii

    /// Strofa care trimite jucatorul la dulap, adaugata la finalul ghicitorii.
    static int StrofaDinBilet()
    {
        var foaie = Object.FindFirstObjectByType<ReadableNote>(FindObjectsInactive.Include);
        if (foaie == null)
        {
            Debug.LogWarning("Camera 6: n-am gasit nicio foaie cu ReadableNote.");
            return 0;
        }

        if (foaie.text.Contains(MARCA_STROFA)) return 0;

        Undo.RecordObject(foaie, "Strofa cu dulapul");
        foaie.text = foaie.text.TrimEnd() + STROFA_NOUA;
        EditorUtility.SetDirty(foaie);
        Debug.Log($"Camera 6: strofa adaugata pe '{foaie.name}'.", foaie);
        return 1;
    }

    /// Mesajul trapei incuiate spune ce cheie lipseste, nu doar ca e incuiata.
    static int MesajulTrapei()
    {
        var trapa = UsaCu(ID_CHEIE_TRAPA);
        if (trapa == null)
        {
            Debug.LogWarning($"Camera 6: n-am gasit usa care cere '{ID_CHEIE_TRAPA}'.");
            return 0;
        }

        if (trapa.lockedMessage == MESAJ_TRAPA) return 0;

        Undo.RecordObject(trapa, "Mesajul trapei");
        trapa.lockedMessage = MESAJ_TRAPA;
        EditorUtility.SetDirty(trapa);
        Debug.Log($"Camera 6: mesaj nou pe '{trapa.name}'.", trapa);
        return 1;
    }

    /// Cheia e de un centimetru si jumatate inaltime, iar interactiunea merge
    /// pe raza subtire din centrul ecranului. Fara asta n-o poti nimeri.
    static int CutiaCheii()
    {
        var cheie = PickupCu(ID_CHEIE_TRAPA);
        if (cheie == null)
        {
            Debug.LogWarning($"Camera 6: n-am gasit obiectul '{ID_CHEIE_TRAPA}'.");
            return 0;
        }

        var box = cheie.GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogWarning($"Camera 6: '{cheie.name}' n-are BoxCollider.", cheie);
            return 0;
        }

        Vector3 s = cheie.transform.lossyScale;
        if (Mathf.Abs(s.x) < 1e-5f || Mathf.Abs(s.y) < 1e-5f || Mathf.Abs(s.z) < 1e-5f)
        {
            Debug.LogWarning($"Camera 6: '{cheie.name}' are scale zero pe o axa.", cheie);
            return 0;
        }

        // cat masoara acum cutia in metri, nu in unitatile obiectului
        var acum = new Vector3(box.size.x * Mathf.Abs(s.x),
                               box.size.y * Mathf.Abs(s.y),
                               box.size.z * Mathf.Abs(s.z));

        // daca e deja destul de mare, o lasam cum e - nu umblam pentru milimetri
        if (acum.x >= CUTIE_CHEIE.x * 0.9f &&
            acum.y >= CUTIE_CHEIE.y * 0.9f &&
            acum.z >= CUTIE_CHEIE.z * 0.9f) return 0;

        // marimea colliderului e in spatiul obiectului, deci impartim la scale
        var vrem = new Vector3(CUTIE_CHEIE.x / Mathf.Abs(s.x),
                               CUTIE_CHEIE.y / Mathf.Abs(s.y),
                               CUTIE_CHEIE.z / Mathf.Abs(s.z));

        Undo.RecordObject(box, "Cutia cheii");
        box.size = vrem;
        EditorUtility.SetDirty(box);
        Debug.Log($"Camera 6: colliderul cheii marit la {CUTIE_CHEIE.x * 100f:0} x " +
                  $"{CUTIE_CHEIE.y * 100f:0} x {CUTIE_CHEIE.z * 100f:0} cm.", cheie);
        return 1;
    }

    /// Cheia statea pe materialul default alb. Acum, cu lumina peste dulap,
    /// se vede - si un cub alb lucios nu seamana cu o cheie de alama.
    static int AlamaPeCheie()
    {
        var cheie = PickupCu(ID_CHEIE_TRAPA);
        if (cheie == null) return 0;

        var mr = cheie.GetComponent<MeshRenderer>();
        if (mr == null) return 0;

        var alama = AssetDatabase.LoadAssetAtPath<Material>(CALE_ALAMA);
        if (alama == null)
        {
            Debug.LogWarning($"Camera 6: nu exista materialul {CALE_ALAMA}.");
            return 0;
        }

        if (mr.sharedMaterial == alama) return 0;

        Undo.RecordObject(mr, "Alama pe cheie");
        mr.sharedMaterial = alama;
        EditorUtility.SetDirty(mr);
        Debug.Log("Camera 6: cheia a primit materialul de alama.", cheie);
        return 1;
    }

    /// Latura de est era neagra si dulapul nu se distingea de perete.
    static int LuminaDulapului()
    {
        if (Gaseste("L-Dulap") != null) return 0;

        var parinte = GrupulLumini();
        if (parinte == null) return 0;

        FaLumina("L-Dulap", parinte,
                 new Vector3(3.1f, 2f, -1f),
                 new Color(0.78f, 0.85f, 1f),
                 1.1f, 4f);

        Debug.Log("Camera 6: lumina 'L-Dulap' adaugata.");
        return 1;
    }

    /// L-Masa si L-Intrare fusesera trase din greseala peste tavan, unde
    /// tavanul le oprea lumina complet.
    static int LuminiCazuteDeasupraTavanului()
    {
        int mutate = 0;
        mutate += CoboaraLumina("L-Masa", new Vector3(0f, 2.2f, 0f));
        mutate += CoboaraLumina("L-Intrare", new Vector3(0f, 2.5f, -8f));
        return mutate;
    }

    static int CoboaraLumina(string nume, Vector3 unde)
    {
        var go = Gaseste(nume);
        if (go == null) return 0;

        // tavanul e la y = 3; orice lumina peste el nu ajunge in camera
        if (go.transform.position.y <= 3f) return 0;

        Undo.RecordObject(go.transform, "Coboara " + nume);
        go.transform.position = unde;
        EditorUtility.SetDirty(go.transform);
        Debug.Log($"Camera 6: '{nume}' era deasupra tavanului, coborata la {unde}.", go);
        return 1;
    }

    // ------------------------------------------------------------ ajutoare

    static Transform GrupulLumini()
    {
        var grup = Gaseste("Lumini");
        if (grup != null) return grup.transform;

        Debug.LogWarning("Camera 6: n-am gasit grupul 'Lumini'. " +
                         "Esti sigur ca e deschisa vld-room.unity?");
        return null;
    }

    static void FaLumina(string nume, Transform parinte, Vector3 poz,
                         Color culoare, float intensitate, float raza)
    {
        var go = new GameObject(nume);
        Undo.RegisterCreatedObjectUndo(go, "Lumina " + nume);

        go.transform.SetParent(parinte, false);
        go.transform.localPosition = poz;

        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = culoare;
        l.intensity = intensitate;
        l.range = raza;
        l.shadows = LightShadows.Soft;
    }

    static SwingDoor UsaCu(string idCheie)
    {
        foreach (var u in Object.FindObjectsByType<SwingDoor>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (u.requiredItemId == idCheie) return u;
        return null;
    }

    static PickupItem PickupCu(string id)
    {
        foreach (var p in Object.FindObjectsByType<PickupItem>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (p.item != null && p.item.id == id) return p;
        return null;
    }

    /// Cauta dupa nume inclusiv prin obiectele dezactivate, ceea ce
    /// GameObject.Find nu face.
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
