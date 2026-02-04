using UnityEngine;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    public GameObject eventGel;

    public GameObject textVictoire;
    public GameObject textEchec;
    public GameObject meshEau; // sera desac qd échec

    public float delaiAvantEvenement = 5f; // temps avant de déclencher évé gel
    public float seuilMouvementFader = 300f; // seuil minimum pour détecter interaction
    public float delaiAvantReset = 10f; // temps avant reset qd victoire/echec

    private bool evenementDemarre = false;
    private bool interactionDetectee = false;
    private float chronoInteraction = 0f;

    private int derniereFaderX = -1; // -1 = pas encore init
    private int derniereFaderY = -1;

    private bool partieTerminee = false; // state fin

    void Update()
    {
        if (partieTerminee) return; // si partie est terminée, rien faire

        if (evenementDemarre) return; // si événement starté, rien faire

        // si intéraction détectée, count temps
        if (interactionDetectee)
        {
            chronoInteraction += Time.deltaTime;

            // check si delai écoulé
            if (chronoInteraction >= delaiAvantEvenement)
            {
                DeclencherEvenementGel();
            }
        }
    }

    // OSCInputManager
    public void DetecterInteractionFaderX(int valeur)
    {
        // if first lecture, juste sauvegarder
        if (derniereFaderX == -1)
        {
            derniereFaderX = valeur;
            return;
        }

        // calc le changement
        int changement = Mathf.Abs(valeur - derniereFaderX);

        // si au dessus de seuil
        if (changement >= seuilMouvementFader)
        {
            MarquerInteraction();
        }

        // update last value qui est stock
        derniereFaderX = valeur;
    }

    public void DetecterInteractionFaderY(int valeur)
    {
        if (derniereFaderY == -1)
        {
            derniereFaderY = valeur;
            return;
        }

        int changement = Mathf.Abs(valeur - derniereFaderY);

        if (changement >= seuilMouvementFader)
        {
            MarquerInteraction();
        }

        derniereFaderY = valeur;
    }

    void MarquerInteraction()
    {
        if (!interactionDetectee)
        {
            Debug.Log("GAME : Interaction détectée ! Événement Gel dans " + delaiAvantEvenement + " secondes...");
            interactionDetectee = true;
            chronoInteraction = 0f;
        }
    }

    void DeclencherEvenementGel()
    {
        Debug.Log("GAME : Déclenchement de l'événement Gel !");
        evenementDemarre = true;

        if (eventGel != null)
        {
            eventGel.SetActive(true);
        }
    }

    public void EvenementResolu()
    {
        Debug.Log("GAME : Événement résolu ! Affichage VICTOIRE");
        partieTerminee = true;

        // son victoire
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerVictoire();
        }

        // show txt victoire
        if (textVictoire != null)
        {
            textVictoire.gameObject.SetActive(true);
        }

        // reset après délai
        Invoke("ResetPartie", delaiAvantReset);
    }

    // qd évé échoue
    public void EvenementEchoue()
    {
        Debug.Log("GAME : Événement échoué ! Affichage ÉCHEC");
        partieTerminee = true;

        // son echec
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEchec();
        }

        // show texte echec
        if (textEchec != null)
        {
            textEchec.gameObject.SetActive(true);
        }

        // desactive mesh potion
        if (meshEau != null)
        {
            meshEau.SetActive(false);
        }

        // reset apres delai
        Invoke("ResetPartie", delaiAvantReset);
    }

    // reset tout pour nouvelle partie
    void ResetPartie()
    {
        Debug.Log("GAME : Réinitialisation de la partie...");

        // hide les textes
        if (textVictoire != null)
        {
            textVictoire.gameObject.SetActive(false);
        }
        if (textEchec != null)
        {
            textEchec.gameObject.SetActive(false);
        }

        // reactive potion si était désactivée
        if (meshEau != null)
        {
            // désactive puis réactive pour forcer un reset
            meshEau.SetActive(false);

            // invoke après un frame
            Invoke("ReactiverMeshEau", 0.1f);
        }

        // desactive EventGel si encore actif
        if (eventGel != null)
        {
            eventGel.SetActive(false);
        }

        // reset les états
        evenementDemarre = false;
        interactionDetectee = false;
        partieTerminee = false;
        chronoInteraction = 0f;
        derniereFaderX = -1;
        derniereFaderY = -1;

        Debug.Log("GAME : Prêt pour une nouvelle partie !");
    }

    void ReactiverMeshEau()
    {
        if (meshEau != null)
        {
            meshEau.SetActive(true);

            // vide l'eau
            MeshEauController eauController = meshEau.GetComponent<MeshEauController>();
            if (eauController != null)
            {
                eauController.ViderEau();
            }
        }
    }
}