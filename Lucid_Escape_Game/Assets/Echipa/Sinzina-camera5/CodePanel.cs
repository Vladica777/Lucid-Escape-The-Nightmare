using UnityEngine;
using TMPro;

public class CodePanel : MonoBehaviour
{
    public string correctCode = "1233";
    public FinalDoor finalDoor;
    public TMP_Text displayText;
    public KeypadZoom keypadZoom;

    private string enteredCode = "";

    public void AddDigit(string digit)
    {
        enteredCode += digit;
        UpdateDisplay();

        if (enteredCode.Length >= correctCode.Length)
        {
            CheckCode();
        }
    }

    void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = enteredCode;
    }

    void CheckCode()
    {
        if (enteredCode == correctCode)
        {
            GameHUD.Mesaj("Cod corect!");
            if (displayText != null) displayText.text = "GRANTED";
            finalDoor.Unlock();
            Invoke(nameof(ExitAfterSuccess), 1f);
        }
        else
        {
            GameHUD.Mesaj("Cod gresit.");
            if (displayText != null) displayText.text = "DENIED";
            Invoke(nameof(ResetAfterFail), 1f);
        }
    }

    void ExitAfterSuccess()
    {
        if (keypadZoom != null) keypadZoom.ExitZoom();
    }

    void ResetAfterFail()
    {
        enteredCode = "";
        UpdateDisplay();
    }
}