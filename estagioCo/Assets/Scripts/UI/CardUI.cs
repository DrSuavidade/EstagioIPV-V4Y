using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField]
    private Image imgPhoto;

    [SerializeField]
    private TextMeshProUGUI txtName;

    [SerializeField]
    private TextMeshProUGUI txtLocation;

    [SerializeField]
    private TextMeshProUGUI txtPrice;

    [SerializeField]
    private TextMeshProUGUI txtType;

    [SerializeField]
    private TextMeshProUGUI txtSize;

    [SerializeField]
    private TextMeshProUGUI txtArea;

    [Header("Favorite Icon/Button")]
    [SerializeField]
    private Image iconFavorite; // o ícone (estrela, coração…)

    [SerializeField]
    private Button favButton; // o Button sobre o ícone

    private CardData data;
    private CatalogSceneController sceneController;

    void Awake()
    {
        sceneController = Object.FindAnyObjectByType<CatalogSceneController>();
        if (favButton != null)
            favButton.onClick.AddListener(OnFavoriteClicked);
    }

    /// <summary>
    /// Configura o card com os dados e atualiza a UI.
    /// Deve ser chamado logo após Instantiate().
    /// </summary>
    public void Setup(CardData cardData)
    {
        data = cardData;

        // Preenche imagem e textos
        imgPhoto.sprite = data.image;
        txtName.text = data.name;
        txtLocation.text = data.location;
        txtPrice.text = $"{data.price:0,0}€";
        txtType.text = data.type;
        txtSize.text = data.size;
        txtArea.text = $"{data.area:0} m²";

        // Carrega estado de favorito do manager
        bool isFav = FavoritesManager.Instance.IsFavorite(data);
        data.isFavorite = isFav;

        // Atualiza aparência do ícone
        UpdateFavoriteIcon();
    }

    /// <summary>
    /// Clique em qualquer parte do card (exceto no botão favorito) abre o detalhe.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (sceneController != null)
            sceneController.ShowDetail(data);
    }

    public void RefreshFavorite()
    {
        bool isFav = FavoritesManager.Instance.IsFavorite(data);
        data.isFavorite = isFav;
        UpdateFavoriteIcon();
    }

    /// <summary>
    /// Chamado quando o botão de favorito é clicado.
    /// </summary>
    private void OnFavoriteClicked()
    {
        data.isFavorite = !data.isFavorite;
        FavoritesManager.Instance.SetFavorite(data, data.isFavorite);
        UpdateFavoriteIcon();
    }

    /// <summary>
    /// Atualiza a cor ou sprite do ícone consoante o estado de favorito.
    /// </summary>
    private void UpdateFavoriteIcon()
    {
        iconFavorite.enabled = data.isFavorite;
    }
}
