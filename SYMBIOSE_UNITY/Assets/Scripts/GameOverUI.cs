using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI texteTemps;
    public TextMeshProUGUI texteMessage;
    public CanvasGroup canvasGroup; // Pour le fade

    [Header("Paramètres")]
    public float delaiAvantFadeOut = 8f; // 8 secondes avant fade
    public float dureeFadeOut = 2f; // Durée du fade

    public GameObject[] uiManipulationsContinues;

    void Start()
    {
        // Cacher au départ
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Ajouter CanvasGroup si pas présent
        if (canvasGroup == null && gameOverPanel != null)
        {
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }

        // Écouter Game Over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver.AddListener(AfficherGameOver);
        }
    }

    void AfficherGameOver()
    {
        if (gameOverPanel == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MuterTout();
            AudioManager.Instance.JouerSonsGameOver();
        }

        gameOverPanel.SetActive(true);
        CacherUIManipulations();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (GameManager.Instance != null)
        {
            string temps = GameManager.Instance.FormatTemps(GameManager.Instance.tempsEcoule);
            int evenements = GameManager.Instance.evenementsResolus;

            if (texteTemps != null)
            {
                texteTemps.text = $"temps de survie : {temps}";
            }

            if (texteMessage != null)
            {
                texteMessage.text = $"événements résolus : {evenements}";
            }
        }

        StartCoroutine(FadeOutEtRedemarrer());
    }

    void CacherUIManipulations()
    {
        if (uiManipulationsContinues == null || uiManipulationsContinues.Length == 0)
        {
            return;
        }

        foreach (GameObject ui in uiManipulationsContinues)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }
    IEnumerator FadeOutEtRedemarrer()
    {
        // Attendre 8 secondes
        yield return new WaitForSeconds(delaiAvantFadeOut);

        // Fade out
        float elapsed = 0f;
        float alphaDepart = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < dureeFadeOut)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dureeFadeOut;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(alphaDepart, 0f, t);
            }

            yield return null;
        }

        // Recharger la scène (retour au tutoriel)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("GAME OVER : Redémarrage automatique");
    }
}