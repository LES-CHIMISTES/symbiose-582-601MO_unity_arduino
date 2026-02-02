using UnityEngine;

public class MeshFeuController : MonoBehaviour
{
    [Header("Paramètres")]
    public float scaleMin = 0f;
    public float scaleMax = 0.12f;

    void Start()
    {
        // Scale initial à 0
        transform.localScale = new Vector3(
            transform.localScale.x,
            scaleMin,
            transform.localScale.z
        );
    }

    // OSCInputManager
    public void UpdateScale(float valeurAngle)
    {
        // Mapper la valeur 0-4096 vers scaleMin-scaleMax
        float normalized = 1f - (valeurAngle / 4096f); // de 1 à 0
        float newScaleY = Mathf.Lerp(scaleMin, scaleMax, normalized);

        // Appliquer le nouveau scale
        transform.localScale = new Vector3(
            transform.localScale.x,
            newScaleY,
            transform.localScale.z
        );
    }
}