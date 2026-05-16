using ITF.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{
    [CreateAssetMenu(fileName = "Object", menuName = "ITF/Tiles/MultipleTilesObject/Base")]
    public class MultipleTilesObject : ScriptableObject
    {
        [Tooltip("The Object's name.")]
        public new string name;
        [Tooltip("The 2D-size of the object (in number of tiles). Must accoount for the expansion.")]
        public Vector2Int size;
        [Tooltip("All the tiles in the object.")]
        public Tile[] tiles;
        [Tooltip("Offsets from the left-bottom of the object to place the tiles. Should correspond one-to-one with the tiles.")]
        public Vector3Int[] posOffsets;
        [Tooltip("The empty tile at the left-bottom of the object.")]
        public Vector2Int expandLeftBottom;
        [Tooltip("The empty tile at the right-top of the object.")]
        public Vector2Int expandRightTop;
        [Tooltip("If true, fill expand area with placeholder tile")]
        public bool fillExpand = true;
        public TileType mapObjectType;

        [ContextMenu("Auto Create Pos Offsets")]
        public void AutoCreatePosOffsets()
        {
            Vector2Int unexpandedSize = size - expandLeftBottom - expandRightTop;
            posOffsets = new Vector3Int[unexpandedSize.x * unexpandedSize.y];
            for (int y = unexpandedSize.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < unexpandedSize.x; x++)
                {
                    posOffsets[(unexpandedSize.y - 1 - y) * unexpandedSize.x + x] = new Vector3Int(x, y, 0);
                }
            }            
        }
    }
}
