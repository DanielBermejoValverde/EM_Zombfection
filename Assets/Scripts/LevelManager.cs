using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Object = System.Object;

public enum GameMode
{
    Tiempo,
    Monedas
}

public class LevelManager : NetworkBehaviour
{
    #region Properties

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject zombiePrefab;

    [Header("Team Settings")]
    [Tooltip("N�mero de jugadores humanos")]
    [SerializeField] private int numberOfHumans = 0;

    Object lockHumans = new Object();

    [Tooltip("N�mero de zombis")]
    [SerializeField] private int numberOfZombies = 0;

    Object lockZombies = new Object();

    [Header("Game Mode Settings")]
    [Tooltip("Selecciona el modo de juego")]
    [SerializeField] public GameMode gameMode;

    [Tooltip("Tiempo de partida en minutos para el modo tiempo")]
    [SerializeField] private int minutes = 5;

    private List<Vector3> humanSpawnPoints = new List<Vector3>();

    private HashSet<ulong> initialZombies = new();
    private HashSet<ulong> convertedZombies = new();
    private ulong lastHumanId = ulong.MaxValue;
    Dictionary<ulong, string> playerResults = new Dictionary<ulong, string>();
    //private List<Vector3> zombieSpawnPoints = new List<Vector3>();

    public int totalCoinsCollected = 0;

    Object lockMonedas = new Object();

    // Referencias a los elementos de texto en el canvas
    private TextMeshProUGUI humansText;
    private TextMeshProUGUI zombiesText;
    private TextMeshProUGUI gameModeText;

    private MenuManager menuManager;

    private int CoinsGenerated = 0;

    public string PlayerPrefabName => playerPrefab.name;
    public string ZombiePrefabName => zombiePrefab.name;

    private UniqueIdGenerator uniqueIdGenerator;
    private LevelBuilder levelBuilder;

    private PlayerController playerController;

    private float remainingSeconds;
    private bool isGameOver = false;

    public GameObject gameOverPanel; // Asigna el panel desde el inspector
    public TextMeshProUGUI GameOverText;
    public static LevelManager Instance { get; private set; }
    #endregion

    #region Unity game loop methods

    private void Awake()
    {
        Debug.Log("Despertando el nivel");
        menuManager = GameObject.Find("CanvasUI").GetComponent<MenuManager>();

        gameMode = menuManager.gameMode;

        // Obtener la referencia al UniqueIDGenerator
        uniqueIdGenerator = GetComponent<UniqueIdGenerator>();

        // Obtener la referencia al LevelBuilder
        levelBuilder = GetComponent<LevelBuilder>();


        Time.timeScale = 1f; // Asegurarse de que el tiempo no est� detenido

        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    [ServerRpc(RequireOwnership = false)]
    public void DisconnectPlayerServerRpc(ulong clientId)
    {
        bool isZombie = false;
        foreach (var player in GameObject.FindObjectsOfType<PlayerController>())
        {
            if (clientId == player.GetComponent<NetworkObject>().OwnerClientId)
            {
                isZombie = player.GetComponent<PlayerController>().isZombie;
                break;
            }
        }

        if (!isZombie)
        {
            lock (lockHumans)
            {
                numberOfHumans--;
                if (numberOfHumans == 0)
                {
                    isGameOver = true;
                    //GameOverClientRpc();
                }
            }
        }
        else
        {
            lock (lockZombies)
            {
                numberOfZombies--;
                if (numberOfZombies == 0)
                {
                    isGameOver = true;
                    //GameOverClientRpc();
                }
            }
        }
        NetworkManager.Singleton.DisconnectClient(clientId);
    }


    private void Start()
    {
        Debug.Log("Iniciando el nivel");
        // Buscar el objeto "CanvasPlayer" en la escena
        GameObject canvas = GameObject.Find("CanvasPlayer");
        if (canvas != null)
        {
            Debug.Log("Canvas encontrado");

            // Buscar el Panel dentro del CanvasHud
            Transform panel = canvas.transform.Find("PanelHud");
            if (panel != null)
            {
                // Buscar los TextMeshProUGUI llamados "HumansValue" y "ZombiesValue" dentro del Panel
                Transform humansTextTransform = panel.Find("HumansValue");
                Transform zombiesTextTransform = panel.Find("ZombiesValue");
                Transform gameModeTextTransform = panel.Find("GameModeConditionValue");

                if (humansTextTransform != null)
                {
                    humansText = humansTextTransform.GetComponent<TextMeshProUGUI>();
                }

                if (zombiesTextTransform != null)
                {
                    zombiesText = zombiesTextTransform.GetComponent<TextMeshProUGUI>();
                }

                if (gameModeTextTransform != null)
                {
                    gameModeText = gameModeTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        remainingSeconds = minutes * 60;
        // Obtener los puntos de aparici�n y el n�mero de monedas generadas desde LevelBuilder
        if (levelBuilder != null)
        {
            if(IsServer)levelBuilder.Build();
            humanSpawnPoints = levelBuilder.GetHumanSpawnPoints();
            //zombieSpawnPoints = levelBuilder.GetZombieSpawnPoints();
            CoinsGenerated = levelBuilder.GetCoinsGenerated();
        }

        SpawnTeams();
        if(IsServer)
            UpdateTeamUIClientRpc(numberOfHumans,numberOfZombies);
    }

    private void Update()
    {
        if (gameMode == GameMode.Tiempo)
        {
            // L�gica para el modo de juego basado en tiempo
            HandleTimeLimitedGameMode();
        }
        else if (gameMode == GameMode.Monedas)
        {
            // L�gica para el modo de juego basado en monedas
            HandleCoinBasedGameMode();
        }
        /* De momento no lo requerimos
        if (Input.GetKeyDown(KeyCode.Z)) // Presiona "Z" para convertirte en Zombie
        {
            // Comprobar si el jugador actual est� usando el prefab de humano
            GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (currentPlayer != null && currentPlayer.name.Contains(playerPrefab.name))
            {
                ChangeToZombie();
            }
            else
            {
                Debug.Log("El jugador actual no es un humano.");
            }
        }
        else if (Input.GetKeyDown(KeyCode.H)) // Presiona "H" para convertirte en Humano
        {
            // Comprobar si el jugador actual est� usando el prefab de zombie
            GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (currentPlayer != null && currentPlayer.name.Contains(zombiePrefab.name))
            {
                ChangeToHuman();
            }
            else
            {
                Debug.Log("El jugador actual no es un zombie.");
            }
        }
        */
        if(IsServer)
            UpdateTeamUIClientRpc(numberOfHumans,numberOfZombies);

        if (isGameOver)
        {
            GenerateGameOverResults();
        }
    }

    #endregion

    #region Team management methods

    private void ChangeToZombie()
    {
        GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
        ChangeToZombie(currentPlayer, true);
    }

    public void ChangeToZombie(GameObject human, bool enabled)
    {
        if (!IsServer) return;

        var netObj = human.GetComponent<NetworkObject>();
        var clientId = netObj.OwnerClientId;

        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        netObj.Despawn(true); // Notifica a todos que se elimina

        // Instanciar el nuevo prefab 
        GameObject zombie = Instantiate(zombiePrefab, position, rotation);
        var zombieNetObj = zombie.GetComponent<NetworkObject>();
        zombieNetObj.SpawnAsPlayerObject(clientId); // Spawnea para todos los clientes

        PlayerController zombieController = zombie.GetComponent<PlayerController>();
        zombieController.isZombie = true;
        zombieController.clientID.Value = clientId;
        lock (lockZombies)
        {
            numberOfZombies++; // Aumentar el n�mero de zombis

        }
        lock (lockHumans) 
        {
            //Comprobar si es el último humano para dar el Game Over total a ese humano
            numberOfHumans--; // Reducir el n�mero de humanos
            if (numberOfHumans <= 0)
            {
                
                lastHumanId = clientId;
                isGameOver = true;
            }
            else
            {
                convertedZombies.Add(clientId);
            }
        }
        


    }


    private void ChangeToHuman()
    {
        Debug.Log("Cambiando a Humano");

        // Obtener la referencia al jugador actual
        GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer != null)
        {
            // Guardar la posici�n y rotaci�n del jugador actual
            Vector3 playerPosition = currentPlayer.transform.position;
            Quaternion playerRotation = currentPlayer.transform.rotation;

            // Destruir el jugador actual
            Destroy(currentPlayer);

            // Instanciar el prefab del humano en la misma posici�n y rotaci�n
            GameObject human = Instantiate(playerPrefab, playerPosition, playerRotation);
            human.tag = "Player";

            // Obtener la referencia a la c�mara principal
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                // Obtener el script CameraController de la c�mara principal
                CameraController cameraController = mainCamera.GetComponent<CameraController>();

                if (cameraController != null)
                {
                    // Asignar el humano al script CameraController
                    cameraController.player = human.transform;
                }

                // Obtener el componente PlayerController del humano instanciado
                playerController = human.GetComponent<PlayerController>();
                // Asignar el transform de la c�mara al PlayerController
                if (playerController != null)
                {
                    playerController.enabled = true;
                    playerController.cameraTransform = mainCamera.transform;
                    playerController.isZombie = false; // Cambiar el estado a humano
                    numberOfHumans++; // Aumentar el n�mero de humanos
                    numberOfZombies--; // Reducir el n�mero de zombis
                }
                else
                {
                    Debug.LogError("PlayerController no encontrado en el humano instanciado.");
                }
            }
            else
            {
                Debug.LogError("No se encontr� la c�mara principal.");
            }
        }
        else
        {
            Debug.LogError("No se encontr� el jugador actual.");
        }
    }

    private void SpawnPlayer(Vector3 spawnPosition, GameObject prefab, ulong clientId)
    {
        Debug.Log($"Instanciando jugador en {spawnPosition}");
        //GameObject.FindGameObjectsWithTag("PlayableCharacter")[clientId].gameObject.transform.position = spawnPosition;
        
        // Crear una instancia del prefab en el punto especificado
        GameObject player = Instantiate(prefab, spawnPosition, Quaternion.identity);
        //Servidor spawnea los objetos como playerObject de otros dejo conectado porque elcliente no se da cuenta que lo tiene en propiedad
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        player.GetComponent<PlayerController>().clientID.Value = clientId;

        //SpawnPlayerClientRpc(clientId, player.GetComponent<NetworkObject>().NetworkObjectId);
           
    }
    [ClientRpc]
    private void SpawnPlayerClientRpc(ulong clientId, ulong objectId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        

    }

    private void SpawnTeams()
    {
        if (!IsServer) return; //Los clientes no realizan ningun trabajo de spawneo
        Debug.Log("Instanciando equipos");
        if (humanSpawnPoints.Count <= 0) { return; }
        int i = 0;
        Debug.Log($"Connected clients {NetworkManager.Singleton.ConnectedClients.Count}");
        Debug.Log($"Human spaun points count: {humanSpawnPoints.Count}");
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            GameObject prefab = ((int)client.Key) % 2 == 0 ? playerPrefab : zombiePrefab;
            lock (lockHumans)
            {
                numberOfHumans += ((int)client.Key) % 2 == 0 ? 1 : 0;
            }
            lock (lockZombies)
            {
                numberOfZombies += ((int)client.Key) % 2 == 0 ? 0 : 1;
            }
            SpawnPlayer(humanSpawnPoints[i%4],prefab, client.Key);
            Debug.Log($"Personaje jugable instanciado en {humanSpawnPoints[i]}");
            i++;

            if (((int)client.Key) % 2 != 0)
            {
                initialZombies.Add(client.Key);
            }

        }
        /*
        for (int i = 1; i < numberOfHumans; i++)
        {
            if (i < humanSpawnPoints.Count)
            {
                SpawnNonPlayableCharacter(playerPrefab, humanSpawnPoints[i]);
            }
        }

        for (int i = 0; i < numberOfZombies; i++)
        {
            if (i < zombieSpawnPoints.Count)
            {
                SpawnNonPlayableCharacter(zombiePrefab, zombieSpawnPoints[i]);
            }
        }
        */
    }

    private void SpawnNonPlayableCharacter(GameObject prefab, Vector3 spawnPosition)
    {
        if (prefab != null)
        {
            GameObject npc = Instantiate(prefab, spawnPosition, Quaternion.identity);
            // Desactivar el controlador del jugador en los NPCs
            var playerController = npc.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false; // Desactivar el controlador del jugador
                playerController.uniqueID = uniqueIdGenerator.GenerateUniqueID(); // Asignar un identificador �nico
            }
            Debug.Log($"Personaje no jugable instanciado en {spawnPosition}");
        }
    }
    [ClientRpc]
    private void UpdateTeamUIClientRpc(int numberOfHumanss, int numberOfZombiess)
    {
        if (humansText != null)
        {
            humansText.text = $"{numberOfHumanss}";
        }

        if (zombiesText != null)
        {
            zombiesText.text = $"{numberOfZombiess}";
        }
    }

    #endregion

    #region Modo de juego

    private void HandleTimeLimitedGameMode()
    {
        if (isGameOver || !IsServer) return;

        remainingSeconds -= Time.deltaTime;
        if (remainingSeconds <= 0)
        {
            remainingSeconds = 0;
            isGameOver = true;
        }

        int minutes = Mathf.FloorToInt(remainingSeconds / 60);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60);
        UpdateTimeUIClientRpc(minutes, seconds);
    }
    public void CollectCoin()
    {
        lock (lockMonedas)
        {
            totalCoinsCollected++;
        }
    }
    private void HandleCoinBasedGameMode()
    {
        if (isGameOver || !IsServer) return;

        // Implementar la l�gica para el modo de juego basado en monedas
        if (gameModeText != null )
        {
            gameModeText.text = $"{totalCoinsCollected}/{CoinsGenerated}";
            UpdateCoinsUIClientRpc(totalCoinsCollected, CoinsGenerated);
            if (totalCoinsCollected == CoinsGenerated)
            {
                isGameOver = true;
               
            }
        }
    }
    private void GenerateGameOverResults()
    {
        Debug.Log("ENTRAMOS ");

        foreach (var player in GameObject.FindObjectsOfType<PlayerController>())
        {
            ulong id = player.OwnerClientId;
            string result;

            bool humansWon = false;
            bool zombiesWon = false;

            // Comprobar si los humanos ganaron (tiempo o monedas)
            if (gameMode == GameMode.Tiempo && remainingSeconds <= 0)
            {
                humansWon = true;
            }
            else if (gameMode == GameMode.Monedas && totalCoinsCollected == CoinsGenerated)
            {
                humansWon = true;
            }

            // Comprobar si los humanos perdieron (no quedan humanos vivos)
            //bool humansLost = !humansWon;

            if (!player.isZombie)
            {
                // Jugador humano
                if (humansWon)
                {
                    result = "¡Victoria Total (Humano)!";
                }
                else if (!humansWon)
                {
                    result = (id == lastHumanId) ? "¡Derrota! Fuiste el último humano." : "¡Derrota!";
                }
                else
                {
                    // La partida no ha acabado o situación no definida
                    result = "Partida en curso...";
                }
                lock(lockZombies)
                {
                    if (numberOfZombies == 0)
                    {
                        // reescribimos result si es por abandono
                        result = "Victoria por abandono Zombi";
                    }
                }
            }
            else
            {
                // Jugador zombie
                if (humansWon)
                {
                    // Humanos ganaron, zombies perdieron
                    result = "¡Derrota!";
                }
                else if (!humansWon)
                {
                    // Humanos perdieron, zombies ganaron parcialmente o totalmente
                    if (initialZombies.Contains(id))
                    {
                        result = "¡Victoria Total (Zombi)!";
                    }
                    else if (convertedZombies.Contains(id))
                    {
                        result = "¡Victoria Parcial (Convertido)!";
                    }
                    else
                    {
                        result = "¡Derrota!";
                    }
                }
                else
                {
                    // La partida no ha acabado o situación no definida
                    result = "Partida en curso...";
                }
                lock(lockHumans)
                {
                    if (numberOfHumans == 0)
                    {
                        // reescribimos result si es por abandono
                        result = "Victoria por abandono Humano";
                    }
                }
            }


        GameOverClientRpc(id, result);
        }
    }


    [ClientRpc]
    private void GameOverClientRpc(ulong clientId, string result)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            ShowGameOverPanel(result);
        }
    }

    private void ShowGameOverPanel(string message)
    {
        Debug.Log("generamos panel");
        if (gameOverPanel != null)
        {
            Time.timeScale = 0f;
            GameOverText.text = message;
            gameOverPanel.SetActive(true);


            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    [ClientRpc]
    private void UpdateCoinsUIClientRpc(int coinsCollected,int coinsGenerated){
        gameModeText.text = $"{coinsCollected}/{coinsGenerated}";
    }
    [ClientRpc]
    private void UpdateTimeUIClientRpc(int minutesRemaining, int secondsRemaining)
    {
         gameModeText.text = $"{minutesRemaining:D2}:{secondsRemaining:D2}";
    }
    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true); // Muestra el panel de pausa

            // Gesti�n del cursor
            Cursor.lockState = CursorLockMode.None; // Desbloquea el cursor
            Cursor.visible = true; // Hace visible el cursor
        }
    }

    public void ReturnToMainMenu()
    {
        // Gesti�n del cursor
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor
        Cursor.visible = false; // Oculta el cursor

        // Cargar la escena del men� principal
        menuManager.isGame = false;
        SceneManager.LoadScene("MenuScene"); // Cambia "MenuScene" por el nombre de tu escena principal
    }

    #endregion

}




