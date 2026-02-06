using UnityEngine;
using extOSC;

public class OSCInputManager : MonoBehaviour
{
    [Header("OSC")]
    public OSCReceiver oscReceiver;

    [Header("Contrôleurs")]
    public MeshEauController meshEauController;
    public MeshFeuController meshFeuController;
    public BecherController becherController;
    public EventGel eventGel;
    public GameStateManager gameStateManager;

    // Variables pour stocker les valeurs OSC
    private float accelX, accelY, accelZ;
    private int currentKey = 0; // 0 = aucune, 1 = key1, 2 = key2, 3 = key3
    private int dernierFaderX = -1; // -1 = pas encore initialisé
    private int dernierFaderY = -1;
    private float seuilChangementFader = 75f; // changement minimum pour jouer le son

    [Header("Feedback Stations")]
    public StationEauFeedback stationEauFeedback;
    public StationFeuFeedback stationFeuFeedback;
    public StationPoudresFeedback stationPoudresFeedback;
    public StationTourbillonFeedback stationTourbillonFeedback;

    void Start()
    {
        oscReceiver.Bind("/accelX", AccelX);
        oscReceiver.Bind("/accelY", AccelY);
        oscReceiver.Bind("/accelZ", AccelZ);
        oscReceiver.Bind("/gyroX", GyroX);
        oscReceiver.Bind("/gyroY", GyroY);
        oscReceiver.Bind("/gyroZ", GyroZ);
        oscReceiver.Bind("/angle", Angle);
        oscReceiver.Bind("/faderX", FaderX);
        oscReceiver.Bind("/faderY", FaderY);
        oscReceiver.Bind("/key1", Key1);
        oscReceiver.Bind("/key2", Key2);
        oscReceiver.Bind("/key3", Key3);
    }

    void AccelX(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        accelX = value;

        if (meshEauController != null)
        {
            meshEauController.UpdateAccel(value);
        }

        if (stationEauFeedback != null)
        {
            float niveau = meshEauController.GetNiveauEau();
            stationEauFeedback.UpdateNiveauEau(niveau);
        }

        //Debug.Log("ACCEL X = " + value);
    }

    void AccelY(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        accelY = value;
        //Debug.Log("ACCEL Y = " + value);
    }

    void AccelZ(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        accelZ = value;
        //Debug.Log("ACCEL Z = " + value);
    }

    void GyroX(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        //Debug.Log("GYRO X = " + value);
    }

    void GyroY(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        //Debug.Log("GYRO Y = " + value);
    }

    void GyroZ(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        //Debug.Log("GYRO Z = " + value);
    }

    void Angle(OSCMessage message)
    {
        int value = (int)message.Values[0].FloatValue;

        // scale feu
        if (meshFeuController != null)
        {
            meshFeuController.UpdateScale(value);
        }

        if (stationFeuFeedback != null)
        {
            stationFeuFeedback.UpdateAngleKnob(value);
        }

        // event gel
        if (eventGel != null && eventGel.gameObject.activeSelf)
        {
            eventGel.UpdatePotentiometre(value);
        }

        //Debug.Log("ANGLE = " + value);
    }

    void FaderX(OSCMessage message)
    {
        int value = (int)message.Values[0].FloatValue;

        // update à jour la rotation Z du bécher
        if (becherController != null)
        {
            becherController.UpdateRotationZ(value);
        }

        // detect interaction pour GameStateManager
        if (gameStateManager != null)
        {
            gameStateManager.DetecterInteractionFaderX(value);
        }

        if (stationTourbillonFeedback != null)
        {
            int faderY = dernierFaderY != -1 ? dernierFaderY : 2048;
            stationTourbillonFeedback.UpdateJoystick(value, faderY);
        }

        // joue son seulement si changement significatif
        if (dernierFaderX != -1) // if pas la première lecture
        {
            int changement = Mathf.Abs(value - dernierFaderX);
            if (changement >= seuilChangementFader)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.JouerBrassage();
                }
            }
        }
        dernierFaderX = value;

        //Debug.Log("FADER X = " + value);
    }

    void FaderY(OSCMessage message)
    {
        int value = (int)message.Values[0].FloatValue;

        // update à jour la rotation Y du bécher
        if (becherController != null)
        {
            becherController.UpdateRotationY(value);
        }

        // detect interaction pour GameStateManager
        if (gameStateManager != null)
        {
            gameStateManager.DetecterInteractionFaderY(value);
        }

        if (stationTourbillonFeedback != null)
        {
            int faderX = dernierFaderX != -1 ? dernierFaderX : 2048;
            stationTourbillonFeedback.UpdateJoystick(faderX, value);
        }

        // joue son seulement si changement significatif
        if (dernierFaderY != -1) // if pas la première lecture
        {
            int changement = Mathf.Abs(value - dernierFaderY);
            if (changement >= seuilChangementFader)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.JouerBrassage();
                }
            }
        }
        dernierFaderY = value;

        //Debug.Log("FADER Y = " + value);
    }

    void Key1(OSCMessage message)
    {
        int value = message.Values[0].IntValue;

        if (value == 1)
        {
            currentKey = 1;
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(1); // (vert)
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }

            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.AppuyerBouton(1);
            }
        }
        else if (currentKey == 1)
        {
            currentKey = 0;
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(0); // couelur par défaut
            }
        }

        //Debug.Log("KEY 1 = " + value);
    }

    void Key2(OSCMessage message)
    {
        int value = message.Values[0].IntValue;

        if (value == 1)
        {
            currentKey = 2;
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(2); // (bleu)
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }

            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.AppuyerBouton(1);
            }
        }
        else if (currentKey == 2)
        {
            currentKey = 0;
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(0); // couelur par défaut
            }
        }

        //Debug.Log("KEY 2 = " + value);
    }

    void Key3(OSCMessage message)
    {
        int value = message.Values[0].IntValue;

        if (value == 1)
        {
            currentKey = 3;
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(3); // (mauve)
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }

            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.AppuyerBouton(1);
            }
        }
        else if (currentKey == 3)
        {
            currentKey = 0;
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(0); // couelur par défaut
            }
        }

        //Debug.Log("KEY 3 = " + value);
    }
}