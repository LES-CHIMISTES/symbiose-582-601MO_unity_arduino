using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using extOSC;

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
    [Header("OSC")]
    public OSCTransmitter oscTransmitter;

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
        EnvoyerOSCGameOver(true);

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
                texteTemps.text = $"Temps de survie : {temps}";
            }

            if (texteMessage != null)
            {
                texteMessage.text = $"Événements résolus : {evenements}";
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
        EnvoyerOSCGameOver(false);
        ResetToutOSC();
        // Recharger la scène (retour au tutoriel)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("GAME OVER : Redémarrage automatique");
    }

    void EnvoyerOSCGameOver(bool actif)
    {
        if (oscTransmitter == null) return;

        var message = new OSCMessage("/gameover");
        message.AddValue(OSCValue.Int(actif ? 1 : 0));
        oscTransmitter.Send(message);
    }
    void OnApplicationQuit()
    {
        ResetToutOSC();
    }
    public void ResetToutOSC()
    {
        if (oscTransmitter == null) return;

        string[] adressesInt = new string[]
        {
        "/gameover",
        "/eau/deplacer",
        "/feu/angle",
        "/poudres/key1",
        "/poudres/key2",
        "/poudres/key3",
        "/lumiere/gel",
        "/lumiere/evaporation",
        "/lumiere/cristallisation",
        "/lumiere/vortex"
        };

        string[] adressesFloat = new string[]
        {
        "/tourbillon/angle",
        "/tourbillon/delta"
        };

        foreach (string adresse in adressesInt)
        {
            var msg = new OSCMessage(adresse);
            msg.AddValue(OSCValue.Int(0));
            oscTransmitter.Send(msg);
        }

        foreach (string adresse in adressesFloat)
        {
            var msg = new OSCMessage(adresse);
            msg.AddValue(OSCValue.Float(0f));
            oscTransmitter.Send(msg);
        }

        Debug.Log("OSC : toutes les adresses remises a 0");
    }
}