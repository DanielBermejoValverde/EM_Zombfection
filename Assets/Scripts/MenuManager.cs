using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;

public class MenuManager : NetworkBehaviour
{
    public GameObject menuUI;

    public GameObject joinHostUI;
    public Button modeButton;

    public static MenuManager self;

    public bool isGame = false;
    public GameMode gameMode = GameMode.Monedas;

    public void Awake()
    {
        Time.timeScale = 1f; // Aseg�rate de que el tiempo est� restaurado al cargar la escena
        //lobbyUI = GameObject.Find("LobbyUI");
        menuUI = GameObject.Find("MenuUI");
        if (self == null)
        {
            self = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            menuUI = GameObject.Find("MenuUI");
            //joinHostUI = GameObject.Find("JoinHostUI");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
            joinHostUI.SetActive(false);
    }


    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Cambia "MainScene" por el nombre de tu escena principal
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isGame) return;
        joinHostUI.SetActive(true); 
    }
    public void ClearCanvas()
    {
        menuUI.SetActive(false);
        joinHostUI.SetActive(false); 

    }
    public void HostGame()
    {
        joinHostUI.SetActive(true); 
        menuUI.SetActive(false); 
        NetworkManager.Singleton.StartHost();
    }

    public void JoinGame()
    {
        joinHostUI.SetActive(true); 
        menuUI.SetActive(false); 
        NetworkManager.Singleton.StartClient();
    }
    
    public void Disconnect()
    {
        joinHostUI.SetActive(false); 
        menuUI.SetActive(true); 
        NetworkManager.Singleton.Shutdown();
    }
    public void ChangeMode()
    {
        if (!IsServer) return;
        gameMode = gameMode == GameMode.Tiempo ? GameMode.Monedas : GameMode.Tiempo;
        string gameModeString = gameMode == GameMode.Tiempo ? "Tiempo" : "Monedas";
        ChangeModeClientRpc(gameModeString);
    }
    [ClientRpc]
    public void ChangeModeClientRpc(string gameMode)
    {
        modeButton.GetComponentInChildren<TextMeshProUGUI>().text = gameMode;
    }
    public void ToggleLobby()
    {
        joinHostUI.SetActive(!joinHostUI.activeInHierarchy);
        menuUI.SetActive(!menuUI.activeInHierarchy);
        
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Salir en el editor
#else
            Application.Quit(); // Salir en una build
#endif
    }
}
