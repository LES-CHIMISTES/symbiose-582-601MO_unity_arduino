using UnityEngine;
using UnityEngine.UI;

public class StabilityManager : MonoBehaviour
{
    public static StabilityManager Instance { get; private set; }

    // =====================================================================
    // REFERENCES STATIONS
    // =====================================================================

    [Header("Stations")]
    public StationEauFeedback stationEau;
    public StationFeuFeedback stationFeu;
    public StationPoudresFeedback stationPoudres;
    public StationTourbillonFeedback stationTourbillon;

    // =====================================================================
    // PARAMETRES STABILITE
    // =====================================================================

    [Header("Stabilite")]
    [Range(0f, 100f)]
    public float stabiliteActuelle = 100f;
    public float stabiliteMax = 100f;

    [Header("Perte par station hors equilibre")]
    public float perteParStationParSeconde = 3f;

    [Header("Gain par station en equilibre")]
    public float gainParStationParSeconde = 1.5f;

    [Header("Regeneration passive")]
    public float regenerationPassive = 0.5f;

    [Header("Bonus evenement resolu")]
    public float bonusEvenementResolu = 8f;

    // =====================================================================
    // UI
    // =====================================================================

    [Header("UI")]
    public Slider jaugeStabilite;
    public Image fillStabilite;

    [Header("Couleurs jauge")]
    public Color couleurSaine = new Color(0.3f, 0.9f, 0.3f, 1f);
    public Color couleurDanger = new Color(0.9f, 0.9f, 0.2f, 1f);
    public Color couleurCritique = new Color(0.9f, 0.2f, 0.2f, 1f);

    [Header("Alerte visuelle")]
    public Image vignetteAlerte;
    public float seuilAlerte = 30f;

    // =====================================================================
    // VARIABLES PRIVEES
    // =====================================================================

    private bool actif = false;
    private float stabiliteAffichee;
    private bool etaitCritique = false;

    // =====================================================================
    // INITIALISATION
    // =====================================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        stabiliteActuelle = stabiliteMax;
        stabiliteAffichee = stabiliteMax;

        if (vignetteAlerte != null)
        {
            vignetteAlerte.gameObject.SetActive(false);
        }

        UpdateUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTutorialComplete.AddListener(Activer);
        }
    }

    void Activer()
    {
        actif = true;
        Debug.Log("STABILITE : systeme active");
    }

    // =====================================================================
    // UPDATE
    // =====================================================================

    void Update()
    {
        if (!actif) return;
        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;

        // compter stations en equilibre / hors equilibre
        int stationsEnEquilibre = 0;
        int stationsHorsEquilibre = 0;

        VerifierStation(stationEau, ref stationsEnEquilibre, ref stationsHorsEquilibre);
        VerifierStation(stationFeu, ref stationsEnEquilibre, ref stationsHorsEquilibre);
        VerifierStation(stationPoudres, ref stationsEnEquilibre, ref stationsHorsEquilibre);
        VerifierStation(stationTourbillon, ref stationsEnEquilibre, ref stationsHorsEquilibre);

        // calculer variation
        float gain = stationsEnEquilibre * gainParStationParSeconde * Time.deltaTime;
        float multiplicateurDifficulte = 1f;
        if (GameManager.Instance != null)
        {
            float d = GameManager.Instance.GetProgressionDifficulte();
            multiplicateurDifficulte = Mathf.Lerp(1f, 2f, d);
        }
        float perte = stationsHorsEquilibre * perteParStationParSeconde * multiplicateurDifficulte * Time.deltaTime;
        float regen = regenerationPassive * Time.deltaTime;

        stabiliteActuelle += gain - perte + regen;
        stabiliteActuelle = Mathf.Clamp(stabiliteActuelle, 0f, stabiliteMax);

        // alerte visuelle
        UpdateVignetteAlerte();

        bool critique = stabiliteActuelle < seuilAlerte;
        if (critique && !etaitCritique)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerAlerteCritique();
            }
        }
        etaitCritique = critique;

        // UI smooth
        stabiliteAffichee = Mathf.Lerp(stabiliteAffichee, stabiliteActuelle, Time.deltaTime * 8f);
        UpdateUI();

        // game over
        if (stabiliteActuelle <= 0f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DeclencherGameOver();
            }
        }
    }

    void VerifierStation(MonoBehaviour station, ref int enEquilibre, ref int horsEquilibre)
    {
        // station desactivee (pendant un evenement) = on l'ignore
        if (station == null || !station.enabled) return;

        bool equilibre = false;

        if (station is StationEauFeedback eau)
            equilibre = eau.EstEnEquilibre();
        else if (station is StationFeuFeedback feu)
            equilibre = feu.EstEnEquilibre();
        else if (station is StationPoudresFeedback poudres)
            equilibre = poudres.EstEnEquilibre();
        else if (station is StationTourbillonFeedback tourbillon)
            equilibre = tourbillon.EstEnEquilibre();

        if (equilibre)
            enEquilibre++;
        else
            horsEquilibre++;
    }

    // =====================================================================
    // METHODES PUBLIQUES
    // =====================================================================

    public void BonusEvenement()
    {
        if (!actif) return;

        stabiliteActuelle = Mathf.Min(stabiliteMax, stabiliteActuelle + bonusEvenementResolu);
        Debug.Log($"STABILITE : +{bonusEvenementResolu} (evenement resolu) -> {stabiliteActuelle:F0}%");
    }

    public float GetPourcentage()
    {
        return stabiliteActuelle / stabiliteMax;
    }

    public bool EstCritique()
    {
        return stabiliteActuelle < seuilAlerte;
    }

    // =====================================================================
    // UI
    // =====================================================================

    void UpdateUI()
    {
        if (jaugeStabilite != null)
        {
            jaugeStabilite.value = stabiliteAffichee / stabiliteMax;
        }

        if (fillStabilite != null)
        {
            float pct = stabiliteAffichee / stabiliteMax;

            if (pct > 0.5f)
                fillStabilite.color = Color.Lerp(couleurDanger, couleurSaine, (pct - 0.5f) * 2f);
            else if (pct > 0.2f)
                fillStabilite.color = Color.Lerp(couleurCritique, couleurDanger, (pct - 0.2f) / 0.3f);
            else
                fillStabilite.color = couleurCritique;
        }
    }

    void UpdateVignetteAlerte()
    {
        if (vignetteAlerte == null) return;

        if (stabiliteActuelle < seuilAlerte && stabiliteActuelle > 0f)
        {
            vignetteAlerte.gameObject.SetActive(true);

            float intensite = 1f - (stabiliteActuelle / seuilAlerte);
            float pulse = Mathf.Sin(Time.time * 4f) * 0.5f + 0.5f;
            float alpha = intensite * pulse * 0.3f;

            Color c = vignetteAlerte.color;
            c.a = alpha;
            vignetteAlerte.color = c;
        }
        else
        {
            vignetteAlerte.gameObject.SetActive(false);
        }
    }
}