using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    private TextMeshProUGUI coinText;

    [Header("Stats")]
    public int CoinsCollected = 0;

    [Header("Character settings")]
    public bool isZombie = false; // Añadir una propiedad para el estado del jugador
    public string uniqueID; // Añadir una propiedad para el identificador único

    public NetworkVariable<ulong> clientID;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;           // Velocidad de movimiento
    public float zombieSpeedModifier = 0.8f; // Modificador de velocidad para zombies
    public Animator animator;              // Referencia al Animator
    public Transform cameraTransform;      // Referencia a la cámara

    private float horizontalInput;         // Entrada horizontal (A/D o flechas)
    private float verticalInput;           // Entrada vertical (W/S o flechas)

    void Start()
    {
        CoinsCollected = 0;
        // Buscar el objeto "CanvasPlayer" en la escena
        GameObject canvas = GameObject.Find("CanvasPlayer");

        if (canvas != null)
        {
            Debug.Log("Canvas encontrado");

            // Buscar el Panel dentro del CanvasHud
            Transform panel = canvas.transform.Find("PanelHud");
            if (panel != null)
            {
                // Buscar el TextMeshProUGUI llamado "CoinsValue" dentro del Panel
                Transform coinTextTransform = panel.Find("CoinsValue");
                if (coinTextTransform != null)
                {
                    coinText = coinTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        UpdateCoinUIClientRpc(0);
    }

    void Update()
    {
        // Leer entrada del teclado
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Mover el jugador
        MovePlayer();

        // Manejar las animaciones del jugador
        HandleAnimations();
    }

    void MovePlayer()
    {
        if (cameraTransform == null) { return; }

        // Calcular la dirección de movimiento en relación a la cámara
        Vector3 moveDirection = (cameraTransform.forward * verticalInput + cameraTransform.right * horizontalInput).normalized;
        moveDirection.y = 0f; // Asegurarnos de que el movimiento es horizontal (sin componente Y)

        // Mover el jugador usando el Transform
        if (moveDirection != Vector3.zero)
        {
            // Calcular la rotación en Y basada en la dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime);

            // Ajustar la velocidad si es zombie
            float adjustedSpeed = isZombie ? moveSpeed * zombieSpeedModifier : moveSpeed;

            // Mover al jugador en la dirección deseada
            transform.Translate(moveDirection * adjustedSpeed * Time.deltaTime, Space.World);
        }
    }

    void HandleAnimations()
    {
        // Animaciones basadas en la dirección del movimiento
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));  // Controla el movimiento (caminar/correr)
    }

    public void CoinCollected()
    {

        if (!isZombie && IsServer) // Solo los humanos pueden recoger monedas, en teoria solo el 
        {
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            if(levelManager != null)
            {
                levelManager.CollectCoin();
            }
            UpdateCoinUIClientRpc(1);
        }
    }
    [ClientRpc]
    void UpdateCoinUIClientRpc(int coinCollected)
    {   
        CoinsCollected += coinCollected;
        if (coinText != null)
        {
            coinText.text = $"{CoinsCollected}";
        }
    }
    public override void OnNetworkSpawn(){
        if(IsServer){
            clientID.Value = OwnerClientId;
        }
        if (IsOwner)
        {
        transform.SetParent(NetworkManager.Singleton.LocalClient.PlayerObject.transform);
        // Obtener la referencia a la c�mara principal
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Obtener el script CameraController de la c�mara principal
            CameraController cameraController = mainCamera.GetComponent<CameraController>();

            if (cameraController != null)
            {
                Debug.Log($"CameraController encontrado en la c�mara principal.");
                // Asignar el jugador al script CameraController
                cameraController.player = this.gameObject.transform;
            }

            Debug.Log($"C�mara principal encontrada en {mainCamera}");
            // Obtener el componente PlayerController del jugador instanciado
            // Asignar el transform de la c�mara al PlayerController
                Debug.Log($"PlayerController encontrado en el jugador instanciado.");
                this.enabled = true;
                this.cameraTransform = mainCamera.transform;
                //this.uniqueID = uniqueIdGenerator.GenerateUniqueID(); // Generar un identificador �nico

        }
        else
        {
            Debug.LogError("No se encontr� la c�mara principal.");
        }
        
        
        }
    }
}

