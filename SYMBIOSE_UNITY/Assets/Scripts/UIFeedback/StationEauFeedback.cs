using UnityEngine;
using UnityEngine.UI;

public class StationEauFeedback : MonoBehaviour
{
    [Header("ui")]
    public Slider jaugeEau; // jauge verticale
    public RectTransform cibleEau; // tiret rouge

    [Header("params")]
    public float positionCibleMin = 0.2f; // position min cible (20%)
    public float positionCibleMax = 0.8f; // position max cible (80%)
    public float tolerance = 0.05f; // marge d'erreur (5%)
    public float vitesseChangementCible = 2f; // vitesse déplacement cible

    private float positionCibleActuelle = 0.5f; // position actuelle cible (0-1)
    private float niveauEauActuel = 0f; // niveau eau actuel (0-1)
    private bool cibleAtteinte = false;

    void Start()
    {
        // position cible initiale
        positionCibleActuelle = Random.Range(positionCibleMin, positionCibleMax);
        UpdateCiblePosition();
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

        if (difference <= tolerance && !cibleAtteinte)
        {
            // cible atteinte ! déplacer vers nouvelle position
            cibleAtteinte = true;
            DeplacerCible();
        }
        else if (difference > tolerance)
        {
            cibleAtteinte = false;
        }
    }

    // appelé par MeshEauController ou OSCInputManager
    public void UpdateNiveauEau(float niveau)
    {
        niveauEauActuel = Mathf.Clamp01(niveau);
    }

    void DeplacerCible()
    {
        // nouvelle position aléatoire
        positionCibleActuelle = Random.Range(positionCibleMin, positionCibleMax);
        UpdateCiblePosition();
        Debug.Log("EAU : Nouvelle cible à " + (positionCibleActuelle * 100) + "%");
    }

    void UpdateCiblePosition()
    {
        if (cibleEau == null || jaugeEau == null) return;

        // calculer position Y du tiret selon position cible
        float hauteurJauge = jaugeEau.GetComponent<RectTransform>().rect.height;
        float positionY = hauteurJauge * positionCibleActuelle;

        // positionner le tiret
        cibleEau.anchoredPosition = new Vector2(cibleEau.anchoredPosition.x, positionY);
    }

    // méthode pour débug/pour plus tard
    public bool EstEnEquilibre()
    {
        float difference = Mathf.Abs(niveauEauActuel - positionCibleActuelle);
        return difference <= tolerance;
    }
}