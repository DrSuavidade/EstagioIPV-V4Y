using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CatalogFilterController : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private CanvasGroup catalogGroup;
    [SerializeField] private CanvasGroup filteredGroup;

    [Header("Filtered List")]
    [SerializeField] private Transform   filteredContent;
    [SerializeField] private GameObject  cardPrefab;
    [SerializeField] private GameObject  emptyMessage;

    [Header("Source of Data")]
    [SerializeField] private CatalogController catalogController;
    [SerializeField] private Map map;

    [Header("UI Buttons & Sprites")]
    [Tooltip("Toggles the search dropdown")]
    [SerializeField] private Button searchButton;
    [SerializeField] private Sprite searchNormalSprite;
    [SerializeField] private Sprite searchActiveSprite;

    [Tooltip("Filters only your favorites")]
    [SerializeField] private Button favoritesButton;
    [SerializeField] private Sprite favoritesNormalSprite;
    [SerializeField] private Sprite favoritesActiveSprite;

    [Tooltip("Filters by closest distance")]
    [SerializeField] private Button closestButton;
    [SerializeField] private Sprite closestNormalSprite;
    [SerializeField] private Sprite closestActiveSprite;

    private List<CardData> allCards;

    IEnumerator Start()
    {
        // wait for JSON → CatalogController to populate
        yield return new WaitUntil(() =>
            catalogController.Sections != null &&
            catalogController.Sections.Count > 0
        );

        // flatten all
        allCards = catalogController.Sections
                    .SelectMany(s => s.cards)
                    .ToList();

        // clear any stray filtered cards
        foreach (Transform ch in filteredContent)
            Destroy(ch.gameObject);

        HookUpButtons();
        ShowCatalog();
    }

    private void HookUpButtons()
    {
        // SEARCH button (dropdown itself is handled elsewhere,
        // here we just swap sprites on click)
        if (searchButton != null)
        {
            searchButton.onClick.AddListener(() =>
            {
                var searchDropdown = FindFirstObjectByType<SearchDropdownToggle>();
                bool isOpen = false;
                if (searchDropdown != null)
                {
                    // Replace 'IsOpen' with the correct property or method if it exists,
                    // or implement 'IsOpen' in SearchDropdownToggle if missing.
                    // Example implementation:
                    isOpen = searchDropdown.isActiveAndEnabled; // or another appropriate property
                }
                SetSearchActive(isOpen);
            });
            SetSearchActive(false);
        }

        // FAVORITES button
        if (favoritesButton != null)
        {
            favoritesButton.onClick.AddListener(FilterFavorites);
            SetFavoritesActive(false);
        }

        // CLOSEST button
        if (closestButton != null)
        {
            closestButton.onClick.AddListener(FilterClosest);
            SetClosestActive(false);
        }
    }

    public void ShowCatalog()
    {
        // bring back original catalog view
        catalogGroup.alpha        = 1f;
        catalogGroup.interactable = true;
        catalogGroup.blocksRaycasts = true;

        filteredGroup.alpha        = 0f;
        filteredGroup.interactable = false;
        filteredGroup.blocksRaycasts = false;

        if (emptyMessage != null)
            emptyMessage.SetActive(false);

        // reset all button states
        SetSearchActive(false);
        SetFavoritesActive(false);
        SetClosestActive(false);
    }

    public void FilterFavorites()
    {
        ApplyFilter(FavoritesManager.Instance.Favorites);
        SetFavoritesActive(true);
        SetSearchActive(false);
        SetClosestActive(false);
    }

    public void FilterPriceLowToHigh()
        => ApplyFilter(allCards.OrderBy(c => c.price));

    public void FilterPriceHighToLow()
        => ApplyFilter(allCards.OrderByDescending(c => c.price));

    public void FilterByType(string type)
        => ApplyFilter(allCards.Where(c => c.type == type));

    public void FilterClosest()
    {
        if (map == null)
        {
            Debug.LogWarning("Map ref missing!");
            return;
        }

        float curLat = map.lat;
        float curLon = map.lon;

        var sorted = allCards
            .OrderBy(c => HaversineDistance(curLat, curLon, c.latitude, c.longitude));

        ApplyFilter(sorted);
        SetClosestActive(true);
        SetFavoritesActive(false);
        SetSearchActive(false);
    }

    private void ApplyFilter(IEnumerable<CardData> subset)
    {
        // hide original catalog
        catalogGroup.alpha = 0f;
        catalogGroup.interactable = false;
        catalogGroup.blocksRaycasts = false;

        // clear old filtered
        foreach (Transform ch in filteredContent)
            Destroy(ch.gameObject);

        var list = subset.ToList();
        if (list.Count == 0)
        {
            if (emptyMessage != null)
            {
                emptyMessage.transform.SetParent(filteredGroup.transform, false);
                emptyMessage.SetActive(true);
            }
        }
        else
        {
            if (emptyMessage != null)
                emptyMessage.SetActive(false);

            foreach (var data in list)
            {
                var go = Instantiate(cardPrefab, filteredContent);
                go.GetComponent<CardUI>().Setup(data);
            }
        }

        // show filtered list
        filteredGroup.alpha = 1f;
        filteredGroup.interactable = true;
        filteredGroup.blocksRaycasts = true;

        SetSearchActive(true);
        SetFavoritesActive(false);
        SetClosestActive(false);
        var searchDropdown = FindFirstObjectByType<SearchDropdownToggle>();
        if (searchDropdown != null)
        {
            searchDropdown.ToggleSubmenu();
            searchDropdown.CloseAll();
        }
    }

    #region — Button Sprite Helpers —

    private void SetSearchActive(bool active)
    {
        if (searchButton != null)
            searchButton.image.sprite = active
                ? searchActiveSprite
                : searchNormalSprite;
    }

    private void SetFavoritesActive(bool active)
    {
        if (favoritesButton != null)
            favoritesButton.image.sprite = active
                ? favoritesActiveSprite
                : favoritesNormalSprite;
    }

    private void SetClosestActive(bool active)
    {
        if (closestButton != null)
            closestButton.image.sprite = active
                ? closestActiveSprite
                : closestNormalSprite;
    }

    #endregion

    #region — Distance Utility —

    private static double HaversineDistance(
        double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // meters
        double dLat = Deg2Rad(lat2 - lat1);
        double dLon = Deg2Rad(lon2 - lon1);
        lat1 = Deg2Rad(lat1);
        lat2 = Deg2Rad(lat2);

        double a = Math.Sin(dLat/2)*Math.Sin(dLat/2)
                 + Math.Cos(lat1)*Math.Cos(lat2)
                 * Math.Sin(dLon/2)*Math.Sin(dLon/2);
        double c = 2*Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        return R * c;
    }

    private static double Deg2Rad(double deg) => deg * Math.PI / 180.0;

    #endregion
}
