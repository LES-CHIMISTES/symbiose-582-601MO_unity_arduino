using extOSC;
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

    [Header("Difficulte progressive")]
    public float tempsMinChangementInitial = 7f;
    public float tempsMinChangementFinal = 3f;
    public float tempsMaxChangementInitial = 11f;
    public float tempsMaxChangementFinal = 5f;

    [Header("OSC")]
    public OSCTransmitter oscTransmitter;

    private enum SensRotation { Horaire, AntiHoraire }
    private SensRotation sensActuel = SensRotation.Horaire;
    private float angleJoystickPrecedent = 0f;
    private float angleJoystickActuel = 0f;
    private float accumulateurRotation = 0f; // accumule les rotations dans le bon sens
    private float tempsRequis = 2f;
    private Vector3 scaleInitialCercleProgression;
    private bool cercleProgressionActif = false;
    [HideInInspector]
    public float dernierTempsReussite = 0f;

    void Start()
    {
        dernierTempsReussite = Time.time;
        // sens initial aléatoire
        sensActuel = Random.Range(0, 2) == 0 ? SensRotation.Horaire : SensRotation.AntiHoraire;

        ActualiserTempsRequis();

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
            accumulateurRotation = Mathf.Max(0, accumulateurRotation - Time.deltaTime * 140f);

            if (cercleProgressionActif && cercleProgression != null)
            {
                float rotationTotaleRequisePourScale = rotationRequiseParSeconde * tempsRequis;
                float prog = Mathf.Clamp01(accumulateurRotation / rotationTotaleRequisePourScale);

                float scaleFacteur = Mathf.Lerp(0.5f, 1.5f, prog);
                cercleProgression.localScale = scaleInitialCercleProgression * scaleFacteur;

                Image cercleImage = cercleProgression.GetComponent<Image>();
                if (cercleImage != null)
                {
                    Color couleur = cercleImage.color;
                    couleur.a = Mathf.Lerp(0f, 0.8f, prog);
                    cercleImage.color = couleur;
                }

                if (accumulateurRotation <= 0)
                {
                    cercleProgressionActif = false;
                    cercleProgression.gameObject.SetActive(false);
                }
            }
        }

        // sauvegarder angle précédent
        angleJoystickPrecedent = angleJoystickActuel;

        EnvoyerOSCAngle();
    }

    public void UpdateJoystick(int faderX, int faderY)
    {
        float rawX = (faderX - 512f) / 512f;
        float rawY = (faderY - 512f) / 512f;

        // inverser les deux axes
        float normalizedX = -rawX;
        float normalizedY = -rawY;

        angleJoystickActuel = Mathf.Atan2(normalizedY, normalizedX) * Mathf.Rad2Deg;
        angleJoystickActuel = (angleJoystickActuel + 90f) % 360f;
        if (angleJoystickActuel < 0) angleJoystickActuel += 360f;

        if (petitCercle != null)
        {
            float posX = normalizedX * rayonGrandCercle;
            float posY = normalizedY * rayonGrandCercle;
            petitCercle.anchoredPosition = new Vector2(posX, posY);
        }
    }

    void ChangerSens()
    {
        dernierTempsReussite = Time.time;
        // inverser le sens
        sensActuel = (sensActuel == SensRotation.Horaire) ? SensRotation.AntiHoraire : SensRotation.Horaire;

        // nouveau temps requis
        ActualiserTempsRequis();

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

    void ActualiserTempsRequis()
    {
        if (GameManager.Instance != null && !GameManager.Instance.EstEnTutoriel())
        {
            float d = GameManager.Instance.GetProgressionDifficulte();
            float minT = Mathf.Lerp(tempsMinChangementInitial, tempsMinChangementFinal, d);
            float maxT = Mathf.Lerp(tempsMaxChangementInitial, tempsMaxChangementFinal, d);
            tempsRequis = Random.Range((int)minT, (int)maxT + 1);
        }
        else
        {
            tempsRequis = Random.Range((int)tempsMinAvantChangement, (int)tempsMaxAvantChangement + 1);
        }
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

    void EnvoyerOSCAngle()
    {
        if (oscTransmitter == null) return;

        var message = new OSCMessage("/tourbillon/angle");
        message.AddValue(OSCValue.Float(angleJoystickActuel));
        oscTransmitter.Send(message);
    }
}