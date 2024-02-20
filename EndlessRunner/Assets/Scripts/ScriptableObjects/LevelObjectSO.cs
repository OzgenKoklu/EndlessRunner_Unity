using UnityEngine;


[CreateAssetMenu()]
public class LevelObjectSO : ScriptableObject
{
    public enum LevelObjectKind
    {
        SlidingObstacle,
        JumpingObstacle,
        Platform,
        PlatformWithRamp,
        Collectible,
        EnvironmetObject,
    }

    public Transform Prefab;
    public LevelObjectKind Kind;
    public string ObjectName;
    //This is for random spawn blockage after this specific object is placed on precedurally generated map.
    public float ObjectSpawnBlockLenght;

    //might be customized by adding height or some other metrics later on.
    
}
