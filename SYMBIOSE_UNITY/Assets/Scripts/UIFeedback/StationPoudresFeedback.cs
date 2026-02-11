using UnityEngine;
using UnityEngine.UI;

public class StationPoudresFeedback : MonoBehaviour
{
    [Header("UI")]
    public Image cercleCouleur; // cercle qui change de couleur

    [Header("Couleurs - ORDRE IMPORTANT")]
    public Color couleurVerte = new Color(0f, 1f, 0f);   // Key 1
    public Color couleurBleue = new Color(0f, 0f, 1f);   // Key 2
    public Color couleurBlanche = Color.white;           // Key 3

    [Header("Params")]
    public float delaiAvantFade = 1f; // délai avant que le fade commence
    public float dureeFade = 3f; // durée du fade out

    private int couleurAttendue = 1; // 1=vert, 2=bleu, 3=blanc
    private float chronoTotal = 0f;
    private float tempsTotalMax; // delai + duree

    void Start()
    {
        tempsTotalMax = delaiAvantFade + dureeFade;
        ChangerCouleur();
    }

    void Update()
    {
        // compter temps total
        chronoTotal += Time.deltaTime;

        // fade out progressif du cercle (APRÈS le délai)
        if (cercleCouleur != null && chronoTotal >= delaiAvantFade)
        {
            float tempsDepuisDebutFade = chronoTotal - delaiAvantFade;
            float progression = tempsDepuisDebutFade / dureeFade;

            Color couleurActuelle = cercleCouleur.color;
            couleurActuelle.a = 1f - progression; // fade de 1 à 0
            cercleCouleur.color = couleurActuelle;
        }

        // échec si timeout
        if (chronoTotal >= tempsTotalMax)
        {
            Debug.LogWarning("POUDRES : ✗ Timeout ! Échec");
            // TODO : notifier échec global
            ChangerCouleur(); // reset avec nouvelle couleur
        }
    }

    // appelé par OSCInputManager ou DebugInputSimulator quand key appuyée
    public void AppuyerBouton(int keyNumber)
    {
        Debug.Log($"POUDRES : Bouton {keyNumber} appuyé, couleur attendue = {couleurAttendue}");

        if (keyNumber == couleurAttendue)
        {
            // bon bouton !
            Debug.Log("POUDRES : ✓ Bon bouton !");
            ChangerCouleur(); // nouvelle couleur
        }
        else
        {
            Debug.LogWarning($"POUDRES : ✗ Mauvais bouton ! Attendu: {couleurAttendue}, Reçu: {keyNumber}");
            // TODO : notifier échec global
        }
    }

    void ChangerCouleur()
    {
        // nouvelle couleur aléatoire (1, 2 ou 3)
        couleurAttendue = Random.Range(1, 4);

        // update ui avec alpha à 1 (pleine opacité)
        if (cercleCouleur != null)
        {
            Color nouvelleCouleur;
            switch (couleurAttendue)
            {
                case 1: // Key 1 = VERT
                    nouvelleCouleur = couleurVerte;
                    break;
                case 2: // Key 2 = BLEU
                    nouvelleCouleur = couleurBleue;
                    break;
                case 3: // Key 3 = BLANC
                    nouvelleCouleur = couleurBlanche;
                    break;
                default:
                    nouvelleCouleur = Color.white;
                    break;
            }

            // reset alpha à 1
            nouvelleCouleur.a = 1f;
            cercleCouleur.color = nouvelleCouleur;
        }

        // reset chrono
        chronoTotal = 0f;

        Debug.Log($"POUDRES : Nouvelle couleur = {couleurAttendue} (1=Vert, 2=Bleu, 3=Blanc), apparait 1s puis fade 3s");
    }

    public bool EstEnEquilibre()
    {
        return chronoTotal < tempsTotalMax;
    }
}