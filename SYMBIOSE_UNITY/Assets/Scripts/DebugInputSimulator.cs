using UnityEngine;

// script debug pour simuler les capteurs avec clavier/souris
// appelle directement les feedbacks (pas besoin d'osc)
public class DebugInputSimulator : MonoBehaviour
{
    [Header("refs feedback stations")]
    public StationEauFeedback stationEauFeedback;
    public StationFeuFeedback stationFeuFeedback;
    public StationPoudresFeedback stationPoudresFeedback;
    public StationTourbillonFeedback stationTourbillonFeedback;

    [Header("refs controllers")]
    public MeshEauController meshEauController;
    public MeshFeuController meshFeuController;
    public BecherController becherController;

    [Header("params simulation")]
    public float sensibiliteSouris = 2f;

    // valeurs simulées
    private int angleSimule = 2048; // 0-4096 (centre par défaut)
    private int faderXSimule = 2048;
    private int faderYSimule = 2048;
    private float accelXSimule = 0f;

    void Update()
    {
        // U (hold) = verse eau (simule agitation)
        if (Input.GetKey(KeyCode.U))
        {
            accelXSimule = 0.5f; // valeur qui dépasse seuil
            SimulerAccelX(accelXSimule);
        }
        else
        {
            accelXSimule = 0f;
            SimulerAccelX(accelXSimule);
        }

        // souris (sans shift) = angle
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            float mouseDeltaX = Input.GetAxis("Mouse X");
            if (Mathf.Abs(mouseDeltaX) > 0.01f)
            {
                angleSimule += (int)(mouseDeltaX * 100f * sensibiliteSouris);
                angleSimule = Mathf.Clamp(angleSimule, 0, 4096);
                SimulerAngle(angleSimule);
            }
        }

        // shift + souris = joystick (faders)
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            float mouseDeltaX = Input.GetAxis("Mouse X");
            float mouseDeltaY = Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseDeltaX) > 0.01f)
            {
                faderXSimule += (int)(mouseDeltaX * 100f * sensibiliteSouris);
                faderXSimule = Mathf.Clamp(faderXSimule, 0, 4096);
                SimulerFaderX(faderXSimule);
            }

            if (Mathf.Abs(mouseDeltaY) > 0.01f)
            {
                faderYSimule += (int)(mouseDeltaY * 100f * sensibiliteSouris);
                faderYSimule = Mathf.Clamp(faderYSimule, 0, 4096);
                SimulerFaderY(faderYSimule);
            }
        }

        // I, O, P = keys (poudres)
        if (Input.GetKeyDown(KeyCode.I))
        {
            SimulerKey(1, true);
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            SimulerKey(1, false);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            SimulerKey(2, true);
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            SimulerKey(2, false);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SimulerKey(3, true);
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            SimulerKey(3, false);
        }
    }

    // appeler directement les controllers et feedbacks
    void SimulerAccelX(float valeur)
    {
        // update controller mesh eau
        if (meshEauController != null)
        {
            meshEauController.UpdateAccel(valeur);

            // update feedback station eau
            if (stationEauFeedback != null)
            {
                float niveau = meshEauController.GetNiveauEau();
                stationEauFeedback.UpdateNiveauEau(niveau);
            }
        }
    }

    void SimulerAngle(int valeur)
    {
        // update controller mesh feu
        if (meshFeuController != null)
        {
            meshFeuController.UpdateScale(valeur);
        }

        // update feedback station feu
        if (stationFeuFeedback != null)
        {
            stationFeuFeedback.UpdateAngleKnob(valeur);
        }
    }

    void SimulerFaderX(int valeur)
    {
        // update controller bécher
        if (becherController != null)
        {
            becherController.UpdateRotationZ(valeur);
        }

        // update feedback station tourbillon
        if (stationTourbillonFeedback != null)
        {
            stationTourbillonFeedback.UpdateJoystick(valeur, faderYSimule);
        }
    }

    void SimulerFaderY(int valeur)
    {
        // update controller bécher
        if (becherController != null)
        {
            becherController.UpdateRotationY(valeur);
        }

        // update feedback station tourbillon
        if (stationTourbillonFeedback != null)
        {
            stationTourbillonFeedback.UpdateJoystick(faderXSimule, valeur);
        }
    }

    void SimulerKey(int keyNumber, bool appuyee)
    {
        if (appuyee)
        {
            // key appuyée
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(keyNumber);
            }
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.AppuyerBouton(keyNumber);
            }
        }
        else
        {
            // key relâchée
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(0); // couleur par défaut
            }
        }
    }

    // afficher valeurs debug
    void OnGUI()
    {
        GUI.color = Color.yellow;
        GUI.Box(new Rect(10, 10, 320, 160), "");
        GUI.color = Color.white;

        GUI.Label(new Rect(20, 20, 300, 20), "=== DEBUG INPUT SIMULATOR ===");
        GUI.Label(new Rect(20, 45, 300, 20), "U (hold) = Verser eau");
        GUI.Label(new Rect(20, 65, 300, 20), "Souris = Angle (" + angleSimule + ")");
        GUI.Label(new Rect(20, 85, 300, 20), "Shift+Souris = Joystick (" + faderXSimule + ", " + faderYSimule + ")");
        GUI.Label(new Rect(20, 105, 300, 20), "I, O, P = Poudres (Vert, Bleu, Blanc)");
        GUI.Label(new Rect(20, 130, 300, 20), "Eau: " + (accelXSimule > 0 ? "VERSE" : "stop"));
    }
}