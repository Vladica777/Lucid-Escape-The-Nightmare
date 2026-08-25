using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    private Light becul;
    public float minIntensity = 0.2f; // Lumina la minim
    public float maxIntensity = 1.5f; // Lumina la maxim
    public float vitezaFlicker = 0.1f; // Cat de des clipeste
    
    private float timer;

    void Start()
    {
        becul = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Daca a trecut timpul, schimba intensitatea la intamplare
        if (timer > vitezaFlicker)
        {
            becul.intensity = Random.Range(minIntensity, maxIntensity);
            timer = 0; // Resetam cronometrul
        }
    }
}