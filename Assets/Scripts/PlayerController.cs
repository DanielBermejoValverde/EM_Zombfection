﻿using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    private TextMeshProUGUI coinText;

    [Header("Stats")]
    public int CoinsCollected = 0;

    Object lockMoneda = new Object();

    [Header("Character settings")]
    public bool isZombie = false;
    public string uniqueID;

    public NetworkVariable<ulong> clientID;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float zombieSpeedModifier = 0.8f;
    public Animator animator;
    public Transform cameraTransform;

    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        // Buscar el objeto "CanvasPlayer" en la escena
        GameObject canvas = GameObject.Find("CanvasPlayer");

        if (canvas != null)
        {
            Transform panel = canvas.transform.Find("PanelHud");
            if (panel != null)
            {
                Transform coinTextTransform = panel.Find("CoinsValue");
                if (coinTextTransform != null)
                {
                    coinText = coinTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        UpdateCoinUIClientRpc(0);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector2 input = new Vector2(horizontalInput, verticalInput);
        if (input != Vector2.zero && cameraTransform != null)
        {
            Vector3 moveDirection = (cameraTransform.forward * input.y + cameraTransform.right * input.x).normalized;
            moveDirection.y = 0f;

            SubmitMoveDirectionServerRpc(moveDirection);
        }

        HandleAnimations(); // Animaciones siguen locales
    }

    [ServerRpc]
    void SubmitMoveDirectionServerRpc(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.fixedDeltaTime);

        float adjustedSpeed = isZombie ? moveSpeed * zombieSpeedModifier : moveSpeed;
        transform.Translate(moveDirection * adjustedSpeed * Time.fixedDeltaTime, Space.World);
    }

    void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
    }

    public void CoinCollected()
    {

        if (!isZombie && IsServer) // Solo los humanos pueden recoger monedas, en solo el server ejecutara la accion de mandar al levelmanager que se ha recogido una moneda 
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
        lock (lockMoneda)
        {
            CoinsCollected += coinCollected;
        }
        if (coinText != null)
        {
            coinText.text = $"{CoinsCollected}";
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            clientID.Value = OwnerClientId;
        }

        if (IsOwner)
        {
            transform.SetParent(NetworkManager.Singleton.LocalClient.PlayerObject.transform);

            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                CameraController cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    cameraController.player = this.gameObject.transform;
                }

                this.cameraTransform = mainCamera.transform;
                if(GameObject.Find("CanvasUI").GetComponent<MenuManager>().isGame)
                    this.enabled = true;
            }
            else
            {
                Debug.LogError("No se encontró la cámara principal.");
            }
        }
    }
}
