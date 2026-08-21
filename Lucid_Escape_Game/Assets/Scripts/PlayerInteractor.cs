using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// Trage un raycast din centrul camerei si gaseste obiectul privit.
/// Apasand E, il activeaza.
public class PlayerInteractor : MonoBehaviour
{
    [Tooltip("Camera din care pleaca raza. Daca e gol, o cauta automat.")]
    public Camera cam;

    [Tooltip("Cat de departe poate ajunge jucatorul, in metri.")]
    public float range = 3f;

    [Tooltip("Ce straturi pot fi interactionate. Lasa Everything daca nu stii.")]
    public LayerMask mask = ~0;

    /// Obiectul privit acum, sau null.
    public Interactable Current { get; private set; }

    public Inventory inventory;

    /// Coliderele jucatorului insusi. Camera sta inauntrul capsulei
    /// CharacterController-ului, deci raza pleaca din interiorul ei si o poate
    /// lovi - mai ales cand privesti in jos, spre un obiect de pe podea.
    /// Rezultatul e o lovitura la 7 cm, in tine, si nimic nu mai e accesibil.
    readonly HashSet<Collider> aleMele = new HashSet<Collider>();

    readonly RaycastHit[] lovituri = new RaycastHit[16];

    void Awake()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>();
        if (inventory == null) inventory = GetComponent<Inventory>();

        foreach (var c in GetComponentsInChildren<Collider>(true)) aleMele.Add(c);
        foreach (var c in GetComponentsInParent<Collider>(true)) aleMele.Add(c);
    }

    /// Colidere pe care raza le ignora fiindca sunt ale jucatorului.
    public bool EAlMeu(Collider c) => c != null && aleMele.Contains(c);

    void Update()
    {
        Current = Cauta();

        if (GameHUD.Blocking) return;

        var kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame && Current != null && Current.CanInteract)
            Current.Interact(this);
    }

    Interactable Cauta()
    {
        if (cam == null) return null;

        // raza pleaca din centrul ecranului, adica din crosshair
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        int n = Physics.RaycastNonAlloc(ray, lovituri, range, mask,
                                        QueryTriggerInteraction.Collide);
        if (n == 0) return null;

        // cel mai apropiat lucru care nu suntem noi insine. Nu sarim peste tot
        // ce nu ne convine: daca intre noi si obiect e un perete, peretele
        // trebuie sa opreasca raza.
        Collider tinta = null;
        float ceaMaiApropiata = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            if (EAlMeu(lovituri[i].collider)) continue;
            if (lovituri[i].distance >= ceaMaiApropiata) continue;

            ceaMaiApropiata = lovituri[i].distance;
            tinta = lovituri[i].collider;
        }

        if (tinta == null) return null;

        // cautam si in parinti, fiindca modelele au colliderul pe un copil
        var it = tinta.GetComponentInParent<Interactable>();
        return (it != null && it.CanInteract) ? it : null;
    }
}
