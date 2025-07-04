using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraNavigator : MonoBehaviour
{
    [Header("Botões de Navegação")]
    public Button nextButton;
    public Button prevButton;
    public Button deleteButton;

    [Header("Configuração das Esferas")]
    // Pai onde todas as esferas criadas (incluindo a inicial) são organizadas
    public Transform spheresParent;

    // Velocidade de transição da câmera (opcional, para suavizar o movimento)
    public float transitionSpeed = 5f;

    private Transform[] sphereTransforms;
    private int currentIndex = 0;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        UpdateSpheresList();
        // Posiciona a câmera na esfera inicial, se houver pelo menos uma
        if (sphereTransforms.Length > 0)
            cam.transform.position = sphereTransforms[currentIndex].position;
        UpdateNavigationButtons();
    }

    // Atualiza a lista de esferas a partir dos filhos do spheresParent
    public void UpdateSpheresList()
    {
        int count = spheresParent.childCount;
        sphereTransforms = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            sphereTransforms[i] = spheresParent.GetChild(i);
        }
    }

    // Método chamado pelo botão "Próximo"
    public void OnNextButton()
    {
        if (sphereTransforms == null || sphereTransforms.Length == 0)
            return;
        if (currentIndex < sphereTransforms.Length - 1)
        {
            currentIndex++;
            MoveCameraToSphere(sphereTransforms[currentIndex].position);
        }
        UpdateNavigationButtons();
    }

    // Método chamado pelo botão "Anterior"
    public void OnPrevButton()
    {
        if (sphereTransforms == null || sphereTransforms.Length == 0)
            return;
        if (currentIndex > 0)
        {
            currentIndex--;
            MoveCameraToSphere(sphereTransforms[currentIndex].position);
        }
        UpdateNavigationButtons();
    }

    // Move a câmera para a posição da esfera; pode usar Lerp para suavizar
    private void MoveCameraToSphere(Vector3 targetPos)
    {
        StopAllCoroutines();
        StartCoroutine(SmoothMove(cam.transform.position, targetPos, transitionSpeed));
    }

    // Coroutine para mover a câmera suavemente
    IEnumerator SmoothMove(Vector3 start, Vector3 end, float speed)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            cam.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        cam.transform.position = end;
    }

    // Atualiza a interatividade dos botões com base no índice atual
    public void UpdateNavigationButtons()
    {
        if (sphereTransforms == null || sphereTransforms.Length == 0)
        {
            nextButton.interactable = false;
            prevButton.interactable = false;
        }
        else
        {
            nextButton.interactable = (currentIndex < sphereTransforms.Length - 1);
            prevButton.interactable = (currentIndex > 0);
        }

        // Supondo que você tenha um botão de delete (deleteButton)
        // Desativa se for a primeira esfera
        if (deleteButton != null)
        {
            deleteButton.interactable = (currentIndex > 0);
        }
    }

    public void RemoveCurrentSphere()
    {
        // Não permite remover a primeira esfera
        if (currentIndex == 0)
        {
            Debug.Log("Não é possível remover a primeira esfera.");
            return;
        }

        // Obtém a esfera a remover
        Transform sphereToRemove = sphereTransforms[currentIndex];

        // Remove a esfera da Hierarquia
        Destroy(sphereToRemove.gameObject);
        Debug.Log("Esfera removida: " + sphereToRemove.name);

        // Inicia uma coroutine para atualizar a lista e reposicionar a câmera
        StartCoroutine(UpdateAfterDeletion());
    }

    private IEnumerator UpdateAfterDeletion()
    {
        // Aguarda um frame para que a Unity remova efetivamente o objeto da Hierarquia
        yield return null;

        // Atualiza a lista de esferas
        UpdateSpheresList();

        // Ajusta o índice atual se necessário
        if (currentIndex >= sphereTransforms.Length)
        {
            currentIndex = sphereTransforms.Length - 1;
        }

        // Reposiciona a câmera, se houver alguma esfera restante
        if (sphereTransforms.Length > 0)
        {
            MoveCameraToSphere(sphereTransforms[currentIndex].position);
        }
        else
        {
            Debug.Log("Nenhuma esfera restante.");
        }

        // Atualiza o estado dos botões
        UpdateNavigationButtons();
    }
}
