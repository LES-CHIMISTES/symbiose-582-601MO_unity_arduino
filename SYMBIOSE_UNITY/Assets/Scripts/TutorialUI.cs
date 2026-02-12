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

    [Header("Couleurs")]
    public Color couleurInactive = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color couleurActive = Color.white;
    public Color couleurComplete = new Color(0f, 1f, 0f, 1f);

    private TextMeshProUGUI[] textes;
    private string[] instructionsBase = new string[]
    {
        "1. Remplissez le niveau d'eau",
        "2. Allumez le feu à la bonne intensité",
        "3. Ajoutez des poudres selon la couleur demandée",
        "4. Brassez la potion dans le sens indiqué"
    };

    void Start()
    {
        textes = new TextMeshProUGUI[] { texteEau, texteFeu, textePoudres, texteTourbillon };

        for (int i = 0; i < textes.Length; i++)
        {
            if (textes[i] != null)
            {
                textes[i].text = instructionsBase[i];
                textes[i].color = (i == 0) ? couleurActive : couleurInactive;
            }
        }
    }

    public void ActiverEtape(int index)
    {
        if (index < 0 || index >= textes.Length || textes[index] == null) return;
        textes[index].color = couleurActive;
        Debug.Log($"TUTORIAL UI : Étape {index + 1} activée");
    }

    public void CompleterEtape(int index)
    {
        if (index < 0 || index >= textes.Length || textes[index] == null) return;
        textes[index].text = "✓ " + instructionsBase[index];
        textes[index].color = couleurComplete;
        Debug.Log($"TUTORIAL UI : Étape {index + 1} complétée");
    }

    public void CacherUI()
    {
        StartCoroutine(FadeOutUI());
    }

    IEnumerator FadeOutUI()
    {
        float duree = 1f;
        float elapsed = 0f;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        while (elapsed < duree)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - (elapsed / duree);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}