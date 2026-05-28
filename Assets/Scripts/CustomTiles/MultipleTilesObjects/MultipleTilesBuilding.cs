using UnityEngine;

namespace ITF.CustomTiles
{
    [CreateAssetMenu(fileName = "Building", menuName = "ITF/Tiles/MultipleTilesObject/Building")]
    public class MultipleTilesBuilding : MultipleTilesObject
    {
        [Tooltip("The index of the tile the road should go in, and the units should spawn from.")]
        public int entranceTileIndex;
    }
}
