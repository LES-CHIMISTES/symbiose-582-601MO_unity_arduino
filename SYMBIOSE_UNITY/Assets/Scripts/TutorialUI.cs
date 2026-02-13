using UnityEngine;
using TMPro;
using System.Collections;

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

    [Header("Couleurs")]
    public Color couleurActive = Color.white;
    public Color couleurComplete = new Color(0f, 1f, 0f, 1f);

    private TextMeshProUGUI[] textes;
    private Vector2[] positionsInitiales;
    private string[] instructionsBase = new string[]
    {
        "Remplissez le niveau d'eau",
        "Allumez le feu à la bonne intensité",
        "Ajoutez des poudres selon la couleur demandée",
        "Brassez la potion dans le sens indiqué"
        /* si t'as le temps faire en sorte que y'aille des checkbox unchecked avant en préfixe 
        "☐ Remplissez le niveau d'eau",
        "☐ Allumez le feu à la bonne intensité",
        "☐ Ajoutez des poudres selon la couleur demandée",
        "☐ Brassez la potion dans le sens indiqué"*/
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
    }

    public void ActiverEtape(int index)
    {
        if (index < 0 || index >= textes.Length || textes[index] == null) return;
        
        // Fade in avec animation
        StartCoroutine(FadeInTexte(index));
        
        Debug.Log($"TUTORIAL UI : Étape {index + 1} activée");
    }

    public void CompleterEtape(int index)
    {
        if (index < 0 || index >= textes.Length || textes[index] == null) return;
        
        // Ajouter checkmark et changer couleur
        textes[index].text = "ˇ " + instructionsBase[index];
        textes[index].color = couleurComplete;
        
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
}