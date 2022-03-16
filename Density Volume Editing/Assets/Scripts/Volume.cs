using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DensityVolumeEdit
{
    [CreateAssetMenu(fileName = "New Density Volume", menuName = "Density Volume")]
    public class Volume : ScriptableObject
    {
        public Vector3Int PointsPerChunk { get; private set; }
        public Vector3Int PointsPerChunkIncludingBorder => PointsPerChunk + Vector3Int.one * 2;
        public Vector3Int PointsPerChunkOffset => PointsPerChunk + Vector3Int.one;
        public Vector3Int ChunkCount { get; private set; }
        public Vector3Int TotalPoints => PointsPerChunk * ChunkCount;

        public Dictionary<Vector3Int, float[,,]> Chunks { get; private set; }

        public void InitializeTestVolume()
        {
            PointsPerChunk = Vector3Int.one * 2;
            ChunkCount = Vector3Int.one * 2;
            Chunks = new Dictionary<Vector3Int, float[,,]>();

            var chunk000 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(0, 0, 0), chunk000);
            var chunk100 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(1, 0, 0), chunk100);
            var chunk010 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(0, 1, 0), chunk010);
            var chunk110 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(1, 1, 0), chunk110);
            var chunk001 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(0, 0, 1), chunk001);
            var chunk101 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(1, 0, 1), chunk101);
            var chunk011 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(0, 1, 1), chunk011);
            var chunk111 = new float[4, 4, 4];
            Chunks.Add(new Vector3Int(1, 1, 1), chunk111);

            SetChunkCube(new Vector3Int(2, 2, 2), Vector3Int.one * 2, 1, chunk000);
            SetChunkCube(new Vector3Int(0, 2, 2), Vector3Int.one * 2, 1, chunk100);
            SetChunkCube(new Vector3Int(2, 0, 2), Vector3Int.one * 2, 1, chunk010);
            SetChunkCube(new Vector3Int(0, 0, 2), Vector3Int.one * 2, 1, chunk110);
            SetChunkCube(new Vector3Int(2, 2, 0), Vector3Int.one * 2, 1, chunk001);
            SetChunkCube(new Vector3Int(0, 2, 0), Vector3Int.one * 2, 1, chunk101);
            SetChunkCube(new Vector3Int(2, 0, 0), Vector3Int.one * 2, 1, chunk011);
            SetChunkCube(new Vector3Int(0, 0, 0), Vector3Int.one * 2, 1, chunk111);
        }

        private void SetChunkCube(Vector3Int offset, Vector3Int size, float value, float[,,] chunk)
        {
            for (int x = offset.x; x < offset.x + size.x; x++)
                for (int y = offset.y; y < offset.y + size.y; y++)
                    for (int z = offset.z; z < offset.z + size.z; z++)
                        chunk[x, y, z] = value;
        }

        public Vector3 WorldFromChunkAndPoint(Vector3Int chunkID, Vector3Int point)
            => chunkID * PointsPerChunk + point;

        public void UnpackVolumePoint(Vector3Int volumePoint, out Vector3Int chunkID, out Vector3Int localPoint)
        {
            chunkID = new Vector3Int(volumePoint.x / PointsPerChunk.x, volumePoint.y / PointsPerChunk.y, volumePoint.z / PointsPerChunk.z);
            localPoint = new Vector3Int(volumePoint.x % PointsPerChunk.x, volumePoint.y % PointsPerChunk.y, volumePoint.z % PointsPerChunk.z);
        }

        public Vector3Int ArrayPointFromLocalPoint(Vector3Int localPoint)
            => localPoint + Vector3Int.one;

        public void SetDensity(Vector3Int volumePoint, float density)
        {
            UnpackVolumePoint(volumePoint, out Vector3Int chunkID, out Vector3Int localPoint);

            var arrayPoint = ArrayPointFromLocalPoint(localPoint);
            Chunks[chunkID][arrayPoint.x, arrayPoint.y, arrayPoint.z] = density;

            var neighbours = new List<Vector3Int>();
            if (localPoint.x == 0)
                neighbours.Add(new Vector3Int(-1, 0, 0));
            if (localPoint.y == 0)
                neighbours.Add(new Vector3Int(0, -1, 0));
            if (localPoint.z == 0)
                neighbours.Add(new Vector3Int(0, 0, -1));
            
            if (localPoint.x == PointsPerChunk.x - 1)
                neighbours.Add(new Vector3Int(1, 0, 0));
            if (localPoint.y == PointsPerChunk.y - 1)
                neighbours.Add(new Vector3Int(0, 1, 0));
            if (localPoint.z == PointsPerChunk.z - 1)
                neighbours.Add(new Vector3Int(0, 0, 1));

            var count = neighbours.Count;
            for(int l = 0; l < count; l++)
            {
                for (int m = l + 1; m < count; m++)
                {
                    var index = neighbours.Count;
                    neighbours.Add(neighbours[l] + neighbours[m]);
                    for (int n = m + 1; n < count; n++)
                        neighbours.Add(neighbours[index] + neighbours[n]);
                }
            }

            foreach (var n in neighbours)
                SetDensityForNeighbour(chunkID, localPoint, n, density);
        }

        private void SetDensityForNeighbour(Vector3Int chunkID, Vector3Int localPoint, Vector3Int offsetToCheck, float density)
        {
            var newChunkID = chunkID + offsetToCheck;
            if (!Chunks.ContainsKey(newChunkID))
                return;

            var newArrayPoint = ArrayPointFromLocalPoint(localPoint);
            newArrayPoint -= offsetToCheck * PointsPerChunk;

            Chunks[newChunkID][newArrayPoint.x, newArrayPoint.y, newArrayPoint.z] = density;
        }
    }
}
