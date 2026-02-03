using UnityEngine;

public class MeshEauController : MonoBehaviour
{
    [Header("Paramètres de remplissage")]
    public float scaleMin = 0f;
    public float scaleMax = 1f; // hauteur maximale du mesh
    public float seuilAgitation = 0.05f; // seuil
    public float vitesseRemplissage = 15f; // vitesse eau se remplit
    public float vitesseEvaporation = 0.05f; // vitesse à laquelle l'eau s'évapore (diminue)

    [Header("Position Z cible")]
    public float positionZMin = -0.00122f; // position Z quand scale = 0
    public float positionZMax = 0f; // position Z quand scale = 1

    [Header("Couleurs")]
    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0f, 1f);
    public Color couleurMauve = new Color(0.6f, 0f, 1f);
    public Color couleurDefaut = new Color(0.3f, 0.6f, 1f);

    private Renderer meshRenderer;
    private Material meshMaterial;
    private float niveauEauActuel = 0f; // eau accumulée (0 à 1)
    private Vector3 scaleInitial; // scale de départ (pour X et Y)
    private Vector3 positionInitiale; // position de départ du mesh

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        meshMaterial = new Material(meshRenderer.material);
        meshRenderer.material = meshMaterial;
        meshMaterial.color = couleurDefaut;

        // save le scale initial (pour garder les valeurs X et Y)
        scaleInitial = transform.localScale;

        // save la position initiale
        positionInitiale = transform.localPosition;

        // scale à 0/min
        transform.localScale = new Vector3(
            scaleInitial.x,
            scaleInitial.y,
            scaleMin
        );

        // position à positionZMin au départ
        transform.localPosition = new Vector3(
            positionInitiale.x,
            positionInitiale.y,
            positionZMin
        );
    }

    void Update()
    {
        // évaporation constante
        niveauEauActuel -= vitesseEvaporation * Time.deltaTime;
        niveauEauActuel = Mathf.Clamp01(niveauEauActuel); // reste entre 0 et 1

        // scale selon état niveau eau
        float targetScale = Mathf.Lerp(scaleMin, scaleMax, niveauEauActuel);

        // position Z interpolée entre positionZMin et positionZMax
        float targetPositionZ = Mathf.Lerp(positionZMin, positionZMax, niveauEauActuel);

        // applique le scale sur Z uniquement
        transform.localScale = new Vector3(
            scaleInitial.x,
            scaleInitial.y,
            targetScale
        );

        // applique la position Z interpolée
        transform.localPosition = new Vector3(
            positionInitiale.x,
            positionInitiale.y,
            targetPositionZ
        );
    }

    // OSCInputManager (détecte agitation)
    public void UpdateAccel(float valeurAccel)
    {
        // dépasse seuil, remplissage
        if (valeurAccel > seuilAgitation)
        {
            // augmente proportionnellement au mouvement
            float augmentation = (valeurAccel - seuilAgitation) * vitesseRemplissage * Time.deltaTime;
            niveauEauActuel += augmentation;
            // clamp entre 0 et 1
            niveauEauActuel = Mathf.Clamp01(niveauEauActuel);
        }
    }

    // OSCInputManager
    public void SetCouleur(int keyNumber)
    {
        switch (keyNumber)
        {
            case 1: // key1 = vert
                meshMaterial.color = couleurVerte;
                break;
            case 2: // key2 = bleu
                meshMaterial.color = couleurBleue;
                break;
            case 3: // key3 = mauve
                meshMaterial.color = couleurMauve;
                break;
            default: // aucun = default
                meshMaterial.color = couleurDefaut;
                break;
        }
    }

    // méthode debug/pour plus tard
    public void ViderEau()
    {
        niveauEauActuel = 0f;
    }

    // méthode debug/pour plus tard
    public float GetNiveauEau()
    {
        return niveauEauActuel;
    }
}