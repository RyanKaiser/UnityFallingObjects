using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkMovementManager : NetworkBehaviour
{
    [Header("Movement Settings")]
    public bool useNetworkRigidbody = true;
    
    private NetworkTransform networkTransform;
    private Unity.Netcode.Components.NetworkRigidbody2D networkRigidbody;
    
    public override void OnNetworkSpawn()
    {
        networkTransform = GetComponent<NetworkTransform>();
        networkRigidbody = GetComponent<Unity.Netcode.Components.NetworkRigidbody2D>();
        
        if (IsOwner)
        {
            // 클라이언트 권한 설정
            if (networkTransform != null)
            {
                // NetworkTransform을 비활성화하고 NetworkRigidbody2D 사용
                networkTransform.enabled = !useNetworkRigidbody;
            }
            
            if (networkRigidbody != null)
            {
                networkRigidbody.enabled = useNetworkRigidbody;
            }
            
            Debug.Log($"Player {OwnerClientId} movement setup - NetworkRigidbody: {useNetworkRigidbody}");
        }
    }
}