using UnityEngine;

public class LightSwitch : Interactable
{
    [Header("Luminile pe care le aprinde acest intrerupator")]
    public Light[] roomLights;

    [Header("Optional: dezactiveaza intrerupatorul dupa prima folosire")]
    public bool oneTimeUse = true;

    private bool isOn = false;

    public override string GetPrompt() => "";

    public override void Interact(PlayerInteractor player)
    {
        isOn = true;
        RoomState.lightsOn = true;

        foreach (Light light in roomLights)
        {
            if (light != null)
                light.enabled = true;
        }

        if (oneTimeUse)
        {
            enabled = false;
        }
    }
}