using UnityEngine;

public class Image3DManager : MonoBehaviour
{
    // Prefab da esfera base (com o collider e possivelmente com um componente para rotação se necessário)
    public GameObject spherePrefab;

    // Material base que usa o shader insideOut (sem textura definida)
    public Material baseInsideOutMaterial;

    // Posição inicial para as esferas (a primeira já está em (0,0,0))
    public Vector3 initialPosition = Vector3.zero;

    // Distância em x entre cada esfera nova
    public float spacing = 5f;

    // Pai para organizar as esferas na hierarquia
    public Transform spheresParent;

    /// <summary>
    /// Cria uma nova esfera com a textura fornecida.
    /// </summary>
    /// <param name="newTexture">A nova imagem 3D a aplicar.</param>
    public void CreateNewImageSphere(Texture2D newTexture)
    {
        Material newMaterial = new Material(baseInsideOutMaterial);
        newMaterial.mainTexture = newTexture;

        // Use o número de esferas existentes
        int existingCount = spheresParent.childCount;
        Vector3 newPosition = initialPosition + new Vector3((existingCount + 1) * spacing, 0f, 0f);

        GameObject newSphere = Instantiate(
            spherePrefab,
            newPosition,
            Quaternion.identity,
            spheresParent
        );

        // Atribui o novo material à esfera
        MeshRenderer renderer = newSphere.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = newMaterial;
        }
        else
        {
            Debug.LogWarning("A nova esfera não possui MeshRenderer.");
        }

        Debug.Log("Nova esfera de imagem criada em: " + newPosition);

        // Atualiza a lista de esferas no CameraNavigator, se necessário
        CameraNavigator camNav = Object.FindAnyObjectByType<CameraNavigator>();
        if (camNav != null)
        {
            camNav.UpdateSpheresList();
            camNav.UpdateNavigationButtons();
        }
    }
}
