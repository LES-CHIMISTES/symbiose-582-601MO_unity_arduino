using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using extOSC;
using TMPro;

public class EventVortex : MonoBehaviour
{
    // =====================================================================
    // REFERENCES UI
    // =====================================================================

    [Header("panel principal")]
    public GameObject eventVortexPanel;

    [Header("zone de jeu")]
    public RectTransform zoneJeu;
    public RectTransform cercleCible;
    public Image cercleCibleFond;
    public Image cercleCibleBordure;
    public RectTransform curseurJoueur;
    public Image curseurImage;

    [Header("jauge progression")]
    public Slider jaugeProgression;
    public Image fillJauge;

    [Header("indicateur direction")]
    public RectTransform indicateurDirection;

    [Header("ui texte")]
    public TextMeshProUGUI texteAlert;
    public float dureeAffichageTexte = 3f;

    [Header("flash reussite")]
    public Image flashReussite;   // image plein ecran verte
    public float dureeFlashReussite = 0.4f;

    // =====================================================================
    // PARAMETRES GAMEPLAY
    // =====================================================================

    [Header("changements de direction")]
    public float tempsMinAvantFlip = 3f;
    public float tempsMaxAvantFlip = 7f;
    private float prochainFlip = 0f;
    private float sensOrbite = 1f;  // 1 = horaire, -1 = anti-horaire

    [Header("params cible")]
    public float rayonOrbite = 100f;
    public float vitesseOrbiteInitiale = 30f;       // degres par seconde
    public float vitesseOrbiteMax = 90f;
    public float accelerationOrbite = 0.5f;          // acceleration par seconde de jeu
    public float rayonTolerance = 55f;                // distance max curseur-cible pour compter

    [Header("params remplissage")]
    public float multiplicateurRemplissage = 0.06f;
    public float penaliteHorsZone = 0.01f;
    public float tempsMaxAvantEchec = 30f;

    [Header("params visuels")]
    public float rayonCurseur = 130f;                 // rayon max du curseur dans la zone

    [Header("difficulte progressive evenement")]
    public float vitesseOrbiteInitialeDebutant = 30f;
    public float vitesseOrbiteInitialeExpert = 50f;
    public float rayonToleranceDebutant = 55f;
    public float rayonToleranceExpert = 35f;
    public float tempsMinFlipDebutant = 3f;
    public float tempsMinFlipExpert = 1.5f;
    public float tempsMaxFlipDebutant = 7f;
    public float tempsMaxFlipExpert = 3f;

    [Header("couleurs cible")]
    public Color couleurCibleDedans = new Color(0.2f, 0.9f, 0.2f, 0.4f);
    public Color couleurCibleDehors = new Color(0.9f, 0.2f, 0.2f, 0.2f);
    public Color couleurBordureDedans = new Color(0.2f, 0.9f, 0.2f, 0.8f);
    public Color couleurBordureDehors = new Color(0.9f, 0.2f, 0.2f, 0.4f);
    public Color couleurCurseurDedans = new Color(0.3f, 1f, 0.3f, 1f);
    public Color couleurCurseurDehors = new Color(1f, 1f, 1f, 0.8f);

    [Header("animation")]
    public float vitessePulseBordure = 3f;
    public float amplitudePulseBordure = 0.15f;

    // =====================================================================
    // REFERENCES 3D ET SYSTEME
    // =====================================================================

    [Header("visuels 3d")]
    public MeshRenderer eauRenderer;
    public GameObject[] meshsVortex;
    [Header("animation tornade")]
    public Transform meshTornade;          // le mesh tornade specifique dans meshsVortex
    public float vitesseDeplacement = 2f;
    public float intervalleChangement = 1.5f;
    [Header("shake becher")]
    public Transform becherTransform;
    public float shakeIntensiteMax = 8f;      // degres max de rotation
    public float shakeVitesse = 12f;

    [Header("post-processing")]
    public Volume volumePostProcess;
    private ColorAdjustments colorAdjustments;
    private float saturationInitiale;
    public float saturationMinimale = -60f;

    [Header("references systeme")]
    public GameManager gameManager;
    public OSCTransmitter oscTransmitter;
    public StationTourbillonFeedback stationTourbillon;
    public CanvasGroup colonneTourbillon;

    // =====================================================================
    // VARIABLES PRIVEES
    // =====================================================================

    private enum PhaseVortex { EnCours, Resolu, Echec }
    private PhaseVortex phaseActuelle;

    private float progression = 0f;
    private float progressionAffichee = 0f;
    private float chronoTotal = 0f;

    // orbite
    private float angleOrbite = 0f;
    private float vitesseOrbiteActuelle;

    // position joystick normalisee (-1 a 1)
    private float joystickX = 0f;
    private float joystickY = 0f;

    // etat
    private bool curseurDansCible = false;

    private Vector3 tornadePosInitiale;
    private Vector3 tornadeCiblePos;
    private float chronoTornade = 0f;
    private float chronoChangement = 0f;

    private Quaternion becherRotationInitiale;

    // =====================================================================
    // INITIALISATION
    // =====================================================================

    void OnEnable()
    {
        DemarrerEvenement();
    }

    void DemarrerEvenement()
    {
        phaseActuelle = PhaseVortex.EnCours;
        progression = 0f;
        progressionAffichee = 0f;
        chronoTotal = 0f;
        angleOrbite = 0f;
        vitesseOrbiteActuelle = vitesseOrbiteInitiale;
        sensOrbite = 1f;
        prochainFlip = Time.time + Random.Range(tempsMinAvantFlip, tempsMaxAvantFlip);
        joystickX = 0f;
        joystickY = 0f;
        curseurDansCible = false;

        // activer panel
        if (eventVortexPanel != null)
        {
            eventVortexPanel.SetActive(true);
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(true);
            texteAlert.text = "La potion se fait secouer... \r\nRéorientez-la!";
            StartCoroutine(FadeTexte());
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEventVortex();
        }

        // initialiser jauge
        if (jaugeProgression != null)
        {
            jaugeProgression.value = 0f;
        }

        // post-processing
        if (volumePostProcess != null && volumePostProcess.profile.TryGet(out colorAdjustments))
        {
            saturationInitiale = colorAdjustments.saturation.value;
            colorAdjustments.saturation.value = saturationMinimale;
        }

        // effets visuels 3d
        ActiverEffetsVisuels();
        if (meshTornade != null)
        {
            tornadePosInitiale = meshTornade.localPosition;
            tornadeCiblePos = PositionAleatoireTornade();
            tornadeCiblePos.y = tornadePosInitiale.y;
            chronoTornade = 0f;
            chronoChangement = 0f;
        }

        if (becherTransform != null)
        {
            becherRotationInitiale = becherTransform.localRotation;
        }

        // desactiver station tourbillon normale
        if (stationTourbillon != null)
        {
            stationTourbillon.enabled = false;
        }

        if (colonneTourbillon != null)
        {
            colonneTourbillon.alpha = 0.25f;
        }

        if (GameManager.Instance != null)
        {
            float d = GameManager.Instance.GetProgressionDifficulte();
            vitesseOrbiteInitiale = Mathf.Lerp(vitesseOrbiteInitialeDebutant, vitesseOrbiteInitialeExpert, d);
            rayonTolerance = Mathf.Lerp(rayonToleranceDebutant, rayonToleranceExpert, d);
            tempsMinAvantFlip = Mathf.Lerp(tempsMinFlipDebutant, tempsMinFlipExpert, d);
            tempsMaxAvantFlip = Mathf.Lerp(tempsMaxFlipDebutant, tempsMaxFlipExpert, d);
        }
        vitesseOrbiteActuelle = vitesseOrbiteInitiale;

        Debug.Log("EVENT VORTEX : demarre");
    }

    // =====================================================================
    // UPDATE PRINCIPAL
    // =====================================================================

    void Update()
    {
        if (phaseActuelle != PhaseVortex.EnCours) return;

        chronoTotal += Time.deltaTime;

        if (chronoTotal >= tempsMaxAvantEchec)
        {
            EchecEvenement();
            return;
        }

        // accelerer l'orbite progressivement
        vitesseOrbiteActuelle = Mathf.Min(
            vitesseOrbiteMax,
            vitesseOrbiteInitiale + accelerationOrbite * chronoTotal
        );

        // changement de direction aleatoire
        if (Time.time >= prochainFlip)
        {
            sensOrbite *= -1f;
            prochainFlip = Time.time + Random.Range(tempsMinAvantFlip, tempsMaxAvantFlip);
            Debug.Log($"VORTEX : flip direction, sens = {(sensOrbite > 0 ? "horaire" : "anti-horaire")}");
        }

        // deplacer cercle cible en orbite
        angleOrbite += vitesseOrbiteActuelle * sensOrbite * Time.deltaTime;
        if (angleOrbite >= 360f) angleOrbite -= 360f;

        UpdatePositionCible();
        UpdatePositionCurseur();
        UpdateDistanceEtFeedback();
        UpdateJauge();
        UpdateIndicateurDirection();

        // animation tornade
        if (meshTornade != null && phaseActuelle == PhaseVortex.EnCours)
        {
            chronoChangement += Time.deltaTime;

            if (chronoChangement >= intervalleChangement)
            {
                chronoChangement = 0f;
                tornadeCiblePos = PositionAleatoireTornade();
                tornadeCiblePos.y = meshTornade.localPosition.y;
            }

            meshTornade.localPosition = Vector3.Lerp(
                meshTornade.localPosition, tornadeCiblePos, Time.deltaTime * vitesseDeplacement
            );
        }

        // shake becher (diminue avec la progression)
        if (becherTransform != null && phaseActuelle == PhaseVortex.EnCours)
        {
            float intensite = shakeIntensiteMax * (1f - progression);
            float time = Time.time * shakeVitesse;

            float rotX = Mathf.PerlinNoise(time, 0f) * 2f - 1f;
            float rotY = Mathf.PerlinNoise(0f, time) * 2f - 1f;
            float rotZ = Mathf.PerlinNoise(time, time) * 2f - 1f;

            Quaternion shake = Quaternion.Euler(
                rotX * intensite,
                rotY * intensite * 0.5f,
                rotZ * intensite
            );

            becherTransform.localRotation = becherRotationInitiale * shake;
        }

        // post-processing progressif
        if (colorAdjustments != null)
        {
            float saturationCible = Mathf.Lerp(saturationMinimale, saturationInitiale, progression);
            colorAdjustments.saturation.value = Mathf.Lerp(
                colorAdjustments.saturation.value, saturationCible, Time.deltaTime * 3f
            );
        }

        if (progression >= 1f)
        {
            ResoudreEvenement();
        }
    }

    // =====================================================================
    // POSITIONS
    // =====================================================================

    void UpdatePositionCible()
    {
        if (cercleCible == null) return;

        float rad = angleOrbite * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * rayonOrbite;
        float y = Mathf.Sin(rad) * rayonOrbite;

        cercleCible.anchoredPosition = new Vector2(x, y);
    }

    void UpdatePositionCurseur()
    {
        if (curseurJoueur == null) return;

        float x = joystickX * rayonCurseur;
        float y = joystickY * rayonCurseur;

        curseurJoueur.anchoredPosition = new Vector2(x, y);
    }

    // =====================================================================
    // DETECTION ET FEEDBACK
    // =====================================================================

    void UpdateDistanceEtFeedback()
    {
        if (cercleCible == null || curseurJoueur == null) return;

        // calculer distance entre curseur et centre de la cible
        float distance = Vector2.Distance(
            curseurJoueur.anchoredPosition,
            cercleCible.anchoredPosition
        );

        curseurDansCible = distance <= rayonTolerance;

        // remplissage ou penalite
        if (curseurDansCible)
        {
            // bonus proportionnel a la proximite du centre
            float facteur = 1f - (distance / rayonTolerance);
            facteur = Mathf.Clamp01(facteur);
            progression += facteur * multiplicateurRemplissage * Time.deltaTime;
        }
        else
        {
            progression -= penaliteHorsZone * Time.deltaTime;
        }
        progression = Mathf.Clamp01(progression);

        // couleurs cible
        if (cercleCibleFond != null)
        {
            Color cible = curseurDansCible ? couleurCibleDedans : couleurCibleDehors;
            cercleCibleFond.color = Color.Lerp(cercleCibleFond.color, cible, Time.deltaTime * 10f);
        }

        if (cercleCibleBordure != null)
        {
            Color cible = curseurDansCible ? couleurBordureDedans : couleurBordureDehors;

            if (curseurDansCible)
            {
                float sin = Mathf.Sin(Time.time * vitessePulseBordure * Mathf.PI);
                cible.a = cible.a - amplitudePulseBordure + amplitudePulseBordure * sin;

                // pulse scale aussi
                float scalePulse = 1f + 0.02f * sin;
                cercleCibleBordure.rectTransform.localScale = Vector3.one * scalePulse;
            }
            else
            {
                cercleCibleBordure.rectTransform.localScale = Vector3.one;
            }

            cercleCibleBordure.color = cible;
        }

        // couleur curseur
        if (curseurImage != null)
        {
            Color cible = curseurDansCible ? couleurCurseurDedans : couleurCurseurDehors;
            curseurImage.color = Color.Lerp(curseurImage.color, cible, Time.deltaTime * 10f);
        }

        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"VORTEX : curseur={curseurJoueur.anchoredPosition}, cible={cercleCible.anchoredPosition}, distance={distance:F1}, tolerance={rayonTolerance}, dedans={curseurDansCible}, progression={progression:F3}");
        }
    }

    // =====================================================================
    // JAUGE
    // =====================================================================

    void UpdateJauge()
    {
        progressionAffichee = Mathf.Lerp(progressionAffichee, progression, Time.deltaTime * 5f);

        if (jaugeProgression != null)
        {
            jaugeProgression.value = progressionAffichee;
        }

        if (fillJauge != null)
        {
            fillJauge.color = new Color(1f, 0.251f, 0.918f, 1f);
        }
    }

    // =====================================================================
    // INDICATEUR DIRECTION
    // =====================================================================

    void UpdateIndicateurDirection()
    {
        if (indicateurDirection == null) return;

        // tourner la fleche dans le sens de l'orbite
        indicateurDirection.localRotation = Quaternion.Euler(0f, 0f, -angleOrbite);
    }

    // =====================================================================
    // RECEPTION JOYSTICK (appele par OSCInputManager)
    // =====================================================================

    public void UpdateJoystick(int faderX, int faderY)
    {
        // normaliser de 0-1024 vers -1 a 1, avec inversion
        float rawX = (faderX - 512f) / 512f;
        float rawY = (faderY - 512f) / 512f;

        // meme correction que StationTourbillonFeedback
        joystickX = -rawX;
        joystickY = -rawY;
    }

    // =====================================================================
    // EFFETS VISUELS 3D
    // =====================================================================

    void ActiverEffetsVisuels()
    {
        if (meshsVortex != null)
        {
            foreach (GameObject mesh in meshsVortex)
            {
                if (mesh != null) mesh.SetActive(true);
            }
        }

        EnvoyerOSCLumiere(true);
    }

    // =====================================================================
    // RESOLUTION / ECHEC
    // =====================================================================

    void ResoudreEvenement()
    {
        phaseActuelle = PhaseVortex.Resolu;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEventReussi();
        }
        StartCoroutine(FlashReussiteCoroutine());
        DesactiverEffets();

        if (gameManager != null)
        {
            gameManager.EvenementResolu();
        }
        if (StabilityManager.Instance != null)
        {
            StabilityManager.Instance.BonusEvenement();
        }
        if (EventManager.Instance != null)
        {
            EventManager.Instance.EvenementTermine();
        }

        Invoke(nameof(DesactiverEvent), 1f);

        Debug.Log("EVENT VORTEX : resolu");
    }

    void EchecEvenement()
    {
        phaseActuelle = PhaseVortex.Echec;

        DesactiverEffets();

        if (gameManager != null)
        {
            gameManager.EvenementEchoue();
        }

        Debug.Log("EVENT VORTEX : echec");
    }

    // =====================================================================
    // NETTOYAGE
    // =====================================================================

    void DesactiverEffets()
    {
        if (meshsVortex != null)
        {
            foreach (GameObject mesh in meshsVortex)
            {
                if (mesh != null) mesh.SetActive(false);
            }
        }

        if (eventVortexPanel != null)
        {
            eventVortexPanel.SetActive(false);
        }

        if (meshTornade != null)
        {
            meshTornade.localPosition = tornadePosInitiale;
        }

        if (becherTransform != null)
        {
            becherTransform.localRotation = becherRotationInitiale;
        }

        if (stationTourbillon != null)
        {
            stationTourbillon.enabled = true;
        }

        if (colonneTourbillon != null)
        {
            colonneTourbillon.alpha = 1f;
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(false);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = saturationInitiale;
        }

        EnvoyerOSCLumiere(false);
    }

    IEnumerator FadeTexte()
    {
        if (texteAlert == null) yield break;
        yield return new WaitForSeconds(dureeAffichageTexte);
        float elapsed = 0f;
        float duree = 1f;
        Color c = texteAlert.color;
        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - (elapsed / duree);
            texteAlert.color = c;
            yield return null;
        }
        texteAlert.gameObject.SetActive(false);
    }

    IEnumerator FlashReussiteCoroutine()
    {
        if (flashReussite == null) yield break;

        flashReussite.gameObject.SetActive(true);
        Color c = flashReussite.color;
        c.a = 0.25f;
        flashReussite.color = c;

        float elapsed = 0f;
        while (elapsed < dureeFlashReussite)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.25f, 0f, elapsed / dureeFlashReussite);
            flashReussite.color = c;
            yield return null;
        }

        flashReussite.gameObject.SetActive(false);
    }

    Vector3 PositionAleatoireTornade()
    {
        // 4 coins de la zone visible
        // on choisit un point aleatoire dans le quadrilatere
        float t1 = Random.Range(0f, 1f);
        float t2 = Random.Range(0f, 1f);

        // interpolation bilineaire entre les 4 coins
        Vector3 procheGauche = new Vector3(365f, 0f, 83f);
        Vector3 procheDroit = new Vector3(338.5f, 0f, 83f);
        Vector3 loinDroit = new Vector3(305.6f, 0f, 5.8f);
        Vector3 loinGauche = new Vector3(399.7f, 0f, -12.6f);

        Vector3 haut = Vector3.Lerp(procheGauche, procheDroit, t1);
        Vector3 bas = Vector3.Lerp(loinGauche, loinDroit, t1);
        Vector3 pos = Vector3.Lerp(haut, bas, t2);

        return pos;
    }
    void EnvoyerOSCLumiere(bool allumer)
    {
        if (oscTransmitter == null) return;

        var message = new OSCMessage("/lumiere/vortex");
        message.AddValue(OSCValue.Int(allumer ? 1 : 0));
        oscTransmitter.Send(message);
    }

    void DesactiverEvent()
    {
        gameObject.SetActive(false);
    }
}