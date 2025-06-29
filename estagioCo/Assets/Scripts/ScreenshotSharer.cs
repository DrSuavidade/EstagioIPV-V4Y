using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NativeGalleryNamespace;  // from Native Gallery plugin
using NativeShareNamespace;    // from Native Share plugin

public class ScreenshotSharer : MonoBehaviour
{
    [Header("UI References (always active)")]
    [Tooltip("Assign this to a GameObject that remains active (e.g. a UI Manager)")]
    [SerializeField] private Button      captureBtn;
    [SerializeField] private CanvasGroup previewGroup;
    [SerializeField] private Image       previewImage;
    [SerializeField] private Button      saveBtn;
    [SerializeField] private Button      shareBtn;

    [Header("Panel to hide during capture")]
    [Tooltip("This panel will be deactivated for the screenshot")]
    [SerializeField] private GameObject  canvasHide;
    [SerializeField] private GameObject  canvas2Hide;

    [SerializeField] private CanvasGroup saveConfirmationGroup;
    [SerializeField] private float       savePopupDuration = 2f;

    private string lastPath;

    void Awake()
    {
        captureBtn.onClick.AddListener(OnCaptureClicked);
        saveBtn.onClick.AddListener(OnSaveClicked);
        shareBtn.onClick.AddListener(OnShareClicked);

        // start with preview hidden
        previewGroup.alpha = 0f;
        previewGroup.interactable = false;
        previewGroup.blocksRaycasts = false;
        
        if (saveConfirmationGroup != null)
        {
            saveConfirmationGroup.alpha          = 0f;
            saveConfirmationGroup.interactable   = false;
            saveConfirmationGroup.blocksRaycasts = false;
        }
    }

    private void OnCaptureClicked()
    {
        // always start the coroutine from an active GameObject
        StartCoroutine(CaptureAndShow());
    }

    private IEnumerator CaptureAndShow()
    {
        // 1) Hide that share panel or any UI you don't want in shot
        if (canvasHide != null)
            canvasHide.SetActive(false);

        if (canvas2Hide != null)
            canvas2Hide.SetActive(false);

        // 2) Wait until end of frame to let Unity finish rendering
        yield return new WaitForEndOfFrame();

        // 3) Read screen into texture
        int w = Screen.width, h = Screen.height;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        // 4) Restore the panel immediately
        if (canvasHide != null)
            canvasHide.SetActive(true);

        if (canvas2Hide != null)
            canvas2Hide.SetActive(true);

        // 5) Show the result in your preview UI
        previewImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f)
        );
        previewGroup.alpha          = 1f;
        previewGroup.interactable   = true;
        previewGroup.blocksRaycasts = true;

        // 6) Save to a temp file for sharing
        lastPath = System.IO.Path.Combine(
            Application.temporaryCachePath,
            $"ARShot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png"
        );
        System.IO.File.WriteAllBytes(lastPath, tex.EncodeToPNG());
    }

    public void HideShare()
    {
        previewGroup.alpha          = 0f;
        previewGroup.interactable   = false;
        previewGroup.blocksRaycasts = false;
    }

    private void OnSaveClicked()
    {
        if (string.IsNullOrEmpty(lastPath)) return;
        NativeGallery.SaveImageToGallery(lastPath, "Vision4You", "AR_{0}.png");

        if (saveConfirmationGroup != null)
            StartCoroutine(ShowSaveConfirmation());
    }

    private IEnumerator ShowSaveConfirmation()
    {
        saveConfirmationGroup.alpha          = 1f;
        saveConfirmationGroup.interactable   = true;
        saveConfirmationGroup.blocksRaycasts = true;

        yield return new WaitForSeconds(savePopupDuration);

        saveConfirmationGroup.alpha          = 0f;
        saveConfirmationGroup.interactable   = false;
        saveConfirmationGroup.blocksRaycasts = false;
    }

    private void OnShareClicked()
    {
        if (string.IsNullOrEmpty(lastPath)) return;
        new NativeShare()
            .AddFile(lastPath)
            .SetSubject("Check out my AR shot!")
            .SetText("I just placed an AR model with Vision4You!")
            .Share();
    }
}
