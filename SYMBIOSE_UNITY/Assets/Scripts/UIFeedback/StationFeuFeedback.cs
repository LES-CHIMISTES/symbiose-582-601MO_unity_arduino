using UnityEngine;
using UnityEngine.UI;

public class StationFeuFeedback : MonoBehaviour
{
    [Header("ui")]
    public RectTransform knobDynamique; // knob qui suit angle
    public RectTransform knobCible; // knob transparent avec indicateur rouge

    [Header("params")]
    public float angleCibleMin = -180f;
    public float angleCibleMax = 180f;
    public float tolerance = 15f; // marge d'erreur (degrés)
    public float tempsAvantChangement = 2f; // temps avant nouvelle cible

    private float angleCibleActuel = 0f; // angle cible actuel
    private float angleKnobActuel = 0f; // angle knob actuel (de l'osc)
    private float chronoChangement = 0f;
    private bool enEquilibre = false;

    void Start()
    {
        // angle cible initial
        angleCibleActuel = Random.Range(angleCibleMin, angleCibleMax);
        UpdateKnobCibleRotation();
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
            }

            // compter temps en équilibre
            chronoChangement += Time.deltaTime;

            // changer cible après temps écoulé
            if (chronoChangement >= tempsAvantChangement)
            {
                DeplacerCible();
            }
        }
        else
        {
            enEquilibre = false;
            chronoChangement = 0f;
        }
    }

    // appelé par MeshFeuController ou OSCInputManager
    public void UpdateAngleKnob(float valeurAngle)
    {
        // convertir valeur osc (0-4096) en angle (-180 à 180)
        // RAPPEL : ton angle est inversé (0 = max, 4096 = éteint)
        float normalized = 1f - (valeurAngle / 4096f); // inverse
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