using UnityEngine;

public class MeshFeuController : MonoBehaviour
{
    [Header("Paramètres")]
    public float scaleMin = 0f;
    public float scaleMax = 1.5f; // hauteur maximale du mesh feu

    [Header("Seuils audio (inversés)")]
    public float seuilAllumage = 3900f;
    public float seuilTheiere = 196f;

    private int angleActuel = 0;
    private bool bruleurAllume = false;

    void Start()
    {
        // Scale initial à 0
        transform.localScale = new Vector3(
            transform.localScale.x,
            scaleMin,
            transform.localScale.z
        );
    }

    void Update()
    {
        // gère les sons du brûleur
        GererSonsBruleur();
    }

    // OSCInputManager mettre à jour le scale
    public void UpdateScale(float valeurAngle)
    {
        angleActuel = (int)valeurAngle;

        // map la valeur 0-4096 vers scaleMin-scaleMax
        float normalized = 1f - (valeurAngle / 4096f); // inversé
        float newScaleY = Mathf.Lerp(scaleMin, scaleMax, normalized);

        // applique le nouveau scale
        transform.localScale = new Vector3(
            transform.localScale.x,
            newScaleY,
            transform.localScale.z
        );
    }

    void GererSonsBruleur()
    {
        if (AudioManager.Instance == null) return;

        // allumé si angle < seuilAllumage (car 0 = max)
        bool estAllume = angleActuel < seuilAllumage;

        // detect le passage de éteint à allumé
        if (estAllume && !bruleurAllume)
        {
            // s'allume
            AudioManager.Instance.JouerBruleurAllumage();
            AudioManager.Instance.DemarrerBruleurConstant();
            Debug.Log("BRULEUR : Allumé (angle < " + seuilAllumage + ")");
        }
        else if (!estAllume && bruleurAllume)
        {
            // s'éteint
            AudioManager.Instance.ArreterBruleurConstant();
            Debug.Log("BRULEUR : Éteint (angle ≥ " + seuilAllumage + ")");
        }

        // volume du brûleur constant
        if (bruleurAllume)
        {
            // de 0 à seuilAllumage 
            float volumeBruleur = CalculerVolumeInverse(angleActuel, 0f, seuilAllumage);
            AudioManager.Instance.SetVolumeBruleurConstant(volumeBruleur);
        }

        // volume de la théière
        // volume de seuilTheiere à 0 
        float volumeTheiere = CalculerVolumeInverse(angleActuel, 0f, seuilTheiere);
        AudioManager.Instance.SetVolumeTheiere(volumeTheiere);

        bruleurAllume = estAllume;
    }

    // calc le volume proportionnel
    float CalculerVolumeInverse(float valeur, float min, float max)
    {
        if (valeur >= max)
            return 0f; // si au-dessus du max, volume à 0
        if (valeur <= min)
            return 1f; // si au minimum, volume max

        // plus l'angle est bas, plus le volume est haut
        return 1f - ((valeur - min) / (max - min));
    }
}