using UnityEngine;

public class BauturaInteractable : Interactable
{
    public string mesajDupaBaut = "Ai baut licoarea.";
    public static bool aAlesOBautura = false;

    public override string GetPrompt()
    {
        if (aAlesOBautura == true) return "";
        return "Bea " + prompt;
    }

    public override void Interact(PlayerInteractor player)
    {
        if (aAlesOBautura == true) return;
        aAlesOBautura = true;

        GameHUD.Mesaj(mesajDupaBaut, 3f);

        // Pornim amețeala
        FindFirstObjectByType<EfectAmeteala>().esteAmetit = true;

        // Întârziem ascunderea cu 0.1 secunde (Asta rezolvă blocajul!)
        Invoke("AscundeSticla", 0.1f);
    }

    // Funcția care se apelează după 0.1 secunde
    void AscundeSticla()
    {
        gameObject.SetActive(false);
    }
}