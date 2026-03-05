using UnityEngine;
using UnityEngine.UI;
using TMPro;
using extOSC;
using System.Collections;

public class EventGel : MonoBehaviour
{
    [Header("ui panel")]
    public GameObject eventGelPanel;

    [Header("ui jauge")]
    public Slider jaugeTemperature;
    public Image fillJauge;

    [Header("ui texte")]
    public TextMeshProUGUI texteAlert;
    public float dureeAffichageTexte = 3f;

    [Header("flash reussite")]
    public Image flashReussite;   // image plein ecran verte
    public float dureeFlashReussite = 0.4f;

    [Header("ui pattern")]
    public GameObject patternContainer;
    public RectTransform knobsContainer;
    public GameObject prefabFondKnob;
    public GameObject prefabIndicateurCible;
    public GameObject prefabIndicateurDynamique;
    public GameObject prefabFleche;
    public GameObject prefabCercleProgression;
    public float tailleKnob = 100f;

    [Header("knob phase 1")]
    public RectTransform knobDynamiquePhase1;
    public Image knobMaxIndicateur;
    public Image flecheCourbe;

    [Header("params phase 1")]
    public float seuilIntensiteMax = 1028f;
    public float dureePhase1 = 3f;

    [Header("params phase 2")]
    public float tolerancePattern = 200f;
    public float tempsMaxParEtape = 5f;
    public float vitesseGelInaction = 5f;

    [Header("difficulte progressive")]
    public int nbKnobsMin = 4;
    public int nbKnobsMax = 10;
    public float tempsSeuilNbKnobs = 360f;

    [Header("visuels")]
    public MeshRenderer potionRenderer;
    public GameObject[] meshsGel;
    public Image vignetteGel;
    public Color couleurGel = new Color(0.7f, 0.9f, 1f);

    [Header("references")]
    public GameManager gameManager;
    public OSCTransmitter oscTransmitter;
    public MeshEauController meshEau;
    public StationFeuFeedback stationFeu;

    [Header("difficulte progressive evenement")]
    public float tolerancePatternInitiale = 200f;
    public float tolerancePatternFinale = 100f;
    public float tempsMaxParEtapeInitial = 5f;
    public float tempsMaxParEtapeFinal = 3f;

    public CanvasGroup colonneFeu;

    private enum PhaseGel { Phase1, Phase2, Resolu, Echec }
    private PhaseGel phaseActuelle;

    private float niveauFroid;
    private float chronoPhase1;
    private int valeurPotentiometre;
    private int valeurPotentiometrePrecedente;

    private int nbKnobsTotal;
    private int[] positionsCibles;
    private int etapeActuelle;
    private float chronoEtape;

    private GameObject[] fondsKnobs;
    private GameObject[] indicateursCibles;
    private GameObject[] indicateursDynamiques;
    private GameObject[] fleches;
    private GameObject[] cerclesProgression;

    private Coroutine fadeEnCours;

    private bool validationEnCours = false;

    private float chronoMaintienCible = 0f;
    private float tempsMaintienRequis = 1f;

    void OnEnable()
    {
        DemarrerEvenement();
    }

    void DemarrerEvenement()
    {
        phaseActuelle = PhaseGel.Phase1;
        niveauFroid = 100f;
        chronoPhase1 = 0f;
        etapeActuelle = 0;
        valeurPotentiometrePrecedente = valeurPotentiometre;

        if (eventGelPanel != null)
        {
            eventGelPanel.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            float progression = Mathf.Clamp01(GameManager.Instance.tempsEcoule / tempsSeuilNbKnobs);
            nbKnobsTotal = Mathf.RoundToInt(Mathf.Lerp(nbKnobsMin, nbKnobsMax, progression));
        }
        else
        {
            nbKnobsTotal = nbKnobsMin;
        }

        if (GameManager.Instance != null)
        {
            float d = GameManager.Instance.GetProgressionDifficulte();
            tolerancePattern = Mathf.Lerp(tolerancePatternInitiale, tolerancePatternFinale, d);
            tempsMaxParEtape = Mathf.Lerp(tempsMaxParEtapeInitial, tempsMaxParEtapeFinal, d);
        }

        GenererPositionsCibles();

        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(true);
            jaugeTemperature.value = 1f;
        }

        if (patternContainer != null)
        {
            patternContainer.SetActive(false);
        }

        if (knobDynamiquePhase1 != null)
        {
            knobDynamiquePhase1.gameObject.SetActive(true);
        }

        if (knobMaxIndicateur != null)
        {
            knobMaxIndicateur.gameObject.SetActive(true);
            StartCoroutine(ClignoterKnobMax());
        }

        if (flecheCourbe != null)
        {
            flecheCourbe.gameObject.SetActive(true);
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(true);
            texteAlert.text = "La potion se gèle... allumez le feu!";
            StartCoroutine(FadeTexte());
        }

        ActiverEffetsVisuels();

        if (stationFeu != null)
        {
            stationFeu.enabled = false;
        }

        if (colonneFeu != null)
        {
            colonneFeu.alpha = 0.25f;
        }
        if (ParticulesPotionController.Instance != null)
        {
            ParticulesPotionController.Instance.SetEtat("gel"); // ou "evaporation", "cristallisation", "vortex"
        }
        Debug.Log($"EVENT GEL : demarre, {nbKnobsTotal} knobs");
    }

    IEnumerator ClignoterKnobMax()
    {
        if (knobMaxIndicateur == null) yield break;

        while (phaseActuelle == PhaseGel.Phase1)
        {
            float elapsed = 0f;
            float duree = 0.4f;
            Image img = knobMaxIndicateur;
            Color c = img.color;

            while (elapsed < duree)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0.3f, 1f, elapsed / duree);
                img.color = c;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duree)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0.3f, elapsed / duree);
                img.color = c;
                yield return null;
            }
        }
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

    void GenererPositionsCibles()
    {
        positionsCibles = new int[nbKnobsTotal];

        for (int i = 0; i < nbKnobsTotal; i++)
        {
            if (i % 2 == 0)
            {
                positionsCibles[i] = Random.Range(300, 1000);
            }
            else
            {
                positionsCibles[i] = Random.Range(3000, 3700);
            }
        }
    }

    void Update()
    {
        if (phaseActuelle == PhaseGel.Resolu || phaseActuelle == PhaseGel.Echec) return;

        switch (phaseActuelle)
        {
            case PhaseGel.Phase1:
                GererPhase1();
                break;
            case PhaseGel.Phase2:
                GererPhase2();
                break;
        }

        if (jaugeTemperature != null)
        {
            jaugeTemperature.value = niveauFroid / 100f;
        }

        if (phaseActuelle == PhaseGel.Phase1 && knobDynamiquePhase1 != null)
        {
            float angle = Mathf.Lerp(-180f, 180f, valeurPotentiometre / 4096f);
            knobDynamiquePhase1.localRotation = Quaternion.Euler(0, 0, angle);
        }

        if (phaseActuelle == PhaseGel.Phase2 && indicateursDynamiques != null)
        {
            float angle = Mathf.Lerp(-180f, 180f, valeurPotentiometre / 4096f);
            foreach (GameObject indicateur in indicateursDynamiques)
            {
                if (indicateur != null)
                {
                    indicateur.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }
    }

    void GererPhase1()
    {
        if (valeurPotentiometre <= seuilIntensiteMax)
        {
            chronoPhase1 += Time.deltaTime;
            niveauFroid -= 10f * Time.deltaTime;
            niveauFroid = Mathf.Max(0f, niveauFroid);

            if (chronoPhase1 >= dureePhase1)
            {
                PasserPhase2();
            }
        }
        else
        {
            chronoPhase1 = 0f;
        }
    }

    void PasserPhase2()
    {
        phaseActuelle = PhaseGel.Phase2;
        etapeActuelle = 0;
        chronoEtape = 0f;

        if (knobMaxIndicateur != null)
        {
            knobMaxIndicateur.gameObject.SetActive(false);
        }

        if (flecheCourbe != null)
        {
            flecheCourbe.gameObject.SetActive(false);
        }

        if (knobDynamiquePhase1 != null)
        {
            knobDynamiquePhase1.gameObject.SetActive(false);
        }

        CreerUIPattern();

        Debug.Log("EVENT GEL : phase 2 - pattern");
    }

    void CreerUIPattern()
    {
        if (patternContainer != null)
        {
            patternContainer.SetActive(true);
        }

        int nbRows = nbKnobsTotal > 5 ? 2 : 1;
        int nbParRow = Mathf.CeilToInt(nbKnobsTotal / (float)nbRows);

        fondsKnobs = new GameObject[nbKnobsTotal];
        indicateursCibles = new GameObject[nbKnobsTotal];
        indicateursDynamiques = new GameObject[nbKnobsTotal];
        fleches = new GameObject[nbKnobsTotal - 1];

        float espacementX = tailleKnob + 40f;
        float espacementY = tailleKnob + 40f;

        // centrer le pattern
        float largeurTotale = (nbParRow - 1) * espacementX;
        float offsetX = -largeurTotale / 2f;

        for (int i = 0; i < nbKnobsTotal; i++)
        {
            int row = i / nbParRow;
            int col = i % nbParRow;
            Vector2 pos = new Vector2(offsetX + col * espacementX, -row * espacementY);

            // fond knob
            GameObject fond = Instantiate(prefabFondKnob, knobsContainer);
            RectTransform fondRT = fond.GetComponent<RectTransform>();
            fondRT.anchoredPosition = pos;
            fondRT.sizeDelta = new Vector2(tailleKnob, tailleKnob);
            fondsKnobs[i] = fond;

            // indicateur cible
            GameObject indicCible = Instantiate(prefabIndicateurCible, knobsContainer);
            RectTransform indicCibleRT = indicCible.GetComponent<RectTransform>();
            indicCibleRT.anchoredPosition = pos;
            indicCibleRT.sizeDelta = new Vector2(tailleKnob, tailleKnob);
            float angleCible = Mathf.Lerp(-180f, 180f, positionsCibles[i] / 4096f);
            indicCibleRT.localRotation = Quaternion.Euler(0, 0, angleCible);
            indicateursCibles[i] = indicCible;

            // indicateur dynamique
            GameObject indicDyn = Instantiate(prefabIndicateurDynamique, knobsContainer);
            RectTransform indicDynRT = indicDyn.GetComponent<RectTransform>();
            indicDynRT.anchoredPosition = pos;
            indicDynRT.sizeDelta = new Vector2(tailleKnob, tailleKnob);
            indicateursDynamiques[i] = indicDyn;

            if (i > 0)
            {
                SetAlphaGroupe(fond, indicCible, indicDyn, 0.5f);
            }

            // fleche
            if (i < nbKnobsTotal - 1)
            {
                int nextRow = (i + 1) / nbParRow;

                GameObject fleche = Instantiate(prefabFleche, knobsContainer);
                RectTransform flecheRT = fleche.GetComponent<RectTransform>();

                if (row == nextRow)
                {
                    flecheRT.anchoredPosition = new Vector2(offsetX + (col + 0.5f) * espacementX, -row * espacementY);
                }
                else
                {
                    flecheRT.anchoredPosition = new Vector2(offsetX + largeurTotale + 30f, -row * espacementY - espacementY / 2f);
                    flecheRT.localRotation = Quaternion.Euler(0, 0, -90f);
                }

                fleches[i] = fleche;
            }
        }

        // cercles progression (un par knob)
        cerclesProgression = new GameObject[nbKnobsTotal];
        for (int i = 0; i < nbKnobsTotal; i++)
        {
            if (prefabCercleProgression != null && fondsKnobs[i] != null)
            {
                GameObject cercle = Instantiate(prefabCercleProgression, fondsKnobs[i].transform);
                RectTransform cercleRT = cercle.GetComponent<RectTransform>();
                cercleRT.anchoredPosition = Vector2.zero;
                cercleRT.sizeDelta = new Vector2(tailleKnob + 30f, tailleKnob + 30f);
                cercleRT.SetAsLastSibling();

                // visible seulement pour premier
                cercle.SetActive(i == 0);

                cerclesProgression[i] = cercle;
            }
        }
    }

    void SetAlphaGroupe(GameObject fond, GameObject cible, GameObject dyn, float alpha)
    {
        Image imgFond = fond.GetComponent<Image>();
        if (imgFond != null)
        {
            Color c = imgFond.color;
            c.a = alpha;
            imgFond.color = c;
        }

        Image imgCible = cible.GetComponent<Image>();
        if (imgCible != null)
        {
            Color c = imgCible.color;
            c.a = alpha;
            imgCible.color = c;
        }

        Image imgDyn = dyn.GetComponent<Image>();
        if (imgDyn != null)
        {
            Color c = imgDyn.color;
            c.a = alpha;
            imgDyn.color = c;
        }
    }

    void GererPhase2()
    {
        chronoEtape += Time.deltaTime;

        if (chronoEtape >= tempsMaxParEtape)
        {
            EchecEvenement();
            return;
        }

        if (Mathf.Abs(valeurPotentiometre - valeurPotentiometrePrecedente) < 10)
        {
            niveauFroid += vitesseGelInaction * Time.deltaTime;
            niveauFroid = Mathf.Min(100f, niveauFroid);
        }

        valeurPotentiometrePrecedente = valeurPotentiometre;

        float diff = Mathf.Abs(valeurPotentiometre - positionsCibles[etapeActuelle]);

        if (cerclesProgression != null && cerclesProgression[etapeActuelle] != null)
        {
            // utiliser tolerance plus large pour le scale visuel
            float toleranceVisuelle = 800f; // plus large que tolerancePattern
            float proximite = 1f - Mathf.Clamp01(diff / toleranceVisuelle);

            // inverser : proche = petit, loin = grand
            cerclesProgression[etapeActuelle].transform.localScale = Vector3.one * Mathf.Lerp(1.5f, 0.8f, proximite);

            Image cercleImg = cerclesProgression[etapeActuelle].GetComponent<Image>();
            if (cercleImg != null)
            {
                Color c = cercleImg.color;
                c.a = 1f;
                cercleImg.color = c;
            }
        }

        // fade knob actuel selon temps restant (si pas en maintien)
        if (fondsKnobs != null && etapeActuelle < fondsKnobs.Length && diff > tolerancePattern)
        {
            float progression = chronoEtape / tempsMaxParEtape;
            float alphaKnob = Mathf.Lerp(1f, 0.5f, progression);

            SetAlphaGroupe(fondsKnobs[etapeActuelle], indicateursCibles[etapeActuelle], indicateursDynamiques[etapeActuelle], alphaKnob);
        }

        if (diff <= tolerancePattern)
        {
            chronoMaintienCible += Time.deltaTime;

            // blink pendant maintien
            float blinkSpeed = 8f;
            float alphaBlink = Mathf.Lerp(0.6f, 1f, (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f);
            SetAlphaGroupe(fondsKnobs[etapeActuelle], indicateursCibles[etapeActuelle], indicateursDynamiques[etapeActuelle], alphaBlink);

            if (chronoMaintienCible >= tempsMaintienRequis)
            {
                ValiderEtape();
            }
        }
        else
        {
            chronoMaintienCible = 0f; // reset si sort de zone
        }
    }

    void ValiderEtape()
    {

        if (validationEnCours) return; // empêcher double validation
        validationEnCours = true;
        chronoMaintienCible = 0f; // reset

        Debug.Log($"EVENT GEL : etape {etapeActuelle + 1}/{nbKnobsTotal} validee");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEtapeGel();
        }

        if (fadeEnCours != null)
        {
            StopCoroutine(fadeEnCours);
        }

        SetAlphaGroupe(fondsKnobs[etapeActuelle], indicateursCibles[etapeActuelle], indicateursDynamiques[etapeActuelle], 0.5f);

        if (etapeActuelle < fleches.Length && fleches[etapeActuelle] != null)
        {
            Image imgFleche = fleches[etapeActuelle].GetComponent<Image>();
            if (imgFleche != null)
            {
                Color c = imgFleche.color;
                c.a = 0.5f;
                imgFleche.color = c;
            }
        }

        niveauFroid -= 100f / nbKnobsTotal;
        niveauFroid = Mathf.Max(0f, niveauFroid);

        etapeActuelle++;
        chronoEtape = 0f;

        if (etapeActuelle >= nbKnobsTotal)
        {
            ResoudreEvenement();
        }
        else
        {
            SetAlphaGroupe(fondsKnobs[etapeActuelle], indicateursCibles[etapeActuelle], indicateursDynamiques[etapeActuelle], 1f);

            // cacher cercle etape precedente
            if (cerclesProgression != null && etapeActuelle > 0 && cerclesProgression[etapeActuelle - 1] != null)
            {
                cerclesProgression[etapeActuelle - 1].SetActive(false);
            }

            // afficher cercle etape suivante
            if (cerclesProgression != null && etapeActuelle < cerclesProgression.Length && cerclesProgression[etapeActuelle] != null)
            {
                cerclesProgression[etapeActuelle].SetActive(true);
            }
        }

        Invoke(nameof(ResetValidation), 0.3f);
    }

    void ResetValidation()
    {
        validationEnCours = false;
    }

    void ActiverEffetsVisuels()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerFrosting();
        }

        if (potionRenderer != null)
        {
            potionRenderer.material.color = couleurGel;
        }

        if (meshsGel != null)
        {
            foreach (GameObject mesh in meshsGel)
            {
                if (mesh != null) mesh.SetActive(true);
            }
        }

        if (vignetteGel != null)
        {
            StartCoroutine(AnimerVignette());
        }

        EnvoyerOSCLumiere(true);
    }

    IEnumerator AnimerVignette()
    {
        if (vignetteGel == null) yield break;

        vignetteGel.gameObject.SetActive(true);
        Color c = vignetteGel.color;
        c.a = 0f;
        vignetteGel.color = c;
        vignetteGel.transform.localScale = Vector3.one * 2f;

        float elapsed = 0f;
        float duree = 1.5f;

        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duree;

            c.a = Mathf.Lerp(0f, 0.2f, t);
            vignetteGel.color = c;
            vignetteGel.transform.localScale = Vector3.one * Mathf.Lerp(2f, 1f, t);

            yield return null;
        }
    }

    void ResoudreEvenement()
    {
        phaseActuelle = PhaseGel.Resolu;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEventReussi();
        }
        StartCoroutine(FlashReussiteCoroutine());
        if (ConfettiManager.Instance != null)
        {
            ConfettiManager.Instance.Exploser();
        }
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

        Debug.Log("EVENT GEL : resolu");
    }

    void EchecEvenement()
    {
        phaseActuelle = PhaseGel.Echec;

        DesactiverEffets();

        if (gameManager != null)
        {
            gameManager.EvenementEchoue();
        }

        Debug.Log("EVENT GEL : echec");
    }

    void DesactiverEffets()
    {
        if (potionRenderer != null && meshEau != null)
        {
            potionRenderer.material = meshEau.meshMaterial;
        }

        if (meshsGel != null)
        {
            foreach (GameObject mesh in meshsGel)
            {
                if (mesh != null) mesh.SetActive(false);
            }
        }

        if (vignetteGel != null)
        {
            vignetteGel.gameObject.SetActive(false);
        }

        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(false);
        }
        if (ParticulesPotionController.Instance != null)
        {
            ParticulesPotionController.Instance.ResetNormal();
        }
        if (patternContainer != null)
        {
            patternContainer.SetActive(false);
        }

        if (knobMaxIndicateur != null)
        {
            knobMaxIndicateur.gameObject.SetActive(false);
        }

        if (flecheCourbe != null)
        {
            flecheCourbe.gameObject.SetActive(false);
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(false);
        }

        if (eventGelPanel != null)
        {
            eventGelPanel.SetActive(false);
        }

        if (stationFeu != null)
        {
            stationFeu.enabled = true;
        }

        if (colonneFeu != null)
        {
            colonneFeu.alpha = 1f;
        }

        EnvoyerOSCLumiere(false);

        if (fondsKnobs != null)
        {
            foreach (GameObject fond in fondsKnobs)
            {
                if (fond != null) Destroy(fond);
            }
        }

        if (indicateursCibles != null)
        {
            foreach (GameObject indic in indicateursCibles)
            {
                if (indic != null) Destroy(indic);
            }
        }

        if (indicateursDynamiques != null)
        {
            foreach (GameObject indic in indicateursDynamiques)
            {
                if (indic != null) Destroy(indic);
            }
        }

        if (fleches != null)
        {
            foreach (GameObject fleche in fleches)
            {
                if (fleche != null) Destroy(fleche);
            }
        }

        if (cerclesProgression != null)
        {
            foreach (GameObject cercle in cerclesProgression)
            {
                if (cercle != null) Destroy(cercle);
            }
        }
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

        var message = new OSCMessage("/lumiere/gel");
        message.AddValue(OSCValue.Int(allumer ? 1 : 0));
        oscTransmitter.Send(message);
    }

    public void UpdatePotentiometre(int valeur)
    {
        valeurPotentiometre = valeur;
    }

    void DesactiverEvent()
    {
        gameObject.SetActive(false);
    }
}