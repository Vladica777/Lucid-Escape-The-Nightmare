using UnityEngine;
using System.Collections;

public class InamicPatrula : MonoBehaviour
{
    [Header("Traseu")]
    public Transform punctulA;
    public Transform punctulB;

    [Header("Setari")]
    public float viteza = 1.5f;
    public float timpAsteptare = 5f; // Stă 5 secunde pe loc

    private Transform punctCurent;
    private bool sePlimba = false;
    private Animator animator;

    void Start()
    {
        // Facem rost de Animatorul monstrului
        animator = GetComponent<Animator>();

        punctCurent = punctulB;

        // Începe direct cu pauza de 5 secunde la Punctul A, apoi pleacă
        StartCoroutine(AsteaptaSiPleaca());
    }

    void Update()
    {
        // Dacă are voie să meargă, execută deplasarea
        if (sePlimba)
        {
            transform.position = Vector3.MoveTowards(transform.position, punctCurent.position, viteza * Time.deltaTime);

            Vector3 directie = punctCurent.position - transform.position;
            directie.y = 0;
            if (directie.magnitude > 0.1f)
            {
                Quaternion rotatieTinta = Quaternion.LookRotation(directie);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotatieTinta, 5f * Time.deltaTime);
            }

            // Verificăm dacă a ajuns la capăt (aproape de punct)
            if (Vector3.Distance(transform.position, punctCurent.position) < 0.2f)
            {
                SchimbaPunctul();

                // Când a ajuns, pornim iar temporizatorul de pauză!
                StartCoroutine(AsteaptaSiPleaca());
            }
        }
    }

    void SchimbaPunctul()
    {
        if (punctCurent == punctulA)
            punctCurent = punctulB;
        else
            punctCurent = punctulA;
    }

    // Aici e magia care îl pune pe pauză
    IEnumerator AsteaptaSiPleaca()
    {
        sePlimba = false; // Îi tăiem accelerația
        animator.SetBool("Merge", false); // Îi spunem să treacă pe animația de Idle

        // Așteptăm 5 secunde (poți schimba numărul de sus din Setări)
        yield return new WaitForSeconds(timpAsteptare);

        sePlimba = true; // Îi dăm voie să meargă din nou
        animator.SetBool("Merge", true); // Îi spunem să treacă pe animația de Mers
    }
}