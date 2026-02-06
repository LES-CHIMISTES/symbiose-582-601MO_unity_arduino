using UnityEngine;
using UnityEngine.UI;

public class StationPoudresFeedback : MonoBehaviour
{
    [Header("ui")]
    public Image cercleCouleur; // cercle qui change de couleur

    [Header("couleurs")]
    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0f, 1f);
    public Color couleurBlanche = Color.white;

    [Header("params")]
    public float tempsPourReagir = 2f; // temps pour appuyer sur bon bouton

    private enum CouleurCible { Vert = 1, Bleu = 2, Blanc = 3 }
    private CouleurCible couleurActuelle = CouleurCible.Vert;
    private float chronoReaction = 0f;

    void Start()
    {
        // couleur initiale
        ChangerCouleur();
    }

    void Update()
    {
        // compter temps de réaction
        chronoReaction += Time.deltaTime;

        // échec si timeout
        if (chronoReaction >= tempsPourReagir)
        {
            Debug.LogWarning("POUDRES : Timeout ! échec");
            // TODO : notifier échec global
            ChangerCouleur(); // reset avec nouvelle couleur
        }
    }

    // appelé par OSCInputManager quand key appuyée
    public void AppuyerBouton(int keyNumber)
    {
        if (keyNumber == (int)couleurActuelle)
        {
            // bon bouton !
            Debug.Log("POUDRES : Bon bouton !");
            ChangerCouleur(); // nouvelle couleur
        }
        else
        {
            Debug.LogWarning("POUDRES : Mauvais bouton !");
            // TODO : notifier échec global
        }
    }

    void ChangerCouleur()
    {
        // nouvelle couleur aléatoire
        couleurActuelle = (CouleurCible)Random.Range(1, 4); // 1, 2 ou 3

        // update ui
        if (cercleCouleur != null)
        {
            switch (couleurActuelle)
            {
                case CouleurCible.Vert:
                    cercleCouleur.color = couleurVerte;
                    break;
                case CouleurCible.Bleu:
                    cercleCouleur.color = couleurBleue;
                    break;
                case CouleurCible.Blanc:
                    cercleCouleur.color = couleurBlanche;
                    break;
            }
        }

        // reset chrono
        chronoReaction = 0f;
        Debug.Log("POUDRES : Nouvelle couleur = " + couleurActuelle);
    }

    public bool EstEnEquilibre()
    {
        return chronoReaction < tempsPourReagir; // pas timeout
    }
}