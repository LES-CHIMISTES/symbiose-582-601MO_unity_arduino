using UnityEngine;
using UnityEngine.UI;

public class StationPoudresFeedback : MonoBehaviour
{
    [Header("ui")]
    public Image cercleCouleur; // cercle qui change de couleur

    [Header("couleurs - ORDRE IMPORTANT")]
    public Color couleurVerte = new Color(0f, 1f, 0f);   // Key 1
    public Color couleurBleue = new Color(0f, 0f, 1f);   // Key 2
    public Color couleurBlanche = Color.white;           // Key 3

    [Header("params")]
    public float tempsPourReagir = 3f; // temps pour appuyer sur bon bouton

    private int couleurAttendue = 1; // 1=vert, 2=bleu, 3=blanc
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

    // appelé par OSCInputManager ou DebugInputSimulator quand key appuyée
    public void AppuyerBouton(int keyNumber)
    {
        Debug.Log("POUDRES : Bouton " + keyNumber + " appuyé, couleur attendue = " + couleurAttendue);
        
        if (keyNumber == couleurAttendue)
        {
            // bon bouton !
            Debug.Log("POUDRES : ✓ Bon bouton !");
            ChangerCouleur(); // nouvelle couleur
        }
        else
        {
            Debug.LogWarning("POUDRES : ✗ Mauvais bouton ! Attendu: " + couleurAttendue + ", Reçu: " + keyNumber);
            // TODO : notifier échec global
        }
    }

    void ChangerCouleur()
    {
        // nouvelle couleur aléatoire (1, 2 ou 3)
        couleurAttendue = Random.Range(1, 4);

        // update ui
        if (cercleCouleur != null)
        {
            switch (couleurAttendue)
            {
                case 1: // Key 1 = VERT
                    cercleCouleur.color = couleurVerte;
                    break;
                case 2: // Key 2 = BLEU
                    cercleCouleur.color = couleurBleue;
                    break;
                case 3: // Key 3 = BLANC
                    cercleCouleur.color = couleurBlanche;
                    break;
            }
        }

        // reset chrono
        chronoReaction = 0f;
        
        Debug.Log("POUDRES : Nouvelle couleur attendue = " + couleurAttendue + " (1=Vert/Key1, 2=Bleu/Key2, 3=Blanc/Key3)");
    }

    public bool EstEnEquilibre()
    {
        return chronoReaction < tempsPourReagir;
    }
}