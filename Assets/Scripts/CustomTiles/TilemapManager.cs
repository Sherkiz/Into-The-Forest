using ITF.WorldGeneration;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{
    [RequireComponent (typeof (Tilemap))]
    public class TilemapManager : MonoBehaviour
    {
        [SerializeField] private TileBase placeHolderTile;
        private Tilemap tilemap;
        public void SetTile(Vector3Int pos, TileBase tile, bool setTileOccupied = true)
        {
            if (tilemap.GetTile(pos) != null) Debug.Log("Overlap !");
            if (pos.x > cellBounds.xMax || pos.y > cellBounds.yMax) { return; }
            tilemap.SetTile(pos, tile);
            if (tile != null && setTileOccupied) occupiedTiles[pos] = tile;
        }
        public void SetTile(Vector2Int pos, TileBase tile) => SetTile(new Vector3Int(pos.x, pos.y), tile);
        public bool TrySetTile(Vector3Int pos, TileBase tile, bool setTileOccupied = true)
        {
            if (tilemap.GetTile(pos) != null) return false;
            else
            {
                SetTile(pos, tile, setTileOccupied);
                return true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos">Position of the center tile</param>
        /// <param name="xNeighbors">Number of tiles in the x direction to set occupied.</param>
        /// <param name="yNeighbors">Number of tiles in the y direction to set occupied.</param>
        public void SetNeighborsOccupied(Vector2Int pos, int xNeighbors, int yNeighbors)
        {
            for (int x = -xNeighbors; x <= xNeighbors; x++) 
            {
                for (int y = -yNeighbors; x <= yNeighbors; x++)
                {
                    if (x == 0 && y == 0) continue;
                    occupiedTiles[(Vector3Int) (pos + new Vector2Int(x, y))] = placeHolderTile;
                }
            }
        }
        public TileBase GetTile(Vector3Int pos) => tilemap.GetTile(pos);
        public TileBase GetTile(Vector2Int pos) => GetTile(new Vector3Int(pos.x, pos.y));
        public BoundsInt cellBounds { get => tilemap.cellBounds; }
        public Vector3Int origin { get => tilemap.origin; set => tilemap.origin = value; }
        public Vector3Int size { get => tilemap.size; set => tilemap.size = value; }
        public void ResizeBounds() => tilemap.ResizeBounds();
        private Dictionary<Vector3Int, TileBase> occupiedTiles = new();
        public Dictionary<Vector3Int, TileBase> OccupiedTiles {  get => occupiedTiles; }
        public void PlaceMultipleTiles(MultipleTilesObject multipleTilesObject, Vector3Int position)
        {
            for (int i = 0; i < multipleTilesObject.tiles.Length; i++)
            {
                var tile = multipleTilesObject.tiles[i];
                var posOffset = multipleTilesObject.posOffsets[i] + (Vector3Int)multipleTilesObject.expandLeftBottom;
                SetTile(position + posOffset, tile);
            }
        }
        public bool IsPlaceable(int xSize, int ySize, Vector3Int pos)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (occupiedTiles.TryGetValue(pos + new Vector3Int(x, y, 0), out TileBase tile)) return false;
                }
            }
            return true;
        }
        public bool OverlapOccupiedTiles(RectInt rect)
        {
            foreach(var pos in occupiedTiles.Keys)
            {
                if (rect.Contains(new Vector2Int(pos.x, pos.y))) return true;
            }
            return false;
        }
        public bool IsTileEmpty(Vector3Int pos) => tilemap.GetTile(pos) == null;
        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
        }
    }
}
