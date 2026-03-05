using UnityEngine;
using System.Collections;

public class CameraShakeGameOver : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Parametres")]
    public float dureeAvantExplosion = 3.73f; // 112 frames a 30fps
    public float shakeMaxAvantExplosion = 0.03f;
    public float shakeExplosion = 0.15f;
    public float dureeShakeExplosion = 1.5f;

    private Vector3 positionInitiale;
    private Coroutine shakeEnCours;

    public void DemarrerShake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        positionInitiale = cameraTransform.localPosition;

        if (shakeEnCours != null)
        {
            StopCoroutine(shakeEnCours);
        }

        shakeEnCours = StartCoroutine(SequenceShake());
    }

    IEnumerator SequenceShake()
    {
        float elapsed = 0f;

        // phase 1 : shake progressif de 0 a shakeMaxAvantExplosion
        while (elapsed < dureeAvantExplosion)
        {
            elapsed += Time.deltaTime;
            float progression = elapsed / dureeAvantExplosion;

            // intensite qui monte progressivement avec une courbe exponentielle
            float intensite = Mathf.Pow(progression, 2f) * shakeMaxAvantExplosion;

            float offsetX = Mathf.PerlinNoise(Time.time * 15f, 0f) * 2f - 1f;
            float offsetY = Mathf.PerlinNoise(0f, Time.time * 15f) * 2f - 1f;

            cameraTransform.localPosition = positionInitiale + new Vector3(
                offsetX * intensite,
                offsetY * intensite,
                0f
            );

            yield return null;
        }

        // declencer explosion a la frame 112
        if (ExplosionNucleaireController.Instance != null)
        {
            ExplosionNucleaireController.Instance.Exploser();
        }

        // phase 2 : gros shake d'explosion qui decroit
        elapsed = 0f;

        while (elapsed < dureeShakeExplosion)
        {
            elapsed += Time.deltaTime;
            float progression = 1f - (elapsed / dureeShakeExplosion);

            float intensite = progression * shakeExplosion;

            float offsetX = Mathf.PerlinNoise(Time.time * 25f, 0f) * 2f - 1f;
            float offsetY = Mathf.PerlinNoise(0f, Time.time * 25f) * 2f - 1f;

            cameraTransform.localPosition = positionInitiale + new Vector3(
                offsetX * intensite,
                offsetY * intensite,
                0f
            );

            yield return null;
        }

        // restaurer position
        cameraTransform.localPosition = positionInitiale;
        shakeEnCours = null;
    }
}