using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Instance unique pour accès facile

    [Header("Sons UI")]
    public AudioClip sonVictoire;
    public AudioClip sonEchec;

    [Header("Sons Interactions")]
    public AudioClip sonBrassage; // Faders
    public AudioClip sonEauVersee; // Accéléromètre
    public AudioClip sonKeyPress; // Keys

    [Header("Sons Feu")]
    public AudioClip sonBruleurAllumage; // Quand angle passe de 0 à >0
    public AudioClip sonBruleurConstant; // Loop tant que angle >0
    public AudioClip sonTheiere; // Quand angle >3500
    public AudioClip sonFrosting; // évé gel démarre

    [Header("Audio Sources")]
    private AudioSource sourceUI; // Pour sons UI (victoire/échec)
    private AudioSource sourceInteractions; // Pour sons interactions
    private AudioSource sourceBruleurLoop; // Pour le son constant du brûleur
    private AudioSource sourceBruleurEffets; // Pour allumage/théière
    private AudioSource sourceBruleurAllumage; // allumage

    void Awake()
    {
        // sons singuliers
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Créer les AudioSources
        sourceUI = gameObject.AddComponent<AudioSource>();
        sourceInteractions = gameObject.AddComponent<AudioSource>();
        sourceBruleurLoop = gameObject.AddComponent<AudioSource>();
        sourceBruleurEffets = gameObject.AddComponent<AudioSource>();
        sourceBruleurAllumage = gameObject.AddComponent<AudioSource>();

        // config le loop du brûleur
        sourceBruleurLoop.loop = true;
    }

    // ===== CONTRÔLE DES VOLUMES =====
    public void SetVolumeBruleurConstant(float volume)
    {
        if (sourceBruleurLoop != null)
        {
            sourceBruleurLoop.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetVolumeTheiere(float volume)
    {
        // Si le volume est > 0, jouer le son (s'il n'est pas déjà en train de jouer)
        if (volume > 0.01f && sonTheiere != null)
        {
            if (!sourceBruleurEffets.isPlaying || sourceBruleurEffets.clip != sonTheiere)
            {
                sourceBruleurEffets.clip = sonTheiere;
                sourceBruleurEffets.loop = true; // Loop pour volume progressif
                sourceBruleurEffets.Play();
            }
            sourceBruleurEffets.volume = Mathf.Clamp01(volume);
        }
        else
        {
            // Arrêter si volume à 0
            if (sourceBruleurEffets.isPlaying && sourceBruleurEffets.clip == sonTheiere)
            {
                sourceBruleurEffets.Stop();
            }
        }
    }

    // ===== SONS UI =====
    public void JouerVictoire()
    {
        if (sonVictoire != null)
        {
            sourceUI.PlayOneShot(sonVictoire);
            Debug.Log("AUDIO : Son Victoire");
        }
    }

    public void JouerEchec()
    {
        if (sonEchec != null)
        {
            sourceUI.PlayOneShot(sonEchec);
            Debug.Log("AUDIO : Son Échec");
        }
    }

    // ===== SONS INTERACTIONS =====
    public void JouerBrassage()
    {
        if (sonBrassage != null && !sourceInteractions.isPlaying)
        {
            sourceInteractions.PlayOneShot(sonBrassage);
        }
    }

    public void JouerEauVersee()
    {
        if (sonEauVersee != null)
        {
            sourceInteractions.PlayOneShot(sonEauVersee);
        }
    }

    public void JouerKeyPress()
    {
        if (sonKeyPress != null)
        {
            sourceInteractions.PlayOneShot(sonKeyPress);
        }
    }

    // ===== SONS FEU =====
    public void JouerBruleurAllumage()
    {
        if (sonBruleurAllumage != null)
        {
            sourceBruleurAllumage.PlayOneShot(sonBruleurAllumage);
            Debug.Log("AUDIO : Brûleur s'allume");
        }
    }

    public void DemarrerBruleurConstant()
    {
        if (sonBruleurConstant != null && !sourceBruleurLoop.isPlaying)
        {
            sourceBruleurLoop.clip = sonBruleurConstant;
            sourceBruleurLoop.Play();
            Debug.Log("AUDIO : Brûleur constant démarre");
        }
    }

    public void ArreterBruleurConstant()
    {
        if (sourceBruleurLoop.isPlaying)
        {
            sourceBruleurLoop.Stop();
            Debug.Log("AUDIO : Brûleur constant s'arrête");
        }
    }

    public void JouerTheiere()
    {
        if (sonTheiere != null && !sourceBruleurEffets.isPlaying)
        {
            sourceBruleurEffets.PlayOneShot(sonTheiere);
            Debug.Log("AUDIO : Théière siffle");
        }
    }

    public void JouerFrosting()
    {
        if (sonFrosting != null)
        {
            sourceUI.PlayOneShot(sonFrosting);
            Debug.Log("AUDIO : Frosting (gel)");
        }
    }
}