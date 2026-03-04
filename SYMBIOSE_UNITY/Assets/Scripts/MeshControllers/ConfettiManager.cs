using UnityEngine;

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

        // arreter si deja en cours (pour pas accumuler)
        confettiSystem.Stop();
        confettiSystem.Clear();

        // relancer
        confettiSystem.Play();

        Debug.Log("CONFETTI : explosion !");
    }
}