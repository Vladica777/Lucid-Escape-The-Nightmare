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

            // Trecerea la camera urmatoare: IesireCamera stie in ce scena
            // mergem, la ce punct de spawn, si bifeaza camera ca terminata
            // ca sa se descuie usa urmatoare din hol.
            var iesire = FindFirstObjectByType<IesireCamera>();

            if (iesire != null) iesire.Pleaca(1.5f);
            else Debug.LogWarning("Camera 3: nu exista IesireCamera in scena.");
        }
        else
        {
            GameHUD.Mesaj("Usa gresita...", 2f);
        }
    }
}