using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System.Collections;

public class StationPoudresFeedback : MonoBehaviour
{
    public OSCTransmitter oscTransmitter;

    [Header("UI")]
    public Image cercleCouleur;
    public Image overlayRouge;
    [Header("Flash inactivite")]
    public Image overlayInactivite;
    public float delaiFlash = 4f;

    [Header("Couleurs")]
    public Color couleurVerte = new Color(0f, 1f, 0f);
    public Color couleurBleue = new Color(0f, 0f, 1f);
    public Color couleurBlanche = Color.white;

    [Header("Animation pincee")]
    public PinceeController pinceeController;

    [Header("Difficulte progressive")]
    public float delaiAvantFadeInitial = 1f;
    public float delaiAvantFadeFinal = 0.3f;
    public float dureeFadeInitiale = 3f;
    public float dureeFadeFinale = 1.2f;

    [Header("Params")]
    public float delaiAvantFade = 1f;
    public float dureeFade = 3f;

    [Header("Animation")]
    public float dureeScaleOut = 0.3f;
    public float scaleOutMax = 1.5f;
    public float dureeTeintRouge = 0.5f;

    [Header("Stabilite")]
    public float perteStabiliteHorsEquilibre = 5f;

    private int couleurAttendue = 1;
    private float chronoTotal = 0f;
    private float tempsTotalMax;
    private bool bonBoutonAppuyeRecemment = false;
    private bool enAnimation = false;
    private Vector3 scaleInitialCercle;
    private Coroutine animationEnCours = null;
    [HideInInspector]
    public float dernierTempsReussite = 0f;

    void Start()
    {
        if (overlayInactivite != null)
        {
            overlayInactivite.enabled = false;
        }
        dernierTempsReussite = Time.time;
        tempsTotalMax = delaiAvantFade + dureeFade;

        if (cercleCouleur != null)
        {
            scaleInitialCercle = cercleCouleur.rectTransform.localScale;
        }

        if (overlayRouge != null)
        {
            overlayRouge.gameObject.SetActive(false);
        }

        ChangerCouleur();
    }

    void Update()
    {
        // flash inactivite (toujours verifier, meme pendant animation)
        if (overlayInactivite != null && GameManager.Instance != null && !GameManager.Instance.EstEnTutoriel() && !GameManager.Instance.enGameOver)
        {
            float tempsInactif = Time.time - dernierTempsReussite;
            if (tempsInactif >= delaiFlash)
            {
                overlayInactivite.enabled = true;
                float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                Color c = overlayInactivite.color;
                c.a = pulse * 1f;
                overlayInactivite.color = c;
            }
            else
            {
                overlayInactivite.enabled = false;
            }
        }

        if (enAnimation) return;


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
            
            bonBoutonAppuyeRecemment = false;
            ChangerCouleur();
        }
    }

    public void AppuyerBouton(int keyNumber)
    {
        //Debug.Log($"POUDRES : bouton {keyNumber} appuye, attendu = {couleurAttendue}");

        EnvoyerOSCKey(keyNumber, true);

        // ignorer si animation en cours
        if (enAnimation) return;

        if (keyNumber == couleurAttendue)
        {
            //Debug.Log("POUDRES : bon bouton");
            bonBoutonAppuyeRecemment = true;
            dernierTempsReussite = Time.time;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }
            // lancer animation pincee
            if (pinceeController != null)
            {
                pinceeController.Pincer(keyNumber);
            }

            LancerAnimation(AnimationReussite());
        }
        else
        {
            //Debug.LogWarning($"POUDRES : mauvais bouton, attendu = {couleurAttendue}, recu = {keyNumber}");
            bonBoutonAppuyeRecemment = false;
            LancerAnimation(AnimationEchec());
        }
    }

    public void RelacherBouton(int keyNumber)
    {
        
        EnvoyerOSCKey(keyNumber, false);
    }

    void LancerAnimation(IEnumerator animation)
    {
        // arreter toute animation precedente proprement
        if (animationEnCours != null)
        {
            StopCoroutine(animationEnCours);
        }

        // reset etat visuel
        ResetVisuels();

        enAnimation = true;
        animationEnCours = StartCoroutine(animation);
    }

    void ResetVisuels()
    {
        if (cercleCouleur != null)
        {
            cercleCouleur.rectTransform.localScale = scaleInitialCercle;
        }

        if (overlayRouge != null)
        {
            overlayRouge.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimationReussite()
    {
        if (cercleCouleur == null) yield break;

        float elapsed = 0f;

        while (elapsed < dureeScaleOut)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dureeScaleOut;

            float scale = Mathf.Lerp(1f, scaleOutMax, t);
            cercleCouleur.rectTransform.localScale = scaleInitialCercle * scale;

            Color c = cercleCouleur.color;
            c.a = 1f - t;
            cercleCouleur.color = c;

            yield return null;
        }

        TerminerAnimation();
    }

    IEnumerator AnimationEchec()
    {
        if (cercleCouleur == null) yield break;

        // afficher overlay rouge
        if (overlayRouge != null)
        {
            overlayRouge.gameObject.SetActive(true);
            Color r = overlayRouge.color;
            r.a = 1f;
            overlayRouge.color = r;
        }

        yield return new WaitForSeconds(dureeTeintRouge);

        // fade out les deux
        float elapsed = 0f;
        float dureeFadeEchec = 0.3f;
        float alphaDepart = cercleCouleur.color.a;

        while (elapsed < dureeFadeEchec)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dureeFadeEchec;

            Color c = cercleCouleur.color;
            c.a = Mathf.Lerp(alphaDepart, 0f, t);
            cercleCouleur.color = c;

            if (overlayRouge != null)
            {
                Color r = overlayRouge.color;
                r.a = Mathf.Lerp(1f, 0f, t);
                overlayRouge.color = r;
            }

            yield return null;
        }

        TerminerAnimation();
    }

    void TerminerAnimation()
    {
        ResetVisuels();
        enAnimation = false;
        animationEnCours = null;
        ChangerCouleur();
    }

    void ChangerCouleur()
    {
        enAnimation = false;
        animationEnCours = null;

        // ajuster difficulte
        if (GameManager.Instance != null && !GameManager.Instance.EstEnTutoriel())
        {
            float d = GameManager.Instance.GetProgressionDifficulte();
            delaiAvantFade = Mathf.Lerp(delaiAvantFadeInitial, delaiAvantFadeFinal, d);
            dureeFade = Mathf.Lerp(dureeFadeInitiale, dureeFadeFinale, d);
            tempsTotalMax = delaiAvantFade + dureeFade;
        }

        couleurAttendue = Random.Range(1, 4);

        if (cercleCouleur != null)
        {
            cercleCouleur.rectTransform.localScale = scaleInitialCercle;

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
        if (overlayRouge != null)
        {
            overlayRouge.gameObject.SetActive(false);
        }

        chronoTotal = 0f;

        //Debug.Log($"POUDRES : nouvelle couleur = {couleurAttendue}");
    }

    public bool EstEnEquilibre()
    {
        bool equilibre = bonBoutonAppuyeRecemment && chronoTotal < tempsTotalMax;
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
    }
}