using UnityEngine;

public class CabinetNoteTrigger : MonoBehaviour
{
    public SwingDoor cabinetDoor;
    public GameObject notePanel;
    public PlayerInteractor playerInteractor;

    Collider doorCollider;

    void Awake()
    {
        doorCollider = cabinetDoor.GetComponent<Collider>();
    }

    void OnEnable()
    {
        cabinetDoor.Deschisa += OnDulapDeschis;
    }

    void OnDisable()
    {
        cabinetDoor.Deschisa -= OnDulapDeschis;
    }

    void OnDulapDeschis(SwingDoor door)
    {
        notePanel.SetActive(true);
        if (doorCollider != null) doorCollider.enabled = false;
        if (playerInteractor != null) playerInteractor.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseNote()
    {
        notePanel.SetActive(false);
        if (doorCollider != null) doorCollider.enabled = true;
        if (playerInteractor != null) playerInteractor.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}