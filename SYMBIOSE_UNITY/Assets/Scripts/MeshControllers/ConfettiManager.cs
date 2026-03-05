using UnityEngine;
using System.Collections;
public class ConfettiManager : MonoBehaviour
{
    public static ConfettiManager Instance { get; private set; }

    [Header("Particle Systems")]
    public ParticleSystem confettiSystem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Exploser()
    {
        if (confettiSystem == null) return;

        confettiSystem.Stop();
        confettiSystem.Clear();
        confettiSystem.Play();

        StartCoroutine(FadeOutConfettis());

        Debug.Log("CONFETTI : explosion !");
    }

    IEnumerator FadeOutConfettis()
    {
        yield return new WaitForSeconds(4f);

        float duree = 2f;
        float elapsed = 0f;

        var main = confettiSystem.main;
        Color couleurDepart = main.startColor.color;

        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duree);

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[confettiSystem.particleCount];
            int count = confettiSystem.GetParticles(particles);

            for (int i = 0; i < count; i++)
            {
                Color32 c = particles[i].startColor;
                c.a = (byte)(alpha * 255);
                particles[i].startColor = c;
            }

            confettiSystem.SetParticles(particles, count);
            yield return null;
        }

        confettiSystem.Stop();
        confettiSystem.Clear();
    }
}