using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f; 
    
    // Setări noi pentru săritură
    public float jumpForce = 8f; // Forța maximă a săriturii (dacă ții apăsat)
    public float gravity = -20f; // Gravitația (mai mare ca să cazi înapoi realist, nu ca pe lună)

    private float xRotation = 0f;
    private Transform playerCamera;
    private CharacterController controller;
    
    private float velocityY = 0f; // Memorează viteza cu care urci/cazi

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- 1. ROTIREA CAMEREI ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); 

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- 2. MIȘCAREA ORIZONTALĂ (W, A, S, D) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Înmulțim direct aici direcția cu viteza
        Vector3 move = (transform.right * x + transform.forward * z) * speed;

        // --- 3. GRAVITAȚIA ȘI SĂRITURA DINAMICĂ ---
        
        // Dacă suntem pe pământ, oprim gravitația să nu ne tragă la infinit în jos
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f; 
        }

        // Când APESI Space -> Începe săritura maximă
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocityY = jumpForce;
        }

        // Când ELIBEREZI Space -> Magia dinamică!
        // Dacă te afli în aer și încă urci (velocityY > 0), tăiem din viteză
        if (Input.GetKeyUp(KeyCode.Space) && velocityY > 0)
        {
            velocityY *= 0.4f; // Cu cât numărul e mai mic, cu atât se oprește mai brusc
        }

        // Aplicăm mereu gravitația ca să fim trași în jos
        velocityY += gravity * Time.deltaTime;

        // Punem viteza verticală în mișcarea finală
        move.y = velocityY;

        // Mutăm caracterul cu totul
        controller.Move(move * Time.deltaTime);
    }
}