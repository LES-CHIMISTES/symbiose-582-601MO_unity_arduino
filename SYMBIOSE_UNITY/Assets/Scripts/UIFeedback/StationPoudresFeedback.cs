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
            bonBoutonAppuyeRecemment = false;
        }
    }

    // appelé par OSCInputManager ou DebugInputSimulator quand key appuyée
    public void AppuyerBouton(int keyNumber)
    {
        Debug.Log($"POUDRES : Bouton {keyNumber} appuyé, couleur attendue = {couleurAttendue}");

        EnvoyerOSCKey(keyNumber, true);

        if (keyNumber == couleurAttendue)
        {
            Debug.Log("POUDRES : ✓ Bon bouton !");
            bonBoutonAppuyeRecemment = true; // MARQUER COMME RÉUSSI
            ChangerCouleur();
        }
        else
        {
            Debug.LogWarning($"POUDRES : ✗ Mauvais bouton ! Attendu: {couleurAttendue}, Reçu: {keyNumber}");
            bonBoutonAppuyeRecemment = false; // RESET
        }
    }

    public void RelacherBouton(int keyNumber)
    {
        Debug.Log($"POUDRES : Bouton {keyNumber} relâché");

        // ENVOYER OSC RELÂCHEMENT (0)
        EnvoyerOSCKey(keyNumber, false);
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
                case 1: nouvelleCouleur = couleurVerte; break;
                case 2: nouvelleCouleur = couleurBleue; break;
                case 3: nouvelleCouleur = couleurBlanche; break;
                default: nouvelleCouleur = Color.white; break;
            }

            nouvelleCouleur.a = 1f;
            cercleCouleur.color = nouvelleCouleur;
        }

        // reset chrono
        chronoTotal = 0f;

        // NE PLUS RESET LE FLAG ICI - le laisser à true pour le tutoriel
        // bonBoutonAppuyeRecemment = false;

        Debug.Log($"POUDRES : Nouvelle couleur = {couleurAttendue} (1=Vert, 2=Bleu, 3=Blanc)");
    }

    public bool EstEnEquilibre()
    {
        // En équilibre si bon bouton appuyé ET on est encore dans la fenêtre de temps
        bool equilibre = bonBoutonAppuyeRecemment && chronoTotal < tempsTotalMax;

        if (equilibre && Time.frameCount % 30 == 0)
        {
            Debug.Log($"POUDRES EstEnEquilibre : true (flag={bonBoutonAppuyeRecemment}, chrono={chronoTotal:F2}/{tempsTotalMax})");
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