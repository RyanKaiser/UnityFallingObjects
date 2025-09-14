using Unity.Netcode;
using UnityEngine;

public class SimpleNetworkManager : MonoBehaviour
{
    [Header("Network Settings")]
    public int maxPlayers = 4;
    
    void Start()
    {
        // NetworkManager의 연결 이벤트 구독
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // 자동으로 호스트 또는 클라이언트로 연결 시도
        TryConnectToGame();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
        
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
        Debug.Log($"Client {clientId} disconnected");
    }

    void Update()
    {
        // 테스트용 키보드 입력
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartHost();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            StartClient();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            StartServer();
        }
    }

    public void TryConnectToGame()
    {
        // 프로토타입에서는 간단하게 호스트로 시작
        // 나중에 실제 검색 로직으로 교체 예정
        StartHost();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Started as Host");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Started as Client");
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        Debug.Log("Started as Server");
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))
                StartHost();
            if (GUILayout.Button("Client"))
                StartClient();
            if (GUILayout.Button("Server"))
                StartServer();
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                NetworkManager.Singleton.Shutdown();
                
                // 씬 Player 다시 활성화
                var scenePlayer = GameObject.FindWithTag("Player");
                if (scenePlayer != null)
                {
                    scenePlayer.SetActive(true);
                }
            }
        }
        
        GUILayout.Label($"Players: {NetworkManager.Singleton.ConnectedClients.Count}");
        
        if (NetworkManager.Singleton.IsHost)
            GUILayout.Label("Mode: Host");
        else if (NetworkManager.Singleton.IsClient)
            GUILayout.Label("Mode: Client");
        else if (NetworkManager.Singleton.IsServer)
            GUILayout.Label("Mode: Server");
        
        GUILayout.EndArea();
    }
}