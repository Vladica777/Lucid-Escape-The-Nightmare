using UnityEngine;
using UnityEngine.InputSystem;

/// Miscare first-person: WASD, mouse look, Space, Shift pentru sprint.
/// Foloseste Input System nou (proiectul e pe activeInputHandler = 1).
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera copil a jucatorului. Daca e gol, o cauta automat.")]
    public Transform cameraPivot;
    public float mouseSensitivity = 0.12f;
    public float maxLookAngle = 85f;

    [Header("Miscare")]
    public float walkSpeed = 3.2f;
    public float sprintSpeed = 5.5f;
    public float jumpHeight = 1.1f;
    public float gravity = -18f;

    CharacterController cc;
    float pitch;              // rotatia verticala acumulata a camerei
    float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraPivot == null && Camera.main != null && Camera.main.transform.IsChildOf(transform))
            cameraPivot = Camera.main.transform;
    }

    void OnEnable() => BlocheazaCursorul(true);
    void OnDisable() => BlocheazaCursorul(false);

    void Update()
    {
        // cand e deschisa o foaie, jucatorul nu se misca si nu se uita in jur,
        // dar gravitatia se aplica in continuare ca sa nu se acumuleze
        if (GameHUD.Blocking)
        {
            AplicaGravitatia();
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            return;
        }

        Priveste();
        Misca();
    }

    void Priveste()
    {
        var mouse = Mouse.current;
        if (mouse == null || cameraPivot == null) return;

        // delta de mouse e deja in pixeli/frame - NU se inmulteste cu deltaTime
        Vector2 d = mouse.delta.ReadValue() * mouseSensitivity;

        transform.Rotate(Vector3.up, d.x, Space.World);

        pitch = Mathf.Clamp(pitch - d.y, -maxLookAngle, maxLookAngle);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Misca()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 dir = transform.right * x + transform.forward * z;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float viteza = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        if (cc.isGrounded && kb.spaceKey.wasPressedThisFrame)
            verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);

        AplicaGravitatia();

        cc.Move((dir * viteza + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    void AplicaGravitatia()
    {
        // valoare mica negativa cand sta pe sol, ca sa ramana lipit de podea
        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity += gravity * Time.deltaTime;
    }

    public static void BlocheazaCursorul(bool blocat)
    {
        Cursor.lockState = blocat ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !blocat;
    }
}
