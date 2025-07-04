using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

[RequireComponent(typeof(ARRaycastManager))]
public class XR_Placement : MonoBehaviour
{
    [Header("AR Prefab & UI")]
    [SerializeField] private GameObject prefab;
    private string modelKey;
    [SerializeField] private Canvas editingUI;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button transformButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private PanelToggleAnimator panelToggler;

    [Header("Sliders")]
    [SerializeField] private Slider sliderA;
    [SerializeField] private Slider sliderB;
    [SerializeField] private float moveRange = 1.0f;
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 2.0f;

    [Header("Reticle (Optional)")]
    [SerializeField] private Image reticleImage;

    [Header("Plane Manager")]
    [SerializeField] private ARPlaneManager planeManager;

    private ARRaycastManager raycastManager;
    private Camera mainCamera;

    private GameObject selectedObject;
    private List<GameObject> spawnedPrefabs = new List<GameObject>();

    private enum EditMode { None, Move, Transform }
    private EditMode currentMode = EditMode.None;

    private Vector3 basePosition;
    private float baseScale;
    private float baseRotationY;

    private bool isDragging = false;
    private Vector2 lastPointerPos;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;

        if (planeManager == null)
            planeManager = FindFirstObjectByType<ARPlaneManager>();

        if (reticleImage != null)
            reticleImage.color = Color.gray;

        editingUI.gameObject.SetActive(false);

        moveButton.onClick.AddListener(() => SetMode(EditMode.Move));
        transformButton.onClick.AddListener(() => SetMode(EditMode.Transform));
        confirmButton.onClick.AddListener(ConfirmPlacement);
        resetButton.onClick.AddListener(ResetPlacement);

        sliderA.onValueChanged.AddListener(OnSliderAChanged);
        sliderB.onValueChanged.AddListener(OnSliderBChanged);

        sliderA.gameObject.SetActive(false);
        sliderB.gameObject.SetActive(false);

        modelKey = ScanSession.ActiveModel;
    }

    void Update()
    {
        UpdateReticle();

        // 1) If no object placed yet, allow tap→place
        if (selectedObject == null)
        {
            bool tapped = false;
#if UNITY_EDITOR
            tapped = Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID || UNITY_IOS
            tapped = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
#endif
            if (tapped && !IsPointerOverUI())
            {
                Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                TryPlacePrefab(center);
            }
        }
        // 2) If in Move/Transform mode, handle drag—but only if not over UI
        else if (currentMode != EditMode.None)
        {
            // Check if pointer (mouse or touch) is over any UI element
            bool pointerOverUI = IsPointerOverUI();
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                if (!pointerOverUI)
                {
                    isDragging = true;
                    lastPointerPos = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
            else if (isDragging && Input.GetMouseButton(0) && !pointerOverUI)
            {
                Vector2 currentPos = Input.mousePosition;
                Vector2 delta = currentPos - lastPointerPos;

                if (currentMode == EditMode.Move)
                    TryMove(currentPos);
                else if (currentMode == EditMode.Transform)
                    TransformDrag(delta);

                lastPointerPos = currentPos;
            }
#else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                bool overUI = IsPointerOverUI()
;

                if (touch.phase == TouchPhase.Began)
                {
                    if (!overUI)
                    {
                        isDragging = true;
                        lastPointerPos = touch.position;
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                }
                else if (isDragging && touch.phase == TouchPhase.Moved && !overUI)
                {
                    if (currentMode == EditMode.Move)
                        TryMove(touch.position);
                    else if (currentMode == EditMode.Transform)
                        TransformDrag(touch.deltaPosition);

                    lastPointerPos = touch.position;
                }
            }
#endif
        }
    }

    private void UpdateReticle()
    {
        if (reticleImage == null) return;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        var hits = new List<ARRaycastHit>();
        bool foundPlane = raycastManager.Raycast(center, hits, TrackableType.PlaneWithinPolygon);

        reticleImage.color = foundPlane ? Color.green : Color.gray;

        if (foundPlane)
        {
            Pose p = hits[0].pose;
            Vector3 screenPt = mainCamera.WorldToScreenPoint(p.position);
            reticleImage.rectTransform.position = screenPt;
        }
        else
        {
            reticleImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    private void TryPlacePrefab(Vector2 screenPos)
    {
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            if (spawnedPrefabs.Count == 0)
            {
                GameObject toSpawn = prefab;
                if (!String.IsNullOrEmpty(modelKey))
                {
                    var found = ModelRepository.Instance.GetPrefab(modelKey);
                    if (found != null) toSpawn = found;
                }

                selectedObject = Instantiate(toSpawn, pose.position, pose.rotation);
                spawnedPrefabs.Add(selectedObject);

                editingUI.gameObject.SetActive(true);
                if (panelToggler != null && !panelToggler.IsOpen)
                    panelToggler.OpenPanel();

                HideAllPlanes();
                currentMode = EditMode.None;
            }
        }
    }

    private void TryMove(Vector2 screenPos)
    {
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            selectedObject.transform.position = pose.position;
            basePosition = pose.position;
            UpdateMoveSliders();
        }
    }

    private void TransformDrag(Vector2 delta)
    {
        if (Mathf.Abs(delta.y) > 0f)
        {
            float scaleDelta = delta.y * ((maxScale - minScale) / 200f);
            float current = selectedObject.transform.localScale.x;
            float next = Mathf.Clamp(current + scaleDelta, minScale, maxScale);
            selectedObject.transform.localScale = Vector3.one * next;
            baseScale = next;
            UpdateTransformSliders();
        }

        if (Mathf.Abs(delta.x) > 0f)
        {
            float rotDelta = delta.x * 0.2f;
            Vector3 e = selectedObject.transform.eulerAngles;
            float newY = (e.y + rotDelta) % 360f;
            selectedObject.transform.eulerAngles = new Vector3(e.x, newY, e.z);
            baseRotationY = newY;
            UpdateTransformSliders();
        }
    }

    private void SetMode(EditMode mode)
    {
        currentMode = mode;
        if (selectedObject == null)
        {
            sliderA.gameObject.SetActive(false);
            sliderB.gameObject.SetActive(false);
            return;
        }

        switch (mode)
        {
            case EditMode.Move:
                basePosition = selectedObject.transform.position;

                sliderA.gameObject.SetActive(true);
                sliderB.gameObject.SetActive(true);

                sliderA.minValue = basePosition.x - moveRange;
                sliderA.maxValue = basePosition.x + moveRange;
                sliderA.value = basePosition.x;

                sliderB.minValue = basePosition.z - moveRange;
                sliderB.maxValue = basePosition.z + moveRange;
                sliderB.value = basePosition.z;
                break;

            case EditMode.Transform:
                baseScale = selectedObject.transform.localScale.x;
                baseRotationY = selectedObject.transform.eulerAngles.y;

                sliderA.gameObject.SetActive(true);
                sliderB.gameObject.SetActive(true);

                sliderA.minValue = minScale;
                sliderA.maxValue = maxScale;
                sliderA.value = baseScale;

                sliderB.minValue = 0f;
                sliderB.maxValue = 360f;
                sliderB.value = baseRotationY;
                break;

            default:
                sliderA.gameObject.SetActive(false);
                sliderB.gameObject.SetActive(false);
                break;
        }
    }

    private void OnSliderAChanged(float newVal)
    {
        if (selectedObject == null) return;

        if (currentMode == EditMode.Move)
        {
            Vector3 pos = selectedObject.transform.position;
            pos.x = newVal;
            selectedObject.transform.position = pos;
        }
        else if (currentMode == EditMode.Transform)
        {
            selectedObject.transform.localScale = Vector3.one * newVal;
        }
    }

    private void OnSliderBChanged(float newVal)
    {
        if (selectedObject == null) return;

        if (currentMode == EditMode.Move)
        {
            Vector3 pos = selectedObject.transform.position;
            pos.z = newVal;
            selectedObject.transform.position = pos;
        }
        else if (currentMode == EditMode.Transform)
        {
            Vector3 e = selectedObject.transform.eulerAngles;
            selectedObject.transform.eulerAngles = new Vector3(e.x, newVal, e.z);
        }
    }

    private void ConfirmPlacement()
    {
        if (panelToggler != null && panelToggler.IsOpen)
            panelToggler.ClosePanel();
        else
            editingUI.gameObject.SetActive(false);

        currentMode = EditMode.None;

        sliderA.gameObject.SetActive(false);
        sliderB.gameObject.SetActive(false);
    }

    private void ResetPlacement()
    {
        if (spawnedPrefabs.Count > 0)
        {
            for (int i = 0; i < spawnedPrefabs.Count; i++)
            {
                if (spawnedPrefabs[i] != null)
#if UNITY_EDITOR
                    DestroyImmediate(spawnedPrefabs[i]);
#else
                    Destroy(spawnedPrefabs[i]);
#endif
            }
            spawnedPrefabs.Clear();
        }

        selectedObject = null;
        currentMode = EditMode.None;

        if (panelToggler != null)
            panelToggler.ClosePanel();
        else
            editingUI.gameObject.SetActive(false);

        sliderA.gameObject.SetActive(false);
        sliderB.gameObject.SetActive(false);

        ShowAllPlanes();
    }

    private void HideAllPlanes()
    {
        if (planeManager == null) return;

        planeManager.enabled = false;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);
    }

    private void ShowAllPlanes()
    {
        if (planeManager == null) return;

        planeManager.enabled = true;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(true);
    }

    private void UpdateMoveSliders()
    {
        if (currentMode != EditMode.Move) return;

        Vector3 pos = selectedObject.transform.position;
        sliderA.minValue = pos.x - moveRange;
        sliderA.maxValue = pos.x + moveRange;
        sliderA.value = pos.x;

        sliderB.minValue = pos.z - moveRange;
        sliderB.maxValue = pos.z + moveRange;
        sliderB.value = pos.z;
    }

    private void UpdateTransformSliders()
    {
        if (currentMode != EditMode.Transform) return;

        float currentScale = selectedObject.transform.localScale.x;
        sliderA.value = currentScale;

        float currentY = selectedObject.transform.eulerAngles.y;
        sliderB.value = currentY;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Returns true if the current pointer (mouse or touch) is over any UI element
    private bool IsPointerOverUI()
    {
#if UNITY_EDITOR
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
#else
        if (Input.touchCount > 0)
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
#endif
    }
}
