using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadMouseInteraction : MonoBehaviour
{
    public Camera zoomCamera;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || zoomCamera == null || !zoomCamera.gameObject.activeInHierarchy) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = zoomCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                var button = hit.collider.GetComponentInParent<CodeButton>();
                if (button != null)
                {
                    button.PressFromMouse();
                }
            }
        }
    }
}