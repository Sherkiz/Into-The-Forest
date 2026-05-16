using ITF.Math;
using ITF.Utilities;
using ITF.CustomTiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ITF.World;

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
            [Tooltip("The building multi-tiles object.")]
            public MultipleTilesBuilding buildingTiles;
            [Tooltip("If num.y > num.x, will random generate the count between [num.x, num.y), else the count = num.x")]
            public Vector2Int num = new(1, 0);
            public Vector2Int size => buildingTiles.size;
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
        [SerializeField]
        BuildingGenerationInfo[] buildingGenerationInfos;
        [SerializeField] RectInt mapRange = new(5, 2, 22, 32);
        [Tooltip("The rate of distance between buildings, [x, y]")]
        public Vector2 distanceRateRange = new(1, 2);

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

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            var excludedPoints = GetOccupiedPoints(tilemap, mapRange);
            generateStatus.progress = .25f;
            yield return null;

            var samplePoints = PoissonDiscSampling(mapRange, new XorShiftRandom((uint)RandomManager.GetSeedFor(name)), excludedPoints);
            generateStatus.progress = .5f;

            yield return null;

            // Place the buildings
            foreach (var samplePoint in samplePoints)
            {
                MultipleTilesBuilding building = samplePoint.building.buildingTiles;
                tilemap.PlaceMultipleTiles(building, (Vector3Int) samplePoint.position);

                Vector2Int realSize = samplePoint.building.size -  building.expandLeftBottom - building.expandRightTop;
                Vector2Int realPos = samplePoint.position + building.expandLeftBottom;
                WorldManager.Map.AddMapObject(new MapObject(building.name, new RectInt(realPos, realSize), building.mapObjectType));
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
            if (generatings.Count == 0) return new();

            // Start with a random point
            var firstBuilding = generatings[0];
            Vector2Int firstPoint = new((int)random.Range(minX, maxX - firstBuilding.size.x), (int)random.Range(minY, maxY - firstBuilding.size.y));
            samplePoints.Add(new SamplePoint(firstPoint, firstBuilding));
            buildingRects.Add(new RectInt(firstPoint, firstBuilding.size));
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
                    float distance = random.Range(minDistance * distanceRateRange.x, minDistance * distanceRateRange.y);
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
