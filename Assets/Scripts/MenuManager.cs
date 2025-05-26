using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject lobbyUI;
    public GameObject menuUI;
    public GameObject modeButton;
    public GameObject readyButton;

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
            lobbyUI = GameObject.Find("LobbyUI");
            menuUI = GameObject.Find("MenuUI");
            modeButton = GameObject.Find("ModeButton");
            readyButton = GameObject.Find("ReaddyButton");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Cambia "MainScene" por el nombre de tu escena principal
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isGame) return;
        lobbyUI.SetActive(true);
        readyButton.SetActive(true);
    }
    public void ClearCanvas()
    {
        lobbyUI.SetActive(false);
        menuUI.SetActive(false);
        readyButton.SetActive(false); 

    }
    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
        lobbyUI.SetActive(true);
        menuUI.SetActive(false); 
    }

    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
        lobbyUI.SetActive(true);
        menuUI.SetActive(false); 
    }
    public void ChangeMode()
    {
        gameMode = gameMode == GameMode.Monedas ? GameMode.Tiempo : GameMode.Monedas;
        Debug.Log(gameMode);
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
