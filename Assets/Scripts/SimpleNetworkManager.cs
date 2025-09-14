using Unity.Netcode;
using UnityEngine;

public class SimpleNetworkManager : MonoBehaviour
{
    [Header("Network Settings")]
    public int maxPlayers = 4;
    
    void Start()
    {
        // 자동으로 호스트 또는 클라이언트로 연결 시도
        TryConnectToGame();
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
        
        GUILayout.Label($"Players: {NetworkManager.Singleton.ConnectedClients.Count}");
        
        GUILayout.EndArea();
    }
}