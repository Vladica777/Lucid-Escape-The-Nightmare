using UnityEngine;
using UnityEngine.UI;

public class ParkourScript : MonoBehaviour
{
    [Header("Setari Parkour")]
    public Transform startPoint; 

    [Header("UI - Elemente de pe ecran")]
    public GameObject startMenu;      
    public Text statusText;           

    [Tooltip("Sare peste meniul de START si incepe direct. PlayerMovement " +
             "blocheaza cursorul la pornire, deci butonul oricum nu se putea " +
             "apasa cu mouse-ul.")]
    public bool pornesteDirect = true;

    private CharacterController controller;
    private bool isRespawning = false; // Ne asigura ca nu murim de 100 de ori pe secunda

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false); 
        }

        if (pornesteDirect)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (startMenu != null)
        {
            startMenu.SetActive(false);
        }
        
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = ""; 
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Daca tocmai ne dam respawn, ignora coliziunile ca sa nu intram in bucla
        if (isRespawning) return; 

        if (hit.gameObject.name.Contains("floor") || hit.gameObject.name.Contains("Podea"))
        {
            Respawn();
        }
    }

    void Respawn()
    {
        isRespawning = true; // Activam "invincibilitatea" temporara
        Debug.Log("Ai atins podeaua! Te intorci la start...");

        if (statusText != null)
        {
            statusText.text = "AI CĂZUT!";
            // Sterge textul de pe ecran dupa 2 secunde (2f)
            Invoke("AscundeText", 2f); 
        }

        if (controller != null)
        {
            controller.enabled = false;
            transform.position = startPoint.position;
            controller.enabled = true;
        }
        else
        {
            transform.position = startPoint.position;
        }

        // Oprim "invincibilitatea" dupa jumatate de secunda ca sa poti juca normal
        Invoke("ResetRespawn", 0.5f);
    }

    // Functie care curata textul
    void AscundeText()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    // Functie care te lasa sa mori din nou
    void ResetRespawn()
    {
        isRespawning = false;
    }
}