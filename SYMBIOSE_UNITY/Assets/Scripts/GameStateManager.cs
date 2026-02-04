using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public GameObject eventGel;

    public float delaiAvantEvenement = 5f; // temps avant de déclencher évé gel
    public float seuilMouvementFader = 300f; // seuil minimum pour détecter interaction

    private bool evenementDemarre = false;
    private bool interactionDetectee = false;
    private float chronoInteraction = 0f;

    private int derniereFaderX = -1; // -1 = pas encore init
    private int derniereFaderY = -1;

    void Update()
    {
        // si événement starté, rien faire
        if (evenementDemarre) return;

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
}