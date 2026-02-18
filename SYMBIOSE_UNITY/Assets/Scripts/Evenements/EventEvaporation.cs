using UnityEngine;
using UnityEngine.UI;
using TMPro;
using extOSC;
using System.Collections;

public class EventEvaporation : MonoBehaviour
{
    [Header("ui panel")]
    public GameObject eventEvaporationPanel;

    [Header("ui jauge")]
    public Slider jaugeRecuperation;
    public Image fillJauge;
    public TextMeshProUGUI texteJauge;

    [Header("ui intensite")]
    public RectTransform indicateurIntensite;
    public Image[] barresIntensite;
    public Image zoneOptimale;
    public TextMeshProUGUI texteFeedback;

    [Header("ui texte")]
    public TextMeshProUGUI texteAlert;
    public float dureeAffichageTexte = 3f;

    [Header("params detection")]
    public float seuilIntensiteMin = 0.5f;
    public float seuilIntensiteOptimale = 2f;
    public float seuilIntensiteMax = 5f;
    public float multiplicateurRemplissage = 1f;
    public float tempsMaxAvantEchec = 30f;

    [Header("difficulte progressive")]
    public float seuilMinDebutant = 0.3f;
    public float seuilMinExpert = 1f;
    public float tempsSeuilDifficulte = 180f;

    [Header("visuels")]
    public MeshRenderer eauRenderer;
    public GameObject[] meshsBrouillard;
    public Color couleurEvaporation = new Color(0.9f, 0.9f, 0.95f);
    public Color couleurEauTransparente = new Color(0.6f, 0.8f, 1f, 0.3f);

    [Header("references")]
    public GameManager gameManager;
    public OSCTransmitter oscTransmitter;
    public MeshEauController meshEau;
    public StationEauFeedback stationEau;
    public CanvasGroup colonneEau;

    private enum PhaseEvaporation { EnCours, Resolu, Echec }
    private PhaseEvaporation phaseActuelle;

    private float progression = 0f;
    private float chronoTotal = 0f;
    private float intensiteActuelle = 0f;

    private float dernierAccelX = 0f;
    private float dernierAccelY = 0f;
    private float dernierAccelZ = 0f;

    private Color couleurEauInitiale;

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

        // ajuster difficulte
        if (GameManager.Instance != null)
        {
            float progressionDifficulte = Mathf.Clamp01(GameManager.Instance.tempsEcoule / tempsSeuilDifficulte);
            seuilIntensiteMin = Mathf.Lerp(seuilMinDebutant, seuilMinExpert, progressionDifficulte);
        }

        if (eventEvaporationPanel != null)
        {
            eventEvaporationPanel.SetActive(true);
        }

        if (jaugeRecuperation != null)
        {
            jaugeRecuperation.gameObject.SetActive(true);
            jaugeRecuperation.value = 0f;
        }

        if (indicateurIntensite != null)
        {
            indicateurIntensite.gameObject.SetActive(true);
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(true);
            texteAlert.text = "l'eau s'évapore...";
            StartCoroutine(FadeTexte());
        }

        if (texteFeedback != null)
        {
            texteFeedback.text = "agitez !";
        }

        ActiverEffetsVisuels();

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

    void Update()
    {
        if (phaseActuelle != PhaseEvaporation.EnCours) return;

        chronoTotal += Time.deltaTime;

        if (chronoTotal >= tempsMaxAvantEchec)
        {
            EchecEvenement();
            return;
        }

        // calculer intensite (sera mis a jour par UpdateAccel)
        UpdateBarresIntensite();
        UpdateFeedback();

        // remplir jauge selon intensite
        if (intensiteActuelle >= seuilIntensiteMin)
        {
            float facteurRemplissage = Mathf.InverseLerp(seuilIntensiteMin, seuilIntensiteOptimale, intensiteActuelle);
            facteurRemplissage = Mathf.Clamp01(facteurRemplissage);

            progression += facteurRemplissage * multiplicateurRemplissage * Time.deltaTime;
            progression = Mathf.Clamp01(progression);
        }

        if (jaugeRecuperation != null)
        {
            jaugeRecuperation.value = progression;
        }

        if (texteJauge != null)
        {
            texteJauge.text = $"{Mathf.RoundToInt(progression * 100)}%";
        }

        // transparence eau proportionnelle
        if (eauRenderer != null)
        {
            Color c = Color.Lerp(couleurEauTransparente, couleurEauInitiale, progression);
            eauRenderer.material.color = c;
        }

        if (progression >= 1f)
        {
            ResoudreEvenement();
        }
    }

    void UpdateBarresIntensite()
    {
        if (barresIntensite == null || barresIntensite.Length == 0) return;

        int nbBarresActives = Mathf.RoundToInt((intensiteActuelle / seuilIntensiteMax) * barresIntensite.Length);
        nbBarresActives = Mathf.Clamp(nbBarresActives, 0, barresIntensite.Length);

        for (int i = 0; i < barresIntensite.Length; i++)
        {
            if (barresIntensite[i] != null)
            {
                barresIntensite[i].enabled = i < nbBarresActives;

                // couleur selon zone
                if (i < nbBarresActives)
                {
                    if (intensiteActuelle < seuilIntensiteMin)
                    {
                        barresIntensite[i].color = Color.red;
                    }
                    else if (intensiteActuelle <= seuilIntensiteOptimale)
                    {
                        barresIntensite[i].color = Color.green;
                    }
                    else
                    {
                        barresIntensite[i].color = Color.yellow;
                    }
                }
            }
        }
    }

    void UpdateFeedback()
    {
        if (texteFeedback == null) return;

        if (intensiteActuelle < seuilIntensiteMin * 0.5f)
        {
            texteFeedback.text = "plus fort !";
            texteFeedback.color = Color.red;
        }
        else if (intensiteActuelle < seuilIntensiteMin)
        {
            texteFeedback.text = "encore !";
            texteFeedback.color = new Color(1f, 0.5f, 0f);
        }
        else if (intensiteActuelle <= seuilIntensiteOptimale)
        {
            texteFeedback.text = "parfait !";
            texteFeedback.color = Color.green;
        }
        else
        {
            texteFeedback.text = "trop fort !";
            texteFeedback.color = Color.yellow;
        }
    }

    public void UpdateAccel(float accelX, float accelY, float accelZ)
    {
        // calculer magnitude du changement
        float deltaX = Mathf.Abs(accelX - dernierAccelX);
        float deltaY = Mathf.Abs(accelY - dernierAccelY);
        float deltaZ = Mathf.Abs(accelZ - dernierAccelZ);

        float magnitudeChangement = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

        // smooth l'intensite
        intensiteActuelle = Mathf.Lerp(intensiteActuelle, magnitudeChangement * 10f, 0.3f);

        dernierAccelX = accelX;
        dernierAccelY = accelY;
        dernierAccelZ = accelZ;
    }

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

        /*if (vignetteEvaporation != null)
        {
            StartCoroutine(AnimerVignette());
        }*/

        EnvoyerOSCLumiere(true);
    }

 /*   IEnumerator AnimerVignette()
    {
        if (vignetteEvaporation == null) yield break;

        vignetteEvaporation.gameObject.SetActive(true);
        Color c = vignetteEvaporation.color;
        c.a = 0f;
        vignetteEvaporation.color = c;
        vignetteEvaporation.transform.localScale = Vector3.one * 2f;

        float elapsed = 0f;
        float duree = 1.5f;

        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duree;

            c.a = Mathf.Lerp(0f, 0.2f, t);
            vignetteEvaporation.color = c;
            vignetteEvaporation.transform.localScale = Vector3.one * Mathf.Lerp(2f, 1f, t);

            yield return null;
        }
    }*/

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

        /*if (vignetteEvaporation != null)
        {
            vignetteEvaporation.gameObject.SetActive(false);
        }*/

        if (jaugeRecuperation != null)
        {
            jaugeRecuperation.gameObject.SetActive(false);
        }

        if (indicateurIntensite != null)
        {
            indicateurIntensite.gameObject.SetActive(false);
        }

        if (texteAlert != null)
        {
            texteAlert.gameObject.SetActive(false);
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