using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.Model
{
    public class GameModel
    {
        public event Action<TileModel> OnTileSpawned;

        int colors;

        public TileModel[,] Tiles { get; private set; }

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
                    Tiles[x, y] = new TileModel(this, x, y, GetValidColorForPosition(x, y));
                }
            }
        }

        public void ClearTiles(List<TileModel> tiles)
        {
            foreach (var tile in tiles)
                Tiles[tile.X, tile.Y] = null;

            for (var x = 0; x < Tiles.GetLength(0); x++)
                for (var y = 0; y < Tiles.GetLength(1); y++)
                    Tiles[x, y]?.TryFall();

            for (var x = 0; x < Tiles.GetLength(0); x++)
                for (var y = 0; y < Tiles.GetLength(1); y++)
                    if (Tiles[x, y] == null)
                        SpawnTile(x, y);
        }

        public void SpawnTile(int x, int y)
        {
            var tile = new TileModel(this, x, y, GetValidColorForPosition(x, y));
            Tiles[x, y] = tile;
            OnTileSpawned?.Invoke(tile);
        }

        int GetValidColorForPosition(int x, int y)
        {
            var validColors = new List<int>();
            for (var color = 0; color < colors; color++)
                validColors.Add(color);

            var tileDown = GetTileAt(x, y - 1);
            var nextDown = GetTileAt(x, y - 2);
            if (tileDown != null && nextDown != null && tileDown.Color == nextDown.Color)
                validColors.Remove(tileDown.Color);

            var tileLeft = GetTileAt(x - 1, y);
            var nextLeft = GetTileAt(x - 2, y);
            if (tileLeft != null && nextLeft != null && tileLeft.Color == nextLeft.Color)
                validColors.Remove(tileLeft.Color);

            var tileRight = GetTileAt(x + 1, y);
            var nextRight = GetTileAt(x + 2, y);
            if (tileRight != null && nextRight != null && tileRight.Color == nextRight.Color)
                validColors.Remove(tileRight.Color);

            if (tileLeft != null && tileRight != null && tileLeft.Color == tileRight.Color)
                validColors.Remove(tileLeft.Color);

            return validColors[UnityEngine.Random.Range(0, validColors.Count)];
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

        TileModel GetTileAt(int x, int y)
        {
            if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1))
                return null;

            return Tiles[x, y];
        }
    }
}