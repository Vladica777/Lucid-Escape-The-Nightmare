using UnityEngine;
using UnityEngine.InputSystem; // Aici e magia: îi spunem lui Unity să folosească noul lui sistem

public class FirstController : MonoBehaviour
{
    [Header("Setări Mișcare")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 0.2f; // E puțin mai mică pentru noul sistem
    public float gravity = -9.81f;

    [Header("Referințe")]
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Verificăm să existe tastatură și mouse ca să nu dea erori
        if (Mouse.current == null || Keyboard.current == null) return;

        // 1. PRIVIREA DIN MOUSE (Folosim noul sistem direct)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        // 2. MIȘCAREA DIN TASTE (W A S D)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = 0f;
        float z = 0f;

        // Citim tastele direct
        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 3. GRAVITAȚIA
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}