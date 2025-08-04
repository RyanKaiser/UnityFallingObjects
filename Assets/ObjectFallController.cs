using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectFallController : MonoBehaviour
{
    [SerializeField] private GameObject _fallingObject;
    [SerializeField] private float _time;
    [SerializeField] private float _repeatTime;
    [SerializeField] private int _maxPool = 10;

    private ObjectPool<GameObject> _fallingObjects;
    void Start()
    {
        InitObjetPool();
        InvokeRepeating("Fall", _time, _repeatTime);
    }

    void InitObjetPool()
    {
        _fallingObjects = new ObjectPool<GameObject>(
            createFunc: () => {
                var obj = Instantiate(_fallingObject);
                obj.GetComponent<FallingObject>().SetPool(_fallingObjects);
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    void Fall()
    {
        var obj = _fallingObjects.Get();

        if (obj != null)
        {
            var pos = new Vector3(Random.Range(-10, 10), 10, 0);
            obj.transform.position = pos;
            obj.SetActive(true);
        }
    }
}
