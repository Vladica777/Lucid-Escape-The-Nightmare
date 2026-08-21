using UnityEngine;
using UnityEngine.InputSystem;

/// Lanterna de pe jos: o ridici cu E, o aprinzi si o stingi cu F.
///
/// Mecanica e copiata cap-coada de la lanterna Karinei din camera 2, din
/// PlayerInteractionKarina: aceeasi pozitie in mana, aceeasi rotatie, acelasi
/// fel de aprins. Numerele sunt gasite prin incercari in joc si nu se pot
/// deduce din prefab, deci nu le schimbati fara sa testati.
///
/// Diferenta fata de ea: la Karina lanterna e cablata in scriptul jucatorului,
/// aici e o componenta pe obiect, ca sa mearga cu PlayerInteractor din
/// Assets/Scripts si sa poata fi pusa in orice camera.
///
/// Ce NU face: nu se atinge de Light. Becul din prefab e spot, intensitate
/// 2.1, raza 10, unghi 62, cu cookie - fasciculul lung din camera 2 vine
/// exact de acolo. Orice reglaj din script il strica. Am incercat sa-l
/// slabesc cat zace pe jos si rezultatul a fost o balta de lumina fara con.
public class Lanterna : Interactable
{
    [Tooltip("Becul din lanterna. Gol = il caut in obiect.")]
    public Light bec;

    [Header("Cum sta in mana")]
    [Tooltip("Fata de camera. Pozitia e a Karinei, din camera 2.")]
    public Vector3 pozitieInMana = new Vector3(0.4f, -0.3f, 0.6f);

    /// Rotatia NU e a ei, si asta e intentionat.
    ///
    /// Becul e un copil al radacinii, cu rotatie proprie, iar directia lui in
    /// spatiul radacinii iese exact pe +Y local. Deci rotatia radacinii e cea
    /// care hotaraste incotro bate fasciculul, si se poate calcula:
    ///
    ///   Euler(0, 0, 90)  trimite +Y in -X  = fix in stanga, pe langa ecran
    ///   Euler(90, 0, 0)  trimite +Y in +Z  = drept inainte
    ///
    /// La Karina lanterna lumineaza in stanga, pe langa ce privesti. Aici e
    /// pornita de la 90 pe X, plus o inclinare mica: 7 grade in jos si 8 la
    /// stanga, ca fasciculul sa cada unde te uiti si sa convearga spre centru,
    /// asa cum tine cineva o lanterna in mana dreapta.
    [Tooltip("Fata de camera. Vezi comentariul din cod inainte sa o schimbi: " +
             "de rotatia asta depinde incotro bate fasciculul.")]
    public Vector3 rotatieInMana = new Vector3(97f, -8f, 0f);

    [Header("Texte")]
    public string mesajLuata = "Ai luat lanterna! Apasa F pentru lumina.";

    bool luata;

    public override bool CanInteract => enabled && !luata;

    public override string GetPrompt() => "Ia lanterna";

    void Awake()
    {
        if (bec == null) bec = GetComponentInChildren<Light>(true);

        if (bec == null)
            Debug.LogWarning("Lanterna n-are bec in ea, F n-o sa faca nimic.", this);
    }

    public override void Interact(PlayerInteractor player)
    {
        if (luata) return;

        // daca jucatorul n-are camera pusa in Inspector, o cautam, ca lanterna
        // sa nu ramana pe jos fara sa spuna nimeni de ce
        Transform camera = null;
        if (player != null && player.cam != null) camera = player.cam.transform;
        else if (Camera.main != null) camera = Camera.main.transform;

        if (camera == null)
        {
            Debug.LogWarning("Lanterna: nu gasesc camera jucatorului, n-am unde s-o pun.", this);
            return;
        }

        luata = true;

        // exact ca la Karina: se agata de camera si primeste pozitia si
        // rotatia din Inspector, indiferent cum zacea pe jos
        transform.SetParent(camera, false);
        transform.localPosition = pozitieInMana;
        transform.localRotation = Quaternion.Euler(rotatieInMana);

        // nu mai are ce cauta in raza de interactiune, ar acoperi restul
        var colider = GetComponent<Collider>();
        if (colider != null) Destroy(colider);

        GameHUD.Mesaj(mesajLuata, 3f);
    }

    void Update()
    {
        if (!luata || bec == null || GameHUD.Blocking) return;

        var kb = Keyboard.current;

        // fara "sau": daca ar raspunde la ambele sisteme deodata, F ar comuta
        // de doua ori in acelasi cadru si becul ar parea ca nu reactioneaza
        bool apasat = kb != null
            ? kb.fKey.wasPressedThisFrame
            : Input.GetKeyDown(KeyCode.F);

        if (apasat) bec.enabled = !bec.enabled;
    }
}
