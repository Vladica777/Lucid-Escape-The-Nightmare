using UnityEngine;

public class FinalDoor : MonoBehaviour
{
    [Header("What rotates when opening")]
    public Transform doorPanel;

    [Header("Rotation")]
    public float openAngle = 0f;
    public float speed = 220f;

    [Header("State")]
    public bool isOpen = false;

    Quaternion startRot;
    float angle, targetAngle;

    void Awake()
    {
        if (doorPanel == null) doorPanel = transform;

        // salvam rotatia curenta (cea pe care ai setat-o tu manual, -90)
        startRot = doorPanel.rotation;

        angle = 0f;       // 0 = "nemiscat fata de starea initiala"
        targetAngle = 0f;
    }

    public void Unlock()
    {
        if (isOpen) return;
        isOpen = true;
        targetAngle = openAngle - CurrentLocalYAngle();
    }

    float CurrentLocalYAngle()
    {
        return doorPanel.localEulerAngles.y > 180f
            ? doorPanel.localEulerAngles.y - 360f
            : doorPanel.localEulerAngles.y;
    }

    void Update()
    {
        if (Mathf.Approximately(angle, targetAngle)) return;
        angle = Mathf.MoveTowards(angle, targetAngle, speed * Time.deltaTime);

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
        doorPanel.rotation = q * startRot;
    }
}