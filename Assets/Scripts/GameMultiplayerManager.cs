using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayerManager : NetworkBehaviour
{
    public static GameMultiplayerManager Instance;

    private const int MAX_PLAYERS = 2; 

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;


    private bool isLocalGamePaused = false;
    private bool isInitialized = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    private Dictionary<ulong, bool> playerPausedDictionary;
    public NetworkVariable<ulong> pausedByPlayer = new NetworkVariable<ulong>(0);
    public List<ulong> connectedPlayers = new List<ulong>();
    private bool autoTestGamePausedState;


    [Header("Event bus")]
    [SerializeField] private GamePauseEvent _pauseEvent;
    // [SerializeField] private TextMeshProUGUI pauseRequestText; 
     [Header("Pause Timeout")]
    [SerializeField] private float pauseTimeoutDuration = 30f; // Timeout dalam detik
    private float pauseTimeoutTimer;
    private bool isTimeoutActive;


    private enum State
    {
        WaitingToStart,
        GamePlaying,
        GameOver,
    }

    void Awake()
    {
        Instance = this;
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        _pauseEvent.OnPause += GameInput_OnPauseAction;
     
    }

    void OnDisable()
    {
        _pauseEvent.OnPause -= GameInput_OnPauseAction;
        //  NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        // NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Server_OnClientDisconnectCallback;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Client_OnClientDisconnectCallback;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }


    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            // NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            // NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            // NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
            // NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;

            playerPausedDictionary[NetworkManager.Singleton.LocalClientId] = false;
        }

        // if (IsServer)
        // {
        //     CheckPlayerLimit();
        // }

        isInitialized = true;
        InitializePlayerPauseStates();
    }
    private void InitializePlayerPauseStates()
    {
        if (!isInitialized || !IsSpawned) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerPausedDictionary.ContainsKey(clientId))
            {
                playerPausedDictionary[clientId] = false;
            }
        }
    }

     private void OnClientConnected(ulong clientId)
    {
  
        if (state.Value == State.GamePlaying)
        {
   
            NetworkManager.Singleton.DisconnectClient(clientId);
            Debug.Log($"Player {clientId} was rejected because the game has already started.");
        }
        else
        {
            connectedPlayers.Add(clientId);
            CheckPlayerLimit();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");

        connectedPlayers.Remove(clientId);
        
        BackToMainMenuClientRpc();
        Debug.Log("Network shutdown initiated.");
        NetworkManager.Singleton.Shutdown();    
    }


    private void CheckPlayerLimit()
    {
        if (connectedPlayers.Count > MAX_PLAYERS)
        {
            ulong excessPlayer = connectedPlayers[2];
            NetworkManager.Singleton.DisconnectClient(excessPlayer);
            connectedPlayers.Remove(excessPlayer);
            Debug.Log($"Player {excessPlayer} has been disconnected because the player limit is 2.");
        }
    }



    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        autoTestGamePausedState = true;
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void BackToMainMenuServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("BackToMainMenuServerRpc called.");
        BackToMainMenuClientRpc();
    }


    [ClientRpc]
    public void BackToMainMenuClientRpc()
    {
        Debug.Log("BackToMainMenuClientRpc triggered on client.");
        Time.timeScale = 1f;
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.LobbyScene);
    }

    [ClientRpc]
    public void BackToMainMenuNetClientRpc()
    {
        NetworkManager.Singleton.Shutdown();
        Loader.LoadNetwork(Loader.Scene.LobbyScene);
    }


    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if(isGamePaused.Value){
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }



    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!IsServer) return;

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.GamePlaying:
                HandlePauseTimeout();
                break;
            case State.GameOver:
                break;
        }
    }

     private void HandlePauseTimeout()
    {
        if (isGamePaused.Value && isTimeoutActive)
        {
            pauseTimeoutTimer -= Time.unscaledDeltaTime;
            
            if (pauseTimeoutTimer <= 0f)
            {
                // Force unpause jika timeout tercapai
                ForceUnpauseGame();
            }
        }
    }

    // Memaksa unpause game (dipanggil saat timeout atau edge cases)
    [ServerRpc]
    private void ForceUnpauseGameServerRpc()
    {
        ForceUnpauseGame();
    }

    private void ForceUnpauseGame()
    {
        if (!IsServer) return;
        
        Debug.Log("Force unpausing game due to timeout or edge case");
        
        // Reset semua state pause
        foreach (var key in playerPausedDictionary.Keys)
        {
            playerPausedDictionary[key] = false;
        }
        
        pausedByPlayer.Value = 0;
        UpdatePauseRequestClientRpc(0);
        StopPauseTimeout();
        
        // Set state game ke unpaused
        isGamePaused.Value = false;
    }

    private void StartPauseTimeout()
    {
        pauseTimeoutTimer = pauseTimeoutDuration;
        isTimeoutActive = true;
        Debug.Log($"Pause timeout started ({pauseTimeoutDuration}s)");
    }
    private void StopPauseTimeout()
    {
        isTimeoutActive = false;
        Debug.Log("Pause timeout stopped");
    }

    private void LateUpdate()
    {
        if (autoTestGamePausedState)
        {
            autoTestGamePausedState = false;
            TestGamePausedState();
        }      
    }
    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }


    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }


// PAUSE CONTROL =================================================================================================
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        
        TogglePauseGame();
    }

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

  [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (playerPausedDictionary.ContainsKey(clientId))
        {
            playerPausedDictionary[clientId] = true;
            pausedByPlayer.Value = clientId;
            UpdatePauseRequestClientRpc(clientId);
            
            // Mulai timeout hanya jika game belum paused
            if (!isGamePaused.Value)
            {
                StartPauseTimeout();
            }
            
            TestGamePausedState();
        }
    }

   [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (playerPausedDictionary.ContainsKey(clientId))
        {
            playerPausedDictionary[clientId] = false;
            
            // Reset pausedByPlayer hanya jika pemain ini yang mempause
            if (pausedByPlayer.Value == clientId)
            {
                pausedByPlayer.Value = 0;
                UpdatePauseRequestClientRpc(0);
            }
            
            // Hentikan timeout
            StopPauseTimeout();
            
            TestGamePausedState();
        }
    }



    [ClientRpc]
    private void UpdatePauseRequestClientRpc(ulong playerId)
    {
        Debug.Log($"Player {playerId} triggered the pause.");
        pausedByPlayer.Value = playerId; 
    }


   private void TestGamePausedState()
    {
        bool anyPlayerPaused = false;
        int pausedPlayersCount = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.TryGetValue(clientId, out bool isPaused) && isPaused)
            {
                anyPlayerPaused = true;
                pausedPlayersCount++;
            }
        }

        if (pausedPlayersCount == NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log("All players have paused the game");
        }

        isGamePaused.Value = anyPlayerPaused;
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Server: Client {clientId} disconnected, handling disconnect.");
        
        // Hapus dari dictionary
        playerPausedDictionary.Remove(clientId);
        
        // Jika pemain yang disconnect adalah yang mempause
        if (pausedByPlayer.Value == clientId)
        {
            pausedByPlayer.Value = 0;
            UpdatePauseRequestClientRpc(0);
            
            // Jika tidak ada pemain lain yang pause, unpause game
            bool anyOtherPlayerPaused = false;
            foreach (var kvp in playerPausedDictionary)
            {
                if (kvp.Value) anyOtherPlayerPaused = true;
            }
            
            if (!anyOtherPlayerPaused)
            {
                isGamePaused.Value = false;
                StopPauseTimeout();
            }
        }
        
        TestGamePausedState();
        
        // Jika host yang disconnect, berhenti
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host disconnected, shutting down");
            NetworkManager.Singleton.Shutdown();
            return;
        }
        
        // Jika hanya tersisa 1 pemain, pause game dan tampilkan pesan
        if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
        {
            HandleSinglePlayerLeft();
        }
    }

    private void HandleSinglePlayerLeft()
    {
        Debug.Log("Only one player left, pausing game");
        
        // Pause game dan set timeout lebih pendek
        isGamePaused.Value = true;
        pausedByPlayer.Value = NetworkManager.ServerClientId;
        UpdatePauseRequestClientRpc(NetworkManager.ServerClientId);
        
        // Set timeout khusus untuk kasus ini
        pauseTimeoutDuration = 10f;
        StartPauseTimeout();
        
        // Kirim notifikasi ke client
        NotifySinglePlayerLeftClientRpc();
    }

    [ClientRpc]
    private void NotifySinglePlayerLeftClientRpc()
    {
        Debug.Log("Other players have left the game. Returning to lobby soon...");
        // Anda bisa menampilkan UI khusus di sini
    }



    // private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    // {
    // }

    // private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    // {
    //     Debug.Log($"Client {clientId} disconnected, initiating scene change.");
    //     Time.timeScale = 1f;
    //     BackToMainMenuClientRpc();
    // }


}
