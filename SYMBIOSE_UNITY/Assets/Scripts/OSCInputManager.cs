using UnityEngine;
using extOSC;

public class OSCInputManager : MonoBehaviour
{
    [Header("OSC")]
    public OSCReceiver oscReceiver;

    [Header("Contr�leurs")]
    public MeshEauController meshEauController;
    public MeshFeuController meshFeuController;
    public BecherController becherController;
    public EventGel eventGel;
    public GameManager GameManager;

    // Variables pour stocker les valeurs OSC
    private float accelX, accelY, accelZ;
    private int currentKey = 0; // 0 = aucune, 1 = key1, 2 = key2, 3 = key3
    private int dernierFaderX = -1; // -1 = pas encore initialis�
    private int dernierFaderY = -1;
    private float seuilChangementFader = 75f; // changement minimum pour jouer le son

    [Header("Feedback Stations")]
    public StationEauFeedback stationEauFeedback;
    public StationFeuFeedback stationFeuFeedback;
    public StationPoudresFeedback stationPoudresFeedback;
    public StationTourbillonFeedback stationTourbillonFeedback;

    public TutorialManager tutorialManager;

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
        if (tutorialManager != null && !tutorialManager.EstStationActive("eau"))
        {
            return; // Bloquer l'input
        }

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

        if (tutorialManager != null && !tutorialManager.EstStationActive("feu"))
        {
            return; // Bloquer l'input
        }


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
        if (tutorialManager != null && !tutorialManager.EstStationActive("tourbillon"))
        {
            return; // Bloquer l'input
        }

        int value = (int)message.Values[0].FloatValue;


        // update � jour la rotation Z du b�cher
        if (becherController != null)
        {
            becherController.UpdateRotationZ(value);
        }

        if (stationTourbillonFeedback != null)
        {
            int faderY = dernierFaderY != -1 ? dernierFaderY : 2048;
            stationTourbillonFeedback.UpdateJoystick(value, faderY);
        }

        // joue son seulement si changement significatif
        if (dernierFaderX != -1) // if pas la premi�re lecture
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
        if (tutorialManager != null && !tutorialManager.EstStationActive("tourbillon"))
        {
            return; // Bloquer l'input
        }

        int value = (int)message.Values[0].FloatValue;

        // update � jour la rotation Y du b�cher
        if (becherController != null)
        {
            becherController.UpdateRotationY(value);
        }

        if (stationTourbillonFeedback != null)
        {
            int faderX = dernierFaderX != -1 ? dernierFaderX : 2048;
            stationTourbillonFeedback.UpdateJoystick(faderX, value);
        }

        // joue son seulement si changement significatif
        if (dernierFaderY != -1) // if pas la premi�re lecture
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
        if (tutorialManager != null && !tutorialManager.EstStationActive("poudres"))
        {
            return; // Bloquer l'input
        }
        int value = message.Values[0].IntValue;

    if (value == 1)
    {
        if (meshEauController != null)
        {
            meshEauController.SetCouleur(1);
        }
        
        
        if (stationPoudresFeedback != null)
        {
            stationPoudresFeedback.AppuyerBouton(1); 
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerKeyPress();
        }

    }

        else if (value == 0) // RELÂCHÉ - AJOUTER CETTE SECTION
        {
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.RelacherBouton(1); // NOUVELLE FONCTION
            }
        }
    }

void Key2(OSCMessage message)
{
        if (tutorialManager != null && !tutorialManager.EstStationActive("poudres"))
        {
            return; // Bloquer l'input
        }
        int value = message.Values[0].IntValue;

    if (value == 1)
    {
        if (meshEauController != null)
        {
            meshEauController.SetCouleur(2);
        }
        
        
        if (stationPoudresFeedback != null)
        {
            stationPoudresFeedback.AppuyerBouton(2);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerKeyPress();
        }
    }
        else if (value == 0) // RELÂCHÉ - AJOUTER CETTE SECTION
        {
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.RelacherBouton(2); // NOUVELLE FONCTION
            }
        }
    }

void Key3(OSCMessage message)
{
        if (tutorialManager != null && !tutorialManager.EstStationActive("poudres"))
        {
            return; // Bloquer l'input
        }
        int value = message.Values[0].IntValue;

    if (value == 1)
    {
        if (meshEauController != null)
        {
            meshEauController.SetCouleur(3);
        }
        
        
        if (stationPoudresFeedback != null)
        {
            stationPoudresFeedback.AppuyerBouton(3);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.JouerKeyPress();
        }
    }

        else if (value == 0) // RELÂCHÉ - AJOUTER CETTE SECTION
        {
            if (stationPoudresFeedback != null)
            {
                stationPoudresFeedback.RelacherBouton(3); // NOUVELLE FONCTION
            }
        }
    }
}