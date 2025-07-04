using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SearchDropdownToggle : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField]
    private Button mainButton; // “Procurar”

    [SerializeField]
    private Button typeButton; // “Tipo de Imóvel”

    [Header("Panels")]
    [SerializeField]
    private CanvasGroup dropdown; // painel principal

    [SerializeField]
    private CanvasGroup typeSubmenu; // submenu de tipos

    [SerializeField]
    private CanvasGroup blocker; // painel full-screen invisível

    [Header("Fade Settings")]
    [SerializeField]
    private float fadeDuration = 0.15f;

    private bool isDropdownOpen = false;
    private bool isSubmenuOpen = false;

    private Coroutine dropdownFade;
    private Coroutine submenuFade;
    private Coroutine blockerFade;

    void Awake()
    {
        // Inicializa tudo fechado
        SetInstant(dropdown, 0f, false);
        SetInstant(typeSubmenu, 0f, false);
        SetInstant(blocker, 0f, false);

        // Wiring dos botões
        mainButton.onClick.AddListener(ToggleDropdown);
        typeButton.onClick.AddListener(ToggleSubmenu);

        // Se o blocker tiver um Button, fecha tudo ao clicar nele
        if (blocker.TryGetComponent<Button>(out var btn))
            btn.onClick.AddListener(CloseAll);
    }

    public void ToggleDropdown()
    {
        isDropdownOpen = !isDropdownOpen;
        // fechar submenu se estiver aberto
        if (isSubmenuOpen)
            ToggleSubmenu();

        // fade dropdown e blocker
        Fade(ref dropdownFade, dropdown, isDropdownOpen);
        Fade(ref blockerFade, blocker, isDropdownOpen);
    }

    public void ToggleSubmenu()
    {
        if (!isDropdownOpen)
            return;
        isSubmenuOpen = !isSubmenuOpen;
        Fade(ref submenuFade, typeSubmenu, isSubmenuOpen);
    }

    public void CloseAll()
    {
        if (isDropdownOpen)
            ToggleDropdown();
    }

    // Faz o fade-in/fade-out do canvas group
    private void Fade(ref Coroutine routine, CanvasGroup cg, bool show)
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(FadeRoutine(cg, show));
    }

    private IEnumerator FadeRoutine(CanvasGroup cg, bool show)
    {
        // habilita raycast e interactivity no início do fade-in
        if (show)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        float start = cg.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }
        cg.alpha = end;

        // desabilita raycast e interactivity no final do fade-out
        if (!show)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    // Ajusta imediatamente sem fade
    private void SetInstant(CanvasGroup cg, float alpha, bool interactable)
    {
        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }
}
