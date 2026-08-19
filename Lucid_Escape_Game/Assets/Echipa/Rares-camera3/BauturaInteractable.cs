using UnityEngine;

// Moștenim clasa Interactable făcută de colegul tău
public class BauturaInteractable : Interactable
{
    [Tooltip("Mesajul care apare pe ecran DUPA ce a baut.")]
    public string mesajDupaBaut = "Ai baut licoarea.";

    // Schimbăm textul care apare când jucătorul se uită la sticlă
    public override string GetPrompt()
    {
        return "Bea " + prompt; // Ex: "Bea licoarea rosie"
    }

    // Ce se întâmplă când apasă E
    public override void Interact(PlayerInteractor player)
    {
        // Afișăm mesajul folosind sistemul HUD al colegului
        GameHUD.Mesaj(mesajDupaBaut, 3f);

        // Facem sticla să dispară
        gameObject.SetActive(false);

        // AICI vom adăuga efectul de cameră mai târziu!
    }
}