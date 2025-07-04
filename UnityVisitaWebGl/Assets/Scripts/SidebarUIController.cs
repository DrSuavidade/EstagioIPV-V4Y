using TMPro;
using UnityEngine;

public class SidebarUIController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject sidebarPanel; // Panel for the main sidebar
    public GameObject hotspotFormPanel; // Panel for the hotspot form (should be sibling to sidebarPanel)

    [Header("Form Fields")]
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;

    private Hotspot currentHotspot;
    public Image3DManager image3DManager;
    public Image3DSelector image3DSelector;
    public GameObject confirmPanel;

    public Model3DSelector model3DSelector;

    void Start()
    {
        // Start with the sidebar visible and the hotspot form hidden
        sidebarPanel.SetActive(true);
        hotspotFormPanel.SetActive(false);
    }

    // Called when clicking the "Add Hotspot" button on the sidebar
    public void OnAddHotspotButton()
    {
        // Hide the sidebar to allow clicking on the sphere for hotspot placement
        sidebarPanel.SetActive(false);
        // Start waiting for the hotspot placement click
        FindAnyObjectByType<HotspotManager>()
            .StartAddingHotspot();
    }

    // Displays the hotspot form, which should now be in front since the sidebar is hidden
    public void ShowHotspotForm(Hotspot hotspot)
    {
        // Show only the hotspot form panel
        hotspotFormPanel.SetActive(true);
        currentHotspot = hotspot;
        // Populate fields if needed
        titleInput.text = currentHotspot.title;
        descriptionInput.text = currentHotspot.description;
    }

    // Called when the user confirms the hotspot form
    public void OnConfirmHotspotForm()
    {
        if (currentHotspot != null)
        {
            currentHotspot.title = titleInput.text;
            currentHotspot.description = descriptionInput.text;
        }

        // Hide the hotspot form panel
        hotspotFormPanel.SetActive(false);
        model3DSelector.ShowModelSelectionForHotspot(currentHotspot);
        // Bring back the main sidebar panel
        sidebarPanel.SetActive(true);
    }

    public void OnAdd3DImageButton()
    {
        // Exibe o painel de seleção de imagens 3D
        image3DSelector.ShowSelectionPanel();
    }

    public void OnDeleteSphereButton()
    {
        // Supondo que você tenha uma referência ao CameraNavigator
        CameraNavigator camNav = Object.FindAnyObjectByType<CameraNavigator>();
        if (camNav != null)
        {
            camNav.RemoveCurrentSphere();
        }

        confirmPanel.SetActive(false);
    }
}
