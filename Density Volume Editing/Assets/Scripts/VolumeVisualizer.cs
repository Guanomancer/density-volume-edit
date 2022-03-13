using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DensityVolumeEdit
{
    [ExecuteInEditMode]
    public class VolumeVisualizer : MonoBehaviour
    {
        [SerializeField]
        private GameObject _visualizerPrefab;
        [SerializeField]
        private Volume _volume;
        [SerializeField]
        private bool _initializeTestVolume = true;

        [Header("Editing")]
        [SerializeField, Range(0, 1)]
        private float _editDensity;
        [SerializeField]
        private Vector3Int _editPoint;
        [SerializeField]
        private Vector3Int _editSize = Vector3Int.one;

        [ContextMenu("Apply edit")]
        private void ApplyEdit()
        {
            var volumePoint = Vector3Int.zero;
            for(volumePoint.x = _editPoint.x; volumePoint.x < _editPoint.x + _editSize.x; volumePoint.x++)
            {
                for (volumePoint.y = _editPoint.y; volumePoint.y < _editPoint.y + _editSize.y; volumePoint.y++)
                {
                    for (volumePoint.z = _editPoint.z; volumePoint.z < _editPoint.z + _editSize.z; volumePoint.z++)
                    {
                        if (volumePoint.x < 0 || volumePoint.x >= _volume.TotalPoints.x ||
                            volumePoint.y < 0 || volumePoint.y >= _volume.TotalPoints.y ||
                            volumePoint.z < 0 || volumePoint.z >= _volume.TotalPoints.z)
                            continue;

                        _volume.SetDensity(volumePoint, _editDensity);
                        UpdateVisualization();
                    }
                }
            }
        }
        
        private void OnEnable()
        {
            InitializeVolume();
            UpdateVisualization();
        }

        private void UpdateVisualization()
        {
            ClearExistingVisualizers();
            InstantiateVisualizers();
        }

        private void InstantiateVisualizers()
        {
            var chunkID = Vector3Int.zero;
            for (chunkID.x = 0; chunkID.x < _volume.ChunkCount.x; chunkID.x++)
            {
                for (chunkID.y = 0; chunkID.y < _volume.ChunkCount.y; chunkID.y++)
                {
                    for (chunkID.z = 0; chunkID.z < _volume.ChunkCount.z; chunkID.z++)
                    {
                        var chunk = _volume.Chunks[chunkID];
                        for (int x = 0; x < _volume.PointsPerChunk.x; x++)
                        {
                            for (int y = 0; y < _volume.PointsPerChunk.y; y++)
                            {
                                for (int z = 0; z < _volume.PointsPerChunk.z; z++)
                                {
                                    var localPoint = new Vector3Int(x, y, z);
                                    var worldPoint = _volume.WorldFromChunkAndPoint(chunkID, localPoint);
                                    var arrayPoint = localPoint + Vector3Int.one;
                                    var density = _volume.Chunks[chunkID][arrayPoint.x, arrayPoint.y, arrayPoint.z];
                                    var obj = Instantiate(_visualizerPrefab, worldPoint, Quaternion.identity, transform);
                                    obj.transform.localScale = (Vector3.one + Vector3.one * density) * 0.5f;
                                    obj.name = $"Point {worldPoint} with density {density}";
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ClearExistingVisualizers()
        {
            var children = gameObject.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child == transform)
                    continue;
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        private void InitializeVolume()
        {
            if (_initializeTestVolume)
            {
                if (_volume == null)
                    _volume = ScriptableObject.CreateInstance<Volume>();
                _volume.InitializeTestVolume();
            }
        }
    }
}