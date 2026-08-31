using UnityEngine;

public class DoarCrosshair : MonoBehaviour
{
    [Header("Aspect (La fel ca la echipa)")]
    public int crosshairSize = 6;
    public Color crosshairColor = new Color(1f, 1f, 1f, 0.75f);

    void OnGUI()
    {
        // Aceasta este formula exacta din GameHUD
        float s = crosshairSize;
        var r = new Rect((Screen.width - s) / 2f, (Screen.height - s) / 2f, s, s);
        var vechi = GUI.color;

        GUI.color = crosshairColor;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = vechi;
    }
}