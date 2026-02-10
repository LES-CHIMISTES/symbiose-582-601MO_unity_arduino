using UnityEngine;

public class MeshEauController : MonoBehaviour
{
    [Header("Param�tres de remplissage")]
    public float scaleMin = 0f;
    public float scaleMax = 1f; // hauteur maximale du mesh
    public float seuilAgitation = 0.05f; // seuil
    public float vitesseRemplissage = 15f; // vitesse eau se remplit
    public float vitesseEvaporation = 0.05f; // vitesse � laquelle l'eau s'�vapore (diminue)

    [Header("Position Z cible")]
    public float positionZMin = -0.00122f; // position Z quand scale = 0
    public float positionZMax = 0f; // position Z quand scale = 1

    [Header("Couleurs")]
    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0f, 1f);
    public Color couleurMauve = new Color(0.6f, 0f, 1f);
    public Color couleurDefaut = new Color(0.3f, 0.6f, 1f);

    private Renderer meshRenderer;
    public Material meshMaterial;
    public float niveauEauActuel = 0f; // eau accumul�e (0 � 1)
    private Vector3 scaleInitial; // scale de d�part (pour X et Y)
    private Vector3 positionInitiale; // position de d�part du mesh
    private float derniereScaleZ = 0f; // d�tecter augmentation
    public float dernierSonEau = 0f; // temps du dernier son
    public float cooldownSonEau = 0.3f; // cooldown entre chaque son

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

        // scale � 0/min
        transform.localScale = new Vector3(
            scaleInitial.x,
            scaleInitial.y,
            scaleMin
        );

        // position � positionZMin au d�part
        transform.localPosition = new Vector3(
            positionInitiale.x,
            positionInitiale.y,
            positionZMin
        );
    }

    void Update()
    {
        // �vaporation constante
        niveauEauActuel -= vitesseEvaporation * Time.deltaTime;
        niveauEauActuel = Mathf.Clamp01(niveauEauActuel);

        // scale selon �tat niveau eau
        float targetScale = Mathf.Lerp(scaleMin, scaleMax, niveauEauActuel);

        // position Z interpol�e entre positionZMin et positionZMax
        float targetPositionZ = Mathf.Lerp(positionZMin, positionZMax, niveauEauActuel);

        // applique le scale sur Z uniquement
        transform.localScale = new Vector3(
            scaleInitial.x,
            scaleInitial.y,
            targetScale
        );

        // applique la position Z interpol�e
        transform.localPosition = new Vector3(
            positionInitiale.x,
            positionInitiale.y,
            targetPositionZ
        );
    }

    // OSCInputManager (d�tecte agitation)
    public void UpdateAccel(float valeurAccel)
    {
        // d�passe seuil, remplissage
        if (valeurAccel > seuilAgitation)
        {
            // augmente proportionnellement au mouvement
            float augmentation = (valeurAccel - seuilAgitation) * vitesseRemplissage * Time.deltaTime;
            niveauEauActuel += augmentation;
            // clamp entre 0 et 1
            niveauEauActuel = Mathf.Clamp01(niveauEauActuel);

            // joue son avec cooldown pour �viter spam
            if (AudioManager.Instance != null)
            {
                // check si assez de temps s'est �coul� depuis le dernier son
                if (Time.time - dernierSonEau >= cooldownSonEau)
                {
                    AudioManager.Instance.JouerEauVersee();
                    dernierSonEau = Time.time; // update � jour le temps du dernier son
                }
            }
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

    // m�thode debug/pour plus tard
    public void ViderEau()
    {
        niveauEauActuel = 0f;
    }

    // m�thode debug/pour plus tard
public float GetNiveauEau()
{
    return Mathf.Clamp01(niveauEauActuel);
}
}