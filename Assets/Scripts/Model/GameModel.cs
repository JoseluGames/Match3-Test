using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.Model
{
    public class GameModel
    {
        public event Action<TileModel, int> OnTileSpawned;
        public event Action<List<List<TileModel>>, List<TileModel>> OnBoardEvaluated;

        int colors;

        public TileModel[,] Tiles { get; private set; }

        public GameModel(int width, int height, int colors)
        {
            this.colors = colors;
            Tiles = new TileModel[width, height];
        }

        public void PopulateGrid()
        {
            var fallingTiles = new List<TileModel>();

            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                    fallingTiles.Add(SpawnTile(x, y, Tiles.GetLength(1) + y));

            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                    Tiles[x, y].RefreshValidSwaps();

            OnBoardEvaluated?.Invoke(new(), fallingTiles);
        }

        public void EvaluateMatches()
        {
            var matches = new List<List<TileModel>>();
            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile == null)
                        continue;

                    var matchesForTile = tile.GetMatches();
                    if (matchesForTile.Count > 0)
                    {
                        matchesForTile.Add(tile);
                        matches.Add(matchesForTile);
                    }
                }

            foreach (var group in matches)
                foreach (var match in group)
                    Tiles[match.X, match.Y] = null;

            var fallingTiles = new List<TileModel>();

            for (var x = 0; x < Tiles.GetLength(0); x++)
                for (var y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile != null && tile.TryFall())
                        fallingTiles.Add(tile);
                }

            for (var x = 0; x < Tiles.GetLength(0); x++)
            {
                var lowerEmptyY = Tiles.GetLength(1);
                for (var y = 0; y < Tiles.GetLength(1); y++)
                    if (Tiles[x, y] == null)
                    {
                        if (y < lowerEmptyY)
                            lowerEmptyY = y;

                        var tile = SpawnTile(x, y, Tiles.GetLength(1) + y - lowerEmptyY);
                        fallingTiles.Add(tile);
                    }
            }

            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                    Tiles[x, y].RefreshValidSwaps();

            OnBoardEvaluated?.Invoke(matches, fallingTiles);
        }

        public TileModel SpawnTile(int x, int y, int spawnY)
        {
            var tile = new TileModel(this, x, y, GetValidColorForPosition(x, y));
            Tiles[x, y] = tile;
            OnTileSpawned?.Invoke(tile, spawnY);

            return tile;
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