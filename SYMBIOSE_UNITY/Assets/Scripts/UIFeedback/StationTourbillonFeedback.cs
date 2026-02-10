using UnityEngine;
using UnityEngine.UI;

public class StationTourbillonFeedback : MonoBehaviour
{
    [Header("ui")]
    public RectTransform flecheDirection; // flèche qui indique sens rotation souhaité
    public Image grandCercle; // cercle extérieur (zone)
    public RectTransform petitCercle; // cercle intérieur (position joystick)

    [Header("params")]
    public float rayonGrandCercle = 50f; // rayon du grand cercle
    public float tempsAvantChangement = 2f; // temps en rotation correcte avant changement
    public float seuilRotation = 10f; // seuil de rotation en degrés pour détecter mouvement

    private enum SensRotation { Horaire, AntiHoraire }
    private SensRotation sensActuel = SensRotation.Horaire;
    private float angleJoystickPrecedent = 0f;
    private float angleJoystickActuel = 0f;
    private float chronoChangement = 0f;
    private float accumulateurRotation = 0f; // accumule les petites rotations

    void Start()
    {
        // sens initial aléatoire
        sensActuel = Random.Range(0, 2) == 0 ? SensRotation.Horaire : SensRotation.AntiHoraire;
        UpdateFlecheDirection();
        Debug.Log("TOURBILLON : Sens initial = " + sensActuel);
    }

    void Update()
    {
        // calculer changement d'angle
        float deltaAngle = Mathf.DeltaAngle(angleJoystickPrecedent, angleJoystickActuel);
        
        // accumuler rotation
        accumulateurRotation += deltaAngle;
        
        // debug
        if (Mathf.Abs(deltaAngle) > 1f)
        {
            Debug.Log($"TOURBILLON : Delta={deltaAngle:F1}°, Accum={accumulateurRotation:F1}°, Sens attendu={sensActuel}");
        }
        
        // check si rotation dans bon sens
        bool enRotationCorrecte = false;
        
        if (sensActuel == SensRotation.Horaire && accumulateurRotation < -seuilRotation)
        {
            // rotation horaire détectée (angle diminue, donc négatif)
            enRotationCorrecte = true;
            accumulateurRotation = 0f; // reset
            Debug.Log("TOURBILLON : ✓ Rotation HORAIRE détectée !");
        }
        else if (sensActuel == SensRotation.AntiHoraire && accumulateurRotation > seuilRotation)
        {
            // rotation anti-horaire détectée (angle augmente, donc positif)
            enRotationCorrecte = true;
            accumulateurRotation = 0f; // reset
            Debug.Log("TOURBILLON : ✓ Rotation ANTI-HORAIRE détectée !");
        }

        if (enRotationCorrecte)
        {
            chronoChangement += Time.deltaTime;
            
            if (chronoChangement >= tempsAvantChangement)
            {
                ChangerSens();
            }
        }
        else
        {
            // si pas de rotation dans bon sens, décrémenter doucement
            chronoChangement = Mathf.Max(0, chronoChangement - Time.deltaTime * 0.5f);
        }
        
        // reset accumulation si trop longtemps sans mouvement
        if (Mathf.Abs(deltaAngle) < 0.1f)
        {
            accumulateurRotation = 0f;
        }
        
        // sauvegarder angle précédent
        angleJoystickPrecedent = angleJoystickActuel;
    }

    // appelé par DebugInputSimulator ou OSCInputManager avec valeurs faders
    public void UpdateJoystick(int faderX, int faderY)
    {
        // convertir faderX/Y (0-4096) en position (-1 à 1)
        float normalizedX = (faderX - 2048f) / 2048f; // -1 à 1
        float normalizedY = (faderY - 2048f) / 2048f; // -1 à 1

        // calculer angle (0° = droite, 90° = haut, 180° = gauche, 270° = bas)
        angleJoystickActuel = Mathf.Atan2(normalizedY, normalizedX) * Mathf.Rad2Deg;
        
        // convertir pour que 0° = haut
        angleJoystickActuel = (angleJoystickActuel + 90f) % 360f;
        if (angleJoystickActuel < 0) angleJoystickActuel += 360f;

        // update position du petit cercle
        if (petitCercle != null)
        {
            float posX = normalizedX * rayonGrandCercle;
            float posY = normalizedY * rayonGrandCercle;
            petitCercle.anchoredPosition = new Vector2(posX, posY);
        }
    }

    void ChangerSens()
    {
        // inverser le sens
        sensActuel = (sensActuel == SensRotation.Horaire) ? SensRotation.AntiHoraire : SensRotation.Horaire;
        UpdateFlecheDirection();
        chronoChangement = 0f;
        accumulateurRotation = 0f;
        
        Debug.Log("TOURBILLON : ✓✓✓ CHANGEMENT DE SENS ! Nouveau sens = " + sensActuel);
    }

    void UpdateFlecheDirection()
    {
        if (flecheDirection != null)
        {
            // afficher flèche circulaire selon sens
            if (sensActuel == SensRotation.Horaire)
            {
                // flèche pointe dans sens horaire (rotation négative)
                flecheDirection.localRotation = Quaternion.Euler(0, 0, 0);
                flecheDirection.localScale = new Vector3(1, 1, 1); // normal
            }
            else
            {
                // flèche pointe dans sens anti-horaire (flip horizontal)
                flecheDirection.localRotation = Quaternion.Euler(0, 0, 0);
                flecheDirection.localScale = new Vector3(-1, 1, 1); // flip X
            }
        }
    }

    public bool EstEnEquilibre()
    {
        return chronoChangement > 1f;
    }
}