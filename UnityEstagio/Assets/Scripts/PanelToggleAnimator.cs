using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelToggleAnimator : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root RectTransform of the panel to hide/show")]
    [SerializeField] private RectTransform panelRoot;

    [Tooltip("The Button that toggles open/close")]
    [SerializeField] private Button toggleButton;

    [Tooltip("Image component on the toggle button (for changing arrow)")]
    [SerializeField] private Image toggleArrowImage;

    [Tooltip("Sprite for 'arrow pointing up' when panel is closed")]
    [SerializeField] private Sprite arrowUpSprite;

    [Tooltip("Sprite for 'arrow pointing down' when panel is open")]
    [SerializeField] private Sprite arrowDownSprite;

    [Header("Move/Transform Buttons & Bgs")]
    [SerializeField] private Button moveButton;
    [SerializeField] private Button transformButton;
    [SerializeField] private Image BgImage;       // background Image component behind Move
    [SerializeField] private Sprite moveActiveBg;      // active sprite
    [SerializeField] private Sprite NormalBg; // normal sprite
    [SerializeField] private Sprite transformActiveBg; // active sprite

    [Header("Animation Settings")]
    [Tooltip("How long (in seconds) for the panel to slide up/down")]
    [SerializeField] private float slideDuration = 0.3f;

    private bool isOpen = true;                 // panel starts open by default
    private Vector2 openPosition;               // anchoredPosition when open
    private Vector2 closedPosition;             // anchoredPosition when closed

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (panelRoot == null || toggleButton == null || toggleArrowImage == null)
        {
            Debug.LogError("PanelToggleAnimator: Missing references. Disabling.");
            enabled = false;
            return;
        }

        // NEW: slide it down by twice its height
        openPosition = panelRoot.anchoredPosition;
        float panelH = panelRoot.rect.height;
        closedPosition = new Vector2(openPosition.x, openPosition.y - panelH * 10f);


        // Ensure the toggle arrow initially shows "down" since panel is open
        toggleArrowImage.sprite = arrowDownSprite;

        // Hook up button click
        toggleButton.onClick.AddListener(OnToggleClicked);

        moveButton.onClick.AddListener(HighlightMove);
        transformButton.onClick.AddListener(HighlightTransform);

        // init both to normal
        BgImage.sprite = NormalBg;
    }

    private void OnToggleClicked()
    {
        // Stop any running slide coroutine
        StopAllCoroutines();
        // Start sliding to the opposite state
        if (isOpen)
            StartCoroutine(SlidePanel(openPosition, closedPosition));
        else
            StartCoroutine(SlidePanel(closedPosition, openPosition));

        // Swap arrow sprite immediately to indicate next action
        toggleArrowImage.sprite = isOpen ? arrowUpSprite : arrowDownSprite;
        isOpen = !isOpen;
    }

    private IEnumerator SlidePanel(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            panelRoot.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        panelRoot.anchoredPosition = to;
    }

    public void ClosePanel()
    {
        if (!isOpen)
            return;

        StopAllCoroutines();
        StartCoroutine(SlidePanel(openPosition, closedPosition));
        toggleArrowImage.sprite = arrowUpSprite;
        BgImage.sprite = NormalBg;
        isOpen = false;
    }

    public void OpenPanel()
    {
        if (isOpen)
            return;

        StopAllCoroutines();
        StartCoroutine(SlidePanel(closedPosition, openPosition));
        toggleArrowImage.sprite = arrowDownSprite;
        isOpen = true;
    }

    private void HighlightMove()
    {
        BgImage.sprite = moveActiveBg;
    }

    private void HighlightTransform()
    {
        BgImage.sprite = transformActiveBg;
    }

    // (optional) call these if you ever reset the panel or close it:
    public void ResetHighlights()
    {
        BgImage.sprite = NormalBg;
    }
}
