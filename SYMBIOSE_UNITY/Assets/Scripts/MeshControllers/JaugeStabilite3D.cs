using UnityEngine;

public class JaugeStabilite3D : MonoBehaviour
{
    [Header("References")]
    public Transform cylindre;
    public MeshRenderer cylindreRenderer;

    [Header("Scale Y")]
    public float scaleYMin = 0.000110661f;
    public float scaleYMax = 0.002330724f;

    [Header("Position Z")]
    public float posZMin = -0.002295f;
    public float posZMax = 0f;

    [Header("Couleurs")]
    public Color couleurSaine = new Color(0.3f, 0.9f, 0.3f, 1f);
    public Color couleurDanger = new Color(0.9f, 0.9f, 0.2f, 1f);
    public Color couleurCritique = new Color(0.9f, 0.2f, 0.2f, 1f);

    [Header("Smooth")]
    public float vitesseLerp = 8f;

    private Material mat;
    private float valeurAffichee = 1f;

    void Start()
    {
        if (cylindreRenderer != null)
        {
            mat = cylindreRenderer.material;
        }

        valeurAffichee = 1f;
        Actualiser(1f);
    }

    public void Actualiser(float pourcentage01)
    {
        // smooth
        valeurAffichee = Mathf.Lerp(valeurAffichee, pourcentage01, Time.deltaTime * vitesseLerp);

        if (cylindre != null)
        {
            // scale Y
            Vector3 scale = cylindre.localScale;
            scale.y = Mathf.Lerp(scaleYMin, scaleYMax, valeurAffichee);
            cylindre.localScale = scale;

            // position Z (compensation pivot)
            Vector3 pos = cylindre.localPosition;
            pos.z = Mathf.Lerp(posZMin, posZMax, valeurAffichee);
            cylindre.localPosition = pos;
        }

        // couleur
        if (mat != null)
        {
            Color couleur;

            if (valeurAffichee > 0.5f)
                couleur = Color.Lerp(couleurDanger, couleurSaine, (valeurAffichee - 0.5f) * 2f);
            else if (valeurAffichee > 0.2f)
                couleur = Color.Lerp(couleurCritique, couleurDanger, (valeurAffichee - 0.2f) / 0.3f);
            else
                couleur = couleurCritique;

            mat.SetColor("_BaseColor", couleur);
        }
    }
}