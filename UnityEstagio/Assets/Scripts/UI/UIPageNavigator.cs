using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPageNavigator : MonoBehaviour
{
    [Header("Pages (CanvasGroups)")]
    [SerializeField] private CanvasGroup[] pages; // 0=Home,1=Location,2=Catalog

    [Header("Optional Animators")]
    [SerializeField] private Animator[] pageAnimators;

    [Header("Home Exit Animation")]
    [Tooltip("State in the Home animator to play when leaving Home")]
    [SerializeField] private string homeExitState = "Exit";

    [Header("Transition Settings")]
    [SerializeField] private float duration = 0.3f;

    [Header("Catalog Reset")]
    [Tooltip("Index of the Catalog page (usually 2)")]
    [SerializeField] private int catalogPageIndex = 2;

    [Header("Info Panel (Privacy)")]
    [SerializeField] private CanvasGroup infoPanel;
    [SerializeField] private Button infoButton;
    [SerializeField] private GameObject mainScreenElement;

    [Header("Bottom Nav Fill")]
    [Tooltip("The background Image behind your nav buttons")]
    [SerializeField] private Image fillImage;
    [Tooltip("Sprites for each page: 0=Home,1=Location,2=Catalog")]
    [SerializeField] private Sprite[] fillSprites;

    private int current = 0;
    private bool isTweening = false;
    private bool isInfoOpen = false;
    private Coroutine infoFadeCoroutine;
    private CatalogSceneController catalogSceneController;

    void Awake()
    {
        catalogSceneController = Object.FindAnyObjectByType<CatalogSceneController>();

        // init infoPanel closed
        if (infoPanel != null)
            SetInstant(infoPanel, 0f, false);

        if (infoButton != null)
            infoButton.onClick.AddListener(ToggleInfoPanel);
    }

    void Start()
    {
        // only the current page is visible
        for (int i = 0; i < pages.Length; i++)
        {
            bool isActive = (i == current);
            pages[i].alpha = isActive ? 1f : 0f;
            pages[i].interactable = isActive;
            pages[i].blocksRaycasts = isActive;

            if (pageAnimators != null && i < pageAnimators.Length && pageAnimators[i] != null)
            {
                var anim = pageAnimators[i];
                anim.Rebind();
                anim.Update(0f);
                if (isActive)
                    anim.Play(0, -1, 0f);
            }
        }

        // set initial fill sprite
        UpdateFill(current);
    }

    public void OnNavButton(int pageIndex)
    {
        // clicking same button
        if (pageIndex == current)
        {
            if (pageIndex == catalogPageIndex && catalogSceneController != null)
                catalogSceneController.ShowCatalog();
            return;
        }

        if (isTweening || pageIndex < 0 || pageIndex >= pages.Length)
            return;

        StartCoroutine(SlideTo(pageIndex));
    }

    private IEnumerator SlideTo(int next)
    {
        isTweening = true;

        if (current == 0 && pageAnimators != null && pageAnimators[0] != null)
        {
            var homeAnim = pageAnimators[0];
            homeAnim.Rebind();
            homeAnim.Update(0f);
            homeAnim.Play(homeExitState, -1, 0f);
        }

        var from = pages[current];
        var to = pages[next];

        to.alpha = 0f;
        to.interactable = true;
        to.blocksRaycasts = true;

        if (pageAnimators != null && next < pageAnimators.Length && pageAnimators[next] != null)
        {
            var anim = pageAnimators[next];
            anim.Rebind();
            anim.Update(0f);
            anim.Play(0, -1, 0f);
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(1f, 0f, t / duration);
            from.alpha = a;
            to.alpha = 1f - a;
            yield return null;
        }

        from.interactable = false;
        from.blocksRaycasts = false;
        current = next;
        isTweening = false;

        // update fill background
        UpdateFill(current);
    }

    private void ToggleInfoPanel()
    {
        if (isTweening)
            return;

        isInfoOpen = !isInfoOpen;

        if (mainScreenElement != null)
            mainScreenElement.SetActive(!isInfoOpen);

        if (infoFadeCoroutine != null)
            StopCoroutine(infoFadeCoroutine);
        infoFadeCoroutine = StartCoroutine(FadeCanvas(infoPanel, isInfoOpen ? 1f : 0f));
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float target)
    {
        bool opening = target > cg.alpha;
        if (opening)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        cg.alpha = target;

        if (!opening)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    private void SetInstant(CanvasGroup cg, float alpha, bool interactable)
    {
        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }

    private void UpdateFill(int pageIndex)
    {
        if (fillImage == null || fillSprites == null) return;
        if (pageIndex < 0 || pageIndex >= fillSprites.Length) return;
        fillImage.sprite = fillSprites[pageIndex];
    }
}
