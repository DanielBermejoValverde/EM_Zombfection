using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerLobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texto que muestra el nombre actual del jugador")]
    public TMP_Text playerNameText;

    [Tooltip("InputField para que el jugador local cambie su propio nombre")]
    public TMP_InputField nameInputField;

    [Tooltip("Texto que muestra estado Ready/Not Ready")]
    public TMP_Text readyText;

    private LobbyManager lobbyMgr;

    private void Awake()
    {
        // Se busca la instancia de LobbyManager para enviar RPCs
        lobbyMgr = FindObjectOfType<LobbyManager>();
        if (lobbyMgr == null)
        {
            Debug.LogError("[PlayerLobbyManager] No se encontró LobbyManager en la escena.");
        }
    }

    /// <summary>
    /// Inicializa la fila del jugador en el lobby.
    /// </summary>
    /// <param name="playerId">ID único del jugador.</param>
    /// <param name="name">Nombre inicial o personalizado.</param>
    /// <param name="isReady">Estado de ready.</param>
    public void SetInfo(ulong playerId, string name, bool isReady)
    {
        // Mostrar siempre el nombre actual en el TMP_Text
        playerNameText.text = name;

        // Actualizar estado Ready/Not Ready
        readyText.text = isReady ? "Ready" : "Not Ready";

        // Determinar si esta fila corresponde al jugador local
        bool isLocal = (NetworkManager.Singleton != null) && (playerId == NetworkManager.Singleton.LocalClientId);

        // Mostrar el InputField solo para el jugador local
        nameInputField.gameObject.SetActive(isLocal);

        // Limpiar listeners previos para evitar duplicados
        nameInputField.onEndEdit.RemoveAllListeners();

        if (isLocal)
        {
            // Inicializar texto del InputField
            nameInputField.text = name;
            nameInputField.interactable = true;

            // Al finalizar edición, enviar RPC al servidor con el nuevo nombre
            nameInputField.onEndEdit.AddListener(newName =>
            {
                if (!string.IsNullOrWhiteSpace(newName) && newName != name)
                {
                    lobbyMgr.ChangeNameServerRpc(playerId, newName);
                }
            });
        }
    }
}
