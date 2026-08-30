using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Setari Monstru")]
    public Transform playerTarget;
    public float viteza = 12f; 
    public float distantaInFata = 6f; 

    [Header("Efectul de Tarat")]
    public float vitezaBalans = 15f;
    public float unghiBalans = 15f;
    public float unghiAplecare = 40f; 

    // Aici tinem minte unde era in secunda trecuta
    private Vector3 pozitieAnterioara;

    void Start()
    {
        pozitieAnterioara = transform.position;
    }

    void Update()
    {
        if (playerTarget != null)
        {
            // 1. Directia ta
            Vector3 directiePrivire = playerTarget.forward;
            directiePrivire.y = 0; 
            directiePrivire.Normalize();

            // 2. Destinatia lui
            Vector3 punctInFata = playerTarget.position + (directiePrivire * distantaInFata);
            Vector3 pozitieTinta = new Vector3(punctInFata.x, transform.position.y, punctInFata.z);

            // 3. Fuge spre destinatie
            transform.position = Vector3.MoveTowards(transform.position, pozitieTinta, viteza * Time.deltaTime);

            // 4. Se uita fix in ochii tai
            Vector3 pozitiePlayer = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
            transform.LookAt(pozitiePlayer);

            // 5. TRUCUL PENTRU MERSUL CU SPATELE: 
            // Verificam daca s-a miscat fizic macar un pic fata de cadru anterior
            float balans = 0f;
            if (Vector3.Distance(transform.position, pozitieAnterioara) > 0.001f)
            {
                balans = Mathf.Sin(Time.time * vitezaBalans) * unghiBalans;
            }

            // Memoram noua pozitie pentru data viitoare
            pozitieAnterioara = transform.position;

            // 6. Aplicam aplecarea pe burta si leganatul agitat
            transform.eulerAngles = new Vector3(unghiAplecare, transform.eulerAngles.y, balans); 
        }
    }
}