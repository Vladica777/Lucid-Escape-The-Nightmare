using UnityEngine;
using UnityEngine.InputSystem;

public class NoTargetFeedback : MonoBehaviour
{
    public PlayerInteractor interactor;

    [TextArea]
    public string mesajLumina = "Aici esti orb, dar la intrare nu ai fost...";

    void Awake()
    {
        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || interactor == null) return;
        if (GameHUD.Blocking) return;

        if (kb.eKey.wasPressedThisFrame && interactor.Current == null)
        {
            if (!RoomState.lightsOn)
            {
                GameHUD.Mesaj(mesajLumina);
            }
        }
    }
}