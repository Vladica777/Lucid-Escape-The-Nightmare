using System.Collections;
using UnityEngine;

public class BiletLabirint : MonoBehaviour
{
    [Header("Biletul de pe ecran")]
    public GameObject biletUI;

    void Start()
    {
        if (biletUI != null)
        {
            biletUI.SetActive(false);
        }

        StartCoroutine(ApareDupaPauza());
    }

    IEnumerator ApareDupaPauza()
    {
        yield return new WaitForSeconds(1f);

        biletUI.SetActive(true);

        // DEBLOCĂM CURSORUL ca să poți da click pe X
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void InchideBilet()
    {
        // Ascunde întregul Canvas, nu doar imaginea biletului
        gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}