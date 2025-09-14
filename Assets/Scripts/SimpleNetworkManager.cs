using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleNetworkManager : MonoBehaviour
{
    [Header("Network Settings")]
    public int maxPlayers = 4;
    
    [Header("Multiplayer Play Mode")]
    public bool autoStartInPlayMode = false; // 수동 테스트를 위해 false로 변경
    
    // Input Actions
    private InputAction hostAction;
    private InputAction clientAction;
    private InputAction serverAction;
    
    void Awake()
    {
        // Input Actions 설정
        SetupInputActions();
    }
    
    void SetupInputActions()
    {
        // 키보드 액션들 설정
        hostAction = new InputAction("Host", InputActionType.Button, "<Keyboard>/h");
        clientAction = new InputAction("Client", InputActionType.Button, "<Keyboard>/c");
        serverAction = new InputAction("Server", InputActionType.Button, "<Keyboard>/s");
        
        // 이벤트 연결
        hostAction.performed += OnHostPressed;
        clientAction.performed += OnClientPressed;
        serverAction.performed += OnServerPressed;
        
        // 액션 활성화
        hostAction.Enable();
        clientAction.Enable();
        serverAction.Enable();
    }
    
    void Start()
    {
        Debug.Log("SimpleNetworkManager Started!");
        
        // 프레임 지연 후 초기화 (Unity 6 안정성을 위해)
        StartCoroutine(InitializeNetworkManager());
    }
    
    System.Collections.IEnumerator InitializeNetworkManager()
    {
        // 한 프레임 대기
        yield return null;
        
        // NetworkManager 확인
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null! Make sure NetworkManager is in the scene.");
            yield break;
        }
        
        // NetworkManager의 연결 이벤트 구독
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        Debug.Log("NetworkManager initialized successfully");
        
        // Multiplayer Play Mode 환경에서 자동 시작
        if (autoStartInPlayMode)
        {
            // 더 긴 딜레이 후 자동 연결 시도 (안정성을 위해)
            yield return new UnityEngine.WaitForSeconds(1f);
            TryConnectToGame();
        }
        else
        {
            Debug.Log("Auto start disabled. Use H/C keys or GUI buttons.");
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        // Input Actions 정리
        if (hostAction != null)
        {
            hostAction.performed -= OnHostPressed;
            hostAction.Dispose();
        }
        if (clientAction != null)
        {
            clientAction.performed -= OnClientPressed;
            clientAction.Dispose();
        }
        if (serverAction != null)
        {
            serverAction.performed -= OnServerPressed;
            serverAction.Dispose();
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected. Total clients: {NetworkManager.Singleton.ConnectedClients.Count}");
        
        // 기존 씬의 Player 오브젝트 비활성화 (네트워크 스폰된 플레이어만 사용)
        var scenePlayer = GameObject.FindGameObjectWithTag("Player");
        if (scenePlayer != null && !scenePlayer.GetComponent<NetworkObject>().IsSpawned)
        {
            scenePlayer.SetActive(false);
            Debug.Log("Scene player deactivated");
        }
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected. Remaining clients: {NetworkManager.Singleton.ConnectedClients.Count}");
    }

    // Input System 이벤트 핸들러들
    void OnHostPressed(InputAction.CallbackContext context)
    {
        Debug.Log("H key pressed - Starting Host");
        StartHost();
    }
    
    void OnClientPressed(InputAction.CallbackContext context)
    {
        Debug.Log("C key pressed - Starting Client");
        StartClient();
    }
    
    void OnServerPressed(InputAction.CallbackContext context)
    {
        Debug.Log("S key pressed - Starting Server");
        StartServer();
    }

    public void TryConnectToGame()
    {
        // NetworkManager 확인
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot connect: NetworkManager.Singleton is null!");
            return;
        }
        
        // 이미 연결되어 있다면 무시
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Already connected to network");
            return;
        }
        
        // Multiplayer Play Mode에서 첫 번째 인스턴스는 Host, 나머지는 Client
        bool isFirstInstance = IsFirstInstance();
        
        if (isFirstInstance)
        {
            Debug.Log("Starting as Host (first instance)");
            StartHost();
        }
        else
        {
            Debug.Log("Starting as Client (subsequent instance)");
            // 잠시 대기 후 클라이언트로 연결 (Host가 먼저 시작될 시간을 줌)
            Invoke(nameof(StartClient), 1f);
        }
    }
    
    bool IsFirstInstance()
    {
        // Multiplayer Play Mode에서 인스턴스 구분
        // 간단한 방법: 프로세스 ID나 랜덤 값 사용
        return Random.Range(0f, 1f) > 0.5f || Application.isEditor;
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start host: NetworkManager.Singleton is null!");
            return;
        }
        
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Successfully started as Host");
        }
        else
        {
            Debug.LogError("Failed to start as Host");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start client: NetworkManager.Singleton is null!");
            return;
        }
        
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Successfully started as Client");
        }
        else
        {
            Debug.LogError("Failed to start as Client");
        }
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start server: NetworkManager.Singleton is null!");
            return;
        }
        
        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Successfully started as Server");
        }
        else
        {
            Debug.LogError("Failed to start as Server");
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        
        // NetworkManager 상태 확인
        if (NetworkManager.Singleton == null)
        {
            GUILayout.Label("Status: NO NETWORK MANAGER", GUI.skin.box);
            GUILayout.Label("NetworkManager.Singleton is null!");
            GUILayout.EndArea();
            return;
        }
        
        // 연결 상태 표시
        if (NetworkManager.Singleton.IsHost)
        {
            GUILayout.Label("Status: HOST", GUI.skin.box);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            GUILayout.Label("Status: CLIENT", GUI.skin.box);
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("Status: SERVER", GUI.skin.box);
        }
        else
        {
            GUILayout.Label("Status: DISCONNECTED", GUI.skin.box);
        }
        
        GUILayout.Label($"Connected Players: {NetworkManager.Singleton.ConnectedClients?.Count ?? 0}");
        
        GUILayout.Space(10);
        
        // 연결 버튼들
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host"))
                StartHost();
            if (GUILayout.Button("Start Client"))
                StartClient();
            if (GUILayout.Button("Start Server"))
                StartServer();
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                NetworkManager.Singleton.Shutdown();
                
                // 씬 Player 다시 활성화
                var scenePlayer = GameObject.FindWithTag("Player");
                if (scenePlayer != null && !scenePlayer.activeSelf)
                {
                    scenePlayer.SetActive(true);
                }
            }
        }
        
        GUILayout.Space(10);
        
        // 테스트 정보
        GUILayout.Label("=== MULTIPLAYER TEST ===");
        GUILayout.Label("1. First instance: Press H (Host)");
        GUILayout.Label("2. Second instance: Press C (Client)");
        GUILayout.Space(5);
        GUILayout.Label("Controls:");
        GUILayout.Label("H - Start Host");
        GUILayout.Label("C - Start Client");
        GUILayout.Label("S - Start Server");
        GUILayout.Space(5);
        GUILayout.Label("Player Controls:");
        GUILayout.Label("Move: WASD/Arrows");
        GUILayout.Label("Jump: Space");
        
        GUILayout.EndArea();
    }
}