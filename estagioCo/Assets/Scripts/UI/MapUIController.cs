using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUIController : MonoBehaviour
{
    public static MapUIController Instance { get; private set; }
    
    [Header("Shared Info Panel")]
    [SerializeField] private CanvasGroup      infoPanel;
    [SerializeField] private TextMeshProUGUI  streetNameText;
    [SerializeField] private TextMeshProUGUI  areaText;
    [SerializeField] private Image            displayImage;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        Hide();
    }

    /// <summary> Populates & shows the panel. </summary>
    public void Show(string street, float area, Sprite sprite)
    {
        streetNameText.text = street;
        areaText.text       = $"{area:F1} m²";
        displayImage.sprite = sprite;

        infoPanel.alpha          = 1f;
        infoPanel.interactable   = true;
        infoPanel.blocksRaycasts = true;
    }

    /// <summary> Hides the panel. </summary>
    public void Hide()
    {
        infoPanel.alpha          = 0f;
        infoPanel.interactable   = false;
        infoPanel.blocksRaycasts = false;
    }
}
