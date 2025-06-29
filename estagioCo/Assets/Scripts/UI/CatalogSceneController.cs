using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatalogSceneController : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField]
    private CanvasGroup catalogGroup;

    [SerializeField]
    private CanvasGroup filteredGroup;

    [SerializeField]
    private CanvasGroup detailGroup;

    [Header("Detail Panel Controller")]
    [SerializeField]
    private CardDetailController detailController;

    [Header("Top Bar")]
    [SerializeField]
    private GameObject topBar;

    [Header("Transition")]
    [SerializeField]
    private float duration = 0.3f;

    void Start()
    {
        // No arranque, só o catálogo está visível
        SetGroup(catalogGroup, true);
        SetGroup(filteredGroup, false);
        SetGroup(detailGroup, false);
        if (topBar != null)
            topBar.SetActive(true);
    }

    /// <summary>
    /// Chamado quer o card venha do catálogo original
    /// quer do filteredGroup — mostra o detalhe e esconde ambos.
    /// </summary>
    public void ShowDetail(CardData data)
    {
        // popula o painel
        detailController.Show(data);

        // inicia fade-out do catálogo e do filtered, fade-in do detalhe
        StopAllCoroutines();
        StartCoroutine(FadeGroups(catalogGroup, detailGroup));
        StartCoroutine(FadeGroups(filteredGroup, detailGroup));

        // esconde a top bar
        if (topBar != null)
            topBar.SetActive(false);
    }

    private void RefreshFavoritesInCatalog()
    {
        foreach (var card in catalogGroup.GetComponentsInChildren<CardUI>(true))
            card.RefreshFavorite();
    }

    /// <summary>
    /// Chama RefreshFavorite() em todos os cards ativos no container filtrado.
    /// </summary>
    private void RefreshFavoritesInFiltered()
    {
        foreach (var card in filteredGroup.GetComponentsInChildren<CardUI>(true))
            card.RefreshFavorite();
    }

    /// <summary>
    /// Volta à vista de catálogo original.
    /// </summary>
    public void ShowCatalog()
    {
        StopAllCoroutines();
        StartCoroutine(FadeGroups(detailGroup, catalogGroup));
        SetGroup(filteredGroup, false);
        RefreshFavoritesInCatalog();

        if (topBar != null)
            topBar.SetActive(true);
    }

    /// <summary>
    /// Volta à vista de resultados filtrados.
    /// </summary>
    public void ShowFiltered()
    {
        StopAllCoroutines();
        StartCoroutine(FadeGroups(detailGroup, filteredGroup));
        SetGroup(catalogGroup, false);
        RefreshFavoritesInCatalog();

        if (topBar != null)
            topBar.SetActive(true);
    }

    private IEnumerator FadeGroups(CanvasGroup from, CanvasGroup to)
    {
        // desativa interações no “from” e ativa no “to”
        from.interactable = false;
        from.blocksRaycasts = false;
        to.interactable = true;
        to.blocksRaycasts = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = t / duration;
            from.alpha = Mathf.Lerp(from.alpha, 0f, a);
            to.alpha = Mathf.Lerp(to.alpha, 1f, a);
            yield return null;
        }
        from.alpha = 0f;
        to.alpha = 1f;
    }

    private void SetGroup(CanvasGroup cg, bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }
}
