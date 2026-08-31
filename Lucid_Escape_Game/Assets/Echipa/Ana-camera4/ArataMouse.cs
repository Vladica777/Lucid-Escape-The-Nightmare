using UnityEngine;

public class ArataMouse : MonoBehaviour
{
    // OnEnable se activeaza automat in secunda in care biletul apare pe ecran
    void OnEnable()
    {
        // Deblocam cursorul din centrul ecranului
        Cursor.lockState = CursorLockMode.None;
        // Il facem vizibil
        Cursor.visible = true;
    }
}