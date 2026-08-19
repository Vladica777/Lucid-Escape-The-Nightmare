using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float amplitude = 0.5f; // Cât de mult urcă și coboară (0.5 metri)
    public float speed = 1f;       // Cât de repede plutește

    private Vector3 startPos;

    void Start()
    {
        // Memorează poziția inițială în care ai pus tu obiectul în scenă
        startPos = transform.position;
    }

    void Update()
    {
        // Calculează o mișcare lină în sus și în jos folosind o undă matematică
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        
        // Aplică noua poziție
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}