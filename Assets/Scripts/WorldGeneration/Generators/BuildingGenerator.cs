using ITF.Math;
using ITF.Utilities;
using ITF.CustomTiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generates buildings. Use PoissonDiscSampling to place buildings.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingGenerator", menuName = "ITF/WorldGeneration/BuildingGenerator")]

    public class BuildingGenerator : ObjectGenerator
    {
        [System.Serializable]
        class BuildingGenerationInfo
        {
            public string name;
            public Vector2Int size;
            [Tooltip("The empty tile at the left-bottom of the building.")]
            public Vector2Int expandLeftBottom;
            [Tooltip("The empty tile at the right-top of the building.")]
            public Vector2Int expandRightTop;
            public Tile[] tiles;
            [Tooltip("Offsets from the left-bottom of the building to place the tiles. Should correspond one-to-one with the tiles.")]
            public Vector3Int[] posOffsets;
            [Tooltip("If num.y > num.x, will random generate the count between [num.x, num.y), else the count = num.x")]
            public Vector2Int num = new(1, 0);
            [Tooltip("If true, fill expand area with placeholder tile")]
            public bool fillExpand = true;
        }

        class SamplePoint
        {
            public Vector2Int position;
            public BuildingGenerationInfo building;

            public SamplePoint(Vector2Int position, BuildingGenerationInfo building)
            {
                this.position = position;
                this.building = building;
            }
        }

        int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }

        [Space(20)]
        [SerializeField] Tile placeholderTile;
        [SerializeField]
        BuildingGenerationInfo[] buildingGenerationInfos;
        [SerializeField] RectInt mapRange = new(5, 2, 22, 32);

        [Space(20)]
        [SerializeField] string targetBuildingName;

        // Map the generate status to the task, 
        Dictionary<GenerateStatus, Task> statusTaskMap = new();

        public override GenerateStatus Generate(TilemapManager tilemap)
        {
            GenerateStatus generateStatus = new();
            statusTaskMap.Add(generateStatus, new(GenerateCoroutine(generateStatus, tilemap)));
            return generateStatus;
        }

        public override void StopAllGeneration()
        {
            foreach (var pair in statusTaskMap)
            {
                pair.Value.Stop();
                pair.Key.failed = !pair.Key.finished;
            }
            statusTaskMap.Clear();
        }

        [ContextMenu("Auto Create Pos Offsets")]
        public void AutoCreatePosOffsets()
        {
            foreach(var building in buildingGenerationInfos)
            {
                if (building.name != targetBuildingName) continue;

                Vector2Int size = building.size - building.expandLeftBottom - building.expandRightTop;
                Vector3Int[] posOffsets = new Vector3Int[size.x * size.y];
                for(int y = size.y - 1; y >= 0; y--)
                {
                    for(int x = 0; x < size.x; x++)
                    {
                        posOffsets[(size.y - 1 - y) * size.x + x] = new Vector3Int(x, y, 0);
                    }
                }
                building.posOffsets = posOffsets;
            }
        }

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            var excludedPoints = GetOccupiedPoints(tilemap, mapRange);
            generateStatus.progress = .25f;
            yield return null;

            var samplePoints = PoissonDiscSampling(mapRange, new XorShiftRandom((uint)seed), excludedPoints);
            generateStatus.progress = .5f;

            yield return null;

            // Place the buildings
            foreach (var samplePoint in samplePoints)
            {
                var building = samplePoint.building;
                for (int i = 0; i < samplePoint.building.tiles.Length; i++)
                {
                    var tile = building.tiles[i];
                    var posOffset = building.posOffsets[i] + (Vector3Int)building.expandLeftBottom;
                    tilemap.SetTile((Vector3Int)samplePoint.position + posOffset, tile);
                }
                // place the placeholder tile
                if (building.fillExpand)
                {
                    for (int y = 0; y < building.size.y; y++)
                    {
                        for (int x = 0; x < building.expandLeftBottom.x; x++)
                            tilemap.SetTile(new Vector2Int(x, y) + samplePoint.position, placeholderTile);
                        for (int x = building.size.x - building.expandRightTop.x; x < building.size.x; x++)
                            tilemap.SetTile(new Vector2Int(x, y) + samplePoint.position, placeholderTile);
                    }
                    for(int x = building.size.x - building.expandRightTop.x; x >= building.expandLeftBottom.x; x--)
                    {
                        for (int y = 0; y < building.expandLeftBottom.y; y++)
                            tilemap.SetTile(new Vector2Int(x, y) + samplePoint.position, placeholderTile);
                        for (int y = building.size.y - building.expandRightTop.y; y < building.size.y; y++)
                            tilemap.SetTile(new Vector2Int(x, y) + samplePoint.position, placeholderTile);
                    }
                }
            }

            generateStatus.progress = 1f;
            generateStatus.finished = true;

            yield break;
        }

        List<SamplePoint> PoissonDiscSampling
            (RectInt mapRange, XorShiftRandom random, List<Vector2Int> excludedPoints, int maxAttempts = 32)
        {
            int minX = mapRange.xMin;
            int maxX = mapRange.xMax;
            int minY = mapRange.yMin;
            int maxY = mapRange.yMax;

            List<SamplePoint> samplePoints = new();
            List<RectInt> buildingRects = new();
            // The buildings waiting to be generated
            List<BuildingGenerationInfo> generatings = new();
            foreach(var building in buildingGenerationInfos)
            {
                int num = building.num.y > building.num.x ? (int)random.Range(building.num.x, building.num.y) : building.num.x;
                for (int i = 0; i < num; i++)
                {
                    generatings.Add(building);
                }
            }

            // Start with a random point
            Vector2Int firstPoint = new((int)random.Range(minX, maxX), (int)random.Range(minY, maxY));
            samplePoints.Add(new SamplePoint(firstPoint, generatings[0]));
            buildingRects.Add(new RectInt(firstPoint, generatings[0].size));
            generatings.RemoveAt(0);

            while (samplePoints.Count > 0 && generatings.Count > 0)
            {
                int randomIndex = (int)random.Range(0, samplePoints.Count);
                SamplePoint samplePoint = samplePoints[randomIndex];
                Vector2 center = new(samplePoint.position.x + samplePoint.building.size.x / 2f, 
                    samplePoint.position.y + samplePoint.building.size.y / 2f);
                var nextBuilding = generatings[0];
                float xRadius = samplePoint.building.size.x / 2f + nextBuilding.size.x / 2f;
                float yRadius = samplePoint.building.size.y / 2f + nextBuilding.size.y / 2f;
                float minDistance = Mathf.Sqrt(xRadius * xRadius + yRadius * yRadius);

                bool foundNewPoint = false;
                for (int i = 0; i < maxAttempts; i++)
                {
                    float angle = random.Range(0f, Mathf.PI * 2f);
                    float distance = random.Range(minDistance, minDistance * 2f);
                    Vector2 newCenter = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    Vector2Int newPoint = Vector2Int.FloorToInt(newCenter - new Vector2(nextBuilding.size.x / 2f, nextBuilding.size.y / 2f));
                    RectInt newRect = new RectInt(newPoint, nextBuilding.size);
                    if(newRect.xMin < minX || newRect.xMax > maxX || newRect.yMin < minY || newRect.yMax > maxY)
                    {
                        continue;
                    }

                    if (!Overlap(newRect, buildingRects) && !Overlap(newRect, excludedPoints))
                    {
                        samplePoints.Add(new SamplePoint(newPoint, nextBuilding));
                        buildingRects.Add(newRect);
                        generatings.RemoveAt(0);
                        foundNewPoint = true;
                        break;
                    }
                }
                if (!foundNewPoint)
                {
                    samplePoints.RemoveAt(randomIndex);
                    buildingRects.RemoveAt(randomIndex);
                    generatings.Add(samplePoint.building);
                }
            }

            return samplePoints;
        }

        List<Vector2Int> GetOccupiedPoints(TilemapManager tilemap, RectInt range)
        {
            List<Vector2Int> occupiedPoints = new();
            for (int x = range.xMin; x < range.xMax; x++)
            {
                for(int y = range.yMin; y < range.yMax; y++)
                {
                    if (tilemap.GetTile(new Vector3Int(x, y, 0)))
                    {
                        occupiedPoints.Add(new Vector2Int(x, y));
                    }
                }
            }
            return occupiedPoints;
        }

        bool Overlap(RectInt rect, List<RectInt> otherRects)
        {
            foreach (var otherRect in otherRects)
            {
                if (rect.Overlaps(otherRect)) return true;
            }
            return false;
        }

        bool Overlap(RectInt rect, List<Vector2Int> points)
        {
            foreach (var point in points)
            {
                if (rect.Contains(point)) return true;
            }
            return false;
        }
    }
}
