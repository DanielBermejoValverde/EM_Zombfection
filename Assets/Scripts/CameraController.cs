using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;            // Referencia al jugador
    public Vector3 offset = new Vector3(0f, 2f, -5f);  // Desplazamiento desde el jugador
    public float rotationSpeed = 5f;    // Velocidad de rotaci�n
    public float pitchSpeed = 2f;       // Velocidad de inclinaci�n (eje Y)
    public float minPitch = -20f;       // �ngulo m�nimo de inclinaci�n
    public float maxPitch = 50f;        // �ngulo m�ximo de inclinaci�n

    private float yaw = 0f;             // Rotaci�n alrededor del eje Y
    private float pitch = 2f;           // Inclinaci�n hacia arriba/abajo (eje X)

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is missing.");
            return;
        }

        HandleCameraRotation();
        UpdateCameraPosition();
    }

    private void HandleCameraRotation()
    {
        // Obtener la entrada del rat�n para la rotaci�n de la c�mara
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * pitchSpeed;

        // Modificar los �ngulos de rotaci�n (yaw y pitch)
        yaw += mouseX;
        pitch -= mouseY;

        // Limitar la inclinaci�n de la c�mara
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void UpdateCameraPosition()
    {
        // Calcular la nueva direcci�n de la c�mara
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 rotatedOffset = rotation * offset;

        // Posicionar la c�mara en funci�n del jugador y el nuevo offset
        transform.position = player.position + rotatedOffset;

        // Siempre mirar al jugador
        transform.LookAt(player);
    }
}
