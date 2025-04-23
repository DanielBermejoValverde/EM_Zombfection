using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MenuManager : MonoBehaviour
{
    public GameObject lobbyUI;
    public GameObject menuUI;

    public void Awake()
    {
        Time.timeScale = 1f; // Aseg�rate de que el tiempo est� restaurado al cargar la escena
        lobbyUI = GameObject.Find("LobbyUI");
        menuUI = GameObject.Find("MenuUI");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Cambia "MainScene" por el nombre de tu escena principal
    }

    
    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
        lobbyUI.SetActive(true);
        menuUI.SetActive(false); 
        print("Host");
    }

    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
        lobbyUI.SetActive(true);
        menuUI.SetActive(false); 
        print("Player");
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
