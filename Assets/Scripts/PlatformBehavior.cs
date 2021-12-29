using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private GameObject _goal;
    [SerializeField] private int levels = 1;
    [SerializeField] private GameObject _platformsParent;

    [SerializeField] private Material successMat;
    [SerializeField] private Material failureMat;

    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;

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

        const float heightIncrement = 2f;
        float currentHeight = 0f;

        Vector3 colliderExtents = _meshCollider.bounds.extents;

        // Create platforms
        for (int i = 0; i < levels; ++i)
        {
            currentHeight += heightIncrement;

            float randomX = Random.Range(-colliderExtents.x, colliderExtents.x);
            float randomZ = Random.Range(-colliderExtents.z, colliderExtents.z);

            Vector3 spawnPosition = transform.position + new Vector3(randomX, currentHeight, randomZ);
            GameObject platform = Instantiate(_platformPrefab, spawnPosition, Quaternion.identity);
            platform.transform.parent = _platformsParent.transform;

            // If last platform to spawn
            if (i == levels - 1)
            {
                // Move goal
                _goal.transform.position = spawnPosition + new Vector3(0f, 1f, 0f);
                _goal.transform.parent = platform.transform;
            }
        }
    }

    public void SetStageSuccess(bool success)
    {
        if (success)
        {
            _meshRenderer.sharedMaterial = successMat;
        } else
        {
            _meshRenderer.sharedMaterial = failureMat;
        }
    }
}
