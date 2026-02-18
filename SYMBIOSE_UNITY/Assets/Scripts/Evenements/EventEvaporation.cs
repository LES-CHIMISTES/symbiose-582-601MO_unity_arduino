using UnityEngine;
using UnityEngine.UI;
using TMPro;
using extOSC;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EventEvaporation : MonoBehaviour
{
    // =====================================================================
    // REFERENCES UI
    // =====================================================================

    [Header("panel principal")]
    public GameObject eventEvaporationPanel;

    [Header("metre d'intensite vertical")]
    public RectTransform metreConteneur;        // le conteneur vertical complet
    public Image zoneTropFaible;                // section basse (bleu sombre)
    public Image zoneOptimale;                  // section milieu (verte, cible)
    public Image zoneTropFort;                  // section haute (rouge)
    public RectTransform curseur;               // indicateur qui monte/descend
    public Image curseurImage;                  // pour changer la couleur du curseur
    public Image bordureZoneOptimale;           // bordure qui pulse autour de la zone verte

    [Header("jauge de progression")]
    public Slider jaugeProgression;
    public Image fillJauge;

    [Header("icone secousse (optionnel)")]
    public RectTransform iconeSecousse;         // petite icone qui shake visuellement

    // =====================================================================
    // PARAMETRES GAMEPLAY
    // =====================================================================

    [Header("params detection")]
    public float seuilIntensiteMin = 0.3f;      // en dessous = trop faible
    public float seuilIntensiteOptimale = 1.5f;  // sweet spot haut
    public float seuilIntensiteMax = 3f;         // au dessus = trop fort
    public float multiplicateurRemplissage = 0.04f;
    public float penaliteTropFort = 0.02f;       // recul de progression si trop fort
    public float tempsMaxAvantEchec = 30f;

    [Header("params physique")]
    public float multiplicateurMagnitude = 1.5f;
    public float smoothMontee = 0.15f;
    public float smoothDescente = 0.08f;
    public float decroissanceParSeconde = 2f;

    [Header("difficulte progressive")]
    public float seuilMinDebutant = 0.2f;
    public float seuilMinExpert = 0.8f;
    public float tempsSeuilDifficulte = 180f;

    // =====================================================================
    // PARAMETRES VISUELS
    // =====================================================================

    [Header("couleurs zones")]
    public Color couleurTropFaibleInactif = new Color(0.15f, 0.2f, 0.35f, 0.6f);
    public Color couleurTropFaibleActif = new Color(0.3f, 0.4f, 0.6f, 0.8f);
    public Color couleurOptimaleInactif = new Color(0.2f, 0.5f, 0.2f, 0.5f);
    public Color couleurOptimaleActif = new Color(0.3f, 0.9f, 0.3f, 0.9f);
    public Color couleurTropFortInactif = new Color(0.35f, 0.15f, 0.15f, 0.4f);
    public Color couleurTropFortActif = new Color(0.9f, 0.2f, 0.2f, 0.9f);

    [Header("couleurs curseur")]
    public Color couleurCurseurFaible = new Color(0.5f, 0.6f, 0.8f);
    public Color couleurCurseurOptimal = new Color(0.3f, 1f, 0.3f);
    public Color couleurCurseurTropFort = new Color(1f, 0.3f, 0.3f);

    [Header("couleurs jauge progression")]
    public Color couleurJaugeInactive = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color couleurJaugeActive = new Color(0.3f, 0.9f, 0.3f, 1f);

    [Header("animation")]
    public float vitessePulseBordure = 2f;       // vitesse du blink de la zone optimale
    public float amplitudePulseBordure = 0.3f;
    public float vitessePulseRapide = 5f;        // pulse rapide quand trop faible
    public float intensiteShakeIcone = 5f;        // amplitude du shake de l'icone
    public float vitesseShakeIcone = 25f;

    [Header("post-processing")]
    public Volume volumePostProcess;
    private ColorAdjustments colorAdjustments;
    private float saturationInitiale;
    public float saturationMinimale = -80f;  // valeur desaturee pendant l'event

    // =====================================================================
    // REFERENCES VISUELS 3D ET SYSTEME
    // =====================================================================

    [Header("visuels 3d")]
    public MeshRenderer eauRenderer;
    public GameObject[] meshsBrouillard;
    public Color couleurEauTransparente = new Color(0.6f, 0.8f, 1f, 0.3f);

    [Header("references systeme")]
    public GameManager gameManager;
    public OSCTransmitter oscTransmitter;
    public MeshEauController meshEau;
    public StationEauFeedback stationEau;
    public CanvasGroup colonneEau;

    public GameObject meshACacher;

    // =====================================================================
    // VARIABLES PRIVEES
    // =====================================================================

    private enum PhaseEvaporation { EnCours, Resolu, Echec }
    private PhaseEvaporation phaseActuelle;

    private float progression = 0f;
    private float chronoTotal = 0f;
    private float intensiteActuelle = 0f;
    private float intensiteCible = 0f;

    private float dernierAccelX = 0f;
    private float dernierAccelY = 0f;
    private float dernierAccelZ = 0f;
    private bool accelInitialise = false;

    private Color couleurEauInitiale;
    private Vector3 iconePositionInitiale;

    // hauteur du metre pour positionner le curseur
    private float metreHauteur = 0f;
    private float metrePosYBas = 0f;

    // =====================================================================
    // INITIALISATION
    // =====================================================================

    void OnEnable()
    {
        DemarrerEvenement();
    }

    void DemarrerEvenement()
    {
        phaseActuelle = PhaseEvaporation.EnCours;
        progression = 0f;
        chronoTotal = 0f;
        intensiteActuelle = 0f;
        intensiteCible = 0f;
        accelInitialise = false;

        // ajuster difficulte selon temps de jeu
        if (GameManager.Instance != null)
        {
            float progressionDifficulte = Mathf.Clamp01(GameManager.Instance.tempsEcoule / tempsSeuilDifficulte);
            seuilIntensiteMin = Mathf.Lerp(seuilMinDebutant, seuilMinExpert, progressionDifficulte);
        }

        // activer panel
        if (eventEvaporationPanel != null)
        {
            eventEvaporationPanel.SetActive(true);
        }

        if (volumePostProcess != null && volumePostProcess.profile.TryGet(out colorAdjustments))
        {
            saturationInitiale = colorAdjustments.saturation.value;
            colorAdjustments.saturation.value = saturationMinimale;
        }

        // initialiser jauge
        if (jaugeProgression != null)
        {
            jaugeProgression.value = 0f;
        }

        if (fillJauge != null)
        {
            fillJauge.color = couleurJaugeInactive;
        }

        // cacher curseur au bas du metre
        if (curseur != null && metreConteneur != null)
        {
            metreHauteur = metreConteneur.rect.height;
            metrePosYBas = -metreHauteur / 2f;
            curseur.anchoredPosition = new Vector2(curseur.anchoredPosition.x, metrePosYBas);
        }

        // stocker position initiale de l'icone
        if (iconeSecousse != null)
        {
            iconePositionInitiale = iconeSecousse.anchoredPosition;
        }

        // effets visuels 3d
        ActiverEffetsVisuels();

        // desactiver feedback station eau normal
        if (stationEau != null)
        {
            stationEau.enabled = false;
        }

        if (colonneEau != null)
        {
            colonneEau.alpha = 0.25f;
        }

        Debug.Log($"EVENT EVAPORATION : demarre, seuil min = {seuilIntensiteMin:F2}");
    }

    // =====================================================================
    // UPDATE PRINCIPAL
    // =====================================================================

    void Update()
    {
        if (phaseActuelle != PhaseEvaporation.EnCours) return;

        chronoTotal += Time.deltaTime;

        if (chronoTotal >= tempsMaxAvantEchec)
        {
            EchecEvenement();
            return;
        }

        // decroissance naturelle entre messages OSC
        intensiteCible = Mathf.Max(0f, intensiteCible - decroissanceParSeconde * Time.deltaTime);

        // smooth differenciee montee/descente
        float smooth = (intensiteCible > intensiteActuelle) ? smoothMontee : smoothDescente;
        intensiteActuelle = Mathf.Lerp(intensiteActuelle, intensiteCible, smooth);

        // ---- mise a jour visuelle ----
        UpdatePositionCurseur();
        UpdateCouleursZones();
        UpdateCouleurCurseur();
        UpdatePulseBordure();
        UpdateJaugeProgression();
        UpdateIconeSecousse();

        // ---- logique de remplissage ----
        if (intensiteActuelle >= seuilIntensiteMin && intensiteActuelle <= seuilIntensiteMax)
        {
            // dans la zone optimale : remplir
            float facteur = Mathf.InverseLerp(seuilIntensiteMin, seuilIntensiteOptimale, intensiteActuelle);
            facteur = Mathf.Clamp01(facteur);
            progression += facteur * multiplicateurRemplissage * Time.deltaTime;
        }
        else if (intensiteActuelle > seuilIntensiteMax)
        {
            // trop fort : penalite (recul)
            progression -= penaliteTropFort * Time.deltaTime;
        }

        progression = Mathf.Clamp01(progression);

        // jauge UI
        if (jaugeProgression != null)
        {
            jaugeProgression.value = progression;
        }

        // transparence eau proportionnelle a la progression
        if (eauRenderer != null)
        {
            Color c = Color.Lerp(couleurEauTransparente, couleurEauInitiale, progression);
            eauRenderer.material.color = c;
        }

        if (colorAdjustments != null)
        {
            float saturationCible = Mathf.Lerp(saturationMinimale, saturationInitiale, progression);
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, saturationCible, Time.deltaTime * 3f);
        }

        if (progression >= 1f)
        {
            ResoudreEvenement();
        }
    }

    // =====================================================================
    // VISUEL : POSITION DU CURSEUR
    // =====================================================================

    void UpdatePositionCurseur()
    {
        if (curseur == null || metreConteneur == null) return;

        // recalculer au cas ou layout change
        metreHauteur = metreConteneur.rect.height;
        metrePosYBas = -metreHauteur / 2f;

        // normaliser l'intensite sur la hauteur du metre (0 = bas, seuilMax = haut)
        float t = Mathf.Clamp01(intensiteActuelle / seuilIntensiteMax);
        float posY = Mathf.Lerp(metrePosYBas, -metrePosYBas, t);

        curseur.anchoredPosition = new Vector2(curseur.anchoredPosition.x, posY);
    }

    // =====================================================================
    // VISUEL : COULEURS DES ZONES
    // =====================================================================

    void UpdateCouleursZones()
    {
        // determiner dans quelle zone on est
        bool dansFaible = intensiteActuelle < seuilIntensiteMin;
        bool dansOptimale = intensiteActuelle >= seuilIntensiteMin && intensiteActuelle <= seuilIntensiteMax;
        bool dansTropFort = intensiteActuelle > seuilIntensiteMax;

        if (zoneTropFaible != null)
        {
            Color cible = dansFaible ? couleurTropFaibleActif : couleurTropFaibleInactif;
            zoneTropFaible.color = Color.Lerp(zoneTropFaible.color, cible, Time.deltaTime * 8f);
        }

        if (zoneOptimale != null)
        {
            Color cible = dansOptimale ? couleurOptimaleActif : couleurOptimaleInactif;
            zoneOptimale.color = Color.Lerp(zoneOptimale.color, cible, Time.deltaTime * 8f);
        }

        if (zoneTropFort != null)
        {
            Color cible = dansTropFort ? couleurTropFortActif : couleurTropFortInactif;
            zoneTropFort.color = Color.Lerp(zoneTropFort.color, cible, Time.deltaTime * 8f);
        }
    }

    // =====================================================================
    // VISUEL : COULEUR DU CURSEUR
    // =====================================================================

    void UpdateCouleurCurseur()
    {
        if (curseurImage == null) return;

        Color cible;
        if (intensiteActuelle < seuilIntensiteMin)
        {
            cible = couleurCurseurFaible;
        }
        else if (intensiteActuelle <= seuilIntensiteMax)
        {
            cible = couleurCurseurOptimal;
        }
        else
        {
            cible = couleurCurseurTropFort;
        }

        curseurImage.color = Color.Lerp(curseurImage.color, cible, Time.deltaTime * 10f);
    }

    // =====================================================================
    // VISUEL : PULSE DE LA BORDURE ZONE OPTIMALE
    // =====================================================================

    void UpdatePulseBordure()
    {
        if (bordureZoneOptimale == null) return;

        bool dansOptimale = intensiteActuelle >= seuilIntensiteMin && intensiteActuelle <= seuilIntensiteMax;
        bool tropFaible = intensiteActuelle < seuilIntensiteMin;

        // choisir vitesse de pulse
        float vitesse = tropFaible ? vitessePulseRapide : vitessePulseBordure;

        // pulse alpha entre (1 - amplitude) et 1
        float alpha = 1f - amplitudePulseBordure + amplitudePulseBordure * Mathf.Sin(Time.time * vitesse * Mathf.PI);

        Color c = bordureZoneOptimale.color;

        if (dansOptimale)
        {
            // dans la zone : glow vert stable
            c = couleurOptimaleActif;
            c.a = alpha;
        }
        else
        {
            // hors zone : pulse pour attirer attention
            c = tropFaible ? couleurOptimaleInactif : couleurTropFortActif;
            c.a = alpha * 0.7f;
        }

        bordureZoneOptimale.color = c;
    }

    // =====================================================================
    // VISUEL : JAUGE DE PROGRESSION
    // =====================================================================

    void UpdateJaugeProgression()
    {
        if (fillJauge == null) return;

        bool enProgression = intensiteActuelle >= seuilIntensiteMin && intensiteActuelle <= seuilIntensiteMax;

        Color cible = enProgression ? couleurJaugeActive : couleurJaugeInactive;
        fillJauge.color = Color.Lerp(fillJauge.color, cible, Time.deltaTime * 6f);
    }

    // =====================================================================
    // VISUEL : ICONE SECOUSSE
    // =====================================================================

    void UpdateIconeSecousse()
    {
        if (iconeSecousse == null) return;

        // shake proportionnel a l'intensite
        float amplitude = (intensiteActuelle / seuilIntensiteMax) * intensiteShakeIcone;
        float offsetX = Mathf.Sin(Time.time * vitesseShakeIcone) * amplitude;
        float offsetY = Mathf.Cos(Time.time * vitesseShakeIcone * 1.3f) * amplitude * 0.5f;

        iconeSecousse.anchoredPosition = iconePositionInitiale + new Vector3(offsetX, offsetY, 0f);
    }

    // =====================================================================
    // RECEPTION ACCELEROMETRE (appele par OSCInputManager)
    // =====================================================================

    public void UpdateAccel(float accelX, float accelY, float accelZ)
    {
        // ignorer premier message pour baseline
        if (!accelInitialise)
        {
            dernierAccelX = accelX;
            dernierAccelY = accelY;
            dernierAccelZ = accelZ;
            accelInitialise = true;
            return;
        }

        float deltaX = Mathf.Abs(accelX - dernierAccelX);
        float deltaY = Mathf.Abs(accelY - dernierAccelY);
        float deltaZ = Mathf.Abs(accelZ - dernierAccelZ);

        float magnitudeChangement = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

        // appliquer multiplicateur et garder la valeur haute
        float nouvelleIntensite = magnitudeChangement * multiplicateurMagnitude;
        intensiteCible = Mathf.Max(intensiteCible, nouvelleIntensite);

        dernierAccelX = accelX;
        dernierAccelY = accelY;
        dernierAccelZ = accelZ;
    }

    // =====================================================================
    // EFFETS VISUELS 3D
    // =====================================================================

    void ActiverEffetsVisuels()
    {
        if (AudioManager.Instance != null)
        {
            // jouer son evaporation
        }

        if (eauRenderer != null)
        {
            couleurEauInitiale = eauRenderer.material.color;
            eauRenderer.material.color = couleurEauTransparente;
        }

        if (meshsBrouillard != null)
        {
            foreach (GameObject mesh in meshsBrouillard)
            {
                if (mesh != null) mesh.SetActive(true);
            }
        }

        if (meshACacher != null)
        {
            meshACacher.SetActive(false);
        }

        EnvoyerOSCLumiere(true);
    }

    // =====================================================================
    // RESOLUTION / ECHEC
    // =====================================================================

    void ResoudreEvenement()
    {
        phaseActuelle = PhaseEvaporation.Resolu;

        DesactiverEffets();

        if (gameManager != null)
        {
            gameManager.EvenementResolu();
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.EvenementTermine();
        }

        Invoke(nameof(DesactiverEvent), 1f);

        Debug.Log("EVENT EVAPORATION : resolu");
    }

    void EchecEvenement()
    {
        phaseActuelle = PhaseEvaporation.Echec;

        DesactiverEffets();

        if (gameManager != null)
        {
            gameManager.EvenementEchoue();
        }

        Debug.Log("EVENT EVAPORATION : echec");
    }

    // =====================================================================
    // NETTOYAGE
    // =====================================================================

    void DesactiverEffets()
    {
        if (eauRenderer != null)
        {
            eauRenderer.material.color = couleurEauInitiale;
        }

        if (meshsBrouillard != null)
        {
            foreach (GameObject mesh in meshsBrouillard)
            {
                if (mesh != null) mesh.SetActive(false);
            }
        }

        if (eventEvaporationPanel != null)
        {
            eventEvaporationPanel.SetActive(false);
        }

        if (stationEau != null)
        {
            stationEau.enabled = true;
        }

        if (colonneEau != null)
        {
            colonneEau.alpha = 1f;
        }

        // remettre icone a sa position
        if (iconeSecousse != null)
        {
            iconeSecousse.anchoredPosition = iconePositionInitiale;
        }

        if (meshACacher != null)
        {
            meshACacher.SetActive(true);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = saturationInitiale;
        }

        EnvoyerOSCLumiere(false);
    }

    void EnvoyerOSCLumiere(bool allumer)
    {
        if (oscTransmitter == null) return;

        var message = new OSCMessage("/lumiere/evaporation");
        message.AddValue(OSCValue.Int(allumer ? 1 : 0));
        oscTransmitter.Send(message);
    }

    void DesactiverEvent()
    {
        gameObject.SetActive(false);
    }
}
