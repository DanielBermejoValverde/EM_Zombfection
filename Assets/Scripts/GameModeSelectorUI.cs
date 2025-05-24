using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class GameModeSelectorUI : NetworkBehaviour
{
    public Button tiempoButton;
    public Button monedasButton;
    public TMP_Text selectedModeText;
    public TMP_Text statusText;

    private NetworkVariable<GameMode> selectedGameMode = new NetworkVariable<GameMode>(GameMode.Tiempo, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        tiempoButton.onClick.AddListener(() => SelectGameMode(GameMode.Tiempo));
        monedasButton.onClick.AddListener(() => SelectGameMode(GameMode.Monedas));

        selectedGameMode.OnValueChanged += OnGameModeChanged;

        UpdateUI();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            tiempoButton.interactable = false;
            monedasButton.interactable = false;
        }

        UpdateUI();
    }

    private void SelectGameMode(GameMode mode)
    {
        if (IsHost)
        {
            selectedGameMode.Value = mode;
        }
    }

    private void OnGameModeChanged(GameMode prev, GameMode next)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (selectedModeText != null)
        {
            selectedModeText.text = $"Modo seleccionado: {selectedGameMode.Value}";
        }

        if (statusText != null)
        {
            statusText.text = $"Modo de juego elegido: {selectedGameMode.Value}";
        }
    }

    public GameMode GetSelectedGameMode()
    {
        return selectedGameMode.Value;
    }
}
