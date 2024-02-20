using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private LevelObjectListSO _levelObjectListSO;
    [SerializeField] private Transform _groundPlanePrefabTransform;

    private Vector3 _generationPosition = new Vector3(0, 0, 100);

    private Vector3 _leftLanePosition = new Vector3(-0.35f, 0, 0);

    private Vector3 _middleLanePosition = new Vector3(0, 0, 0);
    private Vector3 _rightLanePosition = new Vector3(0.35f, 0, 0);
    private float _spawnOffset = 1f;


    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        if (_levelObjectListSO == null || _groundPlanePrefabTransform == null)
        {
            Debug.LogError("Level Object List or Ground Plane Prefab is not assigned!");
            return;
        }

        // Instantiate the ground plane prefab
        GameObject groundPlane = Instantiate(_groundPlanePrefabTransform.gameObject, _generationPosition, Quaternion.identity);

        // Populate the ground plane with child objects
        PopulateGroundPlane(groundPlane.transform);
    }

    private void PopulateGroundPlane(Transform groundPlaneTransform)
    {
        foreach (Vector3 lanePosition in new Vector3[] { _leftLanePosition, _middleLanePosition, _rightLanePosition })
        {
            foreach (LevelObjectSO levelObject in _levelObjectListSO.levelObjectSOList)
            {
                if (Random.value < 0.5f) // Adjust this threshold according to your needs
                {
                    // Randomly select a level object from the list
                   // LevelObjectSO levelObject = _levelObjectListSO.levelObjectSOList[Random.Range(0, _levelObjectListSO.levelObjectSOList.Count)];

                    float zPosition = CalculateSpawnPosition(levelObject);
                    

                    // Calculate the spawn position relative to the ground plane's local space
                    Vector3 spawnPosition = groundPlaneTransform.TransformPoint(lanePosition + new Vector3(0, 0, zPosition));

                    // Instantiate the selected object as a child of the ground plane
                    Transform newGameObject = Instantiate(levelObject.Prefab, spawnPosition, Quaternion.identity, groundPlaneTransform);

                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                }
            }

        }
    }


    
    private void PopulateLane(Vector3 lanePosition)
    {
        // Define initial spawn position
        Vector3 spawnPosition = lanePosition;

        // Randomly select objects from the levelObjectListSO and instantiate them along the lane
        foreach (LevelObjectSO levelObject in _levelObjectListSO.levelObjectSOList)
        {
            // Randomly determine if the object should be spawned
            if (Random.value < 0.5f) // Adjust this threshold according to your needs
            {
                // Instantiate the object
                Instantiate(levelObject.Prefab, spawnPosition, Quaternion.identity);

                // Update spawn position for the next object
                spawnPosition.z += levelObject.ObjectSpawnBlockLenght + _spawnOffset;
            }
        }
    }

    float CalculateSpawnPosition(LevelObjectSO levelObjectSO)
    {
        

        float maxZPosition = 5 - levelObjectSO.ObjectSpawnBlockLenght; // Max Z position considering the object spawn block length
        float zPosition = Random.Range(-5, maxZPosition);
        return zPosition;
    }


}