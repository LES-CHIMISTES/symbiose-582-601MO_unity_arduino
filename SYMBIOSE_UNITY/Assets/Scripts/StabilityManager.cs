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
    [Header("Jauge 3D")]
    public JaugeStabilite3D jauge3D;
    [Header("Glow critique potion")]
    public MeshRenderer potionGlowRenderer;
    public Color couleurGlow = new Color(1f, 0.1f, 0.1f, 1f);
    public float alphaGlowMax = 0.4f;
    [Header("Light critique")]
    public Light pointLightCritique;
    public float intensiteLightMax = 0.09f;
    [Header("Grace period")]
    public float gracePeriodInitiale = 5f;
    public float gracePeriodFinale = 2f;
    private float gracePeriodTimer = 0f;
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
        if (potionGlowRenderer != null)
        {
            potionGlowRenderer.gameObject.SetActive(false);
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
        // grace period
        if (gracePeriodTimer > 0f)
        {
            gracePeriodTimer -= Time.deltaTime;
            stabiliteAffichee = Mathf.Lerp(stabiliteAffichee, stabiliteActuelle, Time.deltaTime * 8f);
            UpdateUI();
            return;
        }
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
        DemarrerGracePeriod();
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
#if UNITY_EDITOR
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
#endif
        if (jauge3D != null)
        {
            jauge3D.Actualiser(stabiliteAffichee / stabiliteMax);
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
        if (jaugeStabilite != null && stabiliteActuelle < seuilAlerte)
        {
            float shake = Mathf.Sin(Time.time * 20f) * 2f * (1f - stabiliteActuelle / seuilAlerte);
            jaugeStabilite.GetComponent<RectTransform>().anchoredPosition = new Vector2(shake, 0f);
        }
        if (potionGlowRenderer != null)
        {
            if (stabiliteActuelle < seuilAlerte && stabiliteActuelle > 0f)
            {
                float pulse = Mathf.Sin(Time.time * 7f);
                potionGlowRenderer.gameObject.SetActive(pulse > 0f);
            }
            else
            {
                potionGlowRenderer.gameObject.SetActive(false);
            }
        }

        if (pointLightCritique != null)
        {
            float pct = stabiliteAffichee / stabiliteMax;

            // couleur selon stabilite (meme que jauge)
            Color couleurLight;
            if (pct > 0.5f)
                couleurLight = Color.Lerp(couleurDanger, couleurSaine, (pct - 0.5f) * 2f);
            else if (pct > 0.2f)
                couleurLight = Color.Lerp(couleurCritique, couleurDanger, (pct - 0.2f) / 0.3f);
            else
                couleurLight = couleurCritique;

            pointLightCritique.color = couleurLight;

            // intensite : normale en temps normal, pulse quand critique
            if (stabiliteActuelle < seuilAlerte && stabiliteActuelle > 0f)
            {
                float pulse = Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f;
                float intensiteCritique = (1f - (stabiliteActuelle / seuilAlerte));
                pointLightCritique.intensity = Mathf.Lerp(intensiteLightMax * 0.3f, intensiteLightMax, intensiteCritique * pulse);
            }
            else
            {
                pointLightCritique.intensity = intensiteLightMax * 0.5f;
            }
        }
    }
    public void DemarrerGracePeriod()
    {
        float d = 0f;
        if (GameManager.Instance != null)
        {
            d = GameManager.Instance.GetProgressionDifficulte();
        }
        gracePeriodTimer = Mathf.Lerp(gracePeriodInitiale, gracePeriodFinale, d);
        Debug.Log($"STABILITE : grace period {gracePeriodTimer:F1}s");
    }
}