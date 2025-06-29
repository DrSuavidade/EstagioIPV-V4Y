using System.Collections.Generic;
using UnityEngine;

public class CatalogController : MonoBehaviour
{
    [System.Serializable]
    public struct Section
    {
        public string title;
        public Transform content;
        public List<CardData> cards;
    }

    [Header("Card Prefab")]
    [SerializeField]
    private GameObject cardPrefab;

    [Header("Sections")]
    [SerializeField]
    private CatalogLoader loader;

    [Header("Section Containers")]
    [Tooltip("Arraste os Content Transforms dos teus Scroll Views aqui, na mesma ordem do JSON.")]
    [SerializeField] private Transform[] sectionContainers;

    void OnEnable()
    {
        // subscribe BEFORE loader.Start runs
        loader.OnCatalogLoaded += PopulateAllSections;
    }

    void OnDisable()
    {
        loader.OnCatalogLoaded -= PopulateAllSections;
    }

    void PopulateAllSections()
    {
        var secs = loader.sections;
        int count = Mathf.Min(secs.Count, sectionContainers.Length);

        for (int i = 0; i < count; i++)
            PopulateSection(secs[i], sectionContainers[i]);
    }

    private void PopulateSection(CatalogController.Section sectionData, Transform content)
    {
        // limpa filhos antigos
        foreach (Transform ch in content)
            Destroy(ch.gameObject);

        // instancia cada card
        foreach (var data in sectionData.cards)
        {
            var go = Instantiate(cardPrefab, content);
            go.GetComponent<CardUI>().Setup(data);
        }
    }

    // NOVO: expõe as seções a outros scripts
    public List<Section> Sections => loader.sections;
}
