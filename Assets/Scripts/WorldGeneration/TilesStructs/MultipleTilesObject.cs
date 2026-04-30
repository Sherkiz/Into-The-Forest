using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    public struct MultipleTilesObject
    {
        public Vector2Int size;
        [Tooltip("The empty tile at the left-bottom of the building.")]
        public Vector2Int expandLeftBottom;
        [Tooltip("The empty tile at the right-top of the building.")]
        public Vector2Int expandRightTop;
        public Tile[] tiles;
        [Tooltip("Offsets from the left-bottom of the building to place the tiles. Should correspond one-to-one with the tiles.")]
        public Vector3Int[] posOffsets;
    }
}
