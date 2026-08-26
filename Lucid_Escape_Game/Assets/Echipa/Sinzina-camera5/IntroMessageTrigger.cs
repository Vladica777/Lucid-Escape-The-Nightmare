using UnityEngine;

public class IntroMessageTrigger : MonoBehaviour
{
    public GameObject introPanel;
    public PlayerInteractor playerInteractor;

    void Start()
    {
        introPanel.SetActive(true);
        if (playerInteractor != null) playerInteractor.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseIntro()
    {
        introPanel.SetActive(false);
        if (playerInteractor != null) playerInteractor.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Destroy(gameObject); // panelul nu mai revine niciodata dupa asta
    }
}