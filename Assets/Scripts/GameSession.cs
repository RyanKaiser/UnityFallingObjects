using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GameSession : MonoBehaviour
{
    [Header("Session Settings")]
    public float gameStartDelay = 3f;
    public bool autoStartWhenReady = true;
    
    [Header("Game State")]
    private bool isGameStarted = false;
    private float gameStartTime = 0f;
    
    private Dictionary<ulong, bool> playersReady = new Dictionary<ulong, bool>();
    
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
        if (!NetworkManager.Singleton.IsHost) return;
        
        playersReady[clientId] = false;
        
        // 첫 번째 플레이어가 호스트가 되어 게임을 시작할 수 있도록 알림
        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            Debug.Log("당신이 호스트입니다! 다른 플레이어들을 기다린 후 게임을 시작할 수 있습니다.");
        }
        
        Debug.Log($"세션 상태: {GetReadyPlayerCount()}/{NetworkManager.Singleton.ConnectedClients.Count} 플레이어 준비됨");
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        if (playersReady.ContainsKey(clientId))
        {
            playersReady.Remove(clientId);
        }
        
        Debug.Log($"세션 상태: {GetReadyPlayerCount()}/{NetworkManager.Singleton.ConnectedClients.Count} 플레이어 준비됨");
        
        // 호스트가 나가면 게임 종료
        if (clientId == 0)
        {
            Debug.Log("호스트가 나가서 세션이 종료되었습니다.");
            
            // 메인 메뉴로 돌아가기
            if (!NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
    
    public void SetPlayerReady(ulong clientId, bool ready)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        playersReady[clientId] = ready;
        
        int readyCount = GetReadyPlayerCount();
        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        
        Debug.Log($"세션 상태: {readyCount}/{totalPlayers} 플레이어 준비됨");
        
        // 모든 플레이어가 준비되고 최소 1명 이상이면 게임 시작
        if (autoStartWhenReady && readyCount >= 1 && readyCount == totalPlayers && !isGameStarted)
        {
            StartGameSession();
        }
    }
    
    public void StartGameSession()
    {
        if (!NetworkManager.Singleton.IsHost || isGameStarted) return;
        
        isGameStarted = true;
        gameStartTime = Time.time;
        
        // 게임 딜레이 후 실제 게임 시작
        Invoke(nameof(StartActualGame), gameStartDelay);
        
        Debug.Log($"게임이 {gameStartDelay}초 후에 시작됩니다!");
        
        // 카운트다운 UI 표시
        StartCoroutine(ShowCountdown());
    }
    
    private void StartActualGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        var networkGameController = FindObjectOfType<NetworkGameController>();
        if (networkGameController != null)
        {
            networkGameController.StartGameServerRpc();
        }
    }
    
    private int GetReadyPlayerCount()
    {
        int count = 0;
        foreach (var ready in playersReady.Values)
        {
            if (ready) count++;
        }
        return count;
    }
    
    private System.Collections.IEnumerator ShowCountdown()
    {
        for (int i = (int)gameStartDelay; i > 0; i--)
        {
            Debug.Log($"게임 시작: {i}");
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("게임 시작!");
    }
    
    // 퍼블릭 메서드들
    public bool IsGameInProgress()
    {
        return isGameStarted;
    }
    
    public void SetReady(bool ready)
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            SetPlayerReady(NetworkManager.Singleton.LocalClientId, ready);
        }
    }
}
