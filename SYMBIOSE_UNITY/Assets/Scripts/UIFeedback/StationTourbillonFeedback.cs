using UnityEngine;
using UnityEngine.UI;

public class StationTourbillonFeedback : MonoBehaviour
{
    [Header("UI")]
    public RectTransform flecheDirection; // flèche qui indique sens rotation souhaité
    public Image grandCercle; // cercle extérieur (zone)
    public RectTransform petitCercle; // cercle intérieur (position joystick)
    public RectTransform cercleProgression; // cercle de progression qui grandit

    [Header("Params")]
    public float rayonGrandCercle = 50f;
    public float rotationRequiseParSeconde = 90f; // degrés de rotation par seconde requis
    public float tempsMinAvantChangement = 7f;
    public float tempsMaxAvantChangement = 11f;

    [Header("Stabilité")]
    public float perteStabiliteHorsEquilibre = 5f;

    private enum SensRotation { Horaire, AntiHoraire }
    private SensRotation sensActuel = SensRotation.Horaire;
    private float angleJoystickPrecedent = 0f;
    private float angleJoystickActuel = 0f;
    private float accumulateurRotation = 0f; // accumule les rotations dans le bon sens
    private float tempsRequis = 2f;
    private Vector3 scaleInitialCercleProgression;
    private bool cercleProgressionActif = false;

    void Start()
    {
        // sens initial aléatoire
        sensActuel = Random.Range(0, 2) == 0 ? SensRotation.Horaire : SensRotation.AntiHoraire;

        // temps requis aléatoire (entier entre 1 et 5)
        tempsRequis = Random.Range((int)tempsMinAvantChangement, (int)tempsMaxAvantChangement + 1);

        UpdateFlecheDirection();

        // save scale initial cercle progression
        if (cercleProgression != null)
        {
            scaleInitialCercleProgression = cercleProgression.localScale;
            cercleProgression.gameObject.SetActive(false);
        }

        Debug.Log($"TOURBILLON : Sens = {sensActuel}, Rotation requise = {rotationRequiseParSeconde * tempsRequis}° sur {tempsRequis}s");
    }

    void Update()
    {
        // calculer changement d'angle
        float deltaAngle = Mathf.DeltaAngle(angleJoystickPrecedent, angleJoystickActuel);

        // accumuler rotation dans le bon sens
        bool rotationCorrecteCeFrame = false;

        if (sensActuel == SensRotation.Horaire && deltaAngle < 0) // horaire = angle diminue
        {
            accumulateurRotation += Mathf.Abs(deltaAngle); // ajouter rotation positive
            rotationCorrecteCeFrame = true;
        }
        else if (sensActuel == SensRotation.AntiHoraire && deltaAngle > 0) // anti-horaire = angle augmente
        {
            accumulateurRotation += deltaAngle;
            rotationCorrecteCeFrame = true;
        }
        else if (Mathf.Abs(deltaAngle) > 1f) // rotation dans mauvais sens
        {
            // pénalité : retirer de l'accumulateur
            accumulateurRotation = Mathf.Max(0, accumulateurRotation - Mathf.Abs(deltaAngle) * 0.5f);
        }

        // calculer combien de rotation est nécessaire
        float rotationTotaleRequise = rotationRequiseParSeconde * tempsRequis;
        float progression = Mathf.Clamp01(accumulateurRotation / rotationTotaleRequise);

        // afficher/update cercle progression
        if (progression > 0.01f && !cercleProgressionActif)
        {
            cercleProgressionActif = true;
            if (cercleProgression != null)
            {
                cercleProgression.gameObject.SetActive(true);
            }
            Debug.Log($"TOURBILLON : ✓ Rotation {sensActuel} commencée...");
        }

        if (cercleProgressionActif && cercleProgression != null)
        {
            // scale: de petit (0.5) à grand (1.5)
            float scaleFacteur = Mathf.Lerp(0.5f, 1.5f, progression);
            cercleProgression.localScale = scaleInitialCercleProgression * scaleFacteur;

            // fade in: de 0 à 0.8
            Image cercleImage = cercleProgression.GetComponent<Image>();
            if (cercleImage != null)
            {
                Color couleur = cercleImage.color;
                couleur.a = Mathf.Lerp(0f, 0.8f, progression);
                cercleImage.color = couleur;
            }
        }

        // DEBUG
        if (Time.frameCount % 30 == 0 && progression > 0.01f)
        {
            Debug.Log($"TOURBILLON : Progression={progression:F2} ({accumulateurRotation:F0}°/{rotationTotaleRequise:F0}°)");
        }

        // changement si objectif atteint
        if (accumulateurRotation >= rotationTotaleRequise)
        {
            ChangerSens();
        }

        // décrémentation lente si pas de rotation
        if (Mathf.Abs(deltaAngle) < 0.5f)
        {
            accumulateurRotation = Mathf.Max(0, accumulateurRotation - Time.deltaTime * 20f);

            // cacher cercle si revenu à 0
            if (accumulateurRotation <= 0 && cercleProgressionActif)
            {
                cercleProgressionActif = false;
                if (cercleProgression != null)
                {
                    cercleProgression.gameObject.SetActive(false);
                }
            }
        }

        // sauvegarder angle précédent
        angleJoystickPrecedent = angleJoystickActuel;

        if (GameManager.Instance != null && !GameManager.Instance.EstEnTutoriel())
        {
            if (!EstEnEquilibre() && StabilityManager.Instance != null)
            {
                StabilityManager.Instance.PerdreStabiliteParSeconde(perteStabiliteHorsEquilibre, "Tourbillon hors équilibre");
            }
        }
    }

    public void UpdateJoystick(int faderX, int faderY)
    {
        // convertir faderX/Y (0-4096) en position (-1 à 1)
        float normalizedX = (faderX - 2048f) / 2048f;
        float normalizedY = (faderY - 2048f) / 2048f;

        // calculer angle
        angleJoystickActuel = Mathf.Atan2(normalizedY, normalizedX) * Mathf.Rad2Deg;
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

        // nouveau temps requis
        tempsRequis = Random.Range((int)tempsMinAvantChangement, (int)tempsMaxAvantChangement + 1);

        UpdateFlecheDirection();
        accumulateurRotation = 0f;
        cercleProgressionActif = false;

        // cacher cercle progression
        if (cercleProgression != null)
        {
            cercleProgression.gameObject.SetActive(false);
        }

        Debug.Log($"TOURBILLON : ✓✓✓ FLIP ! Nouveau sens = {sensActuel}, Rotation requise = {rotationRequiseParSeconde * tempsRequis}° sur {tempsRequis}s");
    }

    void UpdateFlecheDirection()
    {
        if (flecheDirection != null)
        {
            if (sensActuel == SensRotation.Horaire)
            {
                flecheDirection.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                flecheDirection.localScale = new Vector3(-1, 1, 1); // flip X
            }
        }
    }

    public bool EstEnEquilibre()
    {
        return accumulateurRotation > (rotationRequiseParSeconde * 1f);
    }
}