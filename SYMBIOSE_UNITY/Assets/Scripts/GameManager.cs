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

    public bool enGameOver { get; private set; } = false;

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
        if (timerActif)
        {
            tempsEcoule += Time.deltaTime;
        }
    }

    public void DemarrerTutoriel()
    {
        phaseActuelle = GamePhase.Tutoriel;
        timerActif = true;
        tempsEcoule = 0f;
        OnTutorialStart?.Invoke();
        Debug.Log("GAME : tutoriel démarré, timer lancé");
    }

    public void TerminerTutoriel()
    {
        OnTutorialComplete?.Invoke();
        Invoke(nameof(DemarrerPhasePrincipale), 1f);
        Debug.Log("GAME : tutoriel terminé");
    }

    void DemarrerPhasePrincipale()
    {
        phaseActuelle = GamePhase.PhasePrincipale;
        OnMainPhaseStart?.Invoke();
        Debug.Log("GAME : phase principale lancée");
    }

    public void DeclencherGameOver()
    {
        if (phaseActuelle == GamePhase.GameOver) return;

        phaseActuelle = GamePhase.GameOver;
        timerActif = false;
        enGameOver = true;

        OnGameOver?.Invoke();

        Debug.Log($"GAME : game over, temps = {FormatTemps(tempsEcoule)}");
    }

    public void EvenementResolu()
    {
        evenementsResolus++;
        Debug.Log($"GAME : événement résolu, total = {evenementsResolus}");
    }

    public void EvenementEchoue()
    {
        evenementsEchoues++;
        Debug.Log($"GAME : événement échoué, total = {evenementsEchoues}");
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