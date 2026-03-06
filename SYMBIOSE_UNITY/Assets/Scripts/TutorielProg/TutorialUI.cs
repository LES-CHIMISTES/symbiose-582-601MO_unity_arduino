using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Textes Instructions")]
    public TextMeshProUGUI texteEau;
    public TextMeshProUGUI texteFeu;
    public TextMeshProUGUI textePoudres;
    public TextMeshProUGUI texteTourbillon;

    [Header("Animation")]
    public float offsetY = 30f; // décalage vertical
    public float dureeFade = 0.5f;
    [Header("Animation images")]
    public CanvasGroup imagesCanvasGroup;
    [Header("Images Tutoriel (2 par etape)")]
    public Image[] imagesEtape1;    // 2 images pour eau
    public Image[] imagesEtape2;    // 2 images pour feu
    public Image[] imagesEtape3;    // 2 images pour poudres
    public Image[] imagesEtape4;    // 2 images pour tourbillon
    public float intervalleAlternance = 1f;

    private Coroutine animationImageEnCours = null;
    private Image[][] toutesImages;

    [Header("Couleurs")]
    public Color couleurActive = Color.white;
    public Color couleurComplete = new Color(0f, 1f, 0f, 1f);

    private TextMeshProUGUI[] textes;
    private Vector2[] positionsInitiales;
    private string[] instructionsBase = new string[]
    {
    "1. Remplissez le niveau d'eau par rapport à la cible jaune",
    "2. Allumez le feu à la bonne intensité",
    "3. Ajoutez des poudres selon la couleur demandée",
    "4. Brassez la potion dans le sens indiqué"
    };

    void Start()
    {
        textes = new TextMeshProUGUI[] { texteEau, texteFeu, textePoudres, texteTourbillon };
        positionsInitiales = new Vector2[textes.Length];

        // Setup initial
        for (int i = 0; i < textes.Length; i++)
        {
            if (textes[i] != null)
            {
                textes[i].text = instructionsBase[i];
                textes[i].color = couleurActive;
                
                // Sauvegarder position initiale
                RectTransform rt = textes[i].GetComponent<RectTransform>();
                positionsInitiales[i] = rt.anchoredPosition;
                
                // Désactiver tous sauf le premier
                if (i == 0)
                {
                    // Fade in le premier
                    StartCoroutine(FadeInTexte(i));
                }
                else
                {
                    textes[i].gameObject.SetActive(false);
                }
            }
        }

        toutesImages = new Image[][] { imagesEtape1, imagesEtape2, imagesEtape3, imagesEtape4 };

        // cacher toutes les images
        for (int i = 0; i < toutesImages.Length; i++)
        {
            CacherImagesEtape(i);
        }
        if (imagesCanvasGroup != null)
        {
            StartCoroutine(PulseImages());
        }
    }

    public void ActiverEtape(int index)
    {
        if (index < 0 || index >= textes.Length || textes[index] == null) return;
        
        // Fade in avec animation
        StartCoroutine(FadeInTexte(index));
        
        

        // arreter animation precedente
        if (animationImageEnCours != null)
        {
            StopCoroutine(animationImageEnCours);
        }

        // cacher images etape precedente
        if (index > 0)
        {
            CacherImagesEtape(index - 1);
        }

        // demarrer animation images
        animationImageEnCours = StartCoroutine(AnimerImages(index));

        Debug.Log($"TUTORIAL UI : Étape {index + 1} activée");
    }

    public void CompleterEtape(int index)
    {
        if (index < 0 || index >= textes.Length || textes[index] == null) return;

        // Ajouter checkmark et changer couleur
        textes[index].text = instructionsBase[index];
        textes[index].color = couleurComplete;

        // arreter animation images
        if (animationImageEnCours != null)
        {
            StopCoroutine(animationImageEnCours);
            animationImageEnCours = null;
        }
        CacherImagesEtape(index);


        // Fade out après 1 seconde
        StartCoroutine(FadeOutTexte(index, 1f));



        Debug.Log($"TUTORIAL UI : Étape {index + 1} complétée");
    }

    IEnumerator FadeInTexte(int index)
    {
        if (textes[index] == null) yield break;
        
        textes[index].gameObject.SetActive(true);
        
        RectTransform rt = textes[index].GetComponent<RectTransform>();
        Vector2 posDepart = positionsInitiales[index] + new Vector2(0, offsetY);
        Vector2 posArrivee = positionsInitiales[index];
        
        float elapsed = 0f;
        
        while (elapsed < dureeFade)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dureeFade;
            
            // Lerp position et alpha
            rt.anchoredPosition = Vector2.Lerp(posDepart, posArrivee, t);
            
            Color c = textes[index].color;
            c.a = t;
            textes[index].color = c;
            
            yield return null;
        }
        
        // S'assurer que c'est bien à la position finale
        rt.anchoredPosition = posArrivee;
        Color finalColor = textes[index].color;
        finalColor.a = 1f;
        textes[index].color = finalColor;
    }

    IEnumerator FadeOutTexte(int index, float delai)
    {
        yield return new WaitForSeconds(delai);
        
        if (textes[index] == null) yield break;
        
        RectTransform rt = textes[index].GetComponent<RectTransform>();
        Vector2 posDepart = rt.anchoredPosition;
        Vector2 posArrivee = positionsInitiales[index] - new Vector2(0, offsetY);
        
        float elapsed = 0f;
        
        while (elapsed < dureeFade)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dureeFade;
            
            // Lerp position et alpha
            rt.anchoredPosition = Vector2.Lerp(posDepart, posArrivee, t);
            
            Color c = textes[index].color;
            c.a = 1f - t;
            textes[index].color = c;
            
            yield return null;
        }
        
        textes[index].gameObject.SetActive(false);
        
        // Reset position pour prochaine fois
        rt.anchoredPosition = positionsInitiales[index];
    }

    public void CacherUI()
    {
        gameObject.SetActive(false);
        Debug.Log("TUTORIAL UI : UI cachée, phase principale active");
    }

    IEnumerator AnimerImages(int index)
    {
        if (index < 0 || index >= toutesImages.Length) yield break;

        Image[] images = toutesImages[index];
        if (images == null || images.Length < 2) yield break;

        // activer la premiere, cacher la deuxieme
        if (images[0] != null) images[0].gameObject.SetActive(true);
        if (images[1] != null) images[1].gameObject.SetActive(false);

        int frameActuelle = 0;

        while (true)
        {
            yield return new WaitForSeconds(intervalleAlternance);

            // alterner
            frameActuelle = 1 - frameActuelle;

            if (images[0] != null) images[0].gameObject.SetActive(frameActuelle == 0);
            if (images[1] != null) images[1].gameObject.SetActive(frameActuelle == 1);
        }
    }

    void CacherImagesEtape(int index)
    {
        if (index < 0 || index >= toutesImages.Length) return;

        Image[] images = toutesImages[index];
        if (images == null) return;

        foreach (Image img in images)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
    }
    IEnumerator PulseImages()
    {
        while (true)
        {
            if (imagesCanvasGroup != null)
            {
                imagesCanvasGroup.alpha = Mathf.Lerp(0.75f, 1f, (Mathf.Sin(Time.time * 2f) + 1f) / 2f);
            }
            yield return null;
        }
    }
}