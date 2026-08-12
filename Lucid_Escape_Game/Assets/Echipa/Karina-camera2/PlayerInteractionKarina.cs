using UnityEngine;

public class PlayerInteractionKarina : MonoBehaviour
{
    [Header("Setări Interacțiune")]
    public float interactionRange = 3f;
    public Camera playerCamera;

    private bool hasKey = false;
    private bool hasFlashlight = false;
    private GameObject heldFlashlight;

    void Update()
    {
        // Verificăm dacă tasta E funcționează măcar
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("✅ TASTA E A FOST APĂSATĂ!"); // Dacă nici asta nu apare, e problemă de Unity

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange))
            {
                Debug.Log("🔍 Raza a lovit: " + hit.collider.name + " | Tag: " + hit.collider.tag);

                if (hit.collider.CompareTag("Interactive"))
                {
                    string objectName = hit.collider.gameObject.name.ToLower();

                    if (objectName.Contains("cheie") || objectName.Contains("key"))
                    {
                        hasKey = true;
                        Destroy(hit.collider.gameObject);
                        Debug.Log("🔑 Ai luat cheia!");
                    }
                    else if (objectName.Contains("lanterna") || objectName.Contains("flashlight"))
                    {
                        hasFlashlight = true;
                        heldFlashlight = hit.collider.gameObject;

                        heldFlashlight.transform.SetParent(playerCamera.transform);
                        heldFlashlight.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
                        heldFlashlight.transform.localRotation = Quaternion.identity;

                        Destroy(heldFlashlight.GetComponent<Collider>());
                        Debug.Log("🔦 Ai luat lanterna!");
                    }
                }
            }
            else
            {
                Debug.Log("❌ Raza nu a lovit nimic în primii 3 metri!");
            }
        }

        if (hasFlashlight && heldFlashlight != null && Input.GetKeyDown(KeyCode.F))
        {
            Light flashlightLight = heldFlashlight.GetComponentInChildren<Light>();
            if (flashlightLight != null) flashlightLight.enabled = !flashlightLight.enabled;
        }
    }
}