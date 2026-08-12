using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f; 
    
    // Setări noi pentru săritură
    public float jumpForce = 8f;
    public float gravity = -20f;

    private float xRotation = 0f;
    private Transform playerCamera;
    private CharacterController controller;
    
    private float velocityY = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); 

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z) * speed;

        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f; 
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocityY = jumpForce;
        }

        if (Input.GetKeyUp(KeyCode.Space) && velocityY > 0)
        {
            velocityY *= 0.4f;
        }

        velocityY += gravity * Time.deltaTime;

        move.y = velocityY;
        
        controller.Move(move * Time.deltaTime);
    }
}