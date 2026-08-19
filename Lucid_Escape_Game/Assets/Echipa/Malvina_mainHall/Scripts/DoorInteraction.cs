using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField] private int doorNumber = 1;
    [SerializeField] private string selectedMessage = "You chose Door 01";

    [Header("Nivelul din spatele usii")]
    [Tooltip("Numele scenei, exact ca fisierul, fara .unity. Gol = usa nu " +
             "duce nicaieri inca si arata doar mesajul, ca inainte.")]
    [SerializeField] private string sceneName = "";

    [Tooltip("Id-ul PunctSpawn din nivel unde apare jucatorul.")]
    [SerializeField] private string spawnId = "intrare";

    [Header("Deblocare")]
    [Tooltip("Camera care trebuie terminata ca sa se deschida usa asta: " +
             "camera2 ... camera6. Gol = deschisa de la inceput.")]
    [SerializeField] private string requiresRoom = "";

    [SerializeField] private string lockedPrompt = "[E] Usa nu se deschide inca";

    [Header("Transition")]
    [SerializeField] private MainHallInteractionController transitionController;

    public int DoorNumber => doorNumber;
    public string SelectedMessage => selectedMessage;
    public string SceneName => sceneName;
    public string SpawnId => spawnId;
    public string RequiresRoom => requiresRoom;

    /// Usa e deschisa? Un requiresRoom gol inseamna fara conditie.
    public bool IsUnlocked => Progres.EIndeplinita(requiresRoom);

    public string InteractionPrompt => IsUnlocked ? "[E] Interact" : lockedPrompt;

    private void Awake()
    {
        if (transitionController == null)
        {
            transitionController = FindFirstObjectByType<MainHallInteractionController>();
        }
    }

    public void Interact(PlayerInteraction interactor)
    {
        if (!IsUnlocked)
        {
            Debug.Log($"Door {doorNumber}: incuiata, cere '{requiresRoom}' terminata.");
            return;
        }

        if (transitionController != null)
        {
            transitionController.PlayDoorTransition(this, interactor);
        }
    }
}
