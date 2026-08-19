using UnityEngine;

public class PlayerInteractionKarina : MonoBehaviour
{
    [Header("Setări Interacțiune")]
    public float interactionRange = 3f;
    public Camera playerCamera;

    [Header("Așezare Lanternă în Mână")]
    public Vector3 pozitieLanterna = new Vector3(0.4f, -0.3f, 0.6f);
    public Vector3 rotatieLanterna = new Vector3(0f, 0f, 90f);

    private bool hasKey = false;
    private bool hasFlashlight = false;
    private GameObject heldFlashlight;

    // Aici stocăm textul care va apărea când te uiți la ceva
    private string hoverText = "";

    void Start()
    {
        GameHUD.Mesaj("Este întuneric... Ia lanterna și caută cheia pentru a deschide ușa!", 5f);
    }

    void Update()
    {
        // 1. Resetăm textul în fiecare cadru ca să dispară când ne uităm în altă parte
        hoverText = "";

        // 2. Lansăm raza NON-STOP ca să vedem ce avem în fața ochilor
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange))
        {
            if (hit.collider.CompareTag("Interactive"))
            {
                string objectName = hit.collider.gameObject.name.ToLower();

                if (objectName.Contains("cheie") || objectName.Contains("key"))
                {
                    hoverText = "[E] Ia cheia"; // Setăm textul!

                    // Doar dacă apasă E în timp ce se uită la cheie, o luăm
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        hasKey = true;
                        Destroy(hit.collider.gameObject);
                        GameHUD.Mesaj("Ai luat o cheie ruginită!");
                    }
                }
                else if (objectName.Contains("lanterna") || objectName.Contains("flashlight"))
                {
                    hoverText = "[E] Ia lanterna"; // Setăm textul!

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        hasFlashlight = true;
                        heldFlashlight = hit.collider.gameObject;

                        heldFlashlight.transform.SetParent(playerCamera.transform);
                        heldFlashlight.transform.localPosition = pozitieLanterna;
                        heldFlashlight.transform.localRotation = Quaternion.Euler(rotatieLanterna);
                        Destroy(heldFlashlight.GetComponent<Collider>());

                        GameHUD.Mesaj("Ai luat lanterna! Apasă F pentru lumină.");
                    }
                }
            }
            else if (hit.collider.CompareTag("Door"))
            {
                if (hasKey)
                {
                    hoverText = "[E] Deschide ușa"; // Setăm textul!

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        GameHUD.Mesaj("Ai descuiat ușa! Felicitări!");

                        // Teleportarea: IesireCamera stie in ce scena mergem
                        // si la ce punct de spawn, si marcheaza camera
                        // terminata ca sa se descuie usa urmatoare din hol.
                        var iesire = FindFirstObjectByType<IesireCamera>();

                        if (iesire != null) iesire.Pleaca(1.5f);
                        else Debug.LogWarning("Camera 2: nu exista IesireCamera in scena.");
                    }
                }
                else
                {
                    hoverText = "Ușa este încuiată. Caută cheia.";

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        GameHUD.Mesaj("Este încuiată! Nu o poți deschide fără cheie.");
                    }
                }
            }
        }

        // Aprindem/stingem lanterna cu F
        if (hasFlashlight && heldFlashlight != null && Input.GetKeyDown(KeyCode.F))
        {
            Light flashlightLight = heldFlashlight.GetComponentInChildren<Light>();
            if (flashlightLight != null)
            {
                flashlightLight.enabled = !flashlightLight.enabled;
            }
        }
    }

    // 3. Această funcție desenează pe ecran textul "hoverText"
    void OnGUI()
    {
        // Dacă avem un text de afișat (adică nu e gol)
        if (hoverText != "")
        {
            GUIStyle stil = new GUIStyle();
            stil.fontSize = 24; // Mărimea textului
            stil.normal.textColor = Color.white; // Culoarea textului
            stil.alignment = TextAnchor.MiddleCenter;

            // Îl desenăm chiar pe centrul ecranului, puțin mai jos de crosshair
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 30, 300, 50), hoverText, stil);
        }
    }
}