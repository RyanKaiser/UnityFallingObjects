using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    private Transform target;
    private Camera cam;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
    
    private void Update()
    {
        // 멀티플레이어에서 모든 플레이어를 볼 수 있도록 카메라 조정
        if (target == null)
        {
            AutoFocusOnPlayers();
        }
    }
    
    private void AutoFocusOnPlayers()
    {
        var players = FindObjectsOfType<NetworkPlayerController>();
        if (players.Length == 0) return;
        
        Vector3 center = Vector3.zero;
        foreach (var player in players)
        {
            if (player.gameObject.activeInHierarchy)
            {
                center += player.transform.position;
            }
        }
        center /= players.Length;
        
        Vector3 desiredPosition = center + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * 0.5f);
        transform.position = smoothedPosition;
        
        // 플레이어들이 퍼져 있으면 카메라 줌 아웃
        float maxDistance = 0f;
        foreach (var player in players)
        {
            if (player.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(center, player.transform.position);
                maxDistance = Mathf.Max(maxDistance, distance);
            }
        }
        
        float targetSize = Mathf.Clamp(5f + maxDistance * 0.5f, 5f, 12f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime);
    }
}