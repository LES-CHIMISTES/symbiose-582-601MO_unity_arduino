using UnityEngine;

public class PinceeMain : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public ParticleSystem particulesGrains;

    [Header("Couleurs")]
    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0.4f, 1f);
    public Color couleurBlanche = Color.white;

    private bool occupee = false;

    public void Lancer(int keyNumber)
    {
        // changer couleur des particules
        if (particulesGrains != null)
        {
            var main = particulesGrains.main;
            main.startColor = GetCouleur(keyNumber);
        }

        // forcer le replay meme si deja en cours
        animator.Play("AnimPincee", 0, 0f);

        occupee = true;
    }

    // appele par Animation Event au moment du frottement
    public void LancerParticules()
    {
        if (particulesGrains != null)
        {
            particulesGrains.Play();
        }
    }

    // appele par Animation Event a la fin de l'anim
    public void FinPincee()
    {
        occupee = false;
    }

    public bool EstOccupee()
    {
        return occupee;
    }

    Color GetCouleur(int keyNumber)
    {
        switch (keyNumber)
        {
            case 1: return couleurVerte;
            case 2: return couleurBleue;
            case 3: return couleurBlanche;
            default: return couleurBlanche;
        }
    }
}