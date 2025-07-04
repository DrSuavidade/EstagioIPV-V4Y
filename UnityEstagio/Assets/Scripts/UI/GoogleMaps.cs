using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

[RequireComponent(typeof(RawImage))]
public class Map : MonoBehaviour
{
    [Header("API Settings")]
    public string apiKey;
    public float lat;
    public float lon;
    [Range(1, 20)] public int zoom = 12;
    public enum Resolution { low = 1, high = 2 }
    public Resolution mapResolution = Resolution.low;
    public enum Type { roadmap, satellite, hybrid, terrain }
    public Type mapType = Type.roadmap;

    [Header("Zoom Controls")]
    public Slider zoomSlider;
    public bool allowPinchZoom = true;
    [Range(1, 20)] public int minZoom = 1, maxZoom = 20;
    public float pinchSpeed = 0.1f;

    [Header("Device Location")]
    public bool useDeviceLocation = true;
    public float locationTimeout = 20f;

    [Header("Pin Overlay (Location Screen)")]
    [Tooltip("Your loader with all cards’ lat/lon")]
    public CatalogLoader loader;
    [Tooltip("UI prefab for a single pin (RectTransform)")]
    public RectTransform pinPrefab;
    [Tooltip("Parent RectTransform (usually the RawImage’s)")]
    public RectTransform pinsParent;

    [SerializeField] private TextMeshProUGUI noConnectionText;

    // internals
    private RawImage rawImage;
    private Rect rect;
    private bool needsUpdate = true;
    private float lastPinchDist;

    // cache to avoid unnecessary reloads
    private float latLast, lonLast;
    private int zoomLast;
    private Resolution resLast;
    private Type typeLast;
    private string keyLast;

    // track instantiated pins
    private readonly List<RectTransform> pins = new List<RectTransform>();

    private Vector2 initialTouch1Pos;
    private Vector2 initialTouch2Pos;
    private float initialZoom;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rect = rawImage.rectTransform.rect;

        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
            zoomSlider.value = zoom;
            zoomSlider.onValueChanged.AddListener(v =>
            {
                zoom = Mathf.RoundToInt(v);
                needsUpdate = true;
            });
        }
    }

    void Start()
    {
        StartCoroutine(InitLocationAndMap());
    }

    IEnumerator InitLocationAndMap()
    {
        yield return StartCoroutine(CheckInternetConnection());
        // once lat/lon decided, do first fetch
        rect = rawImage.rectTransform.rect;
        yield return StartCoroutine(GetGoogleMap());
    }

    private IEnumerator CheckInternetConnection()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://maps.googleapis.com/maps/api/js?key=" + apiKey);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("No internet connection or Google Maps not reachable!");
            if (noConnectionText != null)
                noConnectionText.gameObject.SetActive(true); // Show the "No Connection" message
        }
        else
        {
            if (noConnectionText != null)
                noConnectionText.gameObject.SetActive(false); // Hide the message when the connection is successful
        }
    }

    void Update()
    {
        if (allowPinchZoom && Input.touchCount == 2)
        {
            HandlePinchZoom();
        }

        // detect param changes
        if (needsUpdate &&
           (apiKey != keyLast ||
            lat != latLast ||
            lon != lonLast ||
            zoom != zoomLast ||
            mapResolution != resLast ||
            mapType != typeLast))
        {
            rect = rawImage.rectTransform.rect;
            StartCoroutine(GetGoogleMap());
            needsUpdate = false;
        }
    }

    private void HandlePinchZoom()
    {
        // Get the positions of the two touches
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // Calculate the distance between the two touches
        float currentPinchDist = Vector2.Distance(touch1.position, touch2.position);

        // If this is the first frame, store the initial positions and zoom
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            initialTouch1Pos = touch1.position;
            initialTouch2Pos = touch2.position;
            initialZoom = zoom;
        }

        // Calculate the zoom factor
        float zoomDelta = (currentPinchDist - Vector2.Distance(initialTouch1Pos, initialTouch2Pos)) * pinchSpeed;

        // Apply the zoom change
        zoom = Mathf.Clamp(Mathf.RoundToInt(initialZoom + zoomDelta), minZoom, maxZoom);

        // Update the zoom slider and trigger the map update
        if (zoomSlider != null)
        {
            zoomSlider.value = zoom;
        }
    }

    IEnumerator GetGoogleMap()
    {
        int w = Mathf.RoundToInt(rect.width);
        int h = Mathf.RoundToInt(rect.height);
        string url = $"https://maps.googleapis.com/maps/api/staticmap?" +
                     $"center={lat},{lon}&zoom={zoom}" +
                     $"&size={w}x{h}&scale={(int)mapResolution}" +
                     $"&maptype={mapType}&key={apiKey}";

        using var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            rawImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            // cache for next compare
            keyLast = apiKey;
            latLast = lat;
            lonLast = lon;
            zoomLast = zoom;
            resLast = mapResolution;
            typeLast = mapType;

            // ─────── NEW: place pins after map load ───────
            if (loader != null && pinPrefab != null && pinsParent != null)
                PlacePins();
        }
        else
        {
            Debug.LogWarning($"Map load failed: {www.error}");
        }
    }

    private void PlacePins()
    {
        // clear old pins
        foreach (var p in pins) Destroy(p.gameObject);
        pins.Clear();

        int z = zoom;
        float worldSize = 256f * Mathf.Pow(2f, z);
        Vector2 centerWorld = LatLonToWorldPixel(lat, lon, z);

        // get the actual downloaded texture size
        int texW = rawImage.texture.width;
        int texH = rawImage.texture.height;

        // Unity UI rect size (should match texW/texH if your CanvasScaler is 1:1)
        float uiW = pinsParent.rect.width;
        float uiH = pinsParent.rect.height;

        float sx = uiW / texW;
        float sy = uiH / texH;

        foreach (var sec in loader.sections)
            foreach (var cd in sec.cards)
            {
                // world‐pixel location of this card
                Vector2 worldPx = LatLonToWorldPixel(cd.latitude, cd.longitude, z);
                Vector2 delta = worldPx - centerWorld;

                // scale into UI space
                float x = delta.x * sx;
                float y = -delta.y * sy;

                // instantiate pin at that offset
                var pin = Instantiate(pinPrefab, pinsParent);
                pin.anchoredPosition = new Vector2(x, y);

                var handler = pin.GetComponent<PinClickHandler>();
                Sprite typeSprite = Resources.Load<Sprite>($"PinSprites/{cd.type}");
                handler.Setup(cd.streetName, cd.area, typeSprite);
                pins.Add(pin);
            }
    }


    private static Vector2 LatLonToWorldPixel(double lat, double lon, int z)
    {
        double siny = Math.Sin(lat * Mathf.Deg2Rad);
        siny = Math.Clamp(siny, -0.9999, 0.9999);

        double x = 256.0 * (0.5 + lon / 360.0) * Math.Pow(2, z);
        double y = 256.0 * (0.5 - Math.Log((1 + siny) / (1 - siny)) / (4.0 * Math.PI))
                       * Math.Pow(2, z);

        return new Vector2((float)x, (float)y);
    }

    /// <summary>
    /// If you externally change lat/lon, call this to redraw both map & pins.
    /// </summary>
    public void RefreshMap()
    {
        needsUpdate = true;
    }
}
