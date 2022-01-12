using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private GameObject _goal;
    [SerializeField] private GameObject _platformsParent;
    [SerializeField] private int _minLevels = 1;
    [SerializeField] private int _maxLevels = 1;

    [SerializeField] private Material successMat;
    [SerializeField] private Material failureMat;

    [SerializeField] private float _maxPlatformDistance = 6f;

    private MeshCollider _basePlatformMeshCollider;
    private MeshRenderer _meshRenderer;

    private Platform _previousPlatform = null;
    private Queue<Platform> _previousPlatformQueue = new Queue<Platform>();

    private void Start()
    {
        _basePlatformMeshCollider = GetComponent<MeshCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if(_minLevels <= 0 || _maxLevels <= 0)
        {
            Debug.LogError("minLevels or maxLevels cannot be lower than 0.");
        }

        if(_maxLevels < _minLevels)
        {
            Debug.LogError("maxLevels cannot be smaller than minLevels");
        }
    }

    public void CreateStage()
    {
        _previousPlatform = null;
        _previousPlatformQueue.Clear();

        // Remove all existing platforms
        foreach (Transform child in _platformsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        SpawnPlatforms();
    }

    private void SpawnPlatforms()
    {
        const float heightIncrement = 2f;
        float currentHeight = 0f;
        Vector3 extents = _basePlatformMeshCollider.bounds.extents;

        int platformsToSpawn = Random.Range(_minLevels, _maxLevels);
        for (int i = 0; i < platformsToSpawn; ++i)
        {
            currentHeight += heightIncrement;
            Vector3 spawnPosition;
            int antiInfCounter = 0;

            // Find a random spawn position until it is valid
            do
            {
                // Calculate a random spawn position
                const float margin = .5f;
                float randomX = Random.Range(-extents.x + margin, extents.x - margin);
                float randomZ = Random.Range(-extents.z + margin, extents.z - margin);
                Vector3 randomPoint = new Vector3(randomX, currentHeight, randomZ);
                spawnPosition = transform.position + randomPoint;

                if (_previousPlatform != null)
                {
                    // Make sure platform cannot be too far away (max jump distance)
                    Vector3 prevPlatformPos = _previousPlatform.transform.position;
                    Vector3 previousPlatformNoHeightDiff = new Vector3(prevPlatformPos.x, currentHeight, prevPlatformPos.z);
                    Vector3 distanceVec = spawnPosition - previousPlatformNoHeightDiff;
                    if (distanceVec.magnitude > _maxPlatformDistance)
                    {
                        Vector3 newDistance = distanceVec.normalized * _maxPlatformDistance;
                        spawnPosition = previousPlatformNoHeightDiff + newDistance;
                    }
                }

                // Notify when randomized platform was invalid 30 times in a row and abort
                const int maxSteps = 30;
                ++antiInfCounter;
                if (antiInfCounter > maxSteps)
                {
                    Debug.LogError("Do while went over allowed " + maxSteps + " steps");
                    return;
                }

            } while (!IsPlatformPositionValid(spawnPosition));

            // Create platform
            GameObject platformObject = Instantiate(_platformPrefab, spawnPosition, Quaternion.identity);
            Platform platform = platformObject.GetComponent<Platform>();
            _previousPlatform = platform;

            _previousPlatformQueue.Enqueue(platform);
            if (_previousPlatformQueue.Count > 2)
            {
                _previousPlatformQueue.Dequeue();
            }

            // Set parent for clean hierarchy
            platformObject.transform.parent = _platformsParent.transform;

            // Set goal on last platform
            if (i == platformsToSpawn - 1)
            {
                _goal.transform.position = spawnPosition + new Vector3(0f, 1f, 0f);
            }
        }
    }

    private bool IsPlatformPositionValid(Vector3 spawnPos)
    {
        if (_previousPlatform == null || _previousPlatformQueue.Count == 0)
        {
            return true;
        }

        bool isAbovePreviousPlatforms = false;
        foreach (Platform p in _previousPlatformQueue)
        {
            if (p.IsPositionAbovePlatform(spawnPos))
            {
                isAbovePreviousPlatforms = true;
            }
        }

        // The platform should be above the base platform and not directly above the previous platform
        return IsAboveBasePlatform(spawnPos) && !isAbovePreviousPlatforms;
    }

    private bool IsAboveBasePlatform(Vector3 pos)
    {
        Ray ray = new Ray(pos, Vector3.down);
        return _basePlatformMeshCollider.bounds.IntersectRay(ray);
    }

    public void SetStageSuccess(bool success)
    {
        if (success)
        {
            _meshRenderer.sharedMaterial = successMat;
        }
        else
        {
            _meshRenderer.sharedMaterial = failureMat;
        }
    }
}
