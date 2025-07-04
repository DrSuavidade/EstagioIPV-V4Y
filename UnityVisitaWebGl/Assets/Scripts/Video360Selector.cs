using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Video360Selector : MonoBehaviour
{
    [Header("UI")]
    public GameObject videoSelectionPanel; // Painel de seleção de vídeo

    [Header("Configuração")]
    // Array com os nomes dos arquivos de vídeo armazenados em StreamingAssets (ex.: "video1.mp4")
    public string[] availableVideoFileNames;

    // Referência ao Video360Manager para criar a esfera com o vídeo escolhido
    public Video360Manager video360Manager;

    /// <summary>
    /// Método chamado quando um dos botões de pré-visualização de vídeo é clicado.
    /// O índice indica qual vídeo foi escolhido.
    /// </summary>
    public void OnSelectVideo(int index)
    {
        if (index >= 0 && index < availableVideoFileNames.Length)
        {
            // Constrói a URL completa para o vídeo
            string videoFileName = availableVideoFileNames[index];
            string videoURL = System.IO.Path.Combine(
                Application.streamingAssetsPath,
                videoFileName
            );
            Debug.Log("Vídeo selecionado: " + videoURL);

            // Cria a nova esfera configurada para vídeo 360 com o vídeo selecionado
            video360Manager.CreateNewVideoSphere(videoURL);

            // Esconde o painel de seleção de vídeo
            videoSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Índice de vídeo fora dos limites!");
        }
    }

    /// <summary>
    /// Exibe o painel de seleção de vídeos.
    /// </summary>
    public void ShowSelectionPanel()
    {
        videoSelectionPanel.SetActive(true);
    }
}
