using UnityEngine;
using extOSC;

public class OSCInputManager : MonoBehaviour
{
    [Header("OSC")]
    public OSCReceiver oscReceiver;
    public OSCTransmitter oscTransmitter;

    [Header("Contr leurs")]
    public MeshEauController meshEauController;
    public MeshFeuController meshFeuController;
    public BecherController becherController;
    public EventGel eventGel;
    public GameManager GameManager;

    // Variables pour stocker les valeurs OSC
    private float accelX, accelY, accelZ;
    private int currentKey = 0; // 0 = aucune, 1 = key1, 2 = key2, 3 = key3
    private int dernierFaderX = -1; // -1 = pas encore initialis 
    private int dernierFaderY = -1;
    private float seuilChangementFader = 75f; // changement minimum pour jouer le son

    [Header("Feedback Stations")]
    public StationEauFeedback stationEauFeedback;
    public StationFeuFeedback stationFeuFeedback;
    public StationPoudresFeedback stationPoudresFeedback;
    public StationTourbillonFeedback stationTourbillonFeedback;

    public TutorialManager tutorialManager;

    public EventEvaporation eventEvaporation;

    public EventCristallisation eventCristallisation;

    public EventVortex eventVortex;

    private float progressionAffichee = 0f;
    private bool key1Enfonce = false;
    private bool key2Enfonce = false;
    private bool key3Enfonce = false;
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

        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("eau")) return;

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

        if (eventEvaporation != null && eventEvaporation.gameObject.activeSelf)
        {
            eventEvaporation.UpdateAccel(accelX, accelY, accelZ);
        }
    }

    void AccelY(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        accelY = value;

        if (eventEvaporation != null && eventEvaporation.gameObject.activeSelf)
        {
            eventEvaporation.UpdateAccel(accelX, accelY, accelZ);
        }
    }

    void AccelZ(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        accelZ = value;

        if (eventEvaporation != null && eventEvaporation.gameObject.activeSelf)
        {
            eventEvaporation.UpdateAccel(accelX, accelY, accelZ);
        }
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

        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("feu")) return;


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

        EnvoyerOSCAngle(value);

        //Debug.Log("ANGLE = " + value);
    }

    void EnvoyerOSCAngle(int valeur)
    {
        if (oscTransmitter == null) return;

        var message = new OSCMessage("/feu/angle");
        message.AddValue(OSCValue.Int(valeur));
        oscTransmitter.Send(message);
    }

    void FaderX(OSCMessage message)
    {
        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("tourbillon")) return;

        int value = message.Values[0].IntValue;

        int faderY = dernierFaderY != -1 ? dernierFaderY : 512;

        if (eventVortex != null && eventVortex.gameObject.activeSelf)
        {
            eventVortex.UpdateJoystick(value, faderY);
        }
        else
        {
            if (becherController != null)
            {
                becherController.UpdateRotation(value, faderY);
            }

            if (stationTourbillonFeedback != null)
            {
                stationTourbillonFeedback.UpdateJoystick(value, faderY);
            }
        }

        if (dernierFaderX != -1)
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
    }

    void FaderY(OSCMessage message)
    {
        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("tourbillon")) return;

        int value = message.Values[0].IntValue;

        int faderX = dernierFaderX != -1 ? dernierFaderX : 512;

        if (eventVortex != null && eventVortex.gameObject.activeSelf)
        {
            eventVortex.UpdateJoystick(faderX, value);
        }
        else
        {
            if (becherController != null)
            {
                becherController.UpdateRotation(faderX, value);
            }

            if (stationTourbillonFeedback != null)
            {
                stationTourbillonFeedback.UpdateJoystick(faderX, value);
            }
        }

        if (dernierFaderY != -1)
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
    }
    void Key1(OSCMessage message)
    {
        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("poudres")) return;
        int value = message.Values[0].IntValue;
        if (value == 1)
        {
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(1);
            }
            if (eventCristallisation != null && eventCristallisation.gameObject.activeSelf)
            {
                eventCristallisation.AppuyerBouton(1);
            }
            else
            {
                if (stationPoudresFeedback != null)
                {
                    stationPoudresFeedback.AppuyerBouton(1);
                }
            }
            if (!key1Enfonce && AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }
            key1Enfonce = true;
        }
        else if (value == 0)
        {
            key1Enfonce = false;
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.RelacherBouton(1);
            }
        }
    }
    void Key2(OSCMessage message)
    {
        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("poudres")) return;
        int value = message.Values[0].IntValue;
        if (value == 1)
        {
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(2);
            }
            if (eventCristallisation != null && eventCristallisation.gameObject.activeSelf)
            {
                eventCristallisation.AppuyerBouton(2);
            }
            else
            {
                if (stationPoudresFeedback != null)
                {
                    stationPoudresFeedback.AppuyerBouton(2);
                }
            }
            if (!key2Enfonce && AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }
            key2Enfonce = true;
        }
        else if (value == 0)
        {
            key2Enfonce = false;
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.RelacherBouton(2);
            }
        }
    }
    void Key3(OSCMessage message)
    {
        if (GameManager.Instance != null && GameManager.Instance.enGameOver) return;
        if (tutorialManager != null && !tutorialManager.EstStationActive("poudres")) return;
        int value = message.Values[0].IntValue;
        if (value == 1)
        {
            if (meshEauController != null)
            {
                meshEauController.SetCouleur(3);
            }
            if (eventCristallisation != null && eventCristallisation.gameObject.activeSelf)
            {
                eventCristallisation.AppuyerBouton(3);
            }
            else
            {
                if (stationPoudresFeedback != null)
                {
                    stationPoudresFeedback.AppuyerBouton(3);
                }
            }
            if (!key3Enfonce && AudioManager.Instance != null)
            {
                AudioManager.Instance.JouerKeyPress();
            }
            key3Enfonce = true;
        }
        else if (value == 0)
        {
            key3Enfonce = false;
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.RelacherBouton(3);
            }
        }
    }
}