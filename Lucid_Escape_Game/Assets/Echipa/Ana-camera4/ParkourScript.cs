using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParkourManager : MonoBehaviour
{
    [Header("UI & Start Menu")]
    public GameObject startMenu;      // Fereastra cu Butonul
    public Text statusTextOnScreen;   // Textul care apare ÎN TIMPUL jocului

    [Header("Gameplay")]
    public Transform startPoint;      // Locul de respawn
    
    private CharacterController controller;
    private bool isRespawning = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // 1. Arătăm Meniul de Start și ascundem textul din joc
        startMenu.SetActive(true);
        if(statusTextOnScreen != null) statusTextOnScreen.gameObject.SetActive(false);

        // 2. Punem jocul pe pauză și deblocăm mouse-ul ca să putem apăsa pe buton
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Funcția asta va fi apelată CÂND APEȘI BUTONUL DE START
    public void StartGame()
    {
        // Ascundem meniul și pornim jocul
        startMenu.SetActive(false);
        
        if(statusTextOnScreen != null) 
        {
            statusTextOnScreen.gameObject.SetActive(true);
            statusTextOnScreen.text = "Găsește ieșirea! Nu atinge podeaua!";
        }

        // Scoatem jocul de pe pauză și ascundem mouse-ul
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Funcția care verifică dacă ne-am lovit de ceva
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isRespawning) return; // Dacă deja murim, nu facem nimic

        // DACĂ ATINGE PODEAUA (Podea_Mare)
        if (hit.gameObject.name.Contains("Podea")) 
        {
            StartCoroutine(RespawnPlayer());
        }

        // DACĂ AJUNGE LA UȘĂ (Asigură-te că ușa ta are cuvântul "Door" sau "Usa" în nume)
        if (hit.gameObject.name.Contains("Door") || hit.gameObject.name.Contains("Usa")) 
        {
            if(statusTextOnScreen != null) statusTextOnScreen.text = "AI CÂȘTIGAT! Camera următoare se deblochează...";
        }
    }

    IEnumerator RespawnPlayer()
    {
        isRespawning = true;
        if(statusTextOnScreen != null) statusTextOnScreen.text = "AI CĂZUT! O iei de la capăt...";
        
        // Oprim controllerul o fracțiune de secundă pentru a-l putea teleporta
        controller.enabled = false;
        transform.position = startPoint.position;
        controller.enabled = true;

        // Așteptăm 2 secunde ca jucătorul să proceseze greșeala
        yield return new WaitForSeconds(2f);
        
        if(statusTextOnScreen != null) statusTextOnScreen.text = "Sari din obiect în obiect! Nu atinge podeaua!";
        isRespawning = false;
    }
}