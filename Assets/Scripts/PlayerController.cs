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
    [SerializeField]
    public float moveSpeed = 5f;           // Velocidad de movimiento
    public float zombieSpeedModifier = 0.8f; // Modificador de velocidad para zombies
    public Animator animator;              // Referencia al Animator
    public Transform cameraTransform;      // Referencia a la cámara

    private float horizontalInput;         // Entrada horizontal (A/D o flechas)
    private float verticalInput;           // Entrada vertical (W/S o flechas)

    [SerializeField]
    private NetworkVariable<float> forwardBackPosition = new NetworkVariable<float>();
    [SerializeField]
    private NetworkVariable<float> leftRightPosition = new NetworkVariable<float>();

    //cliente;
    private float oldforwardBackPosition;
    private float oldleftRightPosition;

    void Start()
    {
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

        UpdateCoinUI();
    }

    void Update()
    {
        // Leer entrada del teclado
        //horizontalInput = Input.GetAxis("Horizontal");
        //verticalInput = Input.GetAxis("Vertical");

        // Mover el jugador
        //MovePlayer();
        if (IsServer)
        {
            UpdateServer();
        }
        if(IsClient && IsOwner)
        {
            UpdateClient();
        }


        // Manejar las animaciones del jugador
        HandleAnimations();
    }
    private void UpdateServer()
    {
        Debug.Log("estoy actualizando");
        transform.position = new Vector3(transform.position.x + leftRightPosition.Value, transform.position.y, transform.position.z + forwardBackPosition.Value);
    }
    private void UpdateClient()
    {
        
        float fowardBackward = 0;
        float leftRight = 0;
        if (Input.GetKey(KeyCode.W))
        {
            
            fowardBackward += moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            fowardBackward -= moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            leftRight += moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            leftRight -= moveSpeed;
        }
        if(oldforwardBackPosition != fowardBackward || oldleftRightPosition != leftRight)
        {
            oldforwardBackPosition = fowardBackward;
            oldleftRightPosition = leftRight;
            Debug.Log(fowardBackward + leftRight);
            //Actualizamos el server
            UpdateClientPositionServerRpc( fowardBackward,  leftRight);
        }
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpc(float fowardBackward, float leftRight)
    {
        
        forwardBackPosition.Value = fowardBackward;
        leftRightPosition.Value = leftRight;
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
        if (!isZombie) // Solo los humanos pueden recoger monedas
        {
            this.CoinsCollected++;
            UpdateCoinUI();
        }
    }

    void UpdateCoinUI()
    {
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

