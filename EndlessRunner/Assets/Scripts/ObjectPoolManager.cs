using System.Collections;
using System.Collections.Generic;
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
    public bool isPoolReady = false;
    private bool objectPoolingInProgress = false;

    private void Awake()
    {
        Instance = this;

        InitializePools();
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Update()
    {
        if (!isPoolReady)
        {
            onInitializationComplete?.Invoke();
            isPoolReady = true;
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

        if (poolDictionary[ObjectName].Count > 0)
        {
            GameObject objectToSpawn = poolDictionary[ObjectName].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            return objectToSpawn;
        }
        else
        {
            // Optionally handle the case where there are no objects left in the pool
            // For example, instantiate a new one or return null
            return null;
        }
    }


    public void ReturnToPool(string objectName, GameObject objectToReturn)
    {
        objectPoolingInProgress = true;
        StartCoroutine(ReturnToPoolAsync(objectName, objectToReturn));
    }

    private IEnumerator ReturnToPoolAsync(string objectName, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(objectName))
        {
            Debug.LogWarning($"ReturnToPool: Pool with tag {objectName} doesn't exist.");
            yield break; // Exit if the object name doesn't exist in the dictionary
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.SetParent(null);
        objectToReturn.transform.position = _objectPoolLocationTransform.position;

        // Here you could wait for something, like a fade out animation:
        // yield return new WaitForSeconds(1f); // Wait for 1 second, for example

        poolDictionary[objectName].Enqueue(objectToReturn);

        // Set your flag here to indicate the task is complete
        objectPoolingInProgress = false;
    }

    public bool IsObjectPoolingInProgress()
    {
        return objectPoolingInProgress;
    }

    public bool HasGroundPlaneInPool()
    {
        string objectName = "GroundPlane"; // Use the exact name used in your pool

        // Check if the pool for GroundPlane exists and has any inactive objects left
        if (poolDictionary.ContainsKey(objectName) && poolDictionary[objectName].Count > 0)
        {
            // Iterate through the queue to check for an inactive object
            foreach (var item in poolDictionary[objectName])
            {
                if (!item.activeInHierarchy) // Check if the object is inactive
                {
                    return true; // There is at least one inactive 'GroundPlane' object
                }
            }
        }

        return false; // No inactive 'GroundPlane' objects are left in the pool
    }

}