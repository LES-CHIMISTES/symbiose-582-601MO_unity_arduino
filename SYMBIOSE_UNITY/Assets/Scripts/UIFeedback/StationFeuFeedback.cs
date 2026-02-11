using UnityEngine;
using UnityEngine.UI;

public class StationFeuFeedback : MonoBehaviour
{
    [Header("ui")]
    public RectTransform knobDynamique; // knob qui suit angle
    public RectTransform knobCible; // knob transparent avec indicateur rouge
    public RectTransform barreTemps; // barre horizontale qui scale (RectTransform, pas Image)

    [Header("params")]
    public float angleCibleMin = -180f;
    public float angleCibleMax = 180f;
    public float tolerance = 20f; // marge d'erreur (degrés)
    public float tempsAvantChangement = 3f; // temps avant nouvelle cible

    private float angleCibleActuel = 0f; // angle cible actuel
    private float angleKnobActuel = 0f; // angle knob actuel (de l'osc)
    private float chronoChangement = 0f;
    private bool enEquilibre = false;
    private Vector3 scaleInitialBarre; // scale initial de la barre

    void Start()
    {
        // angle cible initial
        angleCibleActuel = Random.Range(angleCibleMin, angleCibleMax);
        UpdateKnobCibleRotation();
        
        // save scale initial barre
        if (barreTemps != null)
        {
            scaleInitialBarre = barreTemps.localScale;
            // cacher barre au départ
            barreTemps.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // check si knob est aligné avec cible
        float difference = Mathf.Abs(Mathf.DeltaAngle(angleKnobActuel, angleCibleActuel));
        
        if (difference <= tolerance)
        {
            // en équilibre
            if (!enEquilibre)
            {
                enEquilibre = true;
                chronoChangement = 0f;
                
                // afficher barre
                if (barreTemps != null)
                {
                    barreTemps.gameObject.SetActive(true);
                }
            }

            // compter temps en équilibre
            chronoChangement += Time.deltaTime;

            // update barre temps avec scale X
            if (barreTemps != null)
            {
                float progression = chronoChangement / tempsAvantChangement;
                
                // scale x (de 1 à 0)
                Vector3 nouveauScale = scaleInitialBarre;
                nouveauScale.x = scaleInitialBarre.x * (1f - progression);
                barreTemps.localScale = nouveauScale;
                
                // fade out alpha
                Image barreImage = barreTemps.GetComponent<Image>();
                if (barreImage != null)
                {
                    Color couleur = barreImage.color;
                    couleur.a = 1f - (progression * 0.25f);
                    barreImage.color = couleur;
                }
            }

            // changer cible après temps écoulé
            if (chronoChangement >= tempsAvantChangement)
            {
                DeplacerCible();
            }
        }
        else
        {
            // plus en équilibre
            if (enEquilibre)
            {
                enEquilibre = false;
                chronoChangement = 0f;
                
                // cacher barre
                if (barreTemps != null)
                {
                    barreTemps.gameObject.SetActive(false);
                    // reset pour prochaine fois
                    barreTemps.localScale = scaleInitialBarre;
                    Image barreImage = barreTemps.GetComponent<Image>();
                    if (barreImage != null)
                    {
                        Color couleur = barreImage.color;
                        couleur.a = 1f;
                        barreImage.color = couleur;
                    }
                }
            }
        }
    }

    // appelé par DebugInputSimulator ou OSCInputManager
    public void UpdateAngleKnob(float valeurAngle)
    {
        // convertir valeur osc (0-4096) en angle (-180 à 180)
        // FLIPPÉ : 0 = -180, 4096 = 180
        float normalized = valeurAngle / 4096f; // 0 à 1
        angleKnobActuel = Mathf.Lerp(-180f, 180f, normalized);

        // update rotation du knob dynamique
        if (knobDynamique != null)
        {
            knobDynamique.localRotation = Quaternion.Euler(0, 0, angleKnobActuel);
        }
    }

    void DeplacerCible()
    {
        // nouvelle cible aléatoire
        angleCibleActuel = Random.Range(angleCibleMin, angleCibleMax);
        UpdateKnobCibleRotation();
        chronoChangement = 0f;
        enEquilibre = false;
        
        // cacher barre
        if (barreTemps != null)
        {
            barreTemps.gameObject.SetActive(false);
        }
        
        Debug.Log("FEU : Nouvelle cible à " + angleCibleActuel + "°");
    }

    void UpdateKnobCibleRotation()
    {
        if (knobCible != null)
        {
            knobCible.localRotation = Quaternion.Euler(0, 0, angleCibleActuel);
        }
    }

    public bool EstEnEquilibre()
    {
        float difference = Mathf.Abs(Mathf.DeltaAngle(angleKnobActuel, angleCibleActuel));
        return difference <= tolerance;
    }
}