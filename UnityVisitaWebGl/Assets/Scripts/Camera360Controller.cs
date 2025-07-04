using UnityEngine;

public class Camera360Controller : MonoBehaviour
{
    [Header("Configurações de Rotação")]
    public float rotationSpeed = 50f; // Velocidade de rotação
    public bool holdRightMouseButton = true; // Se verdadeiro, gira somente ao segurar o botão direito do mouse

    private float yaw = 0f;   // Rotação em torno do eixo Y
    private float pitch = 0f; // Rotação em torno do eixo X

    void Start()
    {
        // Captura os valores iniciais de rotação, caso já estejam configurados
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        // Verifica se o usuário está pressionando o botão direito do mouse (ou se não precisamos checar)
        if (!holdRightMouseButton || Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Leitura do movimento do mouse
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Atualiza os ângulos de rotação
            yaw   += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;

            // (Opcional) Limita a rotação vertical para evitar "girar de ponta-cabeça"
            // pitch = Mathf.Clamp(pitch, -80f, 80f);

            // Aplica a rotação ao Transform da câmera
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
    }
}
