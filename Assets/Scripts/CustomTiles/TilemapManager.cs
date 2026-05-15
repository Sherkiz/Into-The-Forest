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
        public void SetTile(Vector3Int pos, TileBase tile, bool setTileOccupied = true, bool allowOverlap = true)
        {
            if (GetTile(pos) != null && tile != null && !allowOverlap)
            {
                Debug.Log("Tried to place tile " + tile.name + " on already existing tile! (At position " + pos + ", already occupied by tile " + GetTile(pos).name + ")");
                return;
            }
            if (!cellBounds.Contains(pos)) 
            {
                Debug.Log("Tried to place tile " + tile.name + " out of bounds! (At position " + pos + ")");
                return; 
            }
            tilemap.SetTile(pos, tile);
            if (tile == placeHolderTile) placeHolderTilesPosition.Add(pos);
            if (tile != null && setTileOccupied) occupiedTiles[pos] = tile;
        }
        public void SetTile(Vector2Int pos, TileBase tile) => SetTile(new Vector3Int(pos.x, pos.y), tile);
        public void ClearTile(Vector3Int pos) => SetTile(pos, null);
        public bool TrySetTile(Vector3Int pos, TileBase tile, bool setTileOccupied = true)
        {
            if (GetTile(pos) != null) return false;
            else
            {
                SetTile(pos, tile, setTileOccupied);
                return true;
            }
        }
        public TileBase GetTile(Vector3Int pos)
        {
            if (!cellBounds.Contains(pos)) return null;
            return tilemap.GetTile(pos);
        }
        public TileBase GetTile(Vector2Int pos) => GetTile(new Vector3Int(pos.x, pos.y));
        public BoundsInt cellBounds { get => tilemap.cellBounds; }
        public Vector3Int origin { get => tilemap.origin; set => tilemap.origin = value; }
        public Vector3Int size { get => tilemap.size; set => tilemap.size = value; }
        public void ResizeBounds()
        {
            tilemap.ResizeBounds();
        }
        private Dictionary<Vector3Int, TileBase> occupiedTiles = new();
        public Dictionary<Vector3Int, TileBase> OccupiedTiles {  get => occupiedTiles; }
        private List<Vector3Int> placeHolderTilesPosition = new();
        public void PlaceMultipleTiles(MultipleTilesObject multipleTilesObject, Vector3Int position)
        {
            for (int i = 0; i < multipleTilesObject.tiles.Length; i++)
            {
                var tile = multipleTilesObject.tiles[i];
                var posOffset = multipleTilesObject.posOffsets[i] + (Vector3Int)multipleTilesObject.expandLeftBottom;
                SetTile(position + posOffset, tile);
            }
            if (multipleTilesObject.fillExpand)
            {
                for (int y = 0; y < multipleTilesObject.size.y; y++)
                {
                    for (int x = 0; x < multipleTilesObject.expandLeftBottom.x; x++)
                        SetTile(new Vector3Int(x, y) + position, placeHolderTile);
                    for (int x = multipleTilesObject.size.x - multipleTilesObject.expandRightTop.x; x < multipleTilesObject.size.x; x++)
                        SetTile(new Vector3Int(x, y) + position, placeHolderTile);
                }
                for (int x = multipleTilesObject.size.x - multipleTilesObject.expandRightTop.x; x >= multipleTilesObject.expandLeftBottom.x; x--)
                {
                    for (int y = 0; y < multipleTilesObject.expandLeftBottom.y; y++)
                    {
                        SetTile(new Vector3Int(x, y) + position, placeHolderTile);
                    }
                    for (int y = multipleTilesObject.size.y - multipleTilesObject.expandRightTop.y; y < multipleTilesObject.size.y; y++)
                    {
                        SetTile(new Vector3Int(x, y) + position, placeHolderTile);
                    }
                }
            }
        }
        public void RemoveAllPlaceHolderTiles()
        {
            foreach (var pos in placeHolderTilesPosition) ClearTile(pos);
            placeHolderTilesPosition.Clear();
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
        public bool IsTileEmpty(Vector3Int pos) => GetTile(pos) == null;
        public void Clear()
        {
            tilemap.ClearAllTiles();
            occupiedTiles.Clear();
            placeHolderTilesPosition.Clear();
        }
        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
        }
    }
}
