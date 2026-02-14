using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System.Collections;
using TMPro;

public class EventGel : MonoBehaviour
{
    [Header("ui jauge")]
    public Slider jaugeTemperature;
    public Image fillJauge;

    [Header("ui pattern")]
    public GameObject patternContainer;
    public RectTransform knobsContainer;
    public GameObject prefabKnobCible;
    public GameObject prefabFleche;
    public GameObject prefabCercleProgression;

    [Header("knob dynamique")]
    public RectTransform knobDynamique;

    [Header("ui overlay")]
    public Image overlayRouge;
    public TextMeshProUGUI texteInstruction;

    [Header("params phase 1")]
    public float seuilIntensiteMax = 1028f;
    public float dureePhase1 = 3f;

    [Header("params phase 2")]
    public float tolerancePattern = 200f;
    public float tempsMaxParEtape = 5f;

    [Header("difficulte progressive")]
    public int nbKnobsMin = 4;
    public int nbKnobsMax = 10;
    public float tempsSeuilNbKnobs = 360f; // 6 min

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

    private enum PhaseGel { Phase1, Phase2, Resolu, Echec }
    private PhaseGel phaseActuelle;

    private float niveauFroid;
    private float chronoPhase1;
    private int valeurPotentiometre;

    private int nbKnobsTotal;
    private int[] positionsCibles;
    private int etapeActuelle;
    private float chronoEtape;

    private GameObject[] knobsCibles;
    private GameObject[] fleches;
    private GameObject cercleProgression;

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

        // calculer nb knobs selon difficulte
        if (GameManager.Instance != null)
        {
            float progression = Mathf.Clamp01(GameManager.Instance.tempsEcoule / tempsSeuilNbKnobs);
            nbKnobsTotal = Mathf.RoundToInt(Mathf.Lerp(nbKnobsMin, nbKnobsMax, progression));
        }
        else
        {
            nbKnobsTotal = nbKnobsMin;
        }

        // generer positions aleatoires
        GenererPositionsCibles();

        // ui
        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(true);
            jaugeTemperature.value = 1f;
        }

        if (patternContainer != null)
        {
            patternContainer.SetActive(false);
        }

        if (overlayRouge != null)
        {
            overlayRouge.gameObject.SetActive(true);
            StartCoroutine(AnimerOverlay());
        }

        if (texteInstruction != null)
        {
            texteInstruction.text = "chaleur maximale !";
        }

        ActiverEffetsVisuels();

        // bloquer manipulation continue station feu
        if (stationFeu != null)
        {
            stationFeu.enabled = false;
        }

        Debug.Log($"EVENT GEL : demarre, {nbKnobsTotal} knobs");
    }

    void GenererPositionsCibles()
    {
        positionsCibles = new int[nbKnobsTotal];

        // generer positions espacees
        for (int i = 0; i < nbKnobsTotal; i++)
        {
            // alterner entre haut et bas pour varier
            if (i % 2 == 0)
            {
                positionsCibles[i] = Random.Range(300, 1000); // haut
            }
            else
            {
                positionsCibles[i] = Random.Range(3000, 3700); // bas
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

        // update jauge
        if (jaugeTemperature != null)
        {
            jaugeTemperature.value = niveauFroid / 100f;
        }

        // update knob dynamique rotation
        if (knobDynamique != null)
        {
            float angle = Mathf.Lerp(-180f, 180f, valeurPotentiometre / 4096f);
            knobDynamique.localRotation = Quaternion.Euler(0, 0, angle);
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

        if (texteInstruction != null)
        {
            texteInstruction.text = "suivez le pattern !";
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

        // determiner layout (1 ou 2 rows)
        int nbRows = nbKnobsTotal > 5 ? 2 : 1;
        int nbParRow = Mathf.CeilToInt(nbKnobsTotal / (float)nbRows);

        knobsCibles = new GameObject[nbKnobsTotal];
        fleches = new GameObject[nbKnobsTotal - 1];

        float espacementX = 80f;
        float espacementY = 100f;

        for (int i = 0; i < nbKnobsTotal; i++)
        {
            int row = i / nbParRow;
            int col = i % nbParRow;

            // knob cible
            GameObject knob = Instantiate(prefabKnobCible, knobsContainer);
            RectTransform knobRT = knob.GetComponent<RectTransform>();
            knobRT.anchoredPosition = new Vector2(col * espacementX, -row * espacementY);

            // rotation selon position cible
            float angle = Mathf.Lerp(-180f, 180f, positionsCibles[i] / 4096f);
            knobRT.localRotation = Quaternion.Euler(0, 0, angle);

            knobsCibles[i] = knob;

            // fleche
            if (i < nbKnobsTotal - 1)
            {
                GameObject fleche = Instantiate(prefabFleche, knobsContainer);
                RectTransform flecheRT = fleche.GetComponent<RectTransform>();
                flecheRT.anchoredPosition = new Vector2((col + 0.5f) * espacementX, -row * espacementY);
                fleches[i] = fleche;
            }
        }

        // cercle progression sur premier knob
        if (prefabCercleProgression != null)
        {
            cercleProgression = Instantiate(prefabCercleProgression, knobsCibles[0].transform);
            cercleProgression.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
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

        // check position
        float diff = Mathf.Abs(valeurPotentiometre - positionsCibles[etapeActuelle]);

        // update cercle progression
        if (cercleProgression != null)
        {
            float proximite = 1f - Mathf.Clamp01(diff / tolerancePattern);
            cercleProgression.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, proximite);

            Image cercleImg = cercleProgression.GetComponent<Image>();
            if (cercleImg != null)
            {
                Color c = cercleImg.color;
                c.a = proximite;
                cercleImg.color = c;
            }
        }

        if (diff <= tolerancePattern)
        {
            ValiderEtape();
        }
    }

    void ValiderEtape()
    {
        Debug.Log($"EVENT GEL : etape {etapeActuelle + 1}/{nbKnobsTotal} validee");

        // fade knob actuel
        StartCoroutine(FadeKnob(knobsCibles[etapeActuelle]));

        // fade fleche
        if (etapeActuelle < fleches.Length && fleches[etapeActuelle] != null)
        {
            StartCoroutine(FadeFleche(fleches[etapeActuelle]));
        }

        // update jauge
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
            // deplacer cercle sur prochain knob
            if (cercleProgression != null)
            {
                cercleProgression.transform.SetParent(knobsCibles[etapeActuelle].transform);
                cercleProgression.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

    IEnumerator FadeKnob(GameObject knob)
    {
        Image img = knob.GetComponent<Image>();
        if (img == null) yield break;

        yield return new WaitForSeconds(1f);

        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < 3f)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - (elapsed / 3f);
            img.color = c;
            yield return null;
        }
    }

    IEnumerator FadeFleche(GameObject fleche)
    {
        Image img = fleche.GetComponent<Image>();
        if (img == null) yield break;

        yield return new WaitForSeconds(1f);

        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < 3f)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - (elapsed / 3f);
            img.color = c;
            yield return null;
        }
    }

    IEnumerator AnimerOverlay()
    {
        if (overlayRouge == null) yield break;

        float duree = 0.5f;
        float elapsed = 0f;
        Color c = overlayRouge.color;

        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 0.3f, elapsed / duree);
            overlayRouge.color = c;
            yield return null;
        }
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
            potionRenderer.material.color = meshEau.meshMaterial.color;
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

        if (overlayRouge != null)
        {
            overlayRouge.gameObject.SetActive(false);
        }

        if (jaugeTemperature != null)
        {
            jaugeTemperature.gameObject.SetActive(false);
        }

        if (patternContainer != null)
        {
            patternContainer.SetActive(false);
        }

        // reactiver station feu
        if (stationFeu != null)
        {
            stationFeu.enabled = true;
        }

        EnvoyerOSCLumiere(false);

        // detruire knobs crees
        if (knobsCibles != null)
        {
            foreach (GameObject knob in knobsCibles)
            {
                if (knob != null) Destroy(knob);
            }
        }

        if (fleches != null)
        {
            foreach (GameObject fleche in fleches)
            {
                if (fleche != null) Destroy(fleche);
            }
        }

        if (cercleProgression != null)
        {
            Destroy(cercleProgression);
        }
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