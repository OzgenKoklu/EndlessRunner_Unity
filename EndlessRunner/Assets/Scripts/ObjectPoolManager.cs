using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private Transform _objectPoolLocationTransform;
    
    [System.Serializable]
    public class Pool
    {
        public string ObjectName;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPoolManager Instance;

    public delegate void OnInitializationComplete();
    public event OnInitializationComplete onInitializationComplete;
    public bool isPoolNotReady = false; 

    private void Awake()
    {
        Instance = this;

        InitializePools();
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Update()
    {
        if (!isPoolNotReady)
        {
            onInitializationComplete?.Invoke();
            isPoolNotReady = true;
        }
    }
    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.position = _objectPoolLocationTransform.transform.position;
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.ObjectName, objectPool);
        }

        // Move pool initialization to a separate method called from Awake
        onInitializationComplete?.Invoke();
    }

    public GameObject SpawnFromPool(string ObjectName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(ObjectName))
        {
            Debug.LogWarning("Pool with tag " + ObjectName + " doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[ObjectName].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[ObjectName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void ReturnToPool(string objectName, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(objectName))
        {
            Debug.LogWarning($"ReturnToPool: Pool with tag {objectName} doesn't exist.");
            return;
        }
        objectToReturn.SetActive(false);

        objectToReturn.transform.SetParent(null);

        objectToReturn.transform.position = _objectPoolLocationTransform.transform.position;
   
        poolDictionary[objectName].Enqueue(objectToReturn);

    }
}