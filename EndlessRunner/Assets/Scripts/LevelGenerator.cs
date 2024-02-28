using System;
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
    [SerializeField] private List<LevelObjectSO> _obstacleList;
    [SerializeField] private LevelObjectSO _collectableCoin;
    [SerializeField] private LevelObjectSO _groundPlaneSO;

    private GameObject _lastGroundPlane;

    private Vector3 _generationPosition = new Vector3(0, 0, 0);

    /*
    private Vector3 _leftLanePosition = new Vector3(-0.35f, 0, 0);
    private Vector3 _middleLanePosition = new Vector3(0, 0, 0);
    private Vector3 _rightLanePosition = new Vector3(0.35f, 0, 0);
    */

    private Vector3 _leftLanePosition = new Vector3(-2.3f, 0, 0);
    private Vector3 _middleLanePosition = new Vector3(0, 0, 0);
    private Vector3 _rightLanePosition = new Vector3(2.3f, 0, 0);

    private List<Vector3> _lanePositions;
    private float _spawnOffset = 1f;
    public bool isObjectPoolReady = false;
    private bool _isFirstPlane = true;

    public enum SegmentType
    {
        Empty,
        Platform,
        PlatformWithRamp, 
        Obstacle
    }

    public class Segment
    {
        public SegmentType Type { get; set; }
    }

    //this will be a 3x10 segment map
    private Segment[,] _mapData;

    //this will be a 3x1 segment map
    private Segment[] _lastMapLastRowData;


    private void Start()
    {
        ObjectPoolManager.Instance.onInitializationComplete += OnPoolInitializationComplete;
    

        
    }

    private void OnPoolInitializationComplete()
    {
        isObjectPoolReady = true;
      //  Debug.Log("isObjectPoolReady: " + isObjectPoolReady);

        int laneCount = 3;
        int segmentCount = 10;
        _lanePositions = new List<Vector3>();
        _lanePositions.Add(_leftLanePosition);
        _lanePositions.Add(_middleLanePosition);
        _lanePositions.Add(_rightLanePosition);
        SegmentMap(laneCount, segmentCount);
        // GenerateLevel();
        ObjectPoolManager.Instance.onInitializationComplete -= OnPoolInitializationComplete;
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

        if (_isFirstPlane)
        {
            GameObject groundPlane = ObjectPoolManager.Instance.SpawnFromPool(_groundPlaneSO.ObjectName, _generationPosition, Quaternion.identity);
            _lastGroundPlane = groundPlane;

            //initializes the last row segment map for maps after this to use (to unblock the road)

            _lastMapLastRowData = new Segment[3];
            for(int i = 0; i < 3; i++)
            {
                _lastMapLastRowData[i] = new Segment { Type = SegmentType.Empty };
            }
           // Debug.Log("Last row data lenght: " + _lastMapLastRowData.Length);
        }       

        if (!_isFirstPlane)
        {
            Vector3 spawnPosition = _lastGroundPlane.transform.position + new Vector3(0, 0, 70f);
            GameObject groundPlane = ObjectPoolManager.Instance.SpawnFromPool(_groundPlaneSO.ObjectName, spawnPosition, Quaternion.identity);
            _lastGroundPlane = groundPlane;
            // Populate the map with platforms and ramps
            PopulateMapWithRampsAndPlatforms();
            PopulateMapWithObstacles();
            GenerateLevelWithMapData(groundPlane.transform);
        }
        _isFirstPlane = false;
    }


    private void PopulateMapWithRampsAndPlatforms()
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

        /*
        for(int i = 0; i< _mapData.GetLength(0); i++)
        {
            Debug.Log("Last Row of the map - enum types: i x j (3 x 10 ):" +"i: " + i + "j: "  + (_mapData.GetLength(1) - 1) + "Type: "+ _mapData[i,_mapData.GetLength(1) - 1].Type);
        }
        */
        

        //logging the last row of the road to ensure pathway next time map is generated.
        for (int i = 0; i < _mapData.GetLength(0); i++)
        {
            _lastMapLastRowData[i].Type = _mapData[i, (_mapData.GetLength(1) - 1)].Type;
            Debug.Log(" _lastMapLastRowData[i].Type:" + _lastMapLastRowData[i].Type);
            Debug.Log("Last Row of the map - enum types: i x j (3 x 10 ):" + "i: " + i + "j: " + (_mapData.GetLength(1) - 1) + "Type: " + _mapData[i, _mapData.GetLength(1) - 1].Type);
        }
    }

    private void PopulateMapWithCollectibles(Transform groundPlaneTransform)
    {
        // This can happen after the map is generated since collectibles won't be hardcoded into segment area with an enum; they will spawn on top of the ramps and platforms
        bool coinLimitAchieved = false;
        //to ensure only one string of coins occur in the generated map
        bool notSpawnedInThisSegment = true;
        int coinCount = 0;
        float segmentSize = 70f / _mapData.GetLength(1);

        for (int j = 0; j < _mapData.GetLength(1); j++)
        {
            notSpawnedInThisSegment = true;
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {              
                // If this segment is a ramp OR the segment before this is not empty (meaning that it can be after an obstacle or after a ramp/platform), spawn coins if RNG sees it fit
                //if (_mapData[i, j].Type == SegmentType.PlatformWithRamp || !IsCrossNeighborSegmentEmpty(i, j, 0, -1))
                if ((_mapData[i, j].Type == SegmentType.PlatformWithRamp || _mapData[i, j].Type == SegmentType.Obstacle) && notSpawnedInThisSegment)
                {
                    if (RandomShouldAddCollectable() && !coinLimitAchieved && coinCount < 15)
                    {
                        notSpawnedInThisSegment = false;
                        float posX = _lanePositions[i].x;

                        //-35'den +35'e 10 segment var. Segmentlerin tanesi 7 birim olacak, segment baþýnda deðil 0.4 sonrasýnda baþlýyor.
                        float posZ = -35f + segmentSize * (j + 0.3f);

                        Vector3 segmentPosition = new Vector3(0, 0, 0);

                        if (_mapData[i, j].Type == SegmentType.PlatformWithRamp || _mapData[i, j].Type == SegmentType.Platform)
                        {
                            segmentPosition = new Vector3(posX, 2f, posZ);
                        }
                        else
                        {
                            segmentPosition = new Vector3(posX, 0, posZ);
                        }

                        //Segment segment = _mapData[i, j];

                        Vector3 spawnPosition = groundPlaneTransform.TransformPoint(segmentPosition);

                        // Spawn 5 coins with a 0.05f gap between them
                        for (int k = 0; k < 5; k++)
                        {                           
                            Vector3 coinSpawnPosition =  new Vector3(segmentPosition.x +groundPlaneTransform.position.x,groundPlaneTransform.position.y+ segmentPosition.y,groundPlaneTransform.position.z+ segmentPosition.z +(k * 0.6f));                         
                            GameObject newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_collectableCoin.ObjectName, coinSpawnPosition, Quaternion.identity);
                            newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(coinSpawnPosition);
                            newGameObject.transform.SetParent(groundPlaneTransform);
                        }

                        coinCount += 5; // Increment coin count by 5 for the 5 coins spawned
                    }
                }
            }

            if (coinCount >= 15) // Set a limit for the total number of coins
            {
                coinLimitAchieved = true;
                break; // Break the loop if the coin limit is achieved
            }
        }
    }

    private void PopulateMapWithObstacles()
    {
        bool obstacleSpawned = false;

        for (int j = 0; j < _mapData.GetLength(1); j++)
        {
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                //ensures it spawns to an empty position that also has an empty position before it. (it can be a platform before it after all)
                if (_mapData[i, j].Type == SegmentType.Empty && IsCrossNeighborSegmentEmpty(i, j, 0, -1))
                {
                    if (RandomShouldAddObstacle() && !obstacleSpawned)
                    {
                        _mapData[i, j].Type = SegmentType.Obstacle;
                        obstacleSpawned = true;
                    }
                }
            }
            if (j % 3 == 0) // Reset after every three rows
            {
                obstacleSpawned = false;
            }
        }
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

    private bool RandomShouldAddObstacle()
    {
        // Implement your logic for adding platforms
        // Example: Return true with a certain probability
        return UnityEngine.Random.Range(0f, 1f) < 0.5f;
    }
    private bool RandomShouldAddCollectable()
    {
        // Implement your logic for adding platforms
        // Example: Return true with a certain probability
        return UnityEngine.Random.Range(0f, 1f) < 0.5f;
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
      
        for (int i = 0; i < _mapData.GetLength(1); i++)
        {
            if (i == 0)
            {
                //this is where we should factor in the previous map data:
                for (int j = 0; j < _mapData.GetLength(0); j++)
                {
                    //empties the first row of the map if it starts with a blockage.
                    if(_lastMapLastRowData[j].Type == SegmentType.Empty && _mapData[j,i].Type == SegmentType.Platform)
                    {
                        _mapData[j, i].Type = SegmentType.Empty;
                    }
                }
                // Example: Set a random segment in each lane to be empty
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

                    //checks if path is blocked by platforms for hte case of 
                    //oxo 
                    //xox 
                    //o=empty road, x=platform
                    try
                    {
                        if (j == 1 && _mapData[j, i].Type == SegmentType.Platform)
                        {
                            if (!IsCrossNeighborSegmentEmpty(j, i, -1, -1) && !IsCrossNeighborSegmentEmpty(j, i, 1, -1))
                            {
                                int randomDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                                _mapData[j + randomDirection, i - 1].Type = SegmentType.Empty;
                                Debug.Log(" Row before blockage cleaned, Emptied space at: J,I(3by10) for ensure passable ground: " + "j:" + (j+randomDirection) + " , i:" + (i - 1));
                            }

                            //and also for
                            //xox
                            //oxo
                            if (!IsCrossNeighborSegmentEmpty(j, i, -1, 1) && !IsCrossNeighborSegmentEmpty(j, i, 1, 1))
                            {
                                int randomDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                                _mapData[j + randomDirection, i + 1].Type = SegmentType.Empty;
                                Debug.Log(" Row After blockage cleaned, Emptied space at: J,I(3by10) for ensure passable ground: " + "j:" + (j+randomDirection) + " , i:" + (i + 1));
                            }
                        }
                    }
                    catch(IndexOutOfRangeException e)
                    {
                        //not that important important, intersegment issues are beyond the scope of this project,
                        //but we might add some extra gap between the planes to avoid generating impossable to pass areas
                        Debug.Log("Out of index error: " + e.Message + "Intex J(3 segment part): " + j + " - Intex I(10 segment Part):" + i);
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
        if ((laneIndex + delta >= 0 && laneIndex + delta < _mapData.GetLength(0)) && (segmentIndex + deltaSegment >= 0 && segmentIndex + deltaSegment < _mapData.GetLength(1)))
        {
            // Check if the neighboring segment at laneIndex + delta is empty
            if (_mapData[laneIndex + delta, segmentIndex + deltaSegment].Type == SegmentType.Empty)
            {
                return true;
            }
        }
        return false;
    }

    private void GenerateLevelWithMapData(Transform groundPlaneTransform)
    {
       // GameObject groundPlane = Instantiate(_groundPlanePrefabTransform.gameObject, _generationPosition, Quaternion.identity);

        // Calculate the segment size in the Z direction
        float segmentSize = 70f / _mapData.GetLength(1);

        for (int laneIndex = 0; laneIndex < _mapData.GetLength(0); laneIndex++)
        {
           
            for (int segmentIndex = 0; segmentIndex < _mapData.GetLength(1); segmentIndex++)
            {
                // Calculate the position of the segment based on laneIndex and segmentIndex
                float posX = _lanePositions[laneIndex].x;
                //Debug.Log(posX);
                float posZ = -5f + segmentSize * (segmentIndex + 0.5f);
               // Debug.Log(segmentSize);
                Vector3 segmentPosition = new Vector3(posX, 0, posZ);
                

                Segment segment = _mapData[laneIndex, segmentIndex];

                if (segment.Type == SegmentType.PlatformWithRamp)
                {
                    // Instantiate the platform with ramp prefab
                    Vector3 spawnPosition = segmentPosition + new Vector3(0, 0, groundPlaneTransform.position.z -35f); ;
                    //GameObject newGameObject = Instantiate(_platformWithRampSO.Prefab.gameObject, spawnPosition, Quaternion.identity, groundPlaneTransform.transform);
                    GameObject newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_platformWithRampSO.ObjectName, spawnPosition, Quaternion.identity);
                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                    newGameObject.transform.SetParent(groundPlaneTransform);


                }
                else if(segment.Type == SegmentType.Platform)
                {
                    Vector3 spawnPosition = segmentPosition + new Vector3(0, 0, groundPlaneTransform.position.z - 35f); ;
                    //GameObject newGameObject = Instantiate(_platformSO.Prefab.gameObject, spawnPosition, Quaternion.identity, groundPlaneTransform.transform);
                    GameObject newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_platformSO.ObjectName, spawnPosition, Quaternion.identity);
                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                    newGameObject.transform.SetParent(groundPlaneTransform);

                }
                else if(segment.Type == SegmentType.Obstacle)
                {

                    Vector3 spawnPosition = segmentPosition + new Vector3(0, 0, groundPlaneTransform.position.z -35f); ;
                    int randomValue = UnityEngine.Random.Range(0, 3);
                    //Transform newGameObject = Instantiate(_obstacleList[randomValue].Prefab, spawnPosition, Quaternion.identity, groundPlaneTransform.transform);
                    GameObject newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_obstacleList[randomValue].ObjectName, spawnPosition, Quaternion.identity);
                    newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                    newGameObject.transform.SetParent(groundPlaneTransform);
                }
                else {

                }

            }
        }

        PopulateMapWithCollectibles(groundPlaneTransform.transform);

    }

}