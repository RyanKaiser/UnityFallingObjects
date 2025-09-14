using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public Material[] playerMaterials;
    
    private static PlayerSpawnManager instance;
    public static PlayerSpawnManager Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        
        // 플레이어 스폰 위치 결정
        Vector3 spawnPosition = GetSpawnPosition(clientId);
        
        // 플레이어 스폰
        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject != null)
        {
            playerObject.transform.position = spawnPosition;
            
            // 플레이어 외관 설정
            var networkPlayer = playerObject.GetComponent<NetworkPlayerController>();
            if (networkPlayer != null)
            {
                SetPlayerAppearanceClientRpc(clientId, (int)clientId % playerMaterials.Length);
            }
        }
    }
    
    private Vector3 GetSpawnPosition(ulong clientId)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = (int)clientId % spawnPoints.Length;
            return spawnPoints[spawnIndex].position;
        }
        
        // 기본 스폰 위치 (플레이어 ID에 따라 X 위치 조정)
        return new Vector3((clientId * 2f) - 3f, 0f, 0f);
    }
    
    [ClientRpc]
    private void SetPlayerAppearanceClientRpc(ulong clientId, int materialIndex)
    {
        var players = FindObjectsOfType<NetworkPlayerController>();
        foreach (var player in players)
        {
            if (player.OwnerClientId == clientId)
            {
                var spriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null && playerMaterials.Length > materialIndex)
                {
                    spriteRenderer.material = playerMaterials[materialIndex];
                }
                break;
            }
        }
    }
    
    public Material GetPlayerMaterial(int index)
    {
        if (playerMaterials != null && index < playerMaterials.Length)
        {
            return playerMaterials[index];
        }
        return null;
    }
}