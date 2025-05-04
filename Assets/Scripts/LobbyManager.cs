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
        foreach (var key in new List<ulong>(playerReadyStatus.Keys))
            {
                var value = playerReadyStatus[key];
                ClientConectedClientRpc(key, value);
            }
    }

    [ClientRpc]
    private void ClientConectedClientRpc(ulong playerId,bool isReady)
    {
        playerReadyStatus[playerId] = isReady;
        UpdateLobbyUI();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerReadyStatus.Remove(clientId);
        OnClientDisconnectedClientRpc(clientId);
        UpdateLobbyUI();
    }
    [ClientRpc]
    private void OnClientDisconnectedClientRpc(ulong clientId)
    {
        playerReadyStatus.Remove(clientId);
        UpdateLobbyUI();
    }

    public void SetPlayerReady()
    {
            SubmitReadyServerRpc(NetworkManager.Singleton.LocalClientId, true);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SubmitReadyServerRpc(ulong playerId, bool isReady)
    {
        if(!IsServer)return;
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
        foreach (var ready in playerReadyStatus.Values)
        {
            if (ready) readyCount++;
        }

        if (readyCount == playerReadyStatus.Count)
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

        foreach (var player in playerReadyStatus)
        {
            GameObject playerLobby = Instantiate(playerLobbyPrefab, transform);
            playerLobby.GetComponent<PlayerLobbyManager>().SetInfo("Player " + player.Key, player.Value);
        }
    }
}