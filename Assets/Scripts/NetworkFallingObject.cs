using Unity.Netcode;
using UnityEngine;

public class NetworkFallingObject : NetworkBehaviour
{
    [Header("Object Settings")]
    public float fallSpeed = 5f;
    public float destroyTime = 10f;
    
    private Rigidbody2D rb;
    private float spawnTime;
    
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
        
        if (IsServer)
        {
            // 서버에서만 물리 시뮬레이션 적용
            if (rb != null)
            {
                rb.linearVelocity = Vector2.down * fallSpeed;
            }
        }
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        // 일정 시간 후 자동 삭제
        if (Time.time - spawnTime > destroyTime)
        {
            DestroyObject();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsServer) return;
        
        // 땅에 닿으면 삭제
        if (other.collider.CompareTag("Ground"))
        {
            DestroyObject();
        }
    }
    
    private void DestroyObject()
    {
        if (IsServer && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}