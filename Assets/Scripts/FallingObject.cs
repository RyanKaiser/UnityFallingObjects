using UnityEngine;
using UnityEngine.Pool;

public class FallingObject : MonoBehaviour
{
    private IObjectPool<GameObject> _pool;

    public void SetPool(IObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _pool.Release(gameObject);
    }
}
