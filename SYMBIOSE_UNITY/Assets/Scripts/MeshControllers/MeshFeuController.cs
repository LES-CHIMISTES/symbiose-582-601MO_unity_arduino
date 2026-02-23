using UnityEngine;

public class MeshFeuController : MonoBehaviour
{
    [Header("Paramètres")]
    public float scaleMin = 0f;
    public float scaleMax = 1.5f;

    [Header("Pivot compensation")]
    public float hauteurMeshBase = 1f;  // hauteur du mesh quand scale Z = 1

    [Header("Visibilite")]
    public float seuilVisibilite = 0.05f;

    [Header("Lumiere")]
    public Light pointLight;
    public float intensiteMax = 0.08f;

    [Header("Seuils audio (inversés)")]
    public float seuilAllumage = 3900f;
    public float seuilTheiere = 196f;

    private int angleActuel = 0;
    private bool bruleurAllume = false;
    private Vector3 positionInitiale;
    private Vector3 scaleInitial;

    void Start()
    {
        positionInitiale = transform.localPosition;
        scaleInitial = transform.localScale;

        // scale initial a 0
        AppliquerScaleEtPosition(scaleMin);
    }

    void Update()
    {
        GererSonsBruleur();
    }

    public void UpdateScale(float valeurAngle)
    {
        angleActuel = (int)valeurAngle;

        float normalized = 1f - (valeurAngle / 4096f);
        float newScaleZ = Mathf.Lerp(scaleMin, scaleMax, normalized);

        AppliquerScaleEtPosition(newScaleZ);
    }

    void AppliquerScaleEtPosition(float scaleZ)
    {
        // appliquer scale sur Z
        transform.localScale = new Vector3(
            scaleInitial.x,
            scaleInitial.y,
            scaleZ
        );

        // compenser position Y pour que le bas reste fixe
        // quand scale = 0, le mesh est au sol
        // quand scale augmente, on monte de la moitie de la hauteur ajoutee
        float decalageY = (scaleZ * hauteurMeshBase) / 2f;
        transform.localPosition = new Vector3(
            positionInitiale.x,
            positionInitiale.y + decalageY,
            positionInitiale.z
        );

        // cacher si trop petit
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.enabled = scaleZ > seuilVisibilite;
        }

        if (pointLight != null)
        {
            pointLight.intensity = Mathf.Lerp(0f, intensiteMax, scaleZ / scaleMax);
        }
    }

    void GererSonsBruleur()
    {
        if (AudioManager.Instance == null) return;

        bool estAllume = angleActuel < seuilAllumage;

        if (estAllume && !bruleurAllume)
        {
            AudioManager.Instance.JouerBruleurAllumage();
            AudioManager.Instance.DemarrerBruleurConstant();
        }
        else if (!estAllume && bruleurAllume)
        {
            AudioManager.Instance.ArreterBruleurConstant();
        }

        if (bruleurAllume)
        {
            float volumeBruleur = CalculerVolumeInverse(angleActuel, 0f, seuilAllumage);
            AudioManager.Instance.SetVolumeBruleurConstant(volumeBruleur);
        }

        float volumeTheiere = CalculerVolumeInverse(angleActuel, 0f, seuilTheiere);
        AudioManager.Instance.SetVolumeTheiere(volumeTheiere);

        bruleurAllume = estAllume;
    }

    float CalculerVolumeInverse(float valeur, float min, float max)
    {
        if (valeur >= max) return 0f;
        if (valeur <= min) return 1f;
        return 1f - ((valeur - min) / (max - min));
    }
}