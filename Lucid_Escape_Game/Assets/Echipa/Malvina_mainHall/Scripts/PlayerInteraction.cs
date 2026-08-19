using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private MainHallInteractionController uiController;

    [Header("Raycast")]
    [SerializeField] private float interactionDistance = 3.2f;
    [SerializeField] private LayerMask interactionMask = ~0;

    private IInteractable currentInteractable;
    private bool interactionsEnabled = true;

    public void SetInteractionsEnabled(bool enabled)
    {
        interactionsEnabled = enabled;

        if (!enabled)
        {
            currentInteractable = null;
            SetPromptVisible(false);
        }
    }

    private void Awake()
    {
        if (viewCamera == null)
        {
            viewCamera = GetComponentInChildren<Camera>();
        }

        if (uiController == null)
        {
            uiController = FindFirstObjectByType<MainHallInteractionController>();
        }
    }

    private void Update()
    {
        if (!interactionsEnabled)
        {
            return;
        }

        UpdateTarget();

        Keyboard keyboard = Keyboard.current;
        if (currentInteractable != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            currentInteractable.Interact(this);
        }
    }

    private void UpdateTarget()
    {
        currentInteractable = null;

        if (viewCamera != null)
        {
            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore))
            {
                currentInteractable = FindInteractable(hit.collider);
            }
        }

        SetPromptVisible(currentInteractable != null);
    }

    private static IInteractable FindInteractable(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private void SetPromptVisible(bool visible)
    {
        if (uiController != null)
        {
            uiController.SetPromptVisible(visible, currentInteractable != null ? currentInteractable.InteractionPrompt : "[E] Interact");
        }
    }
}
