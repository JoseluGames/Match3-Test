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

            RefreshValidSwaps();

            OnBoardEvaluated?.Invoke(EvaluateMatches(), fallingTiles);
        }

        public void EvaluateBoard()
        {
            var matches = EvaluateMatches();
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

            RefreshValidSwaps();

            OnBoardEvaluated?.Invoke(matches, fallingTiles);
        }

        List<List<TileModel>> EvaluateMatches()
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

            return matches;
        }

        void RefreshValidSwaps()
        {

            var anySwappable = false;
            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    Tiles[x, y].RefreshValidSwaps();
                    anySwappable |= Tiles[x, y].IsSwappable;
                }

            if (!anySwappable)
                Debug.LogWarning("No valid moves available");
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

            if (validColors.Count == 0)
            {
                Debug.LogWarning($"No valid colors for position ({x}, {y}), returning default.");
                return 0;
            }

            return validColors[UnityEngine.Random.Range(0, validColors.Count)];
        }

        public TileModel GetTileAtDirection(int x, int y, Direction direction)
        {
            return direction switch
            {
                Direction.Top => y < Tiles.GetLength(1) - 1 ? Tiles[x, y + 1] : null,
                Direction.Down => y > 0 ? Tiles[x, y - 1] : null,
                Direction.Left => x > 0 ? Tiles[x - 1, y] : null,
                Direction.Right => x < Tiles.GetLength(0) - 1 ? Tiles[x + 1, y] : null,
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