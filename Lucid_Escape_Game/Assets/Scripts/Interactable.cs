using UnityEngine;

/// Baza pentru orice obiect cu care jucatorul poate interactiona apasand E.
/// Obiectul are nevoie si de un Collider (nu trigger) ca sa fie prins de raycast.
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Textul afisat pe ecran cand privesti obiectul.")]
    public string prompt = "Interactioneaza";

    /// Poate fi dezactivat temporar (usa incuiata, item deja luat).
    public virtual bool CanInteract => enabled;

    /// Ce se afiseaza pe ecran. Suprascrie daca textul depinde de stare.
    public virtual string GetPrompt() => prompt;

    /// Apelat cand jucatorul apasa E privind obiectul.
    public abstract void Interact(PlayerInteractor player);
}
