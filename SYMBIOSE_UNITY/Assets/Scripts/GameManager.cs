using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Phases du jeu")]
    public UnityEvent OnTutorialStart;
    public UnityEvent OnTutorialComplete;
    public UnityEvent OnMainPhaseStart;
    public UnityEvent OnGameOver;

    [Header("Timer")]
    public float tempsEcoule = 0f;

    [Header("Stats événements")]
    public int evenementsResolus = 0;
    public int evenementsEchoues = 0;

    public enum GamePhase
    {
        Tutoriel,
        PhasePrincipale,
        GameOver
    }

    private GamePhase phaseActuelle = GamePhase.Tutoriel;
    private bool timerActif = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        DemarrerTutoriel();
    }

    void Update()
    {
        // Update timer si actif
        if (timerActif)
        {
            tempsEcoule += Time.deltaTime;
        }
    }

    public void DemarrerTutoriel()
    {
        phaseActuelle = GamePhase.Tutoriel;
        timerActif = true; // timer démarre dès le début
        tempsEcoule = 0f;

        OnTutorialStart?.Invoke();

        Debug.Log("GAME : Tutoriel démarré, timer lancé");
    }

    public void TerminerTutoriel()
    {
        OnTutorialComplete?.Invoke();

        // Petit délai avant de lancer la phase principale
        Invoke(nameof(DemarrerPhasePrincipale), 1f);

        Debug.Log("GAME : Tutoriel terminé !");
    }

    void DemarrerPhasePrincipale()
    {
        phaseActuelle = GamePhase.PhasePrincipale;
        OnMainPhaseStart?.Invoke();

        Debug.Log("GAME : Phase principale lancée !");
    }

    public void DeclencherGameOver()
    {
        if (phaseActuelle == GamePhase.GameOver) return;

        phaseActuelle = GamePhase.GameOver;
        timerActif = false;

        OnGameOver?.Invoke();

        Debug.Log($"GAME : Game Over ! Temps de survie : {FormatTemps(tempsEcoule)}");
    }

    // Méthodes pour les événements
    public void EvenementResolu()
    {
        evenementsResolus++;
        Debug.Log($"GAME : Événement résolu ! Total : {evenementsResolus}");
    }

    public void EvenementEchoue()
    {
        evenementsEchoues++;
        Debug.Log($"GAME : Événement échoué ! Total : {evenementsEchoues}");

        // Game Over si échec d'événement
        DeclencherGameOver();
    }

    public string FormatTemps(float temps)
    {
        int minutes = Mathf.FloorToInt(temps / 60f);
        int secondes = Mathf.FloorToInt(temps % 60f);
        return $"{minutes:00}:{secondes:00}";
    }

    public GamePhase GetPhaseActuelle()
    {
        return phaseActuelle;
    }

    public bool EstEnTutoriel()
    {
        return phaseActuelle == GamePhase.Tutoriel;
    }

    public bool EstEnPhasePrincipale()
    {
        return phaseActuelle == GamePhase.PhasePrincipale;
    }
}