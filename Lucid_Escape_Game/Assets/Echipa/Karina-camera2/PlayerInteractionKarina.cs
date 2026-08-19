using UnityEngine;
using System.Collections; // Am adăugat asta ca să putem folosi "temporizatorul"

public class PlayerInteractionKarina : MonoBehaviour
{
    [Header("Elemente UI Bilet")]
    public GameObject biletInceput;

    [Header("Setări Interacțiune")]
    public float interactionRange = 3f;
    public Camera playerCamera;

    [Header("Așezare Lanternă în Mână")]
    public Vector3 pozitieLanterna = new Vector3(0.4f, -0.3f, 0.6f);
    public Vector3 rotatieLanterna = new Vector3(0f, 0f, 90f);

    private bool hasKey = false;
    private bool hasFlashlight = false;
    private GameObject heldFlashlight;

    private string hoverText = "";

    void Start()
    {
        // Pornim numărătoarea inversă pentru apariția biletului (ex: 2 secunde)
        StartCoroutine(ArataBiletCuIntarziere(2f));

        // AM STERS linia cu GameHUD ca sa nu mai apara acel text jos!
    }

    // Funcția magică ce așteaptă câteva secunde
    IEnumerator ArataBiletCuIntarziere(float timp)
    {
        yield return new WaitForSeconds(timp); // Pune pauză pentru 'timp' secunde

        if (biletInceput != null)
        {
            biletInceput.SetActive(true); // Apare biletul

            // Deblocăm mouse-ul ca să poți da click pe butonul X
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (biletInceput != null && biletInceput.activeSelf && Input.GetKeyDown(KeyCode.X))
        {
            InchideBilet();
        }

        hoverText = "";

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange))
        {
            if (hit.collider.CompareTag("Interactive"))
            {
                string objectName = hit.collider.gameObject.name.ToLower();

                if (objectName.Contains("cheie") || objectName.Contains("key"))
                {
                    hoverText = "[E] Ia cheia";

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        hasKey = true;
                        Destroy(hit.collider.gameObject);
                        GameHUD.Mesaj("Ai luat o cheie ruginită!");
                    }
                }
                else if (objectName.Contains("lanterna") || objectName.Contains("flashlight"))
                {
                    hoverText = "[E] Ia lanterna";

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
                    hoverText = "[E] Deschide ușa";

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        GameHUD.Mesaj("Ai descuiat ușa! Felicitări!");

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

        if (hasFlashlight && heldFlashlight != null && Input.GetKeyDown(KeyCode.F))
        {
            Light flashlightLight = heldFlashlight.GetComponentInChildren<Light>();
            if (flashlightLight != null)
            {
                flashlightLight.enabled = !flashlightLight.enabled;
            }
        }
    }

    public void InchideBilet()
    {
        if (biletInceput != null)
        {
            biletInceput.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (hoverText != "")
        {
            GUIStyle stil = new GUIStyle();
            stil.fontSize = 24;
            stil.normal.textColor = Color.white;
            stil.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 30, 300, 50), hoverText, stil);
        }
    }
}