using UnityEngine;

public class PoolItem : MonoBehaviour
{
    [SerializeField] private LevelObjectSO _levelObjectInfo;
    public LevelObjectSO GetLevelObjectSO() { return _levelObjectInfo; }
}