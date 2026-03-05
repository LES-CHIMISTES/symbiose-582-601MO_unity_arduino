using UnityEngine;
using System.Collections;

public class ExplosionNucleaireController : MonoBehaviour
{
    public static ExplosionNucleaireController Instance { get; private set; }

    [Header("Particle Systems")]
    public ParticleSystem bouleDeFeu;
    public ParticleSystem colonneFumee;
    public ParticleSystem chapeauChampignon;

    [Header("Flash")]
    public Light flashLight;
    public float intensiteMax = 5f;

    void Awake()
    {
        Instance = this;
    }

    public void Exploser()
    {
        gameObject.SetActive(true);

        if (bouleDeFeu != null) bouleDeFeu.Play();
        if (colonneFumee != null) colonneFumee.Play();
        if (chapeauChampignon != null) chapeauChampignon.Play();

        if (flashLight != null)
        {
            StartCoroutine(AnimerFlash());
        }

        Debug.Log("EXPLOSION NUCLEAIRE : boom !");
    }

    IEnumerator AnimerFlash()
    {
        // flash instantane
        flashLight.intensity = intensiteMax;

        // fade out rapide
        float elapsed = 0f;
        float duree = 1.5f;

        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(intensiteMax, 0f, elapsed / duree);
            yield return null;
        }

        flashLight.intensity = 0f;
    }
}