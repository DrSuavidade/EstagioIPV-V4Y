using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAvailabilityChecker : MonoBehaviour
{
    [Header("UI Popup")]
    [Tooltip("CanvasGroup of a panel that says “Device Not Supported”")]
    [SerializeField] private CanvasGroup unsupportedPopup;

    private void Awake()
    {
        // Make sure ARSession is not started until after we check
        ARSession.stateChanged += OnARSessionStateChanged;
        StartCoroutine(CheckAvailability());
    }

    private IEnumerator CheckAvailability()
    {
        // Ask ARFoundation whether ARCore is supported/installed
        yield return ARSession.CheckAvailability();

        if (ARSession.state == ARSessionState.Unsupported)
        {
            ShowUnsupported();
        }
        else if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
            if (ARSession.state != ARSessionState.Ready)
                ShowUnsupported();
        }
        // else ARSessionState.Ready or SessionTracking => continue to start ARSession normally
    }

    private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        // if it ever goes unsupported mid-run, show the popup
        if (args.state == ARSessionState.Unsupported)
            ShowUnsupported();
    }

    private void ShowUnsupported()
    {
        // pop the panel up
        unsupportedPopup.alpha          = 1f;
        unsupportedPopup.interactable   = true;
        unsupportedPopup.blocksRaycasts = true;

        // disable ARSession so nothing else tries to run
        var session = FindFirstObjectByType<ARSession>();
        if (session != null) session.enabled = false;
    }
}
