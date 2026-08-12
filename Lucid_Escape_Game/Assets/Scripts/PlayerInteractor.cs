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

    void Awake()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>();
        if (inventory == null) inventory = GetComponent<Inventory>();
    }

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

        if (!Physics.Raycast(ray, out RaycastHit hit, range, mask, QueryTriggerInteraction.Collide))
            return null;

        // cautam si in parinti, fiindca modelele au colliderul pe un copil
        var it = hit.collider.GetComponentInParent<Interactable>();
        return (it != null && it.CanInteract) ? it : null;
    }
}
