using Match3.Model;
using UnityEngine;

namespace Match3.View
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] int colors;
        [SerializeField] TileView tilePrefab;

        GameModel model;

        TileView[,] tiles;

        void Start()
        {
            model = new GameModel(width, height, colors);
            tiles = new TileView[width, height];

            for (var x = 0; x < model.Tiles.GetLength(0); x++)
            {
                for (var y = 0; y < model.Tiles.GetLength(1); y++)
                {
                    var tileModel = model.Tiles[x, y];
                    var tileView = Instantiate(tilePrefab, transform);
                    tileView.Setup(tileModel);
                    tiles[x, y] = tileView;
                }
            }
        }
    }
}