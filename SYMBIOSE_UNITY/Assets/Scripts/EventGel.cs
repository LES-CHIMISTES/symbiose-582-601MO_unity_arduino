using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class EventGel : MonoBehaviour
{
    public Slider jaugeTemperature;
    public GameObject patternRythmique; // parent img
    public Image[] imagesPattern; // 4 images cercles
    public GameStateManager gameStateManager;
    public OSCTransmitter oscTransmitter;

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
    public float dureeAnimationVignette = 1.5f; // durée animation vignette
    public GameObject textAlertGel; // texte
    public float dureeAffichageAlert = 2f; // affiche texte 2s

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

        // afficher texte
        if (textAlertGel != null)
        {
            textAlertGel.SetActive(true);
            Invoke("CacherTextAlert", dureeAffichageAlert);
        }

        ActiverEffetsVisuelsGel();
    }

    void CacherTextAlert()
    {
        if (textAlertGel != null)
        {
            textAlertGel.SetActive(false);
        }
    }
    void ActiverEffetsVisuelsGel()
    {
        // jouer son frosting
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerFrosting();
        }

        EnvoyerOSCLumiere(true);

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

        // show vignette avec animation
        if (vignetteGel != null)
        {
            StartCoroutine(AnimerVignette());
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

        // mettre l'image à 50% d'opacité + scale réduit
        if (imagesPattern != null && etapePatternActuelle < imagesPattern.Length)
        {
            Image img = imagesPattern[etapePatternActuelle];

            // alpha
            Color couleur = img.color;
            couleur.a = 0.3f;
            img.color = couleur;

            // AJOUT : scale réduit
            img.transform.localScale = Vector3.one * 0.7f; // 70% de la taille
        }

        // MODIFIÉ : diminue encore plus le froid pour atteindre ~10% à la fin
        niveauFroid -= vitesseRechauffement * 5f; // CHANGÉ : *5f au lieu de *2f
        niveauFroid = Mathf.Max(0f, niveauFroid);

        // passe à l'étape suivante
        etapePatternActuelle++;
        chronoEtapePattern = 0f;

        // check si toutes les 4 étapes sont complétées
        if (etapePatternActuelle >= valeursCiblesPattern.Length)
        {
            // 4ème étape validée = victoire automatique
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

                img.transform.localScale = Vector3.one; // taille normale
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
        if (gameStateManager != null)
        {
            gameStateManager.EvenementResolu();
        }

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

        DesactiverEffetsVisuelsGel();

        if (gameStateManager != null)
        {
            gameStateManager.EvenementEchoue();
        }
    }

    // anime la vignette scale out + fade in
    System.Collections.IEnumerator AnimerVignette()
    {
        if (vignetteGel == null) yield break;

        vignetteGel.gameObject.SetActive(true);

        // state initial
        Color couleur = vignetteGel.color;
        couleur.a = 0f;
        vignetteGel.color = couleur;
        vignetteGel.transform.localScale = Vector3.one * 2f; // start à 2x la taille

        float tempsEcoule = 0f;

        while (tempsEcoule < dureeAnimationVignette)
        {
            tempsEcoule += Time.deltaTime;
            float progression = tempsEcoule / dureeAnimationVignette;

            // lerp alpha de 0 à 0.2
            couleur.a = Mathf.Lerp(0f, 0.2f, progression);
            vignetteGel.color = couleur;

            // lerp scale de 2 à 1 (scale in)
            float scale = Mathf.Lerp(2f, 1f, progression);
            vignetteGel.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        // assurer les valeurs finales
        couleur.a = 0.2f;
        vignetteGel.color = couleur;
        vignetteGel.transform.localScale = Vector3.one;
    }
    void DesactiverEffetsVisuelsGel()
    {
        // couleur normale potion
        if (potionRenderer != null && meshEau != null)
        {
            // reprend couleur mesh eau (avec keys etc)
            potionRenderer.material.color = meshEau.meshMaterial.color;
        }

        EnvoyerOSCLumiere(false);

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

        if (textAlertGel != null)
        {
            textAlertGel.SetActive(false);
        }
    }

    void EnvoyerOSCLumiere(bool allumer)
    {
        if (oscTransmitter == null) return;

        if (allumer)
        {
            // allumer la lumière en bleu cyan (effet gel)
            var messageAllumer = new OSCMessage("/lumiere/gel");
            messageAllumer.AddValue(OSCValue.Int(1)); // 1 = allumer
            oscTransmitter.Send(messageAllumer);

            Debug.Log("OSC : Lumière gel ALLUMÉE");
        }
        else
        {
            // éteindre la lumière
            var messageEteindre = new OSCMessage("/lumiere/gel");
            messageEteindre.AddValue(OSCValue.Int(0)); // 0 = éteindre
            oscTransmitter.Send(messageEteindre);

            Debug.Log("OSC : Lumière gel ÉTEINTE");
        }
    }
    void DesactiverEvenement()
    {
        gameObject.SetActive(false);
    }
}