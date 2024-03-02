using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private LevelGenerator _levelGenerator;

    void LateUpdate()
    {
        if (ObjectPoolManager.Instance.HasGroundPlaneInPool())
        {
            _levelGenerator.PopulateMapData();
        }
    }
}
