using UnityEngine;
using ITF.CustomTiles;
namespace ITF.World
{
    [CreateAssetMenu(fileName = "TrainingBuilding", menuName = "ITF/Tiles/MultipleTilesObject/TrainingBuilding")]
    public class TrainingBuilding : MultipleTilesBuilding
    {
        public int numberOfTrainingSlots;
    }
}
