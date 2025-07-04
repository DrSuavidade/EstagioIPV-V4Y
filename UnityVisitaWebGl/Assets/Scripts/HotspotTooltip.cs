using TMPro;
using UnityEngine;

public class HotspotTooltip : MonoBehaviour
{
    [Header("Tooltip Setup")]
    public GameObject tooltipPrefab;

    public float offsetAbove = 50f;
    public float offsetBelow = -50f;

    private GameObject tooltipInstance;
    private TextMeshProUGUI tooltipText;

    public Transform modelsHolder;

    void Start()
    {
        Transform tooltipCanvas = FindAnyObjectByType<Canvas>()?.transform;
        if (tooltipPrefab != null && tooltipCanvas != null)
        {
            // Instancia o tooltip como filho do Canvas
            tooltipInstance = Instantiate(tooltipPrefab, tooltipCanvas, false);
            tooltipInstance.SetActive(false);
            tooltipText = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
            tooltipInstance.transform.localScale = Vector3.one;
            Debug.Log("Tooltip instance created.");
        }
        else
        {
            Debug.LogError("Tooltip Prefab or Tooltip Canvas not assigned.");
        }
    }

    void OnMouseEnter()
    {
        Debug.Log("Mouse detected on hotspot.");
        if (tooltipInstance != null)
        {
            Hotspot hotspotData = GetComponent<Hotspot>();
            if (hotspotData != null)
            {
                tooltipText.text = hotspotData.title + "\n" + hotspotData.description;
            }
            else
            {
                tooltipText.text = "No Data";
            }
            tooltipInstance.SetActive(true);
            tooltipInstance.transform.SetAsLastSibling();

            // Posicionamento do tooltip (conversão de coordenadas)
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            RectTransform canvasRect = FindAnyObjectByType<Canvas>().transform as RectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                Camera.main,
                out localPoint
            );
            if (screenPos.y > Screen.height / 2)
                localPoint.y += offsetBelow;
            else
                localPoint.y += offsetAbove;
            tooltipInstance.GetComponent<RectTransform>().localPosition = localPoint;
            Debug.Log("Tooltip positioned at: " + localPoint);

            // Se um modelo 3D foi selecionado para o hotspot, instancie-o (se ainda não existir) no ModelsHolder
            if (hotspotData != null && hotspotData.attachedModelPrefab != null)
            {
                if (hotspotData.attachedModelInstance == null)
                {
                    // Instancia o modelo como filho do modelsHolder
                    GameObject modelInstance = Instantiate(
                        hotspotData.attachedModelPrefab,
                        modelsHolder
                    );
                    modelInstance.name = "AttachedModel_" + hotspotData.hotspotID;

                    // Calcula a posição: modelo a 0.4 unidades da câmera, na direção do hotspot
                    Vector3 camPos = Camera.main.transform.position;
                    Vector3 hotspotPos = transform.position;
                    Vector3 dirFromCamToHotspot = (hotspotPos - camPos).normalized;
                    float desiredDistance = 0.4f;
                    modelInstance.transform.position =
                        camPos + dirFromCamToHotspot * desiredDistance;

                    modelInstance.transform.rotation = Quaternion.identity;
                    // Ajusta a escala para que ele fique visível (ajuste conforme necessário)
                    modelInstance.transform.localScale = Vector3.one * 0.5f;

                    // Adiciona um script de rotação se desejado
                    if (modelInstance.GetComponent<Rotator>() == null)
                    {
                        modelInstance.AddComponent<Rotator>();
                    }
                    hotspotData.attachedModelInstance = modelInstance;
                }
                else
                {
                    // Se já existe, apenas atualize sua posição e garanta que esteja ativo
                    hotspotData.attachedModelInstance.SetActive(true);
                    Vector3 camPos = Camera.main.transform.position;
                    Vector3 hotspotPos = transform.position;
                    Vector3 dirFromCamToHotspot = (hotspotPos - camPos).normalized;
                    float desiredDistance = 0.4f;
                    hotspotData.attachedModelInstance.transform.position =
                        camPos + dirFromCamToHotspot * desiredDistance;
                }
            }
        }
    }

    void OnMouseExit()
    {
        if (tooltipInstance != null)
        {
            tooltipInstance.SetActive(false);
            // Também esconde o modelo, se existir
            Hotspot hotspotData = GetComponent<Hotspot>();
            if (hotspotData != null && hotspotData.attachedModelInstance != null)
            {
                hotspotData.attachedModelInstance.SetActive(false);
            }
        }
    }
}
