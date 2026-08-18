using UnityEngine;
using TMPro;

public class CodePanel : MonoBehaviour
{
    public string correctCode = "2233";
    public FinalDoor finalDoor;
    public TMP_Text displayText;

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
            GameHUD.Mesaj("Correct code!");
            if (displayText != null) displayText.text = "GRANTED";
            finalDoor.Unlock();
        }
        else
        {
            GameHUD.Mesaj("Wrong code.");
            if (displayText != null) displayText.text = "DENIED";
            Invoke(nameof(ClearDisplay), 1f);
        }
    }

    void ClearDisplay()
    {
        enteredCode = "";
        UpdateDisplay();
    }
}