using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{

    public interface ICustomTile
    {
        public string ID { get; }

        public TileType TileType { get; set; }

        /// <summary>
        /// The cost of passing through this Tile
        /// </summary>
        public int PassCose { get; set; }
    }

    public enum TileType
    {
        None,
        Tree,
        Resource,
        Road
    }

}