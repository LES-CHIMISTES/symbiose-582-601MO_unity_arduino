using UnityEngine;
using UnityEngine.UI;

public class StationTourbillonFeedback : MonoBehaviour
{
    [Header("ui")]
    public RectTransform flecheDirection; // flèche qui indique direction

    [Header("params")]
    public float tolerance = 30f; // marge d'erreur (degrés)
    public float tempsAvantChangement = 2f; // temps avant nouvelle direction

    private float directionCibleActuelle = 0f; // angle cible (0 = haut, 90 = droite, etc.)
    private float directionJoystickActuelle = 0f; // angle joystick actuel
    private float chronoChangement = 0f;
    private bool enEquilibre = false;

    void Start()
    {
        // direction initiale
        directionCibleActuelle = Random.Range(0f, 360f);
        UpdateFlecheRotation();
    }

    void Update()
    {
        // check si joystick aligné avec direction
        float difference = Mathf.Abs(Mathf.DeltaAngle(directionJoystickActuelle, directionCibleActuelle));

        if (difference <= tolerance)
        {
            // en équilibre
            if (!enEquilibre)
            {
                enEquilibre = true;
                chronoChangement = 0f;
            }

            chronoChangement += Time.deltaTime;

            // changer direction après temps écoulé
            if (chronoChangement >= tempsAvantChangement)
            {
                ChangerDirection();
            }
        }
        else
        {
            enEquilibre = false;
            chronoChangement = 0f;
        }
    }

    // appelé par OSCInputManager avec valeurs faders
    public void UpdateJoystick(int faderX, int faderY)
    {
        // convertir faderX/Y (0-4096) en angle (0-360)
        // centrer autour de 2048
        float normalizedX = (faderX - 2048f) / 2048f; // -1 à 1
        float normalizedY = (faderY - 2048f) / 2048f; // -1 à 1

        // calculer angle
        directionJoystickActuelle = Mathf.Atan2(normalizedY, normalizedX) * Mathf.Rad2Deg;

        // convertir pour que 0° = haut (ajuster selon ta config)
        directionJoystickActuelle = (directionJoystickActuelle + 90f) % 360f;
        if (directionJoystickActuelle < 0) directionJoystickActuelle += 360f;
    }

    void ChangerDirection()
    {
        // nouvelle direction aléatoire
        directionCibleActuelle = Random.Range(0f, 360f);
        UpdateFlecheRotation();
        chronoChangement = 0f;
        Debug.Log("TOURBILLON : Nouvelle direction = " + directionCibleActuelle + "°");
    }

    void UpdateFlecheRotation()
    {
        if (flecheDirection != null)
        {
            flecheDirection.localRotation = Quaternion.Euler(0, 0, -directionCibleActuelle); // négatif car rotation inverse
        }
    }

    public bool EstEnEquilibre()
    {
        float difference = Mathf.Abs(Mathf.DeltaAngle(directionJoystickActuelle, directionCibleActuelle));
        return difference <= tolerance;
    }
}