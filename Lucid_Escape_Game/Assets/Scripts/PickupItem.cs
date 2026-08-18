using UnityEngine;

/// Obiect care poate fi ridicat si pus in inventar.
/// Se pune pe orice obiect din scena care are Collider.
public class PickupItem : Interactable
{
    [Header("Ce ajunge in inventar")]
    public Item item = new Item("obiect", "Obiect");

    [Tooltip("Daca e bifat, obiectul dispare din scena dupa ce e luat.")]
    public bool hideAfterPickup = true;

    bool luat;

    public override bool CanInteract => enabled && !luat;

    public override string GetPrompt()
    {
        string nume = string.IsNullOrEmpty(item.displayName) ? "obiect" : item.displayName;
        return $"Ia {nume}";
    }

    public override void Interact(PlayerInteractor player)
    {
        var inv = player.inventory;
        if (inv == null)
        {
            Debug.LogWarning("PickupItem: jucatorul nu are componenta Inventory.", this);
            return;
        }

        if (inv.IsFull)
        {
            GameHUD.Mesaj("Inventarul e plin.");
            return;
        }

        inv.Add(item);
        luat = true;
        GameHUD.Mesaj($"Ai luat: {item.displayName}");

        if (hideAfterPickup) gameObject.SetActive(false);
    }
}
