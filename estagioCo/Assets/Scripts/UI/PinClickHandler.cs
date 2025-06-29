using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PinClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Pin Scale on Select")]
    [SerializeField] private float selectedScale = 1.5f;

    private Vector3 originalScale;
    private string  streetName;
    private float   areaSize;
    private Sprite  pinSprite;

    private static PinClickHandler currentSelected;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Setup(string street, float area, Sprite sprite)
    {
        streetName   = street;
        areaSize     = area;
        pinSprite    = sprite;

        // also update the little icon you see on the map
        var img = GetComponent<Image>();
        if (img != null && pinSprite != null)
            img.sprite = pinSprite;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (currentSelected == this)
        {
            Deselect();
            MapUIController.Instance.Hide();
            currentSelected = null;
            return;
        }
        if (currentSelected != null)
            currentSelected.Deselect();

        currentSelected = this;
        transform.localScale = originalScale * selectedScale;

        MapUIController.Instance.Show(streetName, areaSize, pinSprite);
    }

    private void Deselect()
    {
        transform.localScale = originalScale;
    }
}
