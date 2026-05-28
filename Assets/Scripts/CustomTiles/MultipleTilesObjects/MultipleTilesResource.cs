using UnityEngine;

namespace ITF.CustomTiles
{
    [CreateAssetMenu(fileName = "Resource", menuName = "ITF/Tiles/MultipleTilesObject/Resource")]
    public class MultipleTilesResource : MultipleTilesObject
    {
        [Tooltip("The indexes of the tiles that the resources can be harvested from.")]
        public int[] harvestTileIndexes;
    }
}
