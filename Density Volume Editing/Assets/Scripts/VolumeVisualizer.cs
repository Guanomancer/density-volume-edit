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

        private void OnEnable()
        {
            InitializeVolume();
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
                                    obj.name = $"Point {worldPoint}";
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