using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class StationPoudresFeedback : MonoBehaviour
{
    public OSCTransmitter oscTransmitter;

    [Header("UI")]
    public Image cercleCouleur; // cercle qui change de couleur

    [Header("Couleurs - ORDRE IMPORTANT")]
    public Color couleurVerte = new Color(0f, 1f, 0f);   // Key 1
    public Color couleurBleue = new Color(0f, 0f, 1f);   // Key 2
    public Color couleurBlanche = Color.white;           // Key 3

    [Header("Params")]
    public float delaiAvantFade = 1f; // délai avant que le fade commence
    public float dureeFade = 3f; // durée du fade out

    [Header("Stabilité")]
    public float perteStabiliteHorsEquilibre = 5f;

    private int couleurAttendue = 1; // 1=vert, 2=bleu, 3=blanc
    private float chronoTotal = 0f;
    private float tempsTotalMax; // delai + duree

    private bool bonBoutonAppuyeRecemment = false; // flag pour tutoriel

    void Start()
    {
        tempsTotalMax = delaiAvantFade + dureeFade;
        ChangerCouleur();
    }

    void Update()
    {
        chronoTotal += Time.deltaTime;

        if (cercleCouleur != null && chronoTotal >= delaiAvantFade)
        {
            float tempsDepuisDebutFade = chronoTotal - delaiAvantFade;
            float progression = tempsDepuisDebutFade / dureeFade;

            Color couleurActuelle = cercleCouleur.color;
            couleurActuelle.a = 1f - progression;
            cercleCouleur.color = couleurActuelle;
        }

        if (chronoTotal >= tempsTotalMax)
        {
            Debug.LogWarning("POUDRES : timeout échec");
            ChangerCouleur();
            bonBoutonAppuyeRecemment = false;
        }

        // perte stabilité en phase principale
        if (GameManager.Instance != null && !GameManager.Instance.EstEnTutoriel())
        {
            if (!EstEnEquilibre() && StabilityManager.Instance != null)
            {
                StabilityManager.Instance.PerdreStabiliteParSeconde(perteStabiliteHorsEquilibre, "poudres hors équilibre");
            }
        }
    }

    // appelé par OSCInputManager ou DebugInputSimulator quand key appuyée
    public void AppuyerBouton(int keyNumber)
    {
        Debug.Log($"POUDRES : bouton {keyNumber} appuyé, attendu = {couleurAttendue}");

        EnvoyerOSCKey(keyNumber, true);

        if (keyNumber == couleurAttendue)
        {
            Debug.Log("POUDRES : ✓ bon bouton");
            bonBoutonAppuyeRecemment = true;

            // attendre 2 secondes avant de changer (laisse temps au tutoriel de valider)
            Invoke(nameof(ChangerCouleurApresDelai), 2f);
        }
        else
        {
            Debug.LogWarning($"POUDRES : mauvais bouton, attendu = {couleurAttendue}, reçu = {keyNumber}");
            bonBoutonAppuyeRecemment = false;
        }
    }

    public void RelacherBouton(int keyNumber)
    {
        Debug.Log($"POUDRES : Bouton {keyNumber} relâché");

        // ENVOYER OSC RELÂCHEMENT (0)
        EnvoyerOSCKey(keyNumber, false);
    }

    void ChangerCouleurApresDelai()
    {
        ChangerCouleur();
    }

    void ChangerCouleur()
    {
        couleurAttendue = Random.Range(1, 4);

        if (cercleCouleur != null)
        {
            Color nouvelleCouleur;
            switch (couleurAttendue)
            {
                case 1: nouvelleCouleur = couleurVerte; break;
                case 2: nouvelleCouleur = couleurBleue; break;
                case 3: nouvelleCouleur = couleurBlanche; break;
                default: nouvelleCouleur = Color.white; break;
            }

            nouvelleCouleur.a = 1f;
            cercleCouleur.color = nouvelleCouleur;
        }

        chronoTotal = 0f;

        Debug.Log($"POUDRES : nouvelle couleur = {couleurAttendue}");
    }

    public bool EstEnEquilibre()
    {
        bool equilibre = bonBoutonAppuyeRecemment && chronoTotal < tempsTotalMax;

        if (equilibre && Time.frameCount % 60 == 0)
        {
            Debug.Log($"POUDRES équilibre: flag={bonBoutonAppuyeRecemment}, chrono={chronoTotal:F2}/{tempsTotalMax}");
        }

        return equilibre;
    }

    void EnvoyerOSCKey(int keyNumber, bool appuye)
    {
        if (oscTransmitter == null)
        {
            Debug.LogError("OSC : oscTransmitter est NULL !");
            return;
        }

        string adresse = $"/poudres/key{keyNumber}";
        int valeur = appuye ? 1 : 0;

        var message = new OSCMessage(adresse);
        message.AddValue(OSCValue.Int(valeur));
        oscTransmitter.Send(message);

        Debug.Log($"OSC : Key{keyNumber} = {valeur} ({(appuye ? "APPUYÉ" : "RELÂCHÉ")}) envoyé à {adresse}");
    }
}