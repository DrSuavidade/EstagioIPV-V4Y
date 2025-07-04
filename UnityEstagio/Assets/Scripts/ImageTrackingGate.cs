using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using System.IO;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackingGate : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your placement script; will be enabled once the gate opens.")]
    [SerializeField] private XR_Placement placementScript;

    [Tooltip("CanvasGroup containing your \"Scan Image\" instructions UI.")]
    [SerializeField] private CanvasGroup instructionsCanvasGroup;

    [Tooltip("How fast (seconds) the pulse animation cycles.")]
    [SerializeField] private float pulseSpeed = 1.0f;

    [SerializeField] private TextAsset catalogJsonAsset;

    // internally set
    private ARTrackedImageManager trackedImageManager;
    private bool gateOpened = false;
    private Coroutine pulseCoroutine;

    // Maps from ReferenceImageLibrary image name → JSON-defined model key
    private Dictionary<string, string> scanToModelKey;

    void Awake()
    {
        // 1) grab the ARTrackedImageManager
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (ScanSession.SkipImageGate)
        {
            OpenGate();
            return;
        }

        // 4) otherwise, block placement and show instructions
        placementScript.enabled = false;
        instructionsCanvasGroup.alpha = 1f;
        instructionsCanvasGroup.interactable = true;
        instructionsCanvasGroup.blocksRaycasts = true;

        // 5) start pulsing the instructions
        pulseCoroutine = StartCoroutine(PulseInstructions());

        StartCoroutine(BuildMapAndSubscribe());
    }

    private IEnumerator BuildMapAndSubscribe()
    {
        // 1) Load JSON text
        string json = null;
        if (catalogJsonAsset != null)
        {
            json = catalogJsonAsset.text;
        }
        else
        {
            string path = Path.Combine(Application.streamingAssetsPath, "catalog.json");
            if (Application.platform == RuntimePlatform.Android)
            {
                using var www = UnityWebRequest.Get(path);
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                    json = www.downloadHandler.text;
                else
                    Debug.LogError($"[ImageGate] JSON load failed: {www.error}");
            }
            else if (File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
            else
            {
                Debug.LogError($"[ImageGate] catalog.json not found at {path}");
            }
        }

        // 2) Parse into dictionary
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var catalog = JsonUtility.FromJson<CatalogJson>(json);
                scanToModelKey = catalog.sections
                    .SelectMany(sec => sec.cards)
                    .Where(cd => !string.IsNullOrEmpty(cd.scanImageName))
                    .ToDictionary(cd => cd.scanImageName.Trim(), cd => cd.modelPrefabName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ImageGate] JSON parse error: {ex}");
                scanToModelKey.Clear();
            }
        }
    }

    void OnEnable()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        if (gateOpened)
            return;

        // check newly added images
        foreach (var img in args.added)
        {
            if (img.trackingState == TrackingState.Tracking)
            {
                HandleImageTracked(img);
                return;
            }
        }

        // also check images that were updated into tracking
        foreach (var img in args.updated)
        {
            if (img.trackingState == TrackingState.Tracking)
            {
                HandleImageTracked(img);
                return;
            }
        }
    }

    private void HandleImageTracked(ARTrackedImage img)
    {
        // open the gate
        OpenGate();

        // spawn the model linked to this image
        string scanName = img.referenceImage.name;
        if (scanToModelKey.TryGetValue(scanName, out var modelKey))
        {
            SpawnModelForKey(modelKey, img.transform.position, img.transform.rotation);
        }
        else
        {
            Debug.LogWarning($"No modelPrefabName mapping for scanned image '{scanName}'");
        }
    }

    private void SpawnModelForKey(string key, Vector3 position, Quaternion rotation)
    {
        var prefab = ModelRepository.Instance.GetPrefab(key);
        if (prefab != null)
        {
            Instantiate(prefab, position, rotation);
        }
        else
        {
            Debug.LogError($"ModelRepository has no entry for key '{key}'");
        }
    }

    private void OpenGate()
    {
        gateOpened = true;

        // stop pulsing
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        // hide instructions
        instructionsCanvasGroup.alpha = 0f;
        instructionsCanvasGroup.interactable = false;
        instructionsCanvasGroup.blocksRaycasts = false;

        // enable your placement logic
        placementScript.enabled = true;
    }

    private IEnumerator PulseInstructions()
    {
        float halfPeriod = pulseSpeed * 0.5f;
        while (!gateOpened)
        {
            // fade out
            for (float t = 0f; t < halfPeriod; t += Time.deltaTime)
            {
                instructionsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / halfPeriod);
                yield return null;
            }
            // fade in
            for (float t = 0f; t < halfPeriod; t += Time.deltaTime)
            {
                instructionsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / halfPeriod);
                yield return null;
            }
        }
    }
}
