using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MainHallFirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.6f;
    [SerializeField] private float sprintSpeed = 4.2f;
    [SerializeField] private float gravity = -18f;

    [Header("Mouse Look")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float minLookAngle = -75f;
    [SerializeField] private float maxLookAngle = 80f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float cameraPitch;
    private bool controlsEnabled = true;

    public bool ControlsEnabled => controlsEnabled;

    public void SetMovementEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    private void OnEnable()
    {
        controlsEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!controlsEnabled)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        LookAround();
        MovePlayer();
    }

    private void LookAround()
    {
        if (playerCamera == null)
        {
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue() * mouseSensitivity;
        cameraPitch -= mouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, minLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    private void MovePlayer()
    {
        Keyboard keyboard = Keyboard.current;
        Vector2 moveInput = Vector2.zero;

        if (keyboard != null && keyboard.aKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        if (keyboard != null && keyboard.dKey.isPressed)
        {
            moveInput.x += 1f;
        }

        if (keyboard != null && keyboard.sKey.isPressed)
        {
            moveInput.y -= 1f;
        }

        if (keyboard != null && keyboard.wKey.isPressed)
        {
            moveInput.y += 1f;
        }

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        if (characterController.isGrounded)
        {
            verticalVelocity = verticalVelocity < 0f ? -2f : verticalVelocity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        bool sprinting = keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        float currentSpeed = sprinting
            ? sprintSpeed
            : walkSpeed;

        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }
}
