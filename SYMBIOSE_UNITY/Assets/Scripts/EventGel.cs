using UnityEngine;
using UnityEngine.UI;

public class EventGel : MonoBehaviour
{
    public Slider jaugeTemperature;
    public GameObject patternRythmique; // parent img
    public Image[] imagesPattern; // 4 images cercles

    public MeshEauController meshEau;

    public float seuilIntensiteMax = 1028f; // potentiomètre max (etape 1)
    public float dureePhase1 = 3f; // à maintenir au max avant phase 2

    public int[] valeursCiblesPattern = new int[] { 750, 3400, 300, 2800 }; // 4 positions
    public float tolerancePattern = 100f; // marge d'erreur
    public float tempsMaxParEtape = 5; // temps max pour chaque étape du pattern

    public float niveauFroidInitial = 100f; // froid au départ(jauge pleine)
    public float vitesseRechauffement = 10f; // vitesse diminution
    public float tempsMaxAvantEchec = 30f;

    public MeshRenderer potionRenderer; // mesh de la potion (pour changer couleur)
    public GameObject[] meshsGel; // mesh à activer
    public Image vignetteGel; // img UI de vignette
    public Color couleurGel = new Color(0.7f, 0.9f, 1f); // bleutée/blanchatre

    // states
    private enum PhaseGel { Phase1_AtteindreLaMax, Phase2_PatternRythmique, Resolu, Echec }
    private PhaseGel phaseActuelle = PhaseGel.Phase1_AtteindreLaMax;

    private float niveauFroid = 100f; // nv de froid (100 = gelé, 0 = dégelé)
    private float chronoPhase1 = 0f; // temps passé au max en phase 1
    private float chronoInactivite = 0f;
    private int valeurPotentiometre = 0;

    private int etapePatternActuelle = 0; // 0-3
    private float chronoEtapePattern = 0f; // temps sur l'étape actuelle

    void OnEnable()
    {
        DemarrerEvenement();
    }

    void DemarrerEvenement()
    {
        Debug.Log("EVENT GEL : Démarré !");

        // reset
        phaseActuelle = PhaseGel.Phase1_AtteindreLaMax;
        niveauFroid = niveauFroidInitial;
        chronoPhase1 = 0f;
        chronoInactivite = 0f;
        etapePatternActuelle = 0;
        chronoEtapePattern = 0f;

        // afficher jauge pleine
        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(true);
            jaugeTemperature.value = niveauFroid / niveauFroidInitial;
        }

        // cache pattern
        if (patternRythmique != null)
        {
            patternRythmique.SetActive(false);
        }

        ActiverEffetsVisuelsGel();
    }

    void ActiverEffetsVisuelsGel()
    {
        // change couleur potion
        if (potionRenderer != null)
        {
            potionRenderer.material.color = couleurGel;
        }

        // active mesh gels
        if (meshsGel != null)
        {
            foreach (GameObject mesh in meshsGel)
            {
                if (mesh != null)
                {
                    mesh.SetActive(true);
                }
            }
        }

        // show vignette
        if (vignetteGel != null)
        {
            vignetteGel.gameObject.SetActive(true);
            Color couleurVignette = vignetteGel.color;
            couleurVignette.a = 0.2f; // alpha 50
            vignetteGel.color = couleurVignette;
        }
    }

    void Update()
    {
        if (phaseActuelle == PhaseGel.Resolu || phaseActuelle == PhaseGel.Echec)
            return;

        // check échec via inactivité
        chronoInactivite += Time.deltaTime;
        if (chronoInactivite >= tempsMaxAvantEchec)
        {
            EchecEvenement();
            return;
        }

        // logique selon phase
        switch (phaseActuelle)
        {
            case PhaseGel.Phase1_AtteindreLaMax:
                GererPhase1();
                break;
            case PhaseGel.Phase2_PatternRythmique:
                GererPhase2();
                break;
        }

        // update jauge (descend)
        if (jaugeTemperature != null)
        {
            jaugeTemperature.value = niveauFroid / niveauFroidInitial;
        }
    }

    void GererPhase1()
    {
        // phase 1 : doit maintenir le knob au max
        if (valeurPotentiometre <= seuilIntensiteMax)
        {
            // le joueur chauffe fort
            chronoPhase1 += Time.deltaTime;

            // froid diminue
            niveauFroid -= vitesseRechauffement * Time.deltaTime;
            niveauFroid = Mathf.Max(0f, niveauFroid);

            // check si on passe à la phase 2
            if (chronoPhase1 >= dureePhase1)
            {
                PasserPhase2();
            }
        }
        else
        {
            // si pas au max, reset le chrono
            chronoPhase1 = 0f;
        }
    }

    void PasserPhase2()
    {
        Debug.Log("EVENT GEL : Phase 2 - Pattern rythmique !");
        phaseActuelle = PhaseGel.Phase2_PatternRythmique;
        etapePatternActuelle = 0;
        chronoEtapePattern = 0f;

        // show le pattern
        if (patternRythmique != null)
        {
            patternRythmique.SetActive(true);
        }

        // reset transparence des imgs
        ResetTransparenceImages();
    }

    void GererPhase2()
    {
        // phase 2 doit suivre le pattern
        chronoEtapePattern += Time.deltaTime;

        // check timeout de l'étape
        if (chronoEtapePattern >= tempsMaxParEtape)
        {
            Debug.Log("EVENT GEL : Échec - Temps écoulé sur l'étape " + etapePatternActuelle);
            EchecEvenement();
            return;
        }

        // check si l'utilisateur est à la bonne position
        int valeurCible = valeursCiblesPattern[etapePatternActuelle];
        float difference = Mathf.Abs(valeurPotentiometre - valeurCible);

        if (difference <= tolerancePattern)
        {
            // bonne position, valide l'étape
            ValiderEtapePattern();
        }
    }

    void ValiderEtapePattern()
    {
        Debug.Log("EVENT GEL : Étape " + etapePatternActuelle + " validée !");

        // mettre l'image à 50% d'opacité
        if (imagesPattern != null && etapePatternActuelle < imagesPattern.Length)
        {
            Color couleur = imagesPattern[etapePatternActuelle].color;
            couleur.a = 0.5f;
            imagesPattern[etapePatternActuelle].color = couleur;
        }

        // diminue encore froid
        niveauFroid -= vitesseRechauffement * 2f;
        niveauFroid = Mathf.Max(0f, niveauFroid);

        // passe à l'étape suivante
        etapePatternActuelle++;
        chronoEtapePattern = 0f;

        // check si toutes les 4 étapes sont complétées
        if (etapePatternActuelle >= valeursCiblesPattern.Length)
        {
            // 4ème étape validée = VICTOIRE automatique !
            ResoudreEvenement();
        }
    }

    void ResetTransparenceImages()
    {
        if (imagesPattern == null) return;

        foreach (Image img in imagesPattern)
        {
            if (img != null)
            {
                Color couleur = img.color;
                couleur.a = 1f; // opaque
                img.color = couleur;
            }
        }
    }

    public void UpdatePotentiometre(int valeur)
    {
        valeurPotentiometre = valeur;
        chronoInactivite = 0f; // reset inactivité dès qu'il y a mouvement
    }

    void ResoudreEvenement()
    {
        phaseActuelle = PhaseGel.Resolu;
        Debug.Log("EVENT GEL : RÉSOLU !");

        // hide UI
        if (jaugeTemperature != null)
            jaugeTemperature.gameObject.SetActive(false);
        if (patternRythmique != null)
            patternRythmique.SetActive(false);

        DesactiverEffetsVisuelsGel();
        // TODO : Notifier GameManager

        Invoke("DesactiverEvenement", 2f);
    }

    void EchecEvenement()
    {
        phaseActuelle = PhaseGel.Echec;
        Debug.Log("EVENT GEL : ÉCHEC !");

        // hide UI
        if (jaugeTemperature != null)
            jaugeTemperature.gameObject.SetActive(false);
        if (patternRythmique != null)
            patternRythmique.SetActive(false);

        // TODO : Notifier GameManager
    }

    void DesactiverEffetsVisuelsGel()
    {
        // couleur normale potion
        if (potionRenderer != null && meshEau != null)
        {
            // reprend couleur mesh eau (avec keys etc)
            potionRenderer.material.color = meshEau.meshMaterial.color;
        }

        // hide mesh gel
        if (meshsGel != null)
        {
            foreach (GameObject mesh in meshsGel)
            {
                if (mesh != null)
                {
                    mesh.SetActive(false);
                }
            }
        }

        // hide vignette
        if (vignetteGel != null)
        {
            vignetteGel.gameObject.SetActive(false);
        }
    }
    void DesactiverEvenement()
    {
        gameObject.SetActive(false);
    }
}