using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private LevelObjectListSO _levelObjectListSO;
    [SerializeField] private Transform _groundPlanePrefabTransform;
    [SerializeField] private LevelObjectSO _platformSO;
    [SerializeField] private LevelObjectSO _platformWithRampSO;



    private Vector3 _generationPosition = new Vector3(0, 0, 100);


    private Vector3 _leftLanePosition = new Vector3(-0.35f, 0, 0);
    private Vector3 _middleLanePosition = new Vector3(0, 0, 0);
    private Vector3 _rightLanePosition = new Vector3(0.35f, 0, 0);

    private List<Vector3> _lanePositions;
    private float _spawnOffset = 1f;

    public enum SegmentType
    {
        Empty,
        Platform,
        PlatformWithRamp
    }

    public class Segment
    {
        public SegmentType Type { get; set; }
    }

    //this will be a 3x10 segment map
    private Segment[,] _mapData;


    private void Start()
    {
        int laneCount = 3;
        int segmentCount = 10;
        _lanePositions = new List<Vector3>();
        _lanePositions.Add(_leftLanePosition);
        _lanePositions.Add(_middleLanePosition);
        _lanePositions.Add(_rightLanePosition);
        SegmentMap(laneCount, segmentCount);
       // GenerateLevel();
    }

    public void SegmentMap(int lanes, int segments)
    {
        _mapData = new Segment[lanes, segments];
        InitializeMap();
    }

    private void InitializeMap()
    {
        // Initialize the map with empty segments
        for (int i = 0; i < _mapData.GetLength(0); i++)
        {
            for (int j = 0; j < _mapData.GetLength(1); j++)
            {
                _mapData[i, j] = new Segment { Type = SegmentType.Empty };
            }
        }

        // Populate the map with platforms and ramps
        PopulateMap();
        GameObject groundPlane = Instantiate(_groundPlanePrefabTransform.gameObject, _generationPosition, Quaternion.identity);
        GenerateLevelWithMapData(groundPlane.transform);
    }
    /*
    private void PopulateMap()
    {
        // Add platforms and ramps to the map
        for (int i = 0; i < _mapData.GetLength(0); i++)
        {


            for (int j = 0; j < _mapData.GetLength(1); j++)
            {
                // Add platforms or ramps based on random conditions
                if (RandomShouldAddPlatform())
                {
                    _mapData[i, j].Type = SegmentType.Platform;
                    if (j + 1 < _mapData.GetLength(1))
                    {
                        _mapData[i, j + 1].Type = SegmentType.Empty;
                    }
                }
                else if (RandomShouldAddPlatformWithRamp())
                {
                    _mapData[i, j].Type = SegmentType.PlatformWithRamp;
                    if (j + 1 < _mapData.GetLength(1) && RandomShouldAddPlatformNext())
                    {
                        _mapData[i, j + 1].Type = SegmentType.Platform;
                    }
                }
            }
        }

        // Ensure at least one lane is passable on the ground level
        EnsureGroundPassable();
    }
    */

    private void PopulateMap()
    {
        // Flags to track if a ramp has been spawned in the current row
        bool rampSpawned = false;

        // Add platforms and ramps to the map
        for (int j = 0; j < _mapData.GetLength(1); j++)
        {
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                // Add platforms or ramps based on random conditions
                if (RandomShouldAddPlatform())
                {
                    _mapData[i, j].Type = SegmentType.Platform;
                    if (j + 1 < _mapData.GetLength(1))
                    {
                        _mapData[i, j + 1].Type = SegmentType.Empty;
                    }
                }
                else if (RandomShouldAddPlatformWithRamp() && !rampSpawned && IsPreviousSegmentEmpty(i, j))
                {
                    _mapData[i, j].Type = SegmentType.PlatformWithRamp;
                    if (j + 1 < _mapData.GetLength(1) && RandomShouldAddPlatformNext())
                    {
                        _mapData[i, j + 1].Type = SegmentType.Platform;
                    }
                    rampSpawned = true; // Set the flag that a ramp has been spawned
                }
            }

            // Reset the ramp flag if all segments in the row are empty
            if (AllSegmentsEmpty(j))
            {
                rampSpawned = false;
            }
        }

        // Ensure at least one lane is passable on the ground level
        EnsureGroundPassable();
    }

    // Check if the previous segment in the same lane is empty
    private bool IsPreviousSegmentEmpty(int laneIndex, int segmentIndex)
    {
        if (segmentIndex > 0 && _mapData[laneIndex, segmentIndex - 1].Type == SegmentType.Empty)
        {
            return true;
        }
        return false;
    }

    // Check if all segments in the current row are empty
    private bool AllSegmentsEmpty(int segmentIndex)
    {
        for (int i = 0; i < _mapData.GetLength(0); i++)
        {
            if (_mapData[i, segmentIndex].Type != SegmentType.Empty)
            {
                return false;
            }
        }
        return true;
    }
    private bool RandomShouldAddPlatform()
    {
        // Implement your logic for adding platforms
        // Example: Return true with a certain probability
        return UnityEngine.Random.Range(0f, 1f) < 0.5f;
    }

    private bool RandomShouldAddPlatformWithRamp()
    {
        // Implement your logic for adding platforms with ramps
        // Example: Return true with a certain probability
        return UnityEngine.Random.Range(0f, 1f) < 0.3f;
    }

    private bool RandomShouldAddPlatformNext()
    {
        // Implement your logic for adding platform next to a ramp
        // Example: Return true with a certain probability
        return UnityEngine.Random.Range(0f, 1f) < 0.5f;
    }

    private void EnsureGroundPassable()
    {
        // Ensure that at least one lane is passable on the ground level
        // Example: Set a random segment in each lane to be empty
        for (int i = 0; i < _mapData.GetLength(1); i++)
        {
            if (i == 0)
            {
                int randomSegmentIndex = UnityEngine.Random.Range(0, _mapData.GetLength(0));
                _mapData[randomSegmentIndex, i].Type = SegmentType.Empty;
            }
            else
            {
                for(int j = 0; j< _mapData.GetLength(0); j++)
                {

                    //check if the privious segment is empty and this segment is not a ramp
                    if(_mapData[j, i-1].Type == SegmentType.Empty && _mapData[j, i].Type != SegmentType.PlatformWithRamp)
                    {
                        //checks if any line ther than this is empty 
                        if((!IsNeighborSegmentEmpty(j, i, -1) || !IsNeighborSegmentEmpty(j, i, 1)))
                        {
                            _mapData[j, i].Type = SegmentType.Empty;
                        }
                    }

                    //checks if path is blocked for the mid row.
                    if (j == 1 && _mapData[j, i].Type == SegmentType.Platform)
                    {
                        if(!IsCrossNeighborSegmentEmpty(j, i, -1,-1) && !IsCrossNeighborSegmentEmpty(j, i, 1, -1))
                        {
                            int randomDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                            _mapData[j+ randomDirection, i].Type = SegmentType.Empty;
                        }

                    }
                }

            }
            
        }
    }

    private bool IsNeighborSegmentEmpty(int laneIndex, int segmentIndex, int delta)
    {
        // Check if the neighboring segment at laneIndex + delta is within bounds
        if (laneIndex + delta >= 0 && laneIndex + delta < _mapData.GetLength(0))
        {
            // Check if the neighboring segment at laneIndex + delta is empty
            if (_mapData[laneIndex + delta, segmentIndex].Type == SegmentType.Empty)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCrossNeighborSegmentEmpty(int laneIndex, int segmentIndex, int delta, int deltaSegment)
    {
        // Check if the neighboring segment at laneIndex + delta is within bounds
        if (laneIndex + delta >= 0 && laneIndex + delta < _mapData.GetLength(0))
        {
            // Check if the neighboring segment at laneIndex + delta is empty
            if (_mapData[laneIndex + delta, segmentIndex + deltaSegment].Type == SegmentType.Empty)
            {
                return true;
            }
        }
        return false;
    }

    /*
    private void EnsureGroundPassable()
    {
        for (int laneIndex = 0; laneIndex < _mapData.GetLength(0); laneIndex++)
        {
            bool atLeastOneEmptySegment = false;
            for (int segmentIndex = 0; segmentIndex < _mapData.GetLength(1); segmentIndex++)
            {
                if (_mapData[laneIndex, segmentIndex].Type == SegmentType.Empty)
                {
                    atLeastOneEmptySegment = true;
                    break;
                }
            }

            if (!atLeastOneEmptySegment)
            {
                // Choose a random segment in the lane and make it empty (ensure it doesn't have a platform to its right)
                int randomSegmentIndex = UnityEngine.Random.Range(0, _mapData.GetLength(1) - 1);
                if (_mapData[laneIndex, randomSegmentIndex + 1].Type != SegmentType.Platform)
                {
                    _mapData[laneIndex, randomSegmentIndex].Type = SegmentType.Empty;
                }
                else
                {
                    // If the last segment is blocked, make the second-to-last empty
                    _mapData[laneIndex, randomSegmentIndex - 1].Type = SegmentType.Empty;
                }
            }
        }
    }

    */

    private void GenerateLevelWithMapData(Transform groundPlaneTransform)
    {
        GameObject groundPlane = Instantiate(_groundPlanePrefabTransform.gameObject, _generationPosition, Quaternion.identity);

        // Calculate the segment size in the Z direction
        float segmentSize = 10f / _mapData.GetLength(1);

        for (int laneIndex = 0; laneIndex < _mapData.GetLength(0); laneIndex++)
        {
           
            for (int segmentIndex = 0; segmentIndex < _mapData.GetLength(1); segmentIndex++)
            {
                // Calculate the position of the segment based on laneIndex and segmentIndex
                float posX = _lanePositions[laneIndex].x;
                Debug.Log(posX);
                float posZ = -5f + segmentSize * (segmentIndex + 0.5f);
                Debug.Log(segmentSize);
                Vector3 segmentPosition = new Vector3(posX, 0, posZ);
                

                Segment segment = _mapData[laneIndex, segmentIndex];

                if (segment.Type == SegmentType.PlatformWithRamp)
                {
                    // Instantiate the platform with ramp prefab
                    Vector3 spawnPosition = groundPlaneTransform.TransformPoint(segmentPosition);
                    GameObject newGameObject = Instantiate(_platformWithRampSO.Prefab.gameObject, spawnPosition, Quaternion.identity, groundPlane.transform);
                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                    Debug.Log("Segment position: " + segmentPosition);
                }
                else if(segment.Type == SegmentType.Platform)
                {
                    Vector3 spawnPosition = groundPlaneTransform.TransformPoint(segmentPosition);
                    GameObject newGameObject = Instantiate(_platformSO.Prefab.gameObject, spawnPosition, Quaternion.identity, groundPlane.transform);
                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                    Debug.Log("Segment position: " + segmentPosition);
                }
                else
                {

                }

            }
        }

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
            float minZposition = -5;

            foreach (LevelObjectSO levelObject in _levelObjectListSO.levelObjectSOList)
            {
                
                if (Random.value < 0.5f) // Adjust this threshold according to your needs
                {
                    // Randomly select a level object from the list
                    // LevelObjectSO levelObject = _levelObjectListSO.levelObjectSOList[Random.Range(0, _levelObjectListSO.levelObjectSOList.Count)];


                    bool changeMinPos;
                    float zPosition = CalculateSpawnPosition(levelObject, minZposition,out changeMinPos);
                    Debug.Log(changeMinPos);
                   
                    Debug.Log("Min z position for lane: " + lanePosition + " and Level object : " +levelObject.name + ", PositionZ:" + minZposition);
                    if (changeMinPos)
                    {
                        minZposition += zPosition + levelObject.ObjectSpawnBlockLenght;
                    }
                    
                    if (zPosition == -10) continue;

                    // Calculate the spawn position relative to the ground plane's local space
                    Vector3 spawnPosition = groundPlaneTransform.TransformPoint(lanePosition + new Vector3(0, 0, zPosition));

                    // Instantiate the selected object as a child of the ground plane
                    Transform newGameObject = Instantiate(levelObject.Prefab, spawnPosition, Quaternion.identity, groundPlaneTransform);

                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                }
            }

        }
    }


    
    private void PopulateLaneWithPlatforms()
    {
        // Randomly select objects from the levelObjectListSO and instantiate them along the lane
        foreach (LevelObjectSO levelObject in _levelObjectListSO.levelObjectSOList)
        {
            if (levelObject.Kind != LevelObjectSO.LevelObjectKind.Platform || levelObject.Kind != LevelObjectSO.LevelObjectKind.PlatformWithRamp) return;
            // Randomly determine if the object should be spawned
           
        }
    }

    private void PopulateLaneWithObstacles()
    {
    
        // Randomly select objects from the levelObjectListSO and instantiate them along the lane
        foreach (LevelObjectSO levelObject in _levelObjectListSO.levelObjectSOList)
        {
            if (levelObject.Kind != LevelObjectSO.LevelObjectKind.SlidingObstacle|| levelObject.Kind != LevelObjectSO.LevelObjectKind.JumpingObstacle) return;
            

        }
    }

    private void PopulateLaneWithCollectibles(Vector3 lanePosition)
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

    float CalculateSpawnPosition(LevelObjectSO levelObjectSO, float minZposition, out bool changeMinPos)
    {

        //5 = end of the plane, I know magic numbers are bad
        float maxZPosition = 5;  // Max Z position considering the object spawn block length
        //float maxZPosition = 5 - levelObjectSO.ObjectSpawnBlockLenght; // Max Z position considering the object spawn block length
        if (maxZPosition < minZposition)
        {
            changeMinPos = false;
            return -10;
            
        }

        float zPosition = Random.Range(minZposition, maxZPosition);



        changeMinPos = true;
        return zPosition;
    }


}