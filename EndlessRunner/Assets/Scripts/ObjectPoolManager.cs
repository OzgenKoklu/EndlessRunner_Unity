using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private Transform _objectPoolLocationTransform;

    [System.Serializable]
    public class Pool
    {
        public string ObjectName;
        public GameObject Prefab;
        public int PoolSize;
    }

    public static ObjectPoolManager Instance;

    public delegate void OnInitializationComplete();
    public event OnInitializationComplete onInitializationComplete;
    private bool _isInPoolInitializationPhase = false;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;

        InitializePools();
    }


    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.PoolSize; i++)
            {
                GameObject gameObjectInPool = Instantiate(pool.Prefab);
                gameObjectInPool.SetActive(false);
                gameObjectInPool.transform.position = _objectPoolLocationTransform.transform.position;
                objectPool.Enqueue(gameObjectInPool);
            }

            poolDictionary.Add(pool.ObjectName, objectPool);
        }
        _isInPoolInitializationPhase = true;  
    }

    void Update()
    {
        //this is in update due to pool initialization taking too long and levelGenerator misses the event trigger if subbed on start.
        if (_isInPoolInitializationPhase)
        {
            onInitializationComplete?.Invoke();
            _isInPoolInitializationPhase = false;
        }
    }

    public GameObject SpawnFromPool(string ObjectName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.TryGetValue(ObjectName, out Queue<GameObject> objectQueue))
        {
            Debug.LogWarning($"Pool with name {ObjectName} doesn't exist.");
            return null;
        }

        if (objectQueue.Count == 0)
        {
            Debug.LogWarning($"No objects available in pool {ObjectName}");
            return null; // Or handle expansion logic here
        }


        GameObject objectToSpawn = poolDictionary[ObjectName].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }


    public void ReturnToPool(string ObjectName, GameObject objectToReturn)
    {
        if (!poolDictionary.TryGetValue(ObjectName, out Queue<GameObject> objectQueue))
        {
            Debug.LogWarning($"ReturnToPool: Pool with tag {ObjectName} doesn't exist.");
            return;
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.SetParent(null);
        objectToReturn.transform.position = _objectPoolLocationTransform.position;

        poolDictionary[ObjectName].Enqueue(objectToReturn);
    }


    //returns if any ground plane in pool that is inactive.
    public bool HasGroundPlaneInPool()
    {
        if (!poolDictionary.TryGetValue("GroundPlane", out Queue<GameObject> objectQueue))
        {
            return false; // No GroundPlane pool exists
        }

        return objectQueue.Any(item => !item.activeInHierarchy);
    }

}