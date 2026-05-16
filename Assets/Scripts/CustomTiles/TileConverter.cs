#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{

    public class TileConverter : MonoBehaviour
    {
        public Tile[] tiles;

        [Space(20)]
        public string prefixID;
        public TileType tileType;
        public int passCost;

        [Space(20)]
        public string savePath;

        [ContextMenu("Convert to ObjectTile")]
        public void ConvertToObjectTiles()
        {
            for(int i = 0; i < tiles.Length; i++)
            {
                ObjectTile newTile = ScriptableObject.CreateInstance<ObjectTile>();
                newTile.sprite = tiles[i].sprite;
                newTile.color = tiles[i].color;
                newTile.transform = tiles[i].transform;
                newTile.flags = tiles[i].flags;
                newTile.colliderType = tiles[i].colliderType;
                newTile.SetID(prefixID + i);
                newTile.TileType = tileType;
                newTile.PassCost = passCost;
                newTile.name = newTile.ID;
                string assetPath = savePath + "/" + newTile.name + ".asset";
                AssetDatabase.CreateAsset(newTile, assetPath);
            }

            Debug.Log("Conversion complete!");
        }
    }

}

#endif