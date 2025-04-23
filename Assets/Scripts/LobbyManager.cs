using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    private Dictionary<ulong, bool> playerReadyStatus = new();

    public GameObject playerEntryPrefab;
    public Transform playerListParent;

    private void Awake()
    {
        Instance = this;
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
        playerReadyStatus[clientId] = false;
        UpdateLobbyUI();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerReadyStatus.Remove(clientId);
        UpdateLobbyUI();
    }

    public void SetPlayerReady(bool isReady)
    {
        if (IsClient)
        {
            SubmitReadyServerRpc(NetworkManager.Singleton.LocalClientId, isReady);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitReadyServerRpc(ulong clientId, bool isReady)
    {
        playerReadyStatus[clientId] = isReady;
        CheckReadyState();
        UpdateLobbyUI();
    }

    private void CheckReadyState()
    {
        int readyCount = 0;
        foreach (var ready in playerReadyStatus.Values)
        {
            if (ready) readyCount++;
        }

        if (readyCount >= playerReadyStatus.Count / 2f && playerReadyStatus.Count > 1)
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    private void UpdateLobbyUI()
    {
        
    }
}