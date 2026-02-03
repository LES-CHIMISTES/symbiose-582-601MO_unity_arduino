using UnityEngine;
using UnityEngine.UI;

public class EventGel : MonoBehaviour
{
    public Slider jaugeTemperature;
    public MeshEauController meshEau; // pour changer visuels de la potion

    public float seuilIntensiteMax = 3500f; // valeur potentiomètre pour "au max" (sur 4096)
    public float vitesseRechauffement = 3f; // vitesse progression jauge
    public float seuilResolution = 100f; // jauge doit atteindre 100
    public float tempsMaxAvantEchec = 5f; // temps max sans action = échec

    private float progressionTemperature = 0f; // progression de 0 à 100
    private bool resolu = false;
    private bool echec = false;
    private float chronoInactivite = 0f; // compte le temps sans action
    private int valeurPotentiometre = 0; // valeur actuelle du potentiomètre

    void OnEnable()
    {
        DemarrerEvenement();
    }

    void DemarrerEvenement()
    {
        Debug.Log("EVENT GEL : Démarré !");

        // reset
        progressionTemperature = 0f;
        resolu = false;
        echec = false;
        chronoInactivite = 0f;

        // affiche la jauge
        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(true);
            jaugeTemperature.value = 0f;
        }

        // TODO : Activer effets visuels gel (plus tard)
        // - Changer couleur potion en bleuté
        // - Ajouter particules flocons
        // - Changer visuel fenêtre
    }

    void Update()
    {
        if (resolu || echec) return;

        // si angle bas
        if (valeurPotentiometre <= (4096 - seuilIntensiteMax))
        {
            // le joueur chauffe, progression ++
            progressionTemperature += vitesseRechauffement * Time.deltaTime;
            chronoInactivite = 0f;

            if (jaugeTemperature != null)
            {
                jaugeTemperature.value = progressionTemperature / seuilResolution;
            }

            if (progressionTemperature >= seuilResolution)
            {
                ResoudreEvenement();
            }
        }
        else
        {
            chronoInactivite += Time.deltaTime;

            if (chronoInactivite >= tempsMaxAvantEchec)
            {
                EchecEvenement();
            }
        }
    }

    // OSCInputManager
    public void UpdatePotentiometre(int valeur)
    {
        valeurPotentiometre = valeur;
    }

    void ResoudreEvenement()
    {
        resolu = true;
        Debug.Log("EVENT GEL : RÉSOLU !");

        // hide jauge
        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(false);
        }

        // TODO : Désactiver effets visuels gel

        // TODO : Notifier le GameManager
        // GameStateManager.Instance.EvenementResolu();

        // Pour l'instant, désactiver l'événement après 2 secondes
        Invoke("DesactiverEvenement", 2f);
    }

    void EchecEvenement()
    {
        echec = true;
        Debug.Log("EVENT GEL : ÉCHEC par inactivité !");

        // hide la jauge
        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(false);
        }

        // TODO : Notifier le GameManager
        // GameStateManager.Instance.EvenementEchoue();
    }

    void DesactiverEvenement()
    {
        gameObject.SetActive(false);
    }
}