using UnityEngine;
using UnityEngine.UI;

public class Image3DSelector : MonoBehaviour
{
    [Header("UI")]
    // O painel que contém os previews
    public GameObject selectionPanel;

    [Header("Configuração")]
    // Array com as texturas disponíveis (configure no Inspector)
    public Texture2D[] availableTextures;

    // Referência ao Image3DManager para criar a esfera com a textura escolhida
    public Image3DManager image3DManager;

    /// <summary>
    /// Método chamado quando um dos botões de pré-visualização é clicado.
    /// O índice indica qual textura foi escolhida.
    /// </summary>
    /// <param name="index">Índice da textura no array availableTextures</param>
    public void OnSelectTexture(int index)
    {
        if (index >= 0 && index < availableTextures.Length)
        {
            Texture2D selectedTexture = availableTextures[index];
            Debug.Log("Textura selecionada: " + selectedTexture.name);

            // Cria a nova esfera com a textura escolhida
            image3DManager.CreateNewImageSphere(selectedTexture);

            // Esconde o painel de seleção
            selectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Índice de textura fora dos limites!");
        }
    }

    /// <summary>
    /// Exibe o painel de seleção.
    /// </summary>
    public void ShowSelectionPanel()
    {
        selectionPanel.SetActive(true);
    }
}
