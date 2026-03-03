using UnityEngine;

public class PinceeController : MonoBehaviour
{
    [Header("Pool de mains (2-3)")]
    public PinceeMain[] mains;

    private int prochainIndex = 0;

    public void Pincer(int keyNumber)
    {
        if (mains == null || mains.Length == 0) return;

        // trouver la prochaine main disponible
        // si toutes occupees, prendre la prochaine dans le cycle quand meme
        PinceeMain main = mains[prochainIndex];
        prochainIndex = (prochainIndex + 1) % mains.Length;

        main.Lancer(keyNumber);

        Debug.Log($"PINCEE : main {prochainIndex} lancee, couleur key {keyNumber}");
    }

    public bool EstEnCours()
    {
        if (mains == null) return false;

        foreach (PinceeMain main in mains)
        {
            if (main != null && main.EstOccupee())
                return true;
        }

        return false;
    }
}