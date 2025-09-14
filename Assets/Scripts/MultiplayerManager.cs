using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Room Settings")]
    public int maxPlayers = 4;
    
    [Header("UI References")]
    public GameObject hostButton;
    public GameObject joinButton;
    public GameObject leaveButton;
    
    private void Start()
    {
        UpdateUI();
    }
    
    private void OnEnable()
    {
        // NetworkManager 이벤트 구독
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    private void OnDisable()
    {
        // NetworkManager 이벤트 구독 해제
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    public void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started successfully");
            UpdateUI();
        }
        else
        {
            Debug.LogError("Failed to start host");
        }
    }
    
    public void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully");
            UpdateUI();
        }
        else
        {
            Debug.LogError("Failed to start client");
        }
    }
    
    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        bool isConnected = NetworkManager.Singleton != null && 
                          (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost);
        
        hostButton?.SetActive(!isConnected);
        joinButton?.SetActive(!isConnected);
        leaveButton?.SetActive(isConnected);
    }
    
    private void OnClientConnected(ulong clientId)
    {
        // 호스트에서만 플레이어 수 제한 체크
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log($"Client {clientId} connected. Total players: {NetworkManager.Singleton.ConnectedClients.Count}");
            
            if (NetworkManager.Singleton.ConnectedClients.Count > maxPlayers)
            {
                Debug.Log($"Max players ({maxPlayers}) reached. Disconnecting client {clientId}");
                NetworkManager.Singleton.DisconnectClient(clientId);
            }
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected. Total players: {NetworkManager.Singleton.ConnectedClients.Count}");
    }
}
