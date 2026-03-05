using UnityEngine;

public class ParticulesPotionController : MonoBehaviour
{
    public static ParticulesPotionController Instance { get; private set; }

    [Header("References")]
    public ParticleSystemRenderer particulesRenderer;

    [Header("Materials par etat")]
    public Material matNormal;
    public Material matGel;
    public Material matEvaporation;
    public Material matCristallisation;
    public Material matVortex;
    public Material matGameOver;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetEtat(string etat)
    {
        if (particulesRenderer == null) return;

        switch (etat)
        {
            case "gel": particulesRenderer.material = matGel; break;
            case "evaporation": particulesRenderer.material = matEvaporation; break;
            case "cristallisation": particulesRenderer.material = matCristallisation; break;
            case "vortex": particulesRenderer.material = matVortex; break;
            case "gameover": particulesRenderer.material = matGameOver; break;
            default: particulesRenderer.material = matNormal; break;
        }

        Debug.Log($"PARTICULES POTION : etat -> {etat}");
    }

    public void ResetNormal()
    {
        SetEtat("normal");
    }
}