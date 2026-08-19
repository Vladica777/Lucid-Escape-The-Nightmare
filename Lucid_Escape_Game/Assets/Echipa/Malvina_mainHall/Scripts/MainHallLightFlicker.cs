using UnityEngine;

public class MainHallLightFlicker : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private float baseIntensity = 1.6f;
    [SerializeField] private float flickerAmount = 0.12f;
    [SerializeField] private float flickerSpeed = 10f;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
        }
    }

    private void Update()
    {
        if (targetLight == null)
        {
            return;
        }

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.35f);
        targetLight.intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
    }
}
