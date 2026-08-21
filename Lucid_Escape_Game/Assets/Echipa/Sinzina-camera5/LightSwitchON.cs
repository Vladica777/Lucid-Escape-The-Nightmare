using UnityEngine;

public class LightSwitch : Interactable
{
    [Header("Luminile pe care le aprinde acest intrerupator")]
    public Light[] roomLights;

    private bool isOn = false;

    public override string GetPrompt() => isOn ? "Stinge lumina" : "Aprinde lumina";

    public override void Interact(PlayerInteractor player)
    {
        isOn = !isOn;
        RoomState.lightsOn = isOn;

        foreach (Light light in roomLights)
        {
            if (light != null)
                light.enabled = isOn;
        }
    }
}