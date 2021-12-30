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

    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;

    private Platform _previousPlatform = null;
    private Transform _debugTransform;

    private void Start()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void CreateStage()
    {
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

        // Get size of ground plane
        Vector3 colliderExtents = _meshCollider.bounds.extents;

        for (int i = 0; i < levels; ++i)
        {
            currentHeight += heightIncrement;

            Vector3 spawnPosition = Vector3.zero;

            // Find a random spawn position
            int antiInfCounter = 0;
            do
            {
                float randomX = Random.Range(-colliderExtents.x, colliderExtents.x);
                float randomZ = Random.Range(-colliderExtents.z, colliderExtents.z);
                spawnPosition = transform.position + new Vector3(randomX, currentHeight, randomZ);

                ++antiInfCounter;
                if (antiInfCounter > 20)
                {
                    Debug.LogError("Do while went over allowed 20 steps");
                    return;
                }
            } while (!IsPlatformPositionValid(spawnPosition));

            // Create platform
            GameObject platformObject = Instantiate(_platformPrefab, spawnPosition, Quaternion.identity);
            Platform platform = platformObject.GetComponent<Platform>();

            if (_previousPlatform == null) // Debug
                _previousPlatform = platform;
            else
                _debugTransform = platformObject.transform;

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

    private void Update()
    {
        if (_previousPlatform == null || _debugTransform == null)
        {
            return;
        }

        if (IsPlatformPositionValid(_debugTransform.position))
        {
            Debug.Log("Valid");
        }
        else
        {
            Debug.Log("Invalid");
        }
    }

    private bool IsPlatformPositionValid(Vector3 spawnPos)
    {
        if (_previousPlatform == null)
        {
            return true;
        }

        return !_previousPlatform.IsPositionAbovePlatform(spawnPos);
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
