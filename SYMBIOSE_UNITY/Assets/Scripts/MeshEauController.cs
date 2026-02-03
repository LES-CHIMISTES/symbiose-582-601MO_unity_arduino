using UnityEngine;

public class MeshEauController : MonoBehaviour
{
    public float scaleMin = 0f;
    public float scaleMax = 2f; // hauteur maximale du mesh
    public float seuilAgitation = 0.3f; // seuil
    public float vitesseRemplissage = 0.5f; // vitesse eau se remplit
    public float vitesseEvaporation = 0.05f; // vitesse à laquelle l'eau s'évapore (diminue)

    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0f, 1f);
    public Color couleurMauve = new Color(0.6f, 0f, 1f);
    public Color couleurDefaut = new Color(0.3f, 0.6f, 1f);

    private Renderer meshRenderer;
    private Material meshMaterial;
    private float niveauEauActuel = 0f; // eau accumulée (0 à 1)
    private Vector3 positionInitiale; // position de départ du mesh

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        meshMaterial = new Material(meshRenderer.material);
        meshRenderer.material = meshMaterial;
        meshMaterial.color = couleurDefaut;

        // save la position initiale
        positionInitiale = transform.localPosition;

        // scale à 0/min
        transform.localScale = new Vector3(
            transform.localScale.x,
            scaleMin,
            transform.localScale.z
        );
    }

    void Update()
    {
        // évaporation constente
        niveauEauActuel -= vitesseEvaporation * Time.deltaTime;
        niveauEauActuel = Mathf.Clamp01(niveauEauActuel); // reste entre 0 et 1

        // scale selon état niveau eau
        float targetScaleY = Mathf.Lerp(scaleMin, scaleMax, niveauEauActuel);

        // calc le décalage de position pour que ça scale depuis le bas
        // qd scale, le centre monte de la moitié de la différence
        float decalageY = (targetScaleY - scaleMin) / 2f;

        // applique le scale
        transform.localScale = new Vector3(
            transform.localScale.x,
            targetScaleY,
            transform.localScale.z
        );

        // applique le décalage Y pour compenser
        transform.localPosition = new Vector3(
            positionInitiale.x,
            positionInitiale.y + decalageY,
            positionInitiale.z
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