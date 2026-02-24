using UnityEngine;
using UnityEngine.UI;

public class InactiviteFlash : MonoBehaviour
{
    [Header("References")]
    public Image overlayFlash;
    public MonoBehaviour stationFeedback;

    [Header("Parametres")]
    public float delaiAvantFlash = 4f;
    public float vitesseFlash = 3f;
    public float alphaMax = 0.3f;

    private bool enFlash = false;

    void Start()
    {
        if (overlayFlash != null)
        {
            overlayFlash.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (stationFeedback == null || !stationFeedback.enabled) return;
        if (GameManager.Instance == null || GameManager.Instance.EstEnTutoriel()) return;
        if (GameManager.Instance.enGameOver) return;

        float dernierTemps = GetDernierTempsReussite();
        float tempsInactif = Time.time - dernierTemps;

        if (tempsInactif >= delaiAvantFlash)
        {
            if (!enFlash && overlayFlash != null)
            {
                enFlash = true;
                overlayFlash.gameObject.SetActive(true);
            }

            if (enFlash && overlayFlash != null)
            {
                float pulse = Mathf.Sin(Time.time * vitesseFlash) * 0.5f + 0.5f;
                Color c = overlayFlash.color;
                c.a = pulse * alphaMax;
                overlayFlash.color = c;
            }
        }
        else
        {
            if (enFlash)
            {
                enFlash = false;
                if (overlayFlash != null)
                {
                    overlayFlash.gameObject.SetActive(false);
                }
            }
        }
    }

    float GetDernierTempsReussite()
    {
        if (stationFeedback is StationEauFeedback eau)
            return eau.dernierTempsReussite;
        if (stationFeedback is StationFeuFeedback feu)
            return feu.dernierTempsReussite;
        if (stationFeedback is StationPoudresFeedback poudres)
            return poudres.dernierTempsReussite;
        if (stationFeedback is StationTourbillonFeedback tourbillon)
            return tourbillon.dernierTempsReussite;

        return Time.time;
    }
}