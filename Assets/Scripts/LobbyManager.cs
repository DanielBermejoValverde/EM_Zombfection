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
        playerReadyStatus[clientId] = false;
        UpdateLobbyUIClientRpc();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerReadyStatus.Remove(clientId);
        UpdateLobbyUIClientRpc();
    }

    public void SetPlayerReady()
    {
        if (IsClient)
        {
            SubmitReadyServerRpc(NetworkManager.Singleton.LocalClientId, true);
        }
    }

    [ServerRpc]
    private void SubmitReadyServerRpc(ulong clientId, bool isReady)
    {
        playerReadyStatus[clientId] = isReady;
        CheckReadyState();
        UpdateLobbyUIClientRpc();
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
            SceneManager.LoadScene("GameScene");
        }
    }
    [ClientRpc]
    private void UpdateLobbyUIClientRpc()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Button>() != null)
                continue;
            Destroy(child.gameObject);
        }
        foreach (var kvp in playerReadyStatus)
        {
            Transform newTransform = transform;
            newTransform.position = newTransform.position + new Vector3(0,posY, 0);
            GameObject playerLobby = Instantiate(playerLobbyPrefab, newTransform);
            posY -= 30;
            playerLobby.GetComponent<PlayerLobbyManager>().SetInfo("Player " + kvp.Key, kvp.Value);
        }
    }
}