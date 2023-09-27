using UnityEngine;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = "TileColors", menuName = "Match3/Tile Colors")]
    public class TileColors : ScriptableObject
    {
        [SerializeField] Sprite[] tileSprites;

        public Sprite[] TileSprites => tileSprites;
    }
}