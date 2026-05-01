using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{
    [RequireComponent (typeof (Tilemap))]
    public class ObjectTileMap : MonoBehaviour
    {
        private Tilemap tilemap;
        public void SetTile(Vector3Int pos, TileBase tile)
        {
            tilemap.SetTile(pos, tile);
            if (tile != null) occupiedTilesPos.Add(pos);
        }
        public void SetTile(Vector2Int pos, TileBase tile) => SetTile(new Vector3Int(pos.x, pos.y), tile);
        public void GetTile(Vector3Int pos) => tilemap.GetTile(pos);
        public void GetTile(Vector2Int pos) => tilemap.GetTile(new Vector3Int(pos.x, pos.y));
        public BoundsInt cellBounds { get => tilemap.cellBounds; }
        public Vector3Int origin => tilemap.origin;
        public Vector3Int size => tilemap.size;
        public void ResizeBounds() => tilemap.ResizeBounds();
        private List<Vector3Int> occupiedTilesPos = new();
        public List<Vector3Int> OccupiedTilesPos {  get => occupiedTilesPos; }
        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
        }
    }
}
