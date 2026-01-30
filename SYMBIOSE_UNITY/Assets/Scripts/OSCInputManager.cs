using UnityEngine;
using extOSC;

public class OSCInputManager : MonoBehaviour
{
    public OSCReceiver oscReceiver;
    public OscDisplay oscDisplay; // ;THROWAWAY

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
        if (oscDisplay != null) oscDisplay.accelX = value; // ;THROWAWAY
        Debug.Log("ACCEL X = " + value);
    }

    void AccelY(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.accelY = value; // ;THROWAWAY
        Debug.Log("ACCEL Y = " + value);
    }

    void AccelZ(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.accelZ = value; // ;THROWAWAY
        Debug.Log("ACCEL Z = " + value);
    }
    void GyroX(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.gyroX = value; // ;THROWAWAY
        Debug.Log("GYRO X = " + value);
    }

    void GyroY(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.gyroY = value; // ;THROWAWAY
        Debug.Log("GYRO Y = " + value);
    }

    void GyroZ(OSCMessage message)
    {
        float value = message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.gyroZ = value; // ;THROWAWAY
        Debug.Log("GYRO Z = " + value);
    }
    void Angle(OSCMessage message)
    {
        int value = (int)message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.angle = value; // ;THROWAWAY
        Debug.Log("ANGLE = " + value);
    }
    void FaderX(OSCMessage message)
    {
        int value = (int)message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.faderX = value; // ;THROWAWAY
        Debug.Log("FADER X = " + value);
    }
    void FaderY(OSCMessage message)
    {
        int value = (int)message.Values[0].FloatValue;
        if (oscDisplay != null) oscDisplay.faderY = value; // ;THROWAWAY
        Debug.Log("FADER Y = " + value);
    }
    void Key1(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        if (oscDisplay != null) oscDisplay.key1 = value; // ;THROWAWAY
        Debug.Log("KEY 1 = " + value);
    }

    void Key2(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        if (oscDisplay != null) oscDisplay.key2 = value; // ;THROWAWAY
        Debug.Log("KEY 2 = " + value);
    }

    void Key3(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        if (oscDisplay != null) oscDisplay.key3 = value; // ;THROWAWAY
        Debug.Log("KEY 3 = " + value);
    }
}