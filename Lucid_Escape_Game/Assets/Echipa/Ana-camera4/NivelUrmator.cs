using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [Header("Setari Trecere Nivel")]
    [Tooltip("Malvina_mainHall")]
    public string numeleSceneiUrmatoare; 

    void OnTriggerEnter(Collider other)
    {
        // Daca jucatorul a atins zona usii
        if (other.name == "Player" || other.CompareTag("Player"))
        {
            Debug.Log("Jucatorul a ajuns la final! Trimite-l spre: " + numeleSceneiUrmatoare);
            
            if (!string.IsNullOrEmpty(numeleSceneiUrmatoare))
            {
                SceneManager.LoadScene(numeleSceneiUrmatoare);
            }
        }
    }
}