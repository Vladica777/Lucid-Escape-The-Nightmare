using UnityEngine;

/// Foaie cu indiciu. La E se deschide un panou cu textul,
/// iar jucatorul ramane blocat pana il inchide.
/// Optional, foaia poate ajunge si in inventar ca sa fie recitita.
public class ReadableNote : Interactable
{
    [Header("Continut")]
    public string title = "Bilet";

    [TextArea(4, 14)]
    public string text = "Scrie aici ghicitoarea.";

    [Header("Optiuni")]
    [Tooltip("Daca e bifat, foaia intra si in inventar dupa prima citire.")]
    public bool addToInventory = true;

    [Tooltip("Daca e bifat, foaia dispare din scena dupa ce e luata.")]
    public bool hideAfterRead = false;

    bool citita;

    public override string GetPrompt() => citita ? $"Citeste din nou: {title}" : $"Citeste {title}";

    public override void Interact(PlayerInteractor player)
    {
        GameHUD.DeschideNota(title, text);

        if (!citita)
        {
            citita = true;

            if (addToInventory && player.inventory != null && !player.inventory.IsFull)
                player.inventory.Add(new Item(
                    "nota_" + title.ToLower().Replace(' ', '_'), title, text));

            if (hideAfterRead) gameObject.SetActive(false);
        }
    }
}
