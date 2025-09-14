using Unity.Netcode;
using UnityEngine;
using TMPro;

public class NetworkGameController : NetworkBehaviour
{
    [Header("Game Settings")]
    public GameObject fallingObjectPrefab;
    public Transform spawnArea;
    public float spawnInterval = 2f;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI playersAliveText;
    public GameObject gameOverPanel;
    
    private NetworkVariable<int> gameScore = new NetworkVariable<int>(0);
    private NetworkVariable<int> playersAlive = new NetworkVariable<int>(0);
    
    private float spawnTimer;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkPlayerController.OnPlayerDied += OnPlayerDied;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            UpdatePlayersAlive();
        }
        
        gameScore.OnValueChanged += OnScoreChanged;
        playersAlive.OnValueChanged += OnPlayersAliveChanged;
        
        UpdateUI();
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkPlayerController.OnPlayerDied -= OnPlayerDied;
        }
        
        gameScore.OnValueChanged -= OnScoreChanged;
        playersAlive.OnValueChanged -= OnPlayersAliveChanged;
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnFallingObject();
            spawnTimer = 0f;
        }
    }
    
    private void SpawnFallingObject()
    {
        if (spawnArea == null || fallingObjectPrefab == null) return;
        
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnArea.position.x - spawnArea.localScale.x / 2, 
                        spawnArea.position.x + spawnArea.localScale.x / 2),
            spawnArea.position.y,
            spawnArea.position.z
        );
        
        GameObject fallingObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
        fallingObject.GetComponent<NetworkObject>().Spawn();
        
        AddScore(10);
    }
    
    private void AddScore(int points)
    {
        if (!IsServer) return;
        gameScore.Value += points;
    }
    
    private void OnPlayerDied(ulong clientId)
    {
        UpdatePlayersAlive();
        
        if (playersAlive.Value <= 0)
        {
            EndGameClientRpc();
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        UpdatePlayersAlive();
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        UpdatePlayersAlive();
    }
    
    private void UpdatePlayersAlive()
    {
        if (!IsServer) return;
        
        int aliveCount = 0;
        foreach (var player in FindObjectsOfType<NetworkPlayerController>())
        {
            if (player.gameObject.activeInHierarchy)
                aliveCount++;
        }
        
        playersAlive.Value = aliveCount;
    }
    
    [ClientRpc]
    private void EndGameClientRpc()
    {
        gameOverPanel?.SetActive(true);
    }
    
    private void OnScoreChanged(int previousValue, int newValue)
    {
        UpdateUI();
    }
    
    private void OnPlayersAliveChanged(int previousValue, int newValue)
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {gameScore.Value}";
            
        if (playersAliveText != null)
            playersAliveText.text = $"Players Alive: {playersAlive.Value}";
    }
    
    public void RestartGame()
    {
        if (!IsServer) return;
        
        RestartGameClientRpc();
    }
    
    [ClientRpc]
    private void RestartGameClientRpc()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (!IsServer) return;
        
        // 게임 시작 시 초기화
        gameScore.Value = 0;
        UpdatePlayersAlive();
        spawnTimer = 0f;
        
        StartGameClientRpc();
    }
    
    [ClientRpc]
    private void StartGameClientRpc()
    {
        // 게임 오버 패널 숨기기
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // UI 업데이트
        UpdateUI();
        
        Debug.Log("게임이 시작되었습니다!");
    }
}
