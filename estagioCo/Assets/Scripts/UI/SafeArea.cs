using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.orientation != lastOrientation ||
            new Vector2Int(Screen.width, Screen.height) != lastScreenSize)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        if (panel.name.Contains("SafeArea"))
        {
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }


        lastSafeArea = safeArea;
        lastOrientation = Screen.orientation;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
}
