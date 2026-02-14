using UnityEngine;

public class BecherController : MonoBehaviour
{
    public float rotationMinZ = -30f;
    public float rotationMaxZ = 30f;
    public float rotationMinY = -30f;
    public float rotationMaxY = 30f;

    private float currentRotationZ = 0f;
    private float currentRotationY = 0f;

    // rotation de base depuis blender (sans freeze transform)
    public Vector3 baseRotationEuler = new Vector3(-89.98f, 0, 0);
    private Quaternion baseRotation;

    void Start()
    {
        // conversion en quaternion
        baseRotation = Quaternion.Euler(baseRotationEuler);
    }

    public void UpdateRotationZ(float valeurFaderX)
    {
        float normalized = valeurFaderX / 4096f;
        currentRotationZ = Mathf.Lerp(rotationMinZ, rotationMaxZ, normalized);
        ApplyRotation();
    }

    public void UpdateRotationY(float valeurFaderY)
    {
        float normalized = valeurFaderY / 4096f;
        currentRotationY = Mathf.Lerp(rotationMinY, rotationMaxY, normalized);
        ApplyRotation();
    }

    void ApplyRotation()
    {
        // rotation relative
        Quaternion targetRotation = Quaternion.Euler(0, currentRotationY, currentRotationZ);
        transform.rotation = targetRotation * baseRotation;
    }
}