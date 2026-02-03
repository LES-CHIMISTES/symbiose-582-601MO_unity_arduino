using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Événements")]
    public GameObject eventGel;

    [Header("Paramètres Maquette #1")]
    public float delaiAvantEvenement = 5f; // temps avant de déclencher évé gel

    private bool evenementDemarre = false;
    private bool interactionDetectee = false;
    private float chronoInteraction = 0f;

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
    public void DetecterInteraction()
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