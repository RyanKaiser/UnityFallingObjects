using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class MultiplayerUI : MonoBehaviour
{
    [Header("Connection UI")]
    public Button hostButton;
    public Button joinButton;
    public Button leaveButton;
    public TMP_InputField ipAddressField;
    
    [Header("Game UI")]
    public TextMeshProUGUI connectionStatusText;
    public TextMeshProUGUI playerCountText;
    public GameObject connectionPanel;
    public GameObject gamePanel;
    
    [Header("Lobby UI")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI lobbyPlayerCountText;
    public Button startGameButton;
    
    private MultiplayerManager multiplayerManager;
    
    private void Start()
    {
        multiplayerManager = FindObjectOfType<MultiplayerManager>();
        
        if (hostButton != null)
            hostButton.onClick.AddListener(StartHost);
            
        if (joinButton != null)
            joinButton.onClick.AddListener(StartClient);
            
        if (leaveButton != null)
            leaveButton.onClick.AddListener(LeaveGame);
            
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
        
        UpdateUI();
        
        // Set default IP if field is empty
        if (ipAddressField != null && string.IsNullOrEmpty(ipAddressField.text))
        {
            ipAddressField.text = "127.0.0.1";
        }
    }
    
    private void StartHost()
    {
        if (multiplayerManager != null)
        {
            multiplayerManager.StartHost();
            UpdateConnectionStatus("Start hosting...");
            ShowLobby();
        }
    }
    
    private void StartClient()
    {
        if (multiplayerManager != null)
        {
            string ipAddress = ipAddressField?.text;
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "127.0.0.1")
            {
                var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport != null)
                {
                    transport.ConnectionData.Address = ipAddress;
                }
            }
            
            multiplayerManager.StartClient();
            UpdateConnectionStatus("Connecting...");
        }
    }
    
    private void LeaveGame()
    {
        if (multiplayerManager != null)
        {
            multiplayerManager.LeaveGame();
            UpdateConnectionStatus("Disconnected");
            ShowConnectionPanel();
        }
    }
    
    private void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // 로비에서 게임으로 전환
            ShowGamePanel();
            
            // 모든 클라이언트에게 게임 시작 알림
            var networkGameController = FindObjectOfType<NetworkGameController>();
            if (networkGameController != null)
            {
                networkGameController.StartGameServerRpc();
            }
        }
    }
    
    private void Update()
    {
        UpdatePlayerCount();
        UpdateStartButton();
    }
    
    private void UpdateConnectionStatus(string status)
    {
        if (connectionStatusText != null)
            connectionStatusText.text = $"Status: {status}";
    }
    
    private void UpdatePlayerCount()
    {
        if (NetworkManager.Singleton != null)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            
            if (playerCountText != null)
                playerCountText.text = $"Players: {playerCount}/4";
                
            if (lobbyPlayerCountText != null)
                lobbyPlayerCountText.text = $"Players in waiting: {playerCount}/4";
        }
    }
    
    private void UpdateStartButton()
    {
        if (startGameButton != null && NetworkManager.Singleton != null)
        {
            bool canStart = NetworkManager.Singleton.IsHost && 
                           NetworkManager.Singleton.ConnectedClients.Count >= 1;
            startGameButton.interactable = canStart;
        }
    }
    
    private void UpdateUI()
    {
        bool isConnected = NetworkManager.Singleton != null && 
                          (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost);
        
        if (connectionPanel != null)
            connectionPanel.SetActive(!isConnected);
            
        if (gamePanel != null)
            gamePanel.SetActive(isConnected);
    }
    
    private void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
    }
    
    private void ShowLobby()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
    }
    
    private void ShowGamePanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
    }
    
    // NetworkManager 이벤트 구독을 위한 메서드들
    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            UpdateConnectionStatus("HOST");
        }
        else
        {
            UpdateConnectionStatus("Connected");
            ShowLobby();
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            ShowConnectionPanel();
        }
    }
}