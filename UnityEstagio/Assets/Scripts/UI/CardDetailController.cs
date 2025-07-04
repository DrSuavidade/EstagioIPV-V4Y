using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;

public class CardDetailController : MonoBehaviour
{
    [Header("Root CanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Header")]
    [SerializeField] private Image imgPhoto;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private TextMeshProUGUI txtType;
    [SerializeField] private TextMeshProUGUI txtSize;
    [SerializeField] private TextMeshProUGUI txtArea;

    [Header("Favorite Icon/Button")]
    [SerializeField] private Image iconFavorite;
    [SerializeField] private Button favButton;

    [Header("Tabs")]
    [SerializeField] private Button btnDescription;
    [SerializeField] private Button btnDetails;
    [SerializeField] private Button btnMap;

    [Header("Tab Sprites")]
    [SerializeField] private Sprite descriptionNormalSprite;
    [SerializeField] private Sprite descriptionActiveSprite;
    [SerializeField] private Sprite detailsNormalSprite;
    [SerializeField] private Sprite detailsActiveSprite;
    [SerializeField] private Sprite mapNormalSprite;
    [SerializeField] private Sprite mapActiveSprite;

    [Header("Panels")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private GameObject mapPanel;

    [Header("Descrição Content")]
    [SerializeField] private TextMeshProUGUI txtDescription;

    [Header("Detalhes Content")]
    [SerializeField] private TextMeshProUGUI txtDetalhesLeft;
    [SerializeField] private TextMeshProUGUI txtDetalhesRight;

    [Header("Google Maps")]
    [SerializeField] private Map googleMap;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Scan Shortcut")]
    [SerializeField] private Button scanButton;

    private CardData currentData;
    private bool isShowing = false;

    private enum Tab { Description, Details, Map }

    void Awake()
    {
        // Start hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Hook up tab buttons
        btnDescription.onClick.AddListener(() => ShowTab(Tab.Description));
        btnDetails.onClick.AddListener(() => ShowTab(Tab.Details));
        btnMap.onClick.AddListener(() => ShowTab(Tab.Map));

        // Favorites & Scan
        if (favButton != null)
            favButton.onClick.AddListener(OnFavoriteClicked);
        if (scanButton != null)
            scanButton.onClick.AddListener(OnScanButtonClicked);

        // Initialize tab sprites to “none selected”
        SetDescriptionSprite(false);
        SetDetailsSprite(false);
        SetMapSprite(false);
    }

    public void Show(CardData d)
    {
        currentData = d;

        PopulateHeader(d);
        PopulatePanels(d);
        ShowTab(Tab.Description);

        // Favorite icon
        bool isFav = FavoritesManager.Instance.IsFavorite(d);
        currentData.isFavorite = isFav;
        iconFavorite.enabled = isFav;

        // Map
        if (googleMap != null)
        {
            googleMap.lat = d.latitude;
            googleMap.lon = d.longitude;
            googleMap.RefreshMap();
        }

        if (!isShowing)
            StartCoroutine(Fade(canvasGroup.alpha, 1f, true));
    }

    private void OnScanButtonClicked()
    {
        if (currentData == null)
            return;

        ScanSession.SkipImageGate = true;
        ScanSession.ActiveMarker = currentData.scanImageName;
        ScanSession.ActiveModel = currentData.modelPrefabName;

        var navigator = FindFirstObjectByType<SceneNavigator>();
        if (navigator != null)
            navigator.LoadSceneByName("Scan");
        else
            SceneManager.LoadScene("Scan");
    }

    private void OnFavoriteClicked()
    {
        if (currentData == null) return;
        currentData.isFavorite = !currentData.isFavorite;
        FavoritesManager.Instance.SetFavorite(currentData, currentData.isFavorite);
        iconFavorite.enabled = currentData.isFavorite;
    }

    public void Hide()
    {
        if (isShowing)
            StartCoroutine(Fade(canvasGroup.alpha, 0f, false));
    }

    private void ShowTab(Tab tab)
    {
        // Activate only the chosen panel
        descriptionPanel.SetActive(tab == Tab.Description);
        detailsPanel.SetActive(tab == Tab.Details);
        mapPanel.SetActive(tab == Tab.Map);

        // Swap sprites
        SetDescriptionSprite(tab == Tab.Description);
        SetDetailsSprite(tab == Tab.Details);
        SetMapSprite(tab == Tab.Map);
    }

    private void PopulateHeader(CardData d)
    {
        imgPhoto.sprite = d.image;
        txtPrice.text = $"{d.price:0,0}€";
        txtType.text = d.type;
        txtSize.text = d.size;
        txtArea.text = $"{d.area:0} m²";
    }

    private void PopulatePanels(CardData d)
    {
        txtDescription.text = d.description;
        txtDetalhesLeft.text = d.detailsLeft;
        txtDetalhesRight.text = d.detailsRight;
    }

    private IEnumerator Fade(float from, float to, bool enabling)
    {
        isShowing = enabling;
        if (enabling)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;

        if (!enabling)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    #region Sprite Helpers

    private void SetDescriptionSprite(bool active)
    {
        if (btnDescription != null && descriptionNormalSprite != null)
            btnDescription.image.sprite = active
                ? descriptionActiveSprite
                : descriptionNormalSprite;
    }

    private void SetDetailsSprite(bool active)
    {
        if (btnDetails != null && detailsNormalSprite != null)
            btnDetails.image.sprite = active
                ? detailsActiveSprite
                : detailsNormalSprite;
    }

    private void SetMapSprite(bool active)
    {
        if (btnMap != null && mapNormalSprite != null)
            btnMap.image.sprite = active
                ? mapActiveSprite
                : mapNormalSprite;
    }

    #endregion
}
