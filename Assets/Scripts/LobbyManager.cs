using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyStatus = new();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();


    public GameObject playerLobbyPrefab;
    public GameObject canvasUI;

    public LobbyManager self;

    public GameMode gameMode = GameMode.Monedas;

    private void Awake()
    {
        if (self == null)
        {
            self = this;
            DontDestroyOnLoad(gameObject);
            //lobbyUI = GameObject.Find("LobbyUI");
            canvasUI = GameObject.Find("CanvasUI");
            //playerUI = GameObject.Find("PlayerUI");
        }
        else
        {
            Destroy(this);
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (ulong key in playerReadyStatus.Keys)
        {
            playerReadyStatus[key] = false;
        }
        UpdateLobbyUI();
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
        playerNames[playerId] = $"Player {playerId}";
        foreach (var key in new List<ulong>(playerReadyStatus.Keys))
        {
            var value = playerReadyStatus[key];
            var name = playerNames[key];
            ClientConectedClientRpc(key, name, value);
        }
    }

    [ClientRpc]
    private void ClientConectedClientRpc(ulong playerId, string name,  bool isReady)
    {
        playerReadyStatus[playerId] = isReady;
        playerNames[playerId] = name;
        UpdateLobbyUI();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeNameServerRpc(ulong playerId, string newName)
    {
        playerNames[playerId] = newName;
        ChangeNameClientRpc(playerId, newName);
    }

    [ClientRpc]
    private void ChangeNameClientRpc(ulong playerId, string newName)
    {
        playerNames[playerId] = newName;
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
        if (!IsServer) return;
        playerReadyStatus[playerId] = isReady;
        CheckReadyState();
        SubmitReadyClientRpc(playerId, isReady);
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
            ResetReadyClientRpc();
            
            
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }

    private void UpdateLobbyUI()
    {
        foreach (Transform child in transform)
            if (child.GetComponent<Button>() == null)
                Destroy(child.gameObject);

        foreach (var kv in playerReadyStatus)
        {
            ulong id = kv.Key;
            bool ready = kv.Value;
            string name = playerNames.ContainsKey(id)
                ? playerNames[id]
                : $"Player {id}";
            var go = Instantiate(playerLobbyPrefab, transform);
            go.GetComponent<PlayerLobbyManager>().SetInfo(id, name, ready);
            //playerLobby.GetComponent<PlayerLobbyManager>().SetInfo("Player " + player.Key, player.Value);
        }
    }
    
    [ClientRpc]
    public void ResetReadyClientRpc()
    {
        canvasUI.GetComponent<MenuManager>().isGame = true;
        canvasUI.GetComponent<MenuManager>().ClearCanvas();
    }
}