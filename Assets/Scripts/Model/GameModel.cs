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
                    Tiles[x, y] = new TileModel(this, x, y, Random.Range(0, colors));
                }
            }
        }

        public TileModel GetTileAtDirection(Vector2Int position, Direction direction)
        {
            return direction switch
            {
                Direction.Top => position.y < Tiles.GetLength(1) - 1 ? Tiles[position.x, position.y + 1] : null,
                Direction.Down => position.y > 0 ? Tiles[position.x, position.y - 1] : null,
                Direction.Left => position.x > 0 ? Tiles[position.x - 1, position.y] : null,
                Direction.Right => position.x < Tiles.GetLength(0) - 1 ? Tiles[position.x + 1, position.y] : null,
                _ => null,
            };
        }
    }
}