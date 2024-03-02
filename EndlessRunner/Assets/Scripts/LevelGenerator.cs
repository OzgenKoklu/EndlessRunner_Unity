using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private LevelObjectSO _platformSO;
    [SerializeField] private LevelObjectSO _platformWithRampSO;
    [SerializeField] private List<LevelObjectSO> _obstacleList;
    [SerializeField] private LevelObjectSO _collectableCoin;
    [SerializeField] private LevelObjectSO _groundPlaneSO;

    private GameObject _lastGroundPlane;

    private Vector3 _firstPlanegenerationPosition = new Vector3(0, 0, 0);
    private readonly List<Vector3> _lanePositions = new List<Vector3>
    {
        new Vector3(-2.3f, 0, 0), // Left lane
        new Vector3(0, 0, 0),     // Middle lane
        new Vector3(2.3f, 0, 0)   // Right lane
    };

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

    private Segment[,] _mapData; //this will be a 3x10 segment map
    private Segment[] _lastMapLastRowData; //this will be a 3x1 segment map
    private float _rowLenght;


    private void Start()
    {
        ObjectPoolManager.Instance.onInitializationComplete += OnPoolInitializationComplete;

        InitializeMapData();
    }

    private void OnPoolInitializationComplete()
    {
        PopulateMapData();

        ObjectPoolManager.Instance.onInitializationComplete -= OnPoolInitializationComplete; //this event will never trigger in this runtime so unsub
    }

    private void InitializeMapData()
    {
        const int laneCount = 3, rowCount = 10;
        _mapData = new Segment[laneCount, rowCount];

        _rowLenght = 70f / rowCount; //row lenght for segment location calculations, 70 is the Z size of the ground plane

        _lastMapLastRowData = new Segment[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            _lastMapLastRowData[i] = new Segment { Type = SegmentType.Empty };
        }
   
    }

    public void PopulateMapData()
    {
         ClearMapData();

        if (_isFirstPlane)
        {
            _lastGroundPlane = ObjectPoolManager.Instance.SpawnFromPool(_groundPlaneSO.ObjectName, _firstPlanegenerationPosition, Quaternion.identity);           
            _isFirstPlane = false;
        }       

        if (!_isFirstPlane)
        {
            GenerateNextGroundPlane();
        }

    }
    private void ClearMapData()
    {
        for (int i = 0; i < _mapData.GetLength(0); i++)
        {
            for (int j = 0; j < _mapData.GetLength(1); j++)
            {
                _mapData[i, j] = new Segment { Type = SegmentType.Empty };
            }
        }
    }

    private void GenerateNextGroundPlane()
    {
        Vector3 spawnPosition = _lastGroundPlane.transform.position + new Vector3(0, 0, 70f); //70 is the lenght of the ground plane
        _lastGroundPlane = ObjectPoolManager.Instance.SpawnFromPool(_groundPlaneSO.ObjectName, spawnPosition, Quaternion.identity);

        // Populate the map with platforms and ramps
        PopulateMapWithRampsAndPlatforms(); //only populates the segment matrix data
        PopulateMapWithObstacles(); //only populates the segment matrix data
        GenerateLevelWithMapData(_lastGroundPlane.transform); //physically puts gameobjects in the ground plane
        PopulateMapWithCollectibles(_lastGroundPlane.transform); //physically puts gameobjects in the ground plane
    }

    private void GenerateLevelWithMapData(Transform groundPlaneTransform)
    {
        float rowLenght = 70f / _mapData.GetLength(1);// Calculate the segment size in the Z direction

        for (int laneIndex = 0; laneIndex < _mapData.GetLength(0); laneIndex++)
        {

            for (int rowIndex = 0; rowIndex < _mapData.GetLength(1); rowIndex++)
            {
                HandleSegmentPopulation(laneIndex, rowIndex, groundPlaneTransform);
            }
        }
    }

    private void HandleSegmentPopulation(int laneIndex, int rowIndex, Transform groundPlaneTransform)
    {
        // Calculate the position of the segment based on laneIndex and segmentIndex
        float posX = _lanePositions[laneIndex].x;     
        float posZ = -5f + _rowLenght * (rowIndex + 0.5f); //-5 is offset
        Vector3 segmentPosition = new Vector3(posX, 0, posZ); //based on assumption that map datas origin is at 0,0,0

        Segment segment = _mapData[laneIndex, rowIndex];

        PopulateSegmentWithGameObject(segment.Type, segmentPosition, groundPlaneTransform);
    }

    private void PopulateSegmentWithGameObject(SegmentType segmentType, Vector3 segmentPosition, Transform parentTransform)
    {
        Vector3 spawnPosition = segmentPosition + new Vector3(0, 0, parentTransform.position.z - 35f); //adding the parents tranform.pos.z to the calculated segment pos.

        GameObject newGameObject;
        switch (segmentType)
        {
            case SegmentType.PlatformWithRamp:
                newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_platformWithRampSO.ObjectName, spawnPosition, Quaternion.identity);
                newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                newGameObject.transform.SetParent(parentTransform);
                break;
            case SegmentType.Platform:
                newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_platformSO.ObjectName, spawnPosition, Quaternion.identity);
                newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                newGameObject.transform.SetParent(parentTransform);
                break;
            case SegmentType.Obstacle:
                int randomValue = UnityEngine.Random.Range(0, 3);
                newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_obstacleList[randomValue].ObjectName, spawnPosition, Quaternion.identity);
                newGameObject.GetComponent<PivotAdjustmentForObject>()?.SetPosition(spawnPosition);
                newGameObject.transform.SetParent(parentTransform);
                break;
            case SegmentType.Empty:
                //spawn nothing, newGameObject is null so no need to worry about it for the garbage collector, 
                break;
        }
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
        SaveTheLastRowData();
    }

    private void SaveTheLastRowData()
    {
        //logging the last row of the road to ensure pathway next time map is generated.
        for (int i = 0; i < _mapData.GetLength(0); i++)
        {
            _lastMapLastRowData[i].Type = _mapData[i, (_mapData.GetLength(1) - 1)].Type;
        }
    }

    private void PopulateMapWithCollectibles(Transform groundPlaneTransform)
    {
        // Coin generation will happen after the map is generated since collectibles won't be hardcoded into segment area with an enum; they will spawn on top of the ramps and platforms
        bool coinLimitAchieved = false;
        bool notSpawnedInThisRow = true;
        int coinCount = 0;     

        for (int j = 0; j < _mapData.GetLength(1); j++)
        {
            notSpawnedInThisRow = true;
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                // If this segment is a ramp OR the segment before this is not empty (meaning that it can be after an obstacle or after a ramp/platform), spawn coins if RNG sees it fit              
                if ((_mapData[i, j].Type == SegmentType.PlatformWithRamp || _mapData[i, j].Type == SegmentType.Obstacle) && notSpawnedInThisRow)
                {
                    if (RandomShouldAddCollectable() && !coinLimitAchieved)
                    {
                        notSpawnedInThisRow = false;
                        float posX = _lanePositions[i].x;

                        //-35'den +35'e 10 segment var. Segmentlerin tanesi 7 birim olacak, segment baþýnda deðil 0.4 sonrasýnda baþlýyor.
                        float posZ = -35f + _rowLenght * (j + 0.3f);

                        Vector3 segmentPosition = new Vector3(0, 0, 0);

                        //ads Y component to spawnPosition if coin will be spawned on a platform or a ramp
                        if (_mapData[i, j].Type == SegmentType.PlatformWithRamp || _mapData[i, j].Type == SegmentType.Platform)
                        {
                            segmentPosition = new Vector3(posX, 2f, posZ);
                        }
                        else
                        {
                            segmentPosition = new Vector3(posX, 0, posZ);
                        }

                        int randomCoinCount = UnityEngine.Random.Range(2, 8); //nin 3 max 7 in a streak

                        for (int k = 0; k <= randomCoinCount; k++)
                        {
                            Vector3 coinSpawnPosition = new Vector3(segmentPosition.x + groundPlaneTransform.position.x, groundPlaneTransform.position.y + segmentPosition.y, groundPlaneTransform.position.z + segmentPosition.z + (k * 0.8f));
                            GameObject newGameObject = ObjectPoolManager.Instance.SpawnFromPool(_collectableCoin.ObjectName, coinSpawnPosition, Quaternion.identity);
                            newGameObject?.GetComponent<PivotAdjustmentForObject>()?.SetPosition(coinSpawnPosition);
                            newGameObject.transform.SetParent(groundPlaneTransform);
                        }

                        coinCount += randomCoinCount;

                    }
                } 
                if (coinCount >= 15) // Set a limit for the total number of coins, yet with how algoritm is working this will end up generating a maximum of 21 coins per plane on edge case
                {
                    coinLimitAchieved = true;
                    break; // Break the loop if the coin limit is achieved
                }
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
        return UnityEngine.Random.Range(0f, 1f) < 0.5f;
    }
    private bool RandomShouldAddCollectable()
    {      
        return UnityEngine.Random.Range(0f, 1f) < 0.7f;
    }

    private bool RandomShouldAddPlatform()
    {      
        return UnityEngine.Random.Range(0f, 1f) < 0.5f;
    }

    private bool RandomShouldAddPlatformWithRamp()
    {       
        return UnityEngine.Random.Range(0f, 1f) < 0.3f;
    }

    private bool RandomShouldAddPlatformNext()
    {       
        return UnityEngine.Random.Range(0f, 1f) < 0.7f;
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
                            }
                        }
                    }
                    catch(IndexOutOfRangeException e)
                    {
                       //not that important important, intersegment issues are beyond the scope of this project
                      // Debug.Log("Out of index error: " + e.Message + "Intex J(3 segment part): " + j + " - Intex I(10 segment Part):" + i);
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

    private bool IsCrossNeighborSegmentEmpty(int laneIndex, int rowIndex, int delta, int deltaRow)
    {
        // Check if the neighboring segment at laneIndex + delta is within bounds
        if ((laneIndex + delta >= 0 && laneIndex + delta < _mapData.GetLength(0)) && (rowIndex + deltaRow >= 0 && rowIndex + deltaRow < _mapData.GetLength(1)))
        {
            // Check if the neighboring segment at laneIndex + delta is empty
            if (_mapData[laneIndex + delta, rowIndex + deltaRow].Type == SegmentType.Empty)
            {
                return true;
            }
        }
        return false;
    }
}