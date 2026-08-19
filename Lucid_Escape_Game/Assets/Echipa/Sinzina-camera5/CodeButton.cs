using UnityEngine;

public class CodeButton : Interactable
{
    public string digit;
    public CodePanel codePanel;

    public override string GetPrompt() => "";

    public override void Interact(PlayerInteractor player)
    {
        codePanel.AddDigit(digit);
    }
}