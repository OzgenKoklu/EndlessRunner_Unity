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
}
