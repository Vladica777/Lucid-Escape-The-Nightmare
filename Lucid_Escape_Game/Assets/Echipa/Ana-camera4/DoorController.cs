using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform player; // Aici vom pune Player-ul
    public float interactionDistance = 3f; // De la ce distanță merge
    public float openAngle = -90f; // Unghiul de deschidere (pune 90 dacă se deschide invers)
    public float smooth = 3f; // Viteza cu care se deschide

    private bool isOpen = false;
    private Quaternion defaultRotation;
    private Quaternion openRotation;

    void Start()
    {
        // Salvăm rotația de început (ușa închisă)
        defaultRotation = transform.localRotation;
        
        // Calculăm cum va arăta ușa când e deschisă
        openRotation = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, openAngle, 0));

        // Dacă uităm să îi dăm Player-ul, îl găsește el automat
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        }
    }

    void Update()
    {
        // 1. Verificăm dacă jucătorul este suficient de aproape de ușă
        if (Vector3.Distance(transform.position, player.position) <= interactionDistance)
        {
            // 2. Dacă e aproape și apasă tasta E
            if (Input.GetKeyDown(KeyCode.E))
            {
                isOpen = !isOpen; // Schimbă starea (dacă e deschisă o închide și invers)
            }
        }

        // 3. Animăm ușa lin spre starea ei (Deschisă sau Închisă)
        if (isOpen)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, openRotation, Time.deltaTime * smooth);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, defaultRotation, Time.deltaTime * smooth);
        }
    }
}