using UnityEngine;

public class FluxPoudreController : MonoBehaviour
{
    public static FluxPoudreController Instance { get; private set; }

    [Header("References")]
    public ParticleSystem systemeParticules;

    [Header("Couleurs (meme ordre que StationPoudres)")]
    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0.4f, 1f);
    public Color couleurBlanche = Color.white;

    private bool actif = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (systemeParticules == null)
        {
            systemeParticules = GetComponent<ParticleSystem>();
        }

        systemeParticules.Stop();
    }

    public void Demarrer()
    {
        if (actif) return;

        actif = true;
        gameObject.SetActive(true);
        systemeParticules.Play();

        Debug.Log("FLUX POUDRE : demarre");
    }

    public void Arreter()
    {
        if (!actif) return;

        actif = false;
        systemeParticules.Stop();

        Debug.Log("FLUX POUDRE : arrete");
    }

    public void ChangerCouleur(int keyNumber)
    {
        if (systemeParticules == null) return;

        Color nouvelleCouleur;

        switch (keyNumber)
        {
            case 1: nouvelleCouleur = couleurVerte; break;
            case 2: nouvelleCouleur = couleurBleue; break;
            case 3: nouvelleCouleur = couleurBlanche; break;
            default: nouvelleCouleur = couleurBlanche; break;
        }

        var main = systemeParticules.main;
        main.startColor = nouvelleCouleur;

        Debug.Log($"FLUX POUDRE : couleur changee -> key {keyNumber}");
    }

    public bool EstActif()
    {
        return actif;
    }
}