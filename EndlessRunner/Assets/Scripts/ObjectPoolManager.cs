using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{

    private Transform inactiveObjectsParent;

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
        inactiveObjectsParent = new GameObject("InactivePoolObjects").transform;
        InitializePools();
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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

    //For spawning inside a parent
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Set the parent of the spawned object if a parent transform is provided
        if (parent != null)
        {
            objectToSpawn.transform.SetParent(parent, false);
        }

        poolDictionary[tag].Enqueue(objectToSpawn);

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
        objectToReturn.transform.SetParent(inactiveObjectsParent);
        //objectToReturn.transform.SetParent(false);
        poolDictionary[objectName].Enqueue(objectToReturn);
    }
}