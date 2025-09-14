using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class NetworkSetupRequirements
{
    [Header("필수 컴포넌트")]
    [Tooltip("NetworkManager GameObject가 씬에 존재해야 합니다")]
    public bool hasNetworkManager;
    
    [Tooltip("Player Prefab이 NetworkObject 컴포넌트를 가져야 합니다")]
    public bool playerPrefabHasNetworkObject;
    
    [Tooltip("FallingObject Prefab이 NetworkObject 컴포넌트를 가져야 합니다")]
    public bool fallingObjectHasNetworkObject;
    
    [Tooltip("NetworkGameController가 씬에 존재해야 합니다")]
    public bool hasNetworkGameController;
    
    [Tooltip("MultiplayerUI가 설정되어야 합니다")]
    public bool hasMultiplayerUI;
}

public class NetworkSetupGuide : MonoBehaviour
{
    [Header("설정 상태 확인")]
    public NetworkSetupRequirements requirements;
    
    [Header("도움말")]
    [TextArea(5, 10)]
    public string setupInstructions = @"멀티플레이어 설정 가이드:

1. NetworkManager 설정:
   - 빈 GameObject 생성 → 'NetworkManager' 이름 변경
   - NetworkManager 컴포넌트 추가
   - UnityTransport 컴포넌트 추가
   - MultiplayerManager 스크립트 추가

2. Player Prefab 설정:
   - Player GameObject를 Prefab으로 변환
   - NetworkObject 컴포넌트 추가
   - ClientNetworkTransform 컴포넌트 추가
   - NetworkPlayerController 스크립트 추가
   - NetworkManager의 PlayerPrefab 필드에 할당

3. FallingObject Prefab 설정:
   - NetworkObject 컴포넌트 추가
   - NetworkFallingObject 스크립트 추가
   - NetworkManager의 Prefabs List에 추가

4. UI 설정:
   - Canvas에 MultiplayerUI 스크립트 추가
   - 버튼들 연결 (Host, Join, Leave)
   - 텍스트 UI 연결

5. 카메라 설정:
   - Main Camera에 CameraFollow 스크립트 추가

6. 씬 빌드 설정:
   - File → Build Settings
   - 현재 씬을 Build Settings에 추가";
    
    [ContextMenu("설정 상태 확인")]
    public void CheckSetupStatus()
    {
        requirements.hasNetworkManager = FindObjectOfType<NetworkManager>() != null;
        requirements.hasNetworkGameController = FindObjectOfType<NetworkGameController>() != null;
        requirements.hasMultiplayerUI = FindObjectOfType<MultiplayerUI>() != null;
        
        var networkManager = NetworkManager.Singleton;
        if (networkManager != null)
        {
            requirements.playerPrefabHasNetworkObject = 
                networkManager.NetworkConfig.PlayerPrefab != null &&
                networkManager.NetworkConfig.PlayerPrefab.GetComponent<NetworkObject>() != null;
        }
        
        Debug.Log("설정 상태가 업데이트되었습니다. Inspector를 확인하세요.");
    }
    
    private void Start()
    {
        CheckSetupStatus();
        
        if (!IsSetupComplete())
        {
            Debug.LogWarning("멀티플레이어 설정이 완료되지 않았습니다. NetworkSetupGuide의 setupInstructions를 참고하세요.");
        }
    }
    
    public bool IsSetupComplete()
    {
        return requirements.hasNetworkManager &&
               requirements.playerPrefabHasNetworkObject &&
               requirements.hasNetworkGameController &&
               requirements.hasMultiplayerUI;
    }
}