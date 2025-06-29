using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    /// <summary>
    /// Chama esta função no OnClick do botão, passando o nome da cena.
    /// </summary>
    /// <param name="sceneName">Nome exato da cena (conforme Build Settings)</param>
    public void LoadSceneByName(string sceneName)
    {
        // Verifica se a cena está listada em File → Build Settings
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Cena '{sceneName}' não encontrada ou não adicionada ao Build Settings."
            );
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void OnBackFromScan()
    {
        // 1) Clear any previous marker/model so we force a fresh gate next time
        ScanSession.ActiveMarker = null;
        ScanSession.ActiveModel  = null;

        // 2) Now navigate home
        LoadSceneByName("HomeScreen");  // or however you do it
    }
}
