using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyStatus = new();

    public GameObject playerLobbyPrefab;

    private void Awake()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        ClientConectedServerRpc(clientId);
        UpdateLobbyUI();
    }
    [ServerRpc]
    private void ClientConectedServerRpc(ulong playerId)
    {
        playerReadyStatus[playerId] = false;

        // Copiamos la lista para evitar modificarla durante la iteración
        var snapshot = new List<KeyValuePair<ulong, bool>>(playerReadyStatus);

        foreach (var player in snapshot)
        {
            ClientConectedClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void ClientConectedClientRpc(ulong playerId, bool isReady)
    {
        Debug.Log($"ClientRPC recibido -> playerId: {playerId}, ready: {isReady}");
        playerReadyStatus[playerId] = isReady;
        UpdateLobbyUI();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        UpdateLobbyUI();
    }

    public void SetPlayerReady()
    {
        if (IsClient)
        {
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var controller = player.GetComponent<PlayerNetworkController>();
            controller.IsReady.Value = true;

            SubmitReadyServerRpc(player.OwnerClientId, true);
        }
    }
    [ServerRpc]
    private void SubmitReadyServerRpc(ulong playerId, bool isReady)
    {
        playerReadyStatus[playerId] = isReady;
        CheckReadyState();
        SubmitReadyClientRpc(playerId,isReady);
        UpdateLobbyUI();
    }
    [ClientRpc]
    private void SubmitReadyClientRpc(ulong playerId, bool isReady)
    {
        playerReadyStatus[playerId] = isReady;
        UpdateLobbyUI();
    }

    private void CheckReadyState()
    {
        int readyCount = 0;
        int totalCount = NetworkManager.Singleton.ConnectedClients.Count;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var obj = client.PlayerObject;
            var controller = obj.GetComponent<PlayerNetworkController>();
            if (controller != null && controller.IsReady.Value)
            {
                readyCount++;
            }
        }

        if (readyCount == totalCount)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }

    private void UpdateLobbyUI()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Button>() != null)
                continue;
            Destroy(child.gameObject);
        }

        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            var obj = player.PlayerObject;
            var controller = obj.GetComponent<PlayerNetworkController>();
            string name = controller != null ? controller.UniqueId.Value.ToString() : $"Player {player.ClientId}";

            GameObject playerLobby = Instantiate(playerLobbyPrefab, transform);
            playerLobby.GetComponent<PlayerLobbyManager>().SetInfo(name, controller.IsReady.Value);
        }
    }
}