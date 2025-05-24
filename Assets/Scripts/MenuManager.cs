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

     /*GameObject readyButton = GameObject.Find("ReadyButton");
     if (readyButton != null)
     readyButton.SetActive(false); // Ocultar el botón al iniciar */
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

        /*GameObject readyButton = GameObject.Find("ReadyButton");
        if (readyButton != null)
            readyButton.SetActive(true); // Mostrar botón Ready*/

        /*GameObject selector = GameObject.Find("GameModeSelector");
        if (selector != null) selector.SetActive(true); // mostrar solo para el host*/
    }

    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
        lobbyUI.SetActive(true);
        menuUI.SetActive(false);

        /*GameObject readyButton = GameObject.Find("ReadyButton");
        if (readyButton != null)
            readyButton.SetActive(true); // Mostrar botón Ready*/

        /*GameObject selector = GameObject.Find("GameModeSelector");
        if (selector != null) selector.SetActive(false); // ocultar para clientes*/
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Salir en el editor
#else
            Application.Quit(); // Salir en una build
#endif

        /*GameObject readyButton = GameObject.Find("ReadyButton");
        if (readyButton != null)
            readyButton.SetActive(false);*/
    }
}
