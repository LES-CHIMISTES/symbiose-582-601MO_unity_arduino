using UnityEngine;
using TMPro;
public class OscDisplay : MonoBehaviour
{
    public TextMeshProUGUI accelText;
    public TextMeshProUGUI gyroText;
    public TextMeshProUGUI angleText;
    public TextMeshProUGUI faderText;
    public TextMeshProUGUI keyText;

    public OSCInputManager OSCInputManager;

    public Transform cubeTransform;
    public float rotationMultiplier = 1f; // multiplicateur / ajuster la sensibilité

    [HideInInspector] public float accelX, accelY, accelZ;
    [HideInInspector] public float gyroX, gyroY, gyroZ;
    [HideInInspector] public int angle;
    [HideInInspector] public int faderX, faderY;
    [HideInInspector] public int key1, key2, key3;
    void Update()
    {
        UpdateDisplays();
        UpdateCubeRotation();
    }

    void UpdateCubeRotation()
    {
        if (cubeTransform != null)
        {
            cubeTransform.rotation = Quaternion.Euler(
                accelX * rotationMultiplier,
                accelY * rotationMultiplier,
                accelZ * rotationMultiplier
            );
        }
    }

    void UpdateDisplays()
    {
        if (accelText != null)
        {
            accelText.text = $"ACCELEROMÈTRE\nX: {accelX:F2}\nY: {accelY:F2}\nZ: {accelZ:F2}";
        }
        if (gyroText != null)
        {
            gyroText.text = $"GYROSCOPE\nX: {gyroX:F2}\nY: {gyroY:F2}\nZ: {gyroZ:F2}";
        }
        if (angleText != null)
        {
            angleText.text = $"ANGLE: {angle}";
        }
        if (faderText != null)
        {
            faderText.text = $"FADERS\nX: {faderX}\nY: {faderY}";
        }
        if (keyText != null)
        {
            keyText.text = $"KEYS\nKey 1: {key1}\nKey 2: {key2}\nKey 3: {key3}";
        }
    }
}