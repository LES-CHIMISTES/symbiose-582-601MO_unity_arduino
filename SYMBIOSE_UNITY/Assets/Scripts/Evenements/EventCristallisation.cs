using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using extOSC;
using TMPro;
public class EventCristallisation : MonoBehaviour
{
    // =====================================================================
    // REFERENCES UI
    // =====================================================================

    [Header("panel principal")]
    public GameObject eventCristallisationPanel;

    [Header("cercles sequence (ordre : bleu, vert, blanc)")]
    public Image[] cerclesFond;             // taille 3
    public RectTransform[] anneauxApproche; // taille 3
    public Image[] anneauxImage;            // taille 3

    [Header("Flux poudre")]
    public FluxPoudreController fluxPoudre;

    [Header("overlays erreur (ordre : bleu, vert, blanc)")]
    public Image[] overlaysErreur;          // taille 3, un par cercle

    [Header("indicateurs timing (ordre : bleu, vert, blanc)")]
    public GameObject[] groupesExclamation; // taille 3, un par cercle

    [Header("jauge progression")]
    public Slider jaugeProgression;
    public Image fillJauge;

    [Header("ui texte")]
    public TextMeshProUGUI texteAlert;
    public float dureeAffichageTexte = 3f;

    [Header("flash reussite")]
    public Image flashReussite;   // image plein ecran verte
    public float dureeFlashReussite = 0.4f;

    // =====================================================================
    // PARAMETRES GAMEPLAY
    // =====================================================================

    [Header("params sequence")]
    public int cyclesRequis = 5;
    public float dureeAnneauInitiale = 1.5f;
    public float dureeAnneauMinimale = 0.6f;
    public float accelerationParCycle = 0.15f;
    public float fenetreToleranceAvant = 0.5f;
    public float fenetreToleranceApres = 0.4f;
    public float tempsMaxAvantEchec = 30f;

    [Header("params penalite")]
    public float penaliteErreur = 0.1f;
    public float dureeFlashErreur = 0.2f;

    [Header("params visuels")]
    public float alphaInactif = 0.3f;
    public float alphaActif = 1f;
    public float tailleAnneauDepart = 2.5f;
    public float tailleAnneauCible = 1f;

    [Header("animation reussite")]
    public float dureeScaleOut = 0.25f;
    public float scaleOutMax = 1.5f;

    [Header("difficulte progressive evenement")]
    public float dureeAnneauInitialeDebutant = 1.5f;
    public float dureeAnneauInitialeExpert = 0.9f;
    public float dureeAnneauMinimaleDebutant = 0.6f;
    public float dureeAnneauMinimaleExpert = 0.35f;
    public int cyclesRequisDebutant = 5;
    public int cyclesRequisExpert = 7;
    public float toleranceAvantDebutant = 0.5f;
    public float toleranceAvantExpert = 0.3f;
    public float toleranceApresDebutant = 0.4f;
    public float toleranceApresExpert = 0.25f;

    // =====================================================================
    // REFERENCES 3D ET SYSTEME
    // =====================================================================

    [Header("visuels 3d")]
    public MeshRenderer eauRenderer;
    public Material materielCristallise;
    public GameObject[] meshsCristaux;

    [Header("post-processing")]
    public Volume volumePostProcess;
    private ColorAdjustments colorAdjustments;
    private float saturationInitiale;
    public float saturationMinimale = -60f;

    [Header("references systeme")]
    public GameManager gameManager;
    public OSCTransmitter oscTransmitter;
    public StationPoudresFeedback stationPoudres;
    public CanvasGroup colonnePoudres;

    // =====================================================================
    // VARIABLES PRIVEES
    // =====================================================================

    private enum PhaseCristallisation { EnCours, Resolu, Echec }
    private PhaseCristallisation phaseActuelle;

    private int indexActuel = 0;
    private int cyclesCompletes = 0;
    private float progression = 0f;
    private float progressionAffichee = 0f;
    private float chronoTotal = 0f;

    private float dureeAnneauActuelle;
    private float chronoAnneau = 0f;
    private bool anneauActif = false;
    private bool appuiAccepteCeStep = false;

    private bool enAnimationReussite = false;
    private Material materielEauInitial;
    private Vector3[] scalesInitiauxCercles;

    private float chronoBlink = 0f;
    private bool blinkVisible = true;

    // =====================================================================
    // INITIALISATION
    // =====================================================================

    void OnEnable()
    {
        DemarrerEvenement();
    }

    void DemarrerEvenement()
    {
        phaseActuelle = PhaseCristallisation.EnCours;
        indexActuel = 0;
        cyclesCompletes = 0;
        progression = 0f;
        progressionAffichee = 0f;
        chronoTotal = 0f;
        enAnimationReussite = false;
        chronoBlink = 0f;
        blinkVisible = true;

        dureeAnneauActuelle = dureeAnneauInitiale;

        // sauvegarder scales initiaux
        scalesInitiauxCercles = new Vector3[cerclesFond.Length];
        for (int i = 0; i < cerclesFond.Length; i++)
        {
            if (cerclesFond[i] != null)
            {
                scalesInitiauxCercles[i] = cerclesFond[i].rectTransform.localScale;
            }
        }

        // activer panel
        if (eventCristallisationPanel != null)
        {
            eventCristallisationPanel.SetActive(true);
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(true);
            texteAlert.text = "La potion se cristallise... \r\ncomplétez la séquence rythmée!";
            StartCoroutine(FadeTexte());
        }

        // initialiser jauge
        if (jaugeProgression != null)
        {
            jaugeProgression.value = 0f;
        }



        // cacher overlays erreur
        if (overlaysErreur != null)
        {
            foreach (Image overlay in overlaysErreur)
            {
                if (overlay != null) overlay.gameObject.SetActive(false);
            }
        }

        // cacher groupes exclamation
        if (groupesExclamation != null)
        {
            foreach (GameObject groupe in groupesExclamation)
            {
                if (groupe != null) groupe.SetActive(false);
            }
        }

        // post-processing
        if (volumePostProcess != null && volumePostProcess.profile.TryGet(out colorAdjustments))
        {
            saturationInitiale = colorAdjustments.saturation.value;
            colorAdjustments.saturation.value = saturationMinimale;
        }

        // effets visuels 3d
        ActiverEffetsVisuels();

        // desactiver station poudres normale
        if (stationPoudres != null)
        {
            stationPoudres.enabled = false;
        }

        if (colonnePoudres != null)
        {
            colonnePoudres.alpha = 0.25f;
        }

        if (GameManager.Instance != null)
        {
            float d = GameManager.Instance.GetProgressionDifficulte();
            dureeAnneauInitiale = Mathf.Lerp(dureeAnneauInitialeDebutant, dureeAnneauInitialeExpert, d);
            dureeAnneauMinimale = Mathf.Lerp(dureeAnneauMinimaleDebutant, dureeAnneauMinimaleExpert, d);
            cyclesRequis = Mathf.RoundToInt(Mathf.Lerp(cyclesRequisDebutant, cyclesRequisExpert, d));
            fenetreToleranceAvant = Mathf.Lerp(toleranceAvantDebutant, toleranceAvantExpert, d);
            fenetreToleranceApres = Mathf.Lerp(toleranceApresDebutant, toleranceApresExpert, d);
        }

        // demarrer premier anneau
        InitialiserStep();

        Debug.Log($"EVENT CRISTALLISATION : demarre, {cyclesRequis} cycles requis");
    }

    // =====================================================================
    // UPDATE PRINCIPAL
    // =====================================================================

    void Update()
    {
        if (phaseActuelle != PhaseCristallisation.EnCours) return;

        chronoTotal += Time.deltaTime;

        if (chronoTotal >= tempsMaxAvantEchec)
        {
            EchecEvenement();
            return;
        }

        // update anneau
        if (anneauActif && !enAnimationReussite)
        {
            chronoAnneau += Time.deltaTime;

            // calculer scale de l'anneau
            float t = Mathf.Clamp01(chronoAnneau / dureeAnneauActuelle);
            float scale = Mathf.Lerp(tailleAnneauDepart, tailleAnneauCible, t);

            if (anneauxApproche[indexActuel] != null)
            {
                anneauxApproche[indexActuel].localScale = Vector3.one * scale;
            }

            // update opacite anneau
            if (anneauxImage[indexActuel] != null)
            {
                Color c = anneauxImage[indexActuel].color;
                c.a = Mathf.Lerp(0.3f, 0.9f, t);
                anneauxImage[indexActuel].color = c;
            }

            // indicateurs "!" quand on entre dans la fenetre de tolerance
            float tempsAvantCible = dureeAnneauActuelle - chronoAnneau;
            bool dansFenetre = tempsAvantCible <= fenetreToleranceAvant;

            if (groupesExclamation != null && indexActuel < groupesExclamation.Length && groupesExclamation[indexActuel] != null)
            {
                if (dansFenetre)
                {
                    groupesExclamation[indexActuel].SetActive(true);

                    chronoBlink += Time.deltaTime;
                    if (chronoBlink >= 0.12f)
                    {
                        chronoBlink = 0f;
                        blinkVisible = !blinkVisible;
                    }

                    foreach (Transform enfant in groupesExclamation[indexActuel].transform)
                    {
                        enfant.gameObject.SetActive(blinkVisible);
                    }
                }
                else
                {
                    groupesExclamation[indexActuel].SetActive(false);
                }
            }

            // trop tard
            if (chronoAnneau > dureeAnneauActuelle + fenetreToleranceApres)
            {
                StartCoroutine(FlashErreur());
                progression = Mathf.Max(0f, progression - penaliteErreur);
                InitialiserStep();
            }
        }

        // jauge smooth
        progressionAffichee = Mathf.Lerp(progressionAffichee, progression, Time.deltaTime * 5f);
        if (jaugeProgression != null)
        {
            jaugeProgression.value = progressionAffichee;
        }
        if (fillJauge != null)
        {
            fillJauge.color = new Color(0.125f, 1f, 0.522f, 1f);
        }

        // post-processing progressif
        if (colorAdjustments != null)
        {
            float saturationCible = Mathf.Lerp(saturationMinimale, saturationInitiale, progression);
            colorAdjustments.saturation.value = Mathf.Lerp(
                colorAdjustments.saturation.value, saturationCible, Time.deltaTime * 3f
            );
        }
    }

    // =====================================================================
    // RECEPTION INPUT
    // =====================================================================

    public void AppuyerBouton(int keyNumber)
    {
        if (phaseActuelle != PhaseCristallisation.EnCours) return;
        if (enAnimationReussite) return;
        if (!anneauActif) return;

        int indexBouton = KeyNumberToIndex(keyNumber);
        float tempsAvantCible = dureeAnneauActuelle - chronoAnneau;

        Debug.Log($"CRISTAL : key={keyNumber}, index={indexBouton}, attendu={indexActuel}, tempsAvant={tempsAvantCible:F2}");

        // mauvais bouton
        if (indexBouton != indexActuel)
        {
            StartCoroutine(FlashErreur());
            progression = Mathf.Max(0f, progression - penaliteErreur);
            return;
        }

        // trop tot
        if (tempsAvantCible > fenetreToleranceAvant)
        {
            return;
        }

        // bon bouton, bon timing
        // bon bouton, bon timing
        appuiAccepteCeStep = true;
        anneauActif = false;

        // changer couleur du flux
        if (fluxPoudre != null)
        {
            int keyDuCercle = IndexToKeyNumber(indexActuel);
            fluxPoudre.ChangerCouleur(keyDuCercle);
        }

        StartCoroutine(AnimationReussiteStep());
    }

    int IndexToKeyNumber(int index)
    {
        // inverse du mapping KeyNumberToIndex
        switch (index)
        {
            case 0: return 2; // bleu = key2
            case 1: return 1; // vert = key1
            case 2: return 3; // blanc = key3
            default: return 1;
        }
    }
    int KeyNumberToIndex(int keyNumber)
    {
        // mapping : key1=vert=index1, key2=bleu=index0, key3=blanc=index2
        // MODIFIE selon ta configuration physique
        switch (keyNumber)
        {
            case 1: return 1; // vert
            case 2: return 0; // bleu
            case 3: return 2; // blanc
            default: return -1;
        }
    }

    // =====================================================================
    // GESTION STEPS ET CYCLES
    // =====================================================================

    void InitialiserStep()
    {
        chronoAnneau = 0f;
        anneauActif = true;
        appuiAccepteCeStep = false;
        enAnimationReussite = false;
        chronoBlink = 0f;
        blinkVisible = true;

        for (int i = 0; i < cerclesFond.Length; i++)
        {
            if (cerclesFond[i] != null)
            {
                Color c = cerclesFond[i].color;
                c.a = (i == indexActuel) ? alphaActif : alphaInactif;
                cerclesFond[i].color = c;

                cerclesFond[i].rectTransform.localScale = scalesInitiauxCercles[i];
                cerclesFond[i].gameObject.SetActive(true);
            }

            if (anneauxApproche[i] != null)
            {
                if (i == indexActuel)
                {
                    anneauxApproche[i].gameObject.SetActive(true);
                    anneauxApproche[i].localScale = Vector3.one * tailleAnneauDepart;

                    if (anneauxImage[i] != null)
                    {
                        Color c = anneauxImage[i].color;
                        c.a = 0.3f;
                        anneauxImage[i].color = c;
                    }
                }
                else
                {
                    anneauxApproche[i].gameObject.SetActive(false);
                }
            }
        }

        // cacher tous les groupes exclamation
        if (groupesExclamation != null)
        {
            for (int i = 0; i < groupesExclamation.Length; i++)
            {
                if (groupesExclamation[i] != null)
                {
                    groupesExclamation[i].SetActive(false);
                }
            }
        }

        // cacher overlays erreur
        if (overlaysErreur != null)
        {
            for (int i = 0; i < overlaysErreur.Length; i++)
            {
                if (overlaysErreur[i] != null)
                {
                    overlaysErreur[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void PasserAuStepSuivant()
    {
        indexActuel++;

        if (indexActuel >= 3)
        {
            cyclesCompletes++;
            progression = (float)cyclesCompletes / cyclesRequis;
            progression = Mathf.Clamp01(progression);

            Debug.Log($"EVENT CRISTALLISATION : cycle {cyclesCompletes}/{cyclesRequis} complete");

            if (cyclesCompletes >= cyclesRequis)
            {
                ResoudreEvenement();
                return;
            }

            // accelerer
            dureeAnneauActuelle = Mathf.Max(
                dureeAnneauMinimale,
                dureeAnneauActuelle - accelerationParCycle
            );

            indexActuel = 0;
        }

        InitialiserStep();
    }

    // =====================================================================
    // ANIMATIONS
    // =====================================================================

    IEnumerator AnimationReussiteStep()
    {
        enAnimationReussite = true;

        // cacher anneau
        if (anneauxApproche[indexActuel] != null)
        {
            anneauxApproche[indexActuel].gameObject.SetActive(false);
        }

        // cacher exclamations
        if (groupesExclamation != null && indexActuel < groupesExclamation.Length && groupesExclamation[indexActuel] != null)
        {
            groupesExclamation[indexActuel].SetActive(false);
        }

        // scale out + fade du cercle actif
        Image cercle = cerclesFond[indexActuel];

        if (cercle == null)
        {
            enAnimationReussite = false;
            PasserAuStepSuivant();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < dureeScaleOut)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dureeScaleOut;

            float scale = Mathf.Lerp(1f, scaleOutMax, t);
            cercle.rectTransform.localScale = scalesInitiauxCercles[indexActuel] * scale;

            Color c = cercle.color;
            c.a = Mathf.Lerp(alphaActif, 0f, t);
            cercle.color = c;

            yield return null;
        }

        cercle.gameObject.SetActive(false);
        cercle.rectTransform.localScale = scalesInitiauxCercles[indexActuel];

        enAnimationReussite = false;
        PasserAuStepSuivant();
    }

    IEnumerator FlashErreur()
    {
        if (overlaysErreur == null || indexActuel >= overlaysErreur.Length) yield break;

        Image overlay = overlaysErreur[indexActuel];
        if (overlay == null) yield break;

        overlay.gameObject.SetActive(true);
        Color c = overlay.color;
        c.a = 0.7f;
        overlay.color = c;

        float elapsed = 0f;
        while (elapsed < dureeFlashErreur)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.7f, 0f, elapsed / dureeFlashErreur);
            overlay.color = c;
            yield return null;
        }

        overlay.gameObject.SetActive(false);
    }

    // =====================================================================
    // EFFETS VISUELS 3D
    // =====================================================================

    void ActiverEffetsVisuels()
    {
        if (eauRenderer != null)
        {
            materielEauInitial = eauRenderer.material;
            if (materielCristallise != null)
            {
                eauRenderer.material = materielCristallise;
            }
        }

        if (meshsCristaux != null)
        {
            foreach (GameObject mesh in meshsCristaux)
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
        phaseActuelle = PhaseCristallisation.Resolu;
        StartCoroutine(FlashReussiteCoroutine());
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

        Debug.Log("EVENT CRISTALLISATION : resolu");
    }

    void EchecEvenement()
    {
        phaseActuelle = PhaseCristallisation.Echec;

        DesactiverEffets();

        if (gameManager != null)
        {
            gameManager.EvenementEchoue();
        }
        if (FluxPoudreController.Instance != null)
        {
            FluxPoudreController.Instance.Arreter();
        }

        Debug.Log("EVENT CRISTALLISATION : echec");
    }

    // =====================================================================
    // NETTOYAGE
    // =====================================================================

    void DesactiverEffets()
    {
        if (eauRenderer != null && materielEauInitial != null)
        {
            eauRenderer.material = materielEauInitial;
        }
        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(false);
        }
        if (meshsCristaux != null)
        {
            foreach (GameObject mesh in meshsCristaux)
            {
                if (mesh != null) mesh.SetActive(false);
            }
        }

        if (eventCristallisationPanel != null)
        {
            eventCristallisationPanel.SetActive(false);
        }

        if (stationPoudres != null)
        {
            stationPoudres.enabled = true;
        }

        if (colonnePoudres != null)
        {
            colonnePoudres.alpha = 1f;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = saturationInitiale;
        }

        if (overlaysErreur != null)
        {
            foreach (Image overlay in overlaysErreur)
            {
                if (overlay != null) overlay.gameObject.SetActive(false);
            }
        }

        if (groupesExclamation != null)
        {
            foreach (GameObject groupe in groupesExclamation)
            {
                if (groupe != null) groupe.SetActive(false);
            }
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

    void EnvoyerOSCLumiere(bool allumer)
    {
        if (oscTransmitter == null) return;

        var message = new OSCMessage("/lumiere/cristallisation");
        message.AddValue(OSCValue.Int(allumer ? 1 : 0));
        oscTransmitter.Send(message);
    }

    void DesactiverEvent()
    {
        gameObject.SetActive(false);
    }
}