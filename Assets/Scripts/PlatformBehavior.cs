using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private GameObject _goal;
    [SerializeField] private GameObject _platformsParent;
    [SerializeField] private int levels = 1;

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
    }

    public void CreateStage()
    {
        _previousPlatform = null;

        // Remove all existing platforms
        foreach (Transform child in _platformsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        StartCoroutine(SpawnPlatforms());
    }

    private IEnumerator SpawnPlatforms()
    {
        const float heightIncrement = 2f;
        float currentHeight = 0f;

        Vector3 pos = transform.position;
        Vector3 extents = _basePlatformMeshCollider.bounds.extents;

        for (int i = 0; i < levels; ++i)
        {
            yield return new WaitForSeconds(.1f);

            currentHeight += heightIncrement;
            Vector3 spawnPosition;

            // Find a random spawn position
            int antiInfCounter = 0;
            do
            {
                const float margin = .5f;
                float randomX = Random.Range(pos.x - extents.x + margin, pos.x + extents.x - margin);
                float randomZ = Random.Range(pos.z - extents.z + margin, pos.z + extents.z - margin);
                Vector3 randomPoint = new Vector3(randomX, currentHeight, randomZ);

                spawnPosition = transform.position + randomPoint;

                if (_previousPlatform != null)
                {
                    Vector3 previousPlatformNoHeightDiff =
                        new Vector3(_previousPlatform.transform.position.x, currentHeight, _previousPlatform.transform.position.z);

                    Vector3 distanceVec = spawnPosition - previousPlatformNoHeightDiff;
                    if (distanceVec.magnitude > _maxPlatformDistance)
                    {
                        Vector3 newDistance = distanceVec.normalized * _maxPlatformDistance;
                        spawnPosition = previousPlatformNoHeightDiff + newDistance;
                    }
                }

                const int maxSteps = 30;
                ++antiInfCounter;
                if (antiInfCounter > maxSteps)
                {
                    Debug.LogError("Do while went over allowed " + maxSteps + " steps");
                    yield break;
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

            // If last platform to spawn
            if (i == levels - 1)
            {
                // Set goal on platform
                _goal.transform.position = spawnPosition + new Vector3(0f, 1f, 0f);
                _goal.transform.parent = platformObject.transform;
            }
        }
    }

    private bool IsPlatformPositionValid(Vector3 spawnPos)
    {
        if (_previousPlatform == null)
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
