using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System.Collections;

public class StationEauFeedback : MonoBehaviour
{

    public OSCTransmitter oscTransmitter;
    [Header("UI")]
    public Slider jaugeEau;
    public RectTransform cibleEau;
    public RectTransform barreTemps;
    [Header("Versement")]
    public VersementController versementController;

    [Header("Params")]
    public float tolerance = 0.08f;
    public float tempsMaintien = 2f;

    [Header("Stabilité")]
    public float perteStabiliteHorsEquilibre = 5f; // (par seconde)

    private float positionCibleActuelle = 0.5f;
    private float valeurCiblePourComparaison = 0.5f; // NOUVEAU : la vraie valeur pour comparaison
    private float niveauEauActuel = 0f;
    private float chronoMaintien = 0f;
    private bool enEquilibre = false;
    private Vector3 scaleInitialBarre;

    private RectTransform fillRect;
    private RectTransform fillAreaRect;
    private float hauteurCible = 0f;


    void Start()
    {
        // Trouver les composants Fill et Fill Area
        if (jaugeEau != null)
        {
            Transform fillArea = jaugeEau.transform.Find("Fill Area");
            if (fillArea != null)
            {
                fillAreaRect = fillArea.GetComponent<RectTransform>();
                Transform fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    fillRect = fill.GetComponent<RectTransform>();
                    Debug.Log("EAU : Fill et Fill Area trouvés !");
                }
            }
        }

        // Configurer la cible
        if (cibleEau != null && fillAreaRect != null)
        {
            hauteurCible = cibleEau.rect.height;

            cibleEau.anchorMin = fillAreaRect.anchorMin;
            cibleEau.anchorMax = fillAreaRect.anchorMax;
            cibleEau.pivot = new Vector2(0.5f, 0.5f);

            Debug.Log($"EAU : Cible configurée - Hauteur = {hauteurCible}, Anchors = {cibleEau.anchorMin} / {cibleEau.anchorMax}");
        }

        positionCibleActuelle = Random.Range(0.2f, 0.8f);
        UpdateCiblePosition();

        if (barreTemps != null)
        {
            scaleInitialBarre = barreTemps.localScale;
            barreTemps.gameObject.SetActive(false);
        }

        Debug.Log($"EAU : Cible initiale à {positionCibleActuelle:F3}");
    }

    void Update()
    {
        if (jaugeEau != null)
        {
            jaugeEau.value = niveauEauActuel;
        }

        // IMPORTANT : Comparer avec valeurCiblePourComparaison au lieu de positionCibleActuelle
        float difference = Mathf.Abs(niveauEauActuel - valeurCiblePourComparaison);

        if (Time.frameCount % 60 == 0)
        {
            //Debug.Log($"EAU : Niveau={niveauEauActuel:F3}, Cible={valeurCiblePourComparaison:F3}, Diff={difference:F3}, Tolérance={tolerance}, EnÉquilibre={difference <= tolerance}");
        }

        if (difference <= tolerance)
        {
            if (!enEquilibre)
            {
                enEquilibre = true;
                chronoMaintien = 0f;

                if (barreTemps != null)
                {
                    barreTemps.gameObject.SetActive(true);
                }

                Debug.Log("EAU : ✓ Entré en équilibre !");
            }

            chronoMaintien += Time.deltaTime;

            if (barreTemps != null)
            {
                float progression = chronoMaintien / tempsMaintien;

                Vector3 nouveauScale = scaleInitialBarre;
                nouveauScale.x = scaleInitialBarre.x * (1f - progression);
                barreTemps.localScale = nouveauScale;

                Image barreImage = barreTemps.GetComponent<Image>();
                if (barreImage != null)
                {
                    Color couleur = barreImage.color;
                    couleur.a = 1f - (progression * 0.5f);
                    barreImage.color = couleur;
                }
            }

            if (chronoMaintien >= tempsMaintien)
            {
                DeplacerCible();
            }
        }
        else
        {
            if (enEquilibre)
            {
                enEquilibre = false;
                chronoMaintien = 0f;

                if (barreTemps != null)
                {
                    barreTemps.gameObject.SetActive(false);
                    barreTemps.localScale = scaleInitialBarre;
                    Image barreImage = barreTemps.GetComponent<Image>();
                    if (barreImage != null)
                    {
                        Color couleur = barreImage.color;
                        couleur.a = 1f;
                        barreImage.color = couleur;
                    }
                }

                Debug.Log("EAU : Sorti de l'équilibre");
            }
        }
    }

    public void UpdateNiveauEau(float niveau)
    {
        niveauEauActuel = Mathf.Clamp01(niveau);
    }

    void DeplacerCible()
    {
        if (versementController != null)
        {
            versementController.Verser();
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerDeplacerCible();
        }
        positionCibleActuelle = Random.Range(0.2f, 0.8f);
        UpdateCiblePosition();
        chronoMaintien = 0f;
        enEquilibre = false;

        if (barreTemps != null)
        {
            barreTemps.gameObject.SetActive(false);
        }

        EnvoyerOSCDeplacementCible();

        Debug.Log($"EAU : ✓✓ Nouvelle cible à {positionCibleActuelle:F3}");
    }

    void UpdateCiblePosition()
    {
        if (cibleEau == null || fillRect == null || fillAreaRect == null) return;

        float fillAreaHeight = fillAreaRect.rect.height;
        float topOffset = Mathf.Abs(fillRect.offsetMax.y);

        // Positionner visuellement la cible (garde le code actuel)
        float positionHautFillReel = (fillAreaHeight * positionCibleActuelle) + topOffset;
        float positionY = positionHautFillReel + (hauteurCible / 2f);
        cibleEau.anchoredPosition = new Vector2(cibleEau.anchoredPosition.x, positionY);

        // OPTION : Détecter au HAUT de la cible au lieu du centre
        float hautDeLaCible = positionY + (hauteurCible / 2f);
        valeurCiblePourComparaison = (hautDeLaCible - topOffset) / fillAreaHeight;
        valeurCiblePourComparaison = Mathf.Clamp01(valeurCiblePourComparaison);

        Debug.Log($"EAU : Cible Y = {positionY:F1}, Haut cible = {hautDeLaCible:F1}, Valeur pour comparaison = {valeurCiblePourComparaison:F3} (positionCibleActuelle = {positionCibleActuelle:F3})");
    }

    public bool EstEnEquilibre()
    {
        float difference = Mathf.Abs(niveauEauActuel - valeurCiblePourComparaison);
        return difference <= tolerance;
    }

    void EnvoyerOSCDeplacementCible()
    {
        Debug.Log("=== DÉBUT EnvoyerOSCDeplacementCible ===");

        if (oscTransmitter == null)
        {
            Debug.LogError("OSC : oscTransmitter est NULL !");
            return;
        }

        Debug.Log("OSC : oscTransmitter OK, envoi du pulse...");

        // Envoyer 1 (pulse ON)
        var messagePulseOn = new OSCMessage("/eau/deplacer");
        messagePulseOn.AddValue(OSCValue.Int(1));
        oscTransmitter.Send(messagePulseOn);
        Debug.Log("OSC : Pulse ON envoyé (1) à /eau/deplacer");

        // Envoyer 0 après 0.5 secondes
        StartCoroutine(EnvoyerPulseOff());

        Debug.Log("=== FIN EnvoyerOSCDeplacementCible ===");
    }

    System.Collections.IEnumerator EnvoyerPulseOff()
    {
        yield return new WaitForSeconds(0.5f);

        if (oscTransmitter != null)
        {
            var messagePulseOff = new OSCMessage("/eau/deplacer");
            messagePulseOff.AddValue(OSCValue.Int(0));
            oscTransmitter.Send(messagePulseOff);
            Debug.Log("OSC : Pulse OFF envoyé (0) à /eau/deplacer après 0.5s");
        }
    }
}