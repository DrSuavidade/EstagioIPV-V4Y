using UnityEngine;
using UnityEngine.Video;

public class Video360Manager : MonoBehaviour
{
    public GameObject spherePrefab;
    public Material baseInsideOutMaterial;
    public Vector3 initialPosition = Vector3.zero;
    public float spacing = 5f;
    public Transform spheresParent;

    public void CreateNewVideoSphere(string videoURL)
    {
        Material newMaterial = new Material(baseInsideOutMaterial);
        RenderTexture videoRenderTexture = new RenderTexture(1024, 512, 0);
        newMaterial.mainTexture = videoRenderTexture;

        // Use o número de esferas existentes para calcular a nova posição
        int existingCount = spheresParent.childCount;
        Vector3 newPosition = initialPosition + new Vector3((existingCount + 1) * spacing, 0f, 0f);

        GameObject newSphere = Instantiate(
            spherePrefab,
            newPosition,
            Quaternion.identity,
            spheresParent
        );

        MeshRenderer renderer = newSphere.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = newMaterial;
        }
        else
        {
            Debug.LogWarning("A nova esfera não possui MeshRenderer.");
        }

        // Configura o VideoPlayer
        VideoPlayer vp = newSphere.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = videoRenderTexture;
        vp.url = videoURL;
        vp.waitForFirstFrame = true;
        vp.prepareCompleted += (source) =>
        {
            Debug.Log("Vídeo preparado, iniciando reprodução.");
            vp.Play();
        };
        vp.Prepare();

        Debug.Log("Nova esfera de vídeo criada em: " + newPosition);

        CameraNavigator camNav = Object.FindAnyObjectByType<CameraNavigator>();
        if (camNav != null)
        {
            camNav.UpdateSpheresList();
            camNav.UpdateNavigationButtons();
        }
    }
}
