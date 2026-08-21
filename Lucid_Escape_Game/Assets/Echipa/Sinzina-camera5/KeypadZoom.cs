using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadZoom : Interactable
{
    public Camera zoomCamera;
    public Camera playerCamera;
    public PlayerController playerController;

    private bool zoomedIn = false;
    public bool ZoomedIn => zoomedIn;

    public override string GetPrompt() => zoomedIn ? "Inchide panoul" : "Priveste panoul";

    public override void Interact(PlayerInteractor player)
    {
        if (zoomedIn)
            ExitZoom();
        else
            EnterZoom();
    }

    void EnterZoom()
    {
        zoomedIn = true;
        zoomCamera.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);

        PlayerController.BlocheazaCursorul(false);
        if (playerController != null) playerController.enabled = false;
    }

    public void ExitZoom()
    {
        zoomedIn = false;
        zoomCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        PlayerController.BlocheazaCursorul(true);
        if (playerController != null) playerController.enabled = true;
    }

    void Update()
    {
        if (!zoomedIn) return;

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            ExitZoom();
        }
    }
}