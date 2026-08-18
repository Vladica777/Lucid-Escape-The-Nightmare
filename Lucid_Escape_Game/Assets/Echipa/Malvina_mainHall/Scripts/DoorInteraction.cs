using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField] private int doorNumber = 1;
    [SerializeField] private string selectedMessage = "You chose Door 01";

    [Header("Transition")]
    [SerializeField] private MainHallInteractionController transitionController;

    public int DoorNumber => doorNumber;
    public string SelectedMessage => selectedMessage;
    public string InteractionPrompt => "[E] Interact";

    private void Awake()
    {
        if (transitionController == null)
        {
            transitionController = FindFirstObjectByType<MainHallInteractionController>();
        }
    }

    public void Interact(PlayerInteraction interactor)
    {
        if (transitionController != null)
        {
            transitionController.PlayDoorTransition(this, interactor);
        }
    }
}
