using UnityEngine;

public class PoolItem : MonoBehaviour
{
    // Reference to the ScriptableObject that defines this item
    public LevelObjectSO levelObjectInfo;

    /*
    public void Activate(Vector3 position, Quaternion rotation, Transform parent)
    {
        transform.position = position;
        transform.rotation = rotation;
        if (parent != null)
        {
            transform.SetParent(parent, false);
        }
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        transform.SetParent(null);
        //ObjectPoolManager.Instance.ReturnToPool(levelObjectInfo.ObjectName, gameObject);
    }
    */
}