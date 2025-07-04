using UnityEngine;
using UnityEngine.UI;

public class Model3DSelector : MonoBehaviour
{
    [Header("Painel de Seleção de Modelos 3D")]
    public GameObject modelSelectionPanel; // Painel que contém os botões de preview

    [Header("Modelos Disponíveis")]
    // Array com os prefabs dos modelos 3D disponíveis
    public GameObject[] availableModelPrefabs;

    // Hotspot atual para o qual o modelo será anexado
    private Hotspot currentHotspot;

    /// <summary>
    /// Chama este método para exibir o painel de seleção para o hotspot dado.
    /// </summary>
    public void ShowModelSelectionForHotspot(Hotspot hotspot)
    {
        currentHotspot = hotspot;
        modelSelectionPanel.SetActive(true);
    }

    /// <summary>
    /// Chamado pelos botões de seleção de modelo.
    /// O parâmetro index corresponde à posição no array availableModelPrefabs.
    /// </summary>
    public void OnSelectModel(int index)
    {
        if (index >= 0 && index < availableModelPrefabs.Length && currentHotspot != null)
        {
            currentHotspot.attachedModelPrefab = availableModelPrefabs[index];
            Debug.Log(
                "Modelo selecionado: "
                    + availableModelPrefabs[index].name
                    + " para o hotspot "
                    + currentHotspot.name
            );
        }
        modelSelectionPanel.SetActive(false);
    }
}
