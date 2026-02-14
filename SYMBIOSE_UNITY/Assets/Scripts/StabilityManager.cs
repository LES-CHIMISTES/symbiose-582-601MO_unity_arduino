using UnityEngine;
using UnityEngine.UI;

public class StabilityManager : MonoBehaviour
{
    public static StabilityManager Instance { get; private set; }

    [Header("Stabilité")]
    [Range(0f, 100f)]
    public float stabiliteActuelle = 100f;
    public float stabiliteMax = 100f;

    [Header("Perte de stabilité")]
    public float perteParSecondeManipulation = 0f; // Quand manipulation échouée
    public float perteParSecondeEvenement = 0f; // Quand événement non résolu
    public float perteParSecondeCascade = 0f; // Quand 2+ événements actifs

    [Header("UI")]
    public Slider jaugeStabilite; // Barre de stabilité
    public Image fillStabilite; // Pour changer la couleur
    public Color couleurSaine = Color.green;
    public Color couleurDanger = Color.yellow;
    public Color couleurCritique = Color.red;

    [Header("Debug")]
    public bool afficherDebugLogs = true;

    private bool enTutoriel = true;

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
        // Initialiser jauge
        stabiliteActuelle = stabiliteMax;
        UpdateUI();

        // Écouter fin du tutoriel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTutorialComplete.AddListener(ActiverSystemeStabilite);
        }
    }

    void Update()
    {
        // Ne pas perdre de stabilité pendant le tutoriel
        if (enTutoriel) return;

        // Update UI
        UpdateUI();

        // Check Game Over
        if (stabiliteActuelle <= 0f && GameManager.Instance != null)
        {
            GameManager.Instance.DeclencherGameOver();
        }
    }

    void ActiverSystemeStabilite()
    {
        enTutoriel = false;
        Debug.Log("STABILITÉ : Système activé après tutoriel");
    }

    // ===== MÉTHODES PUBLIQUES =====

    public void PerdreStabilite(float montant, string raison = "")
    {
        if (enTutoriel) return; // Pas de perte pendant tutoriel

        stabiliteActuelle = Mathf.Max(0f, stabiliteActuelle - montant);

        if (afficherDebugLogs && !string.IsNullOrEmpty(raison))
        {
            Debug.LogWarning($"STABILITÉ : -{montant:F1} ({raison}) → {stabiliteActuelle:F1}/{stabiliteMax}");
        }

        // Feedback visuel si critique
        if (stabiliteActuelle < 30f && stabiliteActuelle > 0f)
        {
            // TODO: Shake caméra, son d'alerte
        }
    }

    public void GagnerStabilite(float montant, string raison = "")
    {
        if (enTutoriel) return;

        stabiliteActuelle = Mathf.Min(stabiliteMax, stabiliteActuelle + montant);

        if (afficherDebugLogs && !string.IsNullOrEmpty(raison))
        {
            Debug.Log($"STABILITÉ : +{montant:F1} ({raison}) → {stabiliteActuelle:F1}/{stabiliteMax}");
        }
    }

    public void PerdreStabiliteParSeconde(float montantParSeconde, string raison = "")
    {
        PerdreStabilite(montantParSeconde * Time.deltaTime, raison);
    }

    public float GetPourcentageStabilite()
    {
        return (stabiliteActuelle / stabiliteMax) * 100f;
    }

    public bool EstCritique()
    {
        return stabiliteActuelle < 30f;
    }

    // ===== UI =====

    void UpdateUI()
    {
        if (jaugeStabilite != null)
        {
            jaugeStabilite.value = stabiliteActuelle / stabiliteMax;
        }

        if (fillStabilite != null)
        {
            float pourcentage = GetPourcentageStabilite();

            if (pourcentage > 50f)
            {
                fillStabilite.color = couleurSaine;
            }
            else if (pourcentage > 30f)
            {
                fillStabilite.color = couleurDanger;
            }
            else
            {
                fillStabilite.color = couleurCritique;
            }
        }
    }
}