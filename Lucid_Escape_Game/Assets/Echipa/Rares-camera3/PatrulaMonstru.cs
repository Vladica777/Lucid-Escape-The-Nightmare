using UnityEngine;

public class PatrulaMonstru : MonoBehaviour
{
    [Header("Traseu")]
    public Transform punctulA;
    public Transform punctulB;
    public float viteza = 1.5f; // Cat de repede se taraste

    private Transform tintaCurenta;

    void Start()
    {
        // La inceput, pleaca spre punctul A
        tintaCurenta = punctulA;

        // Se intoarce cu fata spre punctul A
        transform.LookAt(new Vector3(tintaCurenta.position.x, transform.position.y, tintaCurenta.position.z));
    }

    void Update()
    {
        // Se misca spre tinta
        transform.position = Vector3.MoveTowards(transform.position, tintaCurenta.position, viteza * Time.deltaTime);

        // Daca a ajuns foarte aproape de tinta
        if (Vector3.Distance(transform.position, tintaCurenta.position) < 0.2f)
        {
            // Schimba directia (daca era la A, merge la B, si invers)
            if (tintaCurenta == punctulA)
                tintaCurenta = punctulB;
            else
                tintaCurenta = punctulA;

            // Se intoarce cu fata spre noua tinta
            transform.LookAt(new Vector3(tintaCurenta.position.x, transform.position.y, tintaCurenta.position.z));
        }
    }
}