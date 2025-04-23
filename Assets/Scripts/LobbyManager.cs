using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    private Dictionary<ulong, bool> playerReadyStatus = new();

    public GameObject playerLobbyPrefab;

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

    [ServerRpc(RequireOwnership = false)]
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
        // Aquí instancia UI de cada jugador, actualiza nombres y estado listo
        // Puedes extender esto para usar nombres de jugador
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Button>() != null)
                continue;
            Destroy(child.gameObject);
        }
        foreach (var kvp in playerReadyStatus)
        {
            GameObject playerLobby = Instantiate(playerLobbyPrefab, transform);
            playerLobby.GetComponent<PlayerLobbyManager>().SetInfo("Player " + kvp.Key, kvp.Value);
        }
    }
}