using UnityEngine;

namespace Match3.Model
{
    public class GameModel
    {
        public TileModel[,] Tiles { get; private set; }
        int colors;

        public GameModel(int width, int height, int colors)
        {
            this.colors = colors;
            Tiles = new TileModel[width, height];

            PopulateGrid();
        }

        void PopulateGrid()
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    Tiles[x, y] = new TileModel(x, y, Random.Range(0, colors));
                }
            }
        }
    }
}