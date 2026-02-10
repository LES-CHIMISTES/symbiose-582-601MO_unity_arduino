using UnityEngine;
using UnityEngine.UI;

public class StationEauFeedback : MonoBehaviour
{
    [Header("ui")]
    public Slider jaugeEau; // jauge verticale
    public RectTransform cibleEau; // tiret rouge
    public RectTransform barreTemps; // barre horizontale qui scale

    [Header("params")]
    public float positionCibleMin = 0.2f; // position min cible (20%)
    public float positionCibleMax = 0.8f; // position max cible (80%)
    public float tolerance = 0.1f; // marge d'erreur (10%)
    public float tempsMaintien = 2f; // temps à maintenir à la cible

    private float positionCibleActuelle = 0.5f; // position actuelle cible (0-1)
    private float niveauEauActuel = 0f; // niveau eau actuel (0-1)
    private float chronoMaintien = 0f; // chrono pour maintenir à la cible
    private bool enEquilibre = false;
    private Vector3 scaleInitialBarre; // scale initial de la barre

    void Start()
    {
        // position cible initiale
        positionCibleActuelle = Random.Range(positionCibleMin, positionCibleMax);
        UpdateCiblePosition();
        
        // save scale initial barre
        if (barreTemps != null)
        {
            scaleInitialBarre = barreTemps.localScale;
            // cacher barre au départ
            barreTemps.gameObject.SetActive(false);
        }
        
        Debug.Log("EAU : Cible initiale à " + (positionCibleActuelle * 100) + "%");
    }

    void Update()
    {
        // update jauge selon niveau eau
    if (jaugeEau != null)
    {
        jaugeEau.value = niveauEauActuel;
    }

    // check si niveau eau atteint cible
    float difference = Mathf.Abs(niveauEauActuel - positionCibleActuelle);
    
    // DEBUG
    Debug.Log($"EAU : Niveau={niveauEauActuel:F3}, Cible={positionCibleActuelle:F3}, Diff={difference:F3}, Tolérance={tolerance}");
    
        if (difference <= tolerance)
        {
            // en équilibre
            if (!enEquilibre)
            {
                enEquilibre = true;
                chronoMaintien = 0f;
                
                // afficher barre
                if (barreTemps != null)
                {
                    barreTemps.gameObject.SetActive(true);
                }
            }

            // compter temps en équilibre
            chronoMaintien += Time.deltaTime;

            // update barre temps avec scale X
            if (barreTemps != null)
            {
                float progression = chronoMaintien / tempsMaintien;
                
                // scale x (de 1 à 0)
                Vector3 nouveauScale = scaleInitialBarre;
                nouveauScale.x = scaleInitialBarre.x * (1f - progression);
                barreTemps.localScale = nouveauScale;
                
                // fade out alpha
                Image barreImage = barreTemps.GetComponent<Image>();
                if (barreImage != null)
                {
                    Color couleur = barreImage.color;
                    couleur.a = 1f - progression;
                    barreImage.color = couleur;
                }
            }

            // changer cible après temps écoulé
            if (chronoMaintien >= tempsMaintien)
            {
                DeplacerCible();
            }
        }
        else
        {
            // plus en équilibre
            if (enEquilibre)
            {
                enEquilibre = false;
                chronoMaintien = 0f;
                
                // cacher barre
                if (barreTemps != null)
                {
                    barreTemps.gameObject.SetActive(false);
                    // reset pour prochaine fois
                    barreTemps.localScale = scaleInitialBarre;
                    Image barreImage = barreTemps.GetComponent<Image>();
                    if (barreImage != null)
                    {
                        Color couleur = barreImage.color;
                        couleur.a = 1f;
                        barreImage.color = couleur;
                    }
                }
            }
        }
    }

    // appelé par MeshEauController ou DebugInputSimulator
    public void UpdateNiveauEau(float niveau)
    {
        niveauEauActuel = Mathf.Clamp01(niveau);
    }

    void DeplacerCible()
    {
        // nouvelle position aléatoire
        positionCibleActuelle = Random.Range(positionCibleMin, positionCibleMax);
        UpdateCiblePosition();
        chronoMaintien = 0f;
        enEquilibre = false;
        
        // cacher barre
        if (barreTemps != null)
        {
            barreTemps.gameObject.SetActive(false);
        }
        
        Debug.Log("EAU : Nouvelle cible à " + (positionCibleActuelle * 100) + "%");
    }

    void UpdateCiblePosition()
    {
        if (cibleEau == null || jaugeEau == null) return;

        // calculer position Y du tiret selon position cible
        RectTransform jaugeRect = jaugeEau.GetComponent<RectTransform>();
        float hauteurJauge = jaugeRect.rect.height;
        float positionY = hauteurJauge * positionCibleActuelle - (hauteurJauge / 2f);

        // positionner le tiret
        cibleEau.anchoredPosition = new Vector2(cibleEau.anchoredPosition.x, positionY);
    }

    public bool EstEnEquilibre()
    {
        float difference = Mathf.Abs(niveauEauActuel - positionCibleActuelle);
        return difference <= tolerance;
    }
}