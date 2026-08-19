using UnityEngine;

/// Marcaj de aparitie. Se pune pe un obiect gol, oriunde in scena.
///
/// La inceputul scenei, punctul a carui id se potriveste cu tranzitia curenta
/// muta jucatorul la el. Daca nimeni n-a cerut nimic - adica ai apasat Play
/// direct in scena asta - actioneaza punctul cu id-ul "intrare".
///
/// Din cauza asta fiecare camera ramane jucabila si separat, fara sa treaca
/// prin hol, ceea ce conteaza cand lucreaza mai multi oameni in paralel.
///
/// Conventia de id-uri:
///   "intrare"        - unde apari cand intri in camera
///   "dupa_cameraN"   - in hol, in fata usii camerei N
[DisallowMultipleComponent]
public class PunctSpawn : MonoBehaviour
{
    [Tooltip("Id-ul cerut de cine face tranzitia. 'intrare' pentru punctul " +
             "normal de intrare in camera.")]
    public string id = Tranzitie.SpawnImplicit;

    [Tooltip("Daca e bifat, jucatorul primeste si orientarea marcajului, " +
             "nu doar pozitia. Sageata albastra din Scene view arata incotro " +
             "va privi.")]
    public bool seteazaOrientarea = true;

    void Start()
    {
        if (!Tranzitie.EsteTinta(id)) return;

        var jucator = GasesteJucatorul();
        if (jucator == null)
        {
            Debug.LogWarning($"PunctSpawn '{id}': n-am gasit niciun jucator in scena.", this);
            return;
        }

        Muta(jucator, transform.position, seteazaOrientarea ? transform.rotation : jucator.rotation);
        Tranzitie.MarcheazaFolosit();
    }

    /// Muta un jucator, tratand corect CharacterController-ul: daca nu il
    /// dezactivezi intai, isi pune la loc pozitia veche in acelasi cadru.
    public static void Muta(Transform jucator, Vector3 pozitie, Quaternion rotatie)
    {
        var cc = jucator.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        jucator.SetPositionAndRotation(pozitie, rotatie);

        if (cc != null) cc.enabled = true;
    }

    /// Toate cele trei controllere din proiect au CharacterController, deci
    /// ala e cel mai sigur reper. Restul sunt plase de siguranta.
    static Transform GasesteJucatorul()
    {
        var cc = Object.FindFirstObjectByType<CharacterController>();
        if (cc != null) return cc.transform;

        var dupaTag = GameObject.FindGameObjectWithTag("Player");
        if (dupaTag != null) return dupaTag.transform;

        if (Camera.main != null) return Camera.main.transform.root;

        return null;
    }

    void OnDrawGizmos()
    {
        // capsula aproximeaza jucatorul: 1.8 m inaltime, 0.3 m raza
        Gizmos.color = new Color(0.4f, 0.85f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.8f);

        if (!seteazaOrientarea) return;

        Gizmos.color = new Color(0.3f, 0.55f, 1f, 0.9f);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.65f, transform.forward * 1.2f);
    }
}
