using UnityEngine;

public class EfectAmeteala : MonoBehaviour
{
    public bool esteAmetit = false;

    [Header("Setari Camera")]
    private Camera cam;
    private float fovNormal;

    [Header("Setari Lumina (TRAGE LUMINA AICI IN UNITY!)")]
    public Light luminaCamerei;
    public Color culoareVerdeToxic = new Color(0.1f, 0.4f, 0.1f);

    void Start()
    {
        cam = GetComponent<Camera>();
        fovNormal = cam.fieldOfView;
    }

    void Update()
    {
        if (esteAmetit == true)
        {
            cam.fieldOfView = fovNormal + Mathf.Sin(Time.time * 3f) * 8f;

            // OPRIM lumina de la cer și îi spunem să folosească o culoare solidă!
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

            // Acum îi dăm culoarea noastră verde pe pereți
            RenderSettings.ambientLight = culoareVerdeToxic;

            if (luminaCamerei != null)
            {
                luminaCamerei.color = culoareVerdeToxic;

                // Aici controlezi intensitatea! 
                luminaCamerei.intensity = Random.Range(0.1f, 0.6f);
            }
        }
    }
}