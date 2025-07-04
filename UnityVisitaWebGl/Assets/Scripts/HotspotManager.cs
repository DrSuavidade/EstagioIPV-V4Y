using UnityEngine;

public class HotspotManager : MonoBehaviour
{
    public GameObject hotspotPrefab; // Prefab do hotspot com o componente Hotspot
    public Transform hotspotParent; // Objeto vazio para organizar os hotspots
    public SidebarUIController sidebarUI; // Referência ao controlador da sidebar

    // Defina o raio da sua esfera (ajuste conforme sua escala)
    public float sphereRadius = 50f;

    private bool waitingForClick = false;

    void Update()
    {
        if (waitingForClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse button down detected.");

                // Obtém o raio baseado na posição do clique
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

                // Como a câmera está no centro da esfera, o ponto na superfície é:
                Vector3 hitPoint =
                    Camera.main.transform.position + ray.direction.normalized * sphereRadius;
                Debug.Log("Calculated hit point: " + hitPoint);

                // Instancia o hotspot no ponto calculado
                GameObject newHotspot = Instantiate(
                    hotspotPrefab,
                    hitPoint,
                    Quaternion.identity,
                    hotspotParent
                );
                Debug.Log("Hotspot prefab instantiated.");

                // Ajusta a rotação para que o hotspot "olhe" para a câmera
                newHotspot.transform.LookAt(Camera.main.transform.position);
                newHotspot.transform.Rotate(0, 180f, 0); // Inverte se necessário

                waitingForClick = false;

                // Obtém o componente Hotspot do prefab
                Hotspot hsComponent = newHotspot.GetComponent<Hotspot>();
                if (hsComponent == null)
                {
                    Debug.LogError("O prefab instanciado não possui o componente Hotspot!");
                }
                else
                {
                    Debug.Log("Componente Hotspot encontrado, abrindo formulário.");
                    sidebarUI.ShowHotspotForm(hsComponent);
                }
            }
        }
    }

    // Este método inicia o modo de adição de hotspot
    public void StartAddingHotspot()
    {
        waitingForClick = true;
    }
}
