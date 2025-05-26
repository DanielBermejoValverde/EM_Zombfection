using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;


        NetworkManager.Singleton.Shutdown();
       

        // Cargar la escena del menú principal
        SceneManager.LoadScene("MenuScene");
    }


    //[ServerRpc(RequireOwnership = false)]
    //private void NotifyServerOfQuitServerRpc(ulong clientId)
    //{
    //    if (LevelManager.Instance != null)
    //    {
    //        LevelManager.Instance.OnClientDisconnected(clientId);
    //    }
    //}
}
