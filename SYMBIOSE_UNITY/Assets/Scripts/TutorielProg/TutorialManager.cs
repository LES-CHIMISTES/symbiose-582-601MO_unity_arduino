using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Références Stations (scripts existants)")]
    public StationEauFeedback stationEau;
    public StationFeuFeedback stationFeu;
    public StationPoudresFeedback stationPoudres;
    public StationTourbillonFeedback stationTourbillon;

    [Header("Colonnes UI (pour alpha) - Ajouter CanvasGroup à chaque colonne")]
    public CanvasGroup colonneEau;
    public CanvasGroup colonneFeu;
    public CanvasGroup colonnePoudres;
    public CanvasGroup colonneTourbillon;

    [Header("UI Tutoriel")]
    public TutorialUI tutorialUI;

    [Header("Paramètres")]
    public float tempsMaintienRequis = 1.5f; // temps minimum en équilibre pour valider l'étape

    public AudioSource[] audioSourcesFeu;

    public enum TutorialStep
    {
        Eau = 0,
        Feu = 1,
        Poudres = 2,
        Tourbillon = 3,
        Termine = 4
    }

    private TutorialStep etapeActuelle = TutorialStep.Eau;
    private bool[] etapesCompletes = new bool[4];
    private float chronoMaintienEtape = 0f; // chrono pour l'étape actuelle

    public bool EstStationActive(string nomStation)
    {
        if (etapeActuelle == TutorialStep.Termine)
            return true; // Toutes actives après tutoriel

        switch (nomStation.ToLower())
        {
            case "eau":
                return etapeActuelle >= TutorialStep.Eau;
            case "feu":
                return etapeActuelle >= TutorialStep.Feu;
            case "poudres":
                return etapeActuelle >= TutorialStep.Poudres;
            case "tourbillon":
                return etapeActuelle >= TutorialStep.Tourbillon;
            default:
                return false;
        }
    }

    void Start()
    {
        // Désactiver toutes les stations sauf l'eau
        if (stationEau != null) stationEau.enabled = true;
        if (stationFeu != null) stationFeu.enabled = false;
        if (stationPoudres != null) stationPoudres.enabled = false;
        if (stationTourbillon != null) stationTourbillon.enabled = false;

        // Set alpha des colonnes (0.5 = grisé)
        SetAlphaColonne(colonneEau, 1f);
        SetAlphaColonne(colonneFeu, 0.5f);
        SetAlphaColonne(colonnePoudres, 0.5f);
        SetAlphaColonne(colonneTourbillon, 0.5f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MuterSonsFeu(true);
        }

        // Update UI
        if (tutorialUI != null)
        {
            tutorialUI.ActiverEtape(0);
        }

        Debug.Log("TUTORIEL : Étape 1/4 - Remplir niveau d'eau");
    }

    void Update()
    {
        // Vérifier si l'étape actuelle est complétée
        if (etapeActuelle != TutorialStep.Termine && !etapesCompletes[(int)etapeActuelle])
        {
            // Utiliser la méthode EstEnEquilibre() existante de chaque station
            bool estEnEquilibre = VerifierEquilibreEtapeActuelle();

            if (estEnEquilibre)
            {
                // Incrémenter chrono maintien
                chronoMaintienEtape += Time.deltaTime;

                // Valider si maintenu assez longtemps
                if (chronoMaintienEtape >= tempsMaintienRequis)
                {
                    CompleterEtape(etapeActuelle);
                }
            }
            else
            {
                // Reset chrono si sort de l'équilibre
                chronoMaintienEtape = 0f;
            }
        }
    }

    bool VerifierEquilibreEtapeActuelle()
    {
        // Appeler la méthode EstEnEquilibre() de la station active
        switch (etapeActuelle)
        {
            case TutorialStep.Eau:
                return stationEau != null && stationEau.EstEnEquilibre();

            case TutorialStep.Feu:
                return stationFeu != null && stationFeu.EstEnEquilibre();

            case TutorialStep.Poudres:
                // Pour poudres, on vérifie juste que le temps n'a pas expiré
                // (le joueur doit appuyer sur un bouton, pas maintenir)
                return stationPoudres != null && stationPoudres.EstEnEquilibre();

            case TutorialStep.Tourbillon:
                return stationTourbillon != null && stationTourbillon.EstEnEquilibre();

            default:
                return false;
        }
    }

    void CompleterEtape(TutorialStep etape)
    {
        etapesCompletes[(int)etape] = true;
        chronoMaintienEtape = 0f;

        // Update UI avec checkmark
        if (tutorialUI != null)
        {
            tutorialUI.CompleterEtape((int)etape);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerEtapeTutoriel();
        }

        Debug.Log($"TUTORIEL : ✓ Étape {(int)etape + 1}/4 complétée !");

        // Passer à l'étape suivante après un court délai
        if (etape == TutorialStep.Tourbillon)
        {
            // Toutes les étapes complétées
            etapeActuelle = TutorialStep.Termine;
            Invoke(nameof(TerminerTutoriel), 1.5f);
        }
        else
        {
            // Débloquer prochaine station après délai
            Invoke(nameof(DebloquerProchaineStation), 1f);
        }
    }

    void DebloquerProchaineStation()
    {
        // Passer à l'étape suivante
        etapeActuelle = (TutorialStep)((int)etapeActuelle + 1);

        // Activer la nouvelle station
        switch (etapeActuelle)
        {
            case TutorialStep.Feu:
                if (stationFeu != null) stationFeu.enabled = true;
                SetAlphaColonne(colonneFeu, 1f);

                // NOUVEAU : Démuter sons feu
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.MuterSonsFeu(false);
                }
                break;

            case TutorialStep.Poudres:
                if (stationPoudres != null) stationPoudres.enabled = true;
                SetAlphaColonne(colonnePoudres, 1f);
                break;

            case TutorialStep.Tourbillon:
                if (stationTourbillon != null) stationTourbillon.enabled = true;
                SetAlphaColonne(colonneTourbillon, 1f);
                break;
        }

        // Update UI
        if (tutorialUI != null)
        {
            tutorialUI.ActiverEtape((int)etapeActuelle);
        }

        Debug.Log($"TUTORIEL : Étape {(int)etapeActuelle + 1}/4 débloquée");
    }

    void SetAlphaColonne(CanvasGroup colonne, float alpha)
    {
        if (colonne != null)
        {
            colonne.alpha = alpha;
        }
    }

    void TerminerTutoriel()
    {
        // S'assurer que toutes les stations sont actives
        if (stationEau != null) stationEau.enabled = true;
        if (stationFeu != null) stationFeu.enabled = true;
        if (stationPoudres != null) stationPoudres.enabled = true;
        if (stationTourbillon != null) stationTourbillon.enabled = true;

        // Toutes colonnes à alpha 1
        SetAlphaColonne(colonneEau, 1f);
        SetAlphaColonne(colonneFeu, 1f);
        SetAlphaColonne(colonnePoudres, 1f);
        SetAlphaColonne(colonneTourbillon, 1f);

        // Notifier GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TerminerTutoriel();
        }

        // Cacher UI tutoriel
        if (tutorialUI != null)
        {
            tutorialUI.CacherUI();
        }

        Debug.Log("TUTORIEL : ✓✓✓ Tutoriel terminé ! Phase principale commence.");
    }

    void MuterAudioFeu(bool mute)
    {
        if (audioSourcesFeu == null || audioSourcesFeu.Length == 0)
        {
            Debug.LogWarning("TUTORIEL : Aucun AudioSource Feu assigné !");
            return;
        }

        foreach (AudioSource source in audioSourcesFeu)
        {
            if (source != null)
            {
                source.mute = mute;
            }
        }

        Debug.Log($"TUTORIEL : Audio Feu {(mute ? "muté" : "démuté")} ({audioSourcesFeu.Length} sources)");
    }
}