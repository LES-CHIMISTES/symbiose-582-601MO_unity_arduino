using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("événements disponibles")]
    public GameObject eventGel;
    public GameObject eventEvaporation;
    public GameObject eventCristallisation;
    public GameObject eventVortex;

    [Header("fréquence")]
    public float delaiPremierEvenement = 10f;
    public float cooldownMinInitial = 20f;
    public float cooldownMaxInitial = 30f;
    public float cooldownMinFinal = 8f;
    public float cooldownMaxFinal = 15f;
    public float tempsPourDifficulteMax = 180f; // 3 minutes

    private GameObject[] evenementsDisponibles;
    private bool evenementEnCours = false;
    private float prochainEvenement = 0f;
    private bool systemeActif = false;

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
        evenementsDisponibles = new GameObject[] { eventGel, eventEvaporation, eventCristallisation, eventVortex };

        // desactiver tous les events au depart
        foreach (GameObject evt in evenementsDisponibles)
        {
            if (evt != null)
            {
                evt.SetActive(false);
            }
        }

        // ecouter fin tutoriel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTutorialComplete.AddListener(ActiverSysteme);
        }
    }

    void ActiverSysteme()
    {
        systemeActif = true;
        prochainEvenement = Time.time + delaiPremierEvenement;
        Debug.Log($"EVENT MANAGER : systeme actif, premier event dans {delaiPremierEvenement}s");
    }

    void Update()
    {
        if (!systemeActif || evenementEnCours) return;

        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;

        if (Time.time >= prochainEvenement)
        {
            DeclencherEvenementAleatoire();
        }
    }

    void DeclencherEvenementAleatoire()
    {
        // filtrer events disponibles (non null et implemente)
        GameObject[] eventsActifs = System.Array.FindAll(evenementsDisponibles, evt => evt != null);

        if (eventsActifs.Length == 0)
        {
            Debug.LogWarning("EVENT MANAGER : aucun event disponible");
            return;
        }

        // choisir aleatoire
        GameObject eventChoisi = eventsActifs[Random.Range(0, eventsActifs.Length)];

        evenementEnCours = true;
        eventChoisi.SetActive(true);

        Debug.Log($"EVENT MANAGER : {eventChoisi.name} declenche");
    }

    public void EvenementTermine()
    {
        evenementEnCours = false;

        // calculer prochain cooldown selon difficulte progressive
        float progression = Mathf.Clamp01(GameManager.Instance.tempsEcoule / tempsPourDifficulteMax);
        float cooldownMin = Mathf.Lerp(cooldownMinInitial, cooldownMinFinal, progression);
        float cooldownMax = Mathf.Lerp(cooldownMaxInitial, cooldownMaxFinal, progression);

        float cooldown = Random.Range(cooldownMin, cooldownMax);
        prochainEvenement = Time.time + cooldown;

        Debug.Log($"EVENT MANAGER : prochain event dans {cooldown:F1}s (difficulte {progression:P0})");
    }

    public bool EventEnCours()
    {
        return evenementEnCours;
    }
}