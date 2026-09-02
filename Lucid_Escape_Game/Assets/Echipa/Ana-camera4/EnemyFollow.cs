using UnityEngine;

public class EnemyWander : MonoBehaviour
{
    [Header("Setari Perimetru Fix")]
    public Transform centrulZonei; 
    public float razaZonei = 8f; 

    [Header("Setari Comportament")]
    public Transform playerTarget; 
    public float viteza = 3.5f; 
    public float timpAsteptare = 3f;

    [Header("Setari Animatie")]
    public Animator anim;
    public string numeParametru = "Merge";
    public bool esteParametruBool = true;

    private Vector3 destinatieCurenta;
    private float timerAsteptare;
    private bool seMisca = false;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (centrulZonei == null) centrulZonei = this.transform; 
        AlegeDestinatieNoua();
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        if (seMisca && playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) < 1.5f)
        {
            seMisca = false;
            timerAsteptare = timpAsteptare;
        }

        if (seMisca)
        {
            Vector3 punctTinta = new Vector3(destinatieCurenta.x, transform.position.y, destinatieCurenta.z);
            Vector3 directie = (punctTinta - transform.position).normalized;
            
            transform.LookAt(punctTinta);

            // --- SISTEMUL "OCHI CU LASER" ---
            // Tragem o raza din pieptul lui (Y + 1 metru in sus), un metru si un pic in fata
            Vector3 origineaLaserului = transform.position + Vector3.up * 1f;
            
            // Daca raza se loveste de ceva inainte cu 1.5 metri...
            if (Physics.Raycast(origineaLaserului, directie, out RaycastHit hit, 1.5f))
            {
                // Verificam daca NU e podeaua si NU e jucatorul
                if (!hit.collider.CompareTag("Player") && 
                    !hit.collider.name.ToLower().Contains("floor") && 
                    !hit.collider.name.ToLower().Contains("podea"))
                {
                    // A vazut patul! Renunta la drumul asta si alege altul instant
                    AlegeDestinatieNoua();
                    return; // Se opreste din miscare in secunda asta
                }
            }

            // Daca drumul e curat, continua sa se miste fluid
            transform.position = Vector3.MoveTowards(transform.position, punctTinta, viteza * Time.deltaTime);

            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(destinatieCurenta.x, 0, destinatieCurenta.z)) < 0.5f)
            {
                seMisca = false; 
                timerAsteptare = timpAsteptare; 
            }
        }
        else
        {
            if (playerTarget != null)
            {
                Vector3 pozitiePlayer = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
                transform.LookAt(pozitiePlayer);
            }

            timerAsteptare -= Time.deltaTime;
            if (timerAsteptare <= 0) AlegeDestinatieNoua();
        }

        if (anim != null)
        {
            if (esteParametruBool) anim.SetBool(numeParametru, seMisca);
            else anim.SetFloat(numeParametru, seMisca ? viteza : 0f);
        }
    }

    void AlegeDestinatieNoua()
    {
        Vector2 punctRandom = Random.insideUnitCircle * razaZonei;
        destinatieCurenta = new Vector3(centrulZonei.position.x + punctRandom.x, transform.position.y, centrulZonei.position.z + punctRandom.y);
        seMisca = true;
    }
}