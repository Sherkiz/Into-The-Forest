using ITF.CustomTiles;
using UnityEngine;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Ensure that all objects are passable between them
    /// </summary>
    [CreateAssetMenu(fileName = "PassableGenerator", menuName = "ITF/WorldGeneration/PassableGenerator")]
    public class PassableGenerator : ObjectGenerator
    {
        public override int Seed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override GenerateStatus Generate(TilemapManager tilemap)
        {
            throw new System.NotImplementedException();
        }

        public override void StopAllGeneration()
        {
            throw new System.NotImplementedException();
        }
    }

}