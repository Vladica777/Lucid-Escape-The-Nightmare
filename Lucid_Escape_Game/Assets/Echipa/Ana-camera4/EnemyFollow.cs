using UnityEngine;

public class EnemyWander : MonoBehaviour
{
    [Header("Setari Plimbare (In Picioare)")]
    public Transform playerTarget;
    public float viteza = 3f; 
    public float razaPlimbare = 15f; 
    public float timpAsteptare = 3f;
    public float distantaMinimaVizibila = 7f;

    [Header("Setari Animatie")]
    public Animator anim;
    
    [Tooltip("Cum se numeste parametrul din Animator-ul Karinei?")]
    public string numeParametru = "Merge";
    public bool esteParametruBool = true;

    private Vector3 destinatieCurenta;
    private float timerAsteptare;
    private bool seMisca = false;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        AlegeDestinatieNoua();
    }

    void Update()
    {
        // FORTAM inamicul sa stea perfect drept in picioare
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // SCUTUL ANTI-FANTOMA: Daca se apropie prea mult de tine, se opreste brusc
        if (seMisca && playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) < 2f)
        {
            seMisca = false;
            timerAsteptare = timpAsteptare;
        }

        if (seMisca)
        {
            // MISCAREA LINA MATEMATICA (fara fizica)
            Vector3 punctTinta = new Vector3(destinatieCurenta.x, transform.position.y, destinatieCurenta.z);
            transform.position = Vector3.MoveTowards(transform.position, punctTinta, viteza * Time.deltaTime);
            
            transform.LookAt(punctTinta);

            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(destinatieCurenta.x, 0, destinatieCurenta.z)) < 0.5f)
            {
                seMisca = false; 
                timerAsteptare = timpAsteptare; 
            }
        }
        else
        {
            // Faza de asteptare: Se uita la tine
            if (playerTarget != null)
            {
                Vector3 pozitiePlayer = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
                transform.LookAt(pozitiePlayer);
            }

            timerAsteptare -= Time.deltaTime;
            
            if (timerAsteptare <= 0)
            {
                AlegeDestinatieNoua();
            }
        }

        // --- SISTEMUL DE ANIMATIE ---
        if (anim != null)
        {
            if (esteParametruBool)
            {
                anim.SetBool(numeParametru, seMisca);
            }
            else
            {
                anim.SetFloat(numeParametru, seMisca ? viteza : 0f);
            }
        }
    }

    void AlegeDestinatieNoua()
    {
        Vector3 centru = playerTarget != null ? playerTarget.position : transform.position;
        Vector2 punctRandom = Vector2.zero;
        float distantaFataDeTine = 0f;
        int salvari = 0;
        
        do
        {
            punctRandom = Random.insideUnitCircle * razaPlimbare;
            distantaFataDeTine = Vector2.Distance(Vector2.zero, punctRandom);
            salvari++;
        } 
        while (distantaFataDeTine < distantaMinimaVizibila && salvari < 20);

        destinatieCurenta = new Vector3(centru.x + punctRandom.x, transform.position.y, centru.z + punctRandom.y);
        seMisca = true;
    }
}