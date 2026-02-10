using UnityEngine;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    public GameObject eventGel;
    public GameObject textVictoire;
    public GameObject textEchec;
    public GameObject meshEau;
    
    private bool partieTerminee = false;

    // méthodes de fin de partie
    public void EvenementResolu()
    {
        Debug.Log("GAME : Événement résolu ! Affichage VICTOIRE");
        partieTerminee = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerVictoire();
        }

        if (textVictoire != null)
        {
            textVictoire.gameObject.SetActive(true);
        }

        Invoke("ResetPartie", 10f);
    }

    public void EvenementEchoue()
    {
        Debug.Log("GAME : Événement échoué ! Affichage ÉCHEC");
        partieTerminee = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEchec();
        }

        if (textEchec != null)
        {
            textEchec.gameObject.SetActive(true);
        }

        if (meshEau != null)
        {
            meshEau.SetActive(false);
        }

        Invoke("ResetPartie", 10f);
    }

    void ResetPartie()
    {
        Debug.Log("GAME : Réinitialisation de la partie...");

        if (textVictoire != null)
        {
            textVictoire.gameObject.SetActive(false);
        }
        if (textEchec != null)
        {
            textEchec.gameObject.SetActive(false);
        }

        if (meshEau != null)
        {
            meshEau.SetActive(false);
            Invoke("ReactiverMeshEau", 0.1f);
        }

        if (eventGel != null)
        {
            eventGel.SetActive(false);
        }

        partieTerminee = false;
        Debug.Log("GAME : Prêt pour une nouvelle partie !");
    }

    void ReactiverMeshEau()
    {
        if (meshEau != null)
        {
            meshEau.SetActive(true);
            MeshEauController eauController = meshEau.GetComponent<MeshEauController>();
            if (eauController != null)
            {
                eauController.ViderEau();
            }
        }
    }
}