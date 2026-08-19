using UnityEngine;

public class UsaInteractable : Interactable
{
    [Tooltip("Bifează asta doar la ușa pe care vrei să o nimerească.")]
    public bool esteCorecta = false;

    public override string GetPrompt()
    {
        return prompt;
    }

    public override void Interact(PlayerInteractor player)
    {
        if (esteCorecta)
        {
            GameHUD.Mesaj("Ai gasit usa corecta!", 3f);
            // Aici vei pune codul ca să se deschidă ușa sau să treacă la următoarea cameră
        }
        else
        {
            GameHUD.Mesaj("Usa gresita...", 2f);
        }
    }
}