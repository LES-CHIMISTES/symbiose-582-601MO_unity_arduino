using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIDropShadow : MonoBehaviour
{
    [Header("Parametres")]
    public Vector2 offset = new Vector2(3f, -3f);
    public Color couleurOmbre = new Color(0f, 0f, 0f, 0.5f);
    public float flou = 0f;

    private GameObject ombreGO;
    private Image ombreImage;

    void OnEnable()
    {
        CreerOmbre();
        UpdateOmbre();
    }

    void OnDisable()
    {
        DetruireOmbre();
    }

    void OnDestroy()
    {
        DetruireOmbre();
    }

    void Update()
    {
        if (ombreGO == null)
        {
            CreerOmbre();
        }
        UpdateOmbre();
    }

    void CreerOmbre()
    {
        if (ombreGO != null) return;

        ombreGO = new GameObject("_DropShadow_" + gameObject.name);

        // placer comme sibling (meme parent)
        ombreGO.transform.SetParent(transform.parent, false);

        // placer juste avant ce gameobject dans la hierarchie
        int monIndex = transform.GetSiblingIndex();
        ombreGO.transform.SetSiblingIndex(monIndex);

        // copier RectTransform
        RectTransform sourceRT = GetComponent<RectTransform>();
        RectTransform ombreRT = ombreGO.AddComponent<RectTransform>();
        ombreRT.anchorMin = sourceRT.anchorMin;
        ombreRT.anchorMax = sourceRT.anchorMax;
        ombreRT.pivot = sourceRT.pivot;
        ombreRT.sizeDelta = sourceRT.sizeDelta;
        ombreRT.anchoredPosition = sourceRT.anchoredPosition;

        // ajouter Image
        ombreImage = ombreGO.AddComponent<Image>();

        Image sourceImage = GetComponent<Image>();
        if (sourceImage != null)
        {
            ombreImage.sprite = sourceImage.sprite;
            ombreImage.type = sourceImage.type;
        }

        ombreImage.raycastTarget = false;
    }

    void UpdateOmbre()
    {
        if (ombreGO == null || ombreImage == null) return;

        RectTransform sourceRT = GetComponent<RectTransform>();
        RectTransform ombreRT = ombreGO.GetComponent<RectTransform>();

        // sync position + offset
        ombreRT.anchorMin = sourceRT.anchorMin;
        ombreRT.anchorMax = sourceRT.anchorMax;
        ombreRT.pivot = sourceRT.pivot;
        ombreRT.anchoredPosition = sourceRT.anchoredPosition + offset;

        // sync taille + flou
        ombreRT.sizeDelta = sourceRT.sizeDelta + Vector2.one * flou * 2f;

        // sync rotation et scale
        ombreRT.localRotation = sourceRT.localRotation;
        ombreRT.localScale = sourceRT.localScale;

        // couleur
        ombreImage.color = couleurOmbre;

        // sync sprite
        Image sourceImage = GetComponent<Image>();
        if (sourceImage != null && ombreImage.sprite != sourceImage.sprite)
        {
            ombreImage.sprite = sourceImage.sprite;
            ombreImage.type = sourceImage.type;
        }

        // s'assurer que l'ombre est juste avant dans la hierarchie
        int monIndex = transform.GetSiblingIndex();
        if (ombreGO.transform.GetSiblingIndex() != monIndex - 1)
        {
            ombreGO.transform.SetSiblingIndex(monIndex);
        }
    }

    void DetruireOmbre()
    {
        if (ombreGO != null)
        {
            if (Application.isPlaying)
                Destroy(ombreGO);
            else
                DestroyImmediate(ombreGO);

            ombreGO = null;
            ombreImage = null;
        }
    }
}