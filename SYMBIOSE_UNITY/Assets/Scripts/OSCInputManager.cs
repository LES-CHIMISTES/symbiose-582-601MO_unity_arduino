using UnityEngine;
using extOSC;

public class OscRoll : MonoBehaviour
{
    public OSCReceiver oscReceiver;

    void Start()
    {
        oscReceiver.Bind("/angle", angle);
        oscReceiver.Bind("/faderX", faderX);
        oscReceiver.Bind("/faderY", faderY);
        oscReceiver.Bind("/key1", key1);
        oscReceiver.Bind("/key2", key2);
        oscReceiver.Bind("/key3", key3);
    }

    void angle(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        Debug.Log("ANGLE = " + value);
    }

    void faderX(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        Debug.Log("FADER X = " + value);
    }

    void faderY(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        Debug.Log("FADER Y = " + value);
    }

    void key1(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        Debug.Log("KEY 1 = " + value);
    }

    void key2(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        Debug.Log("KEY 2 = " + value);
    }

    void key3(OSCMessage message)
    {
        int value = message.Values[0].IntValue;
        Debug.Log("KEY 3 = " + value);
    }
}
