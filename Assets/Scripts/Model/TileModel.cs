using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Match3.Model
{
    public class TileModel
    {
        public event Action<Direction> OnSuccessfulSwap;
        public event Action OnFailedSwap;

        readonly Dictionary<Direction, TileModel> validSwaps = new() {
            {Direction.Top, null},
            {Direction.Down, null},
            {Direction.Left, null},
            {Direction.Right, null}
        };

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Color { get; private set; }

        public GameModel GameModel { get; private set; }
        public bool IsSwappable => validSwaps.Any(kvp => kvp.Value != null);
        public Dictionary<Direction, TileModel> ValidSwaps => validSwaps;

        Vector2Int Position => new(X, Y);

        public TileModel(GameModel gameModel, int x, int y, int color)
        {
            GameModel = gameModel;
            X = x;
            Y = y;
            Color = color;
        }

        public void RefreshValidSwaps()
        {
            foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                validSwaps[direction] = null;

                var other = GameModel.GetTileAtDirection(new(X, Y), direction);
                if (other == null)
                    continue;

                var posA = Position;
                var posB = other.Position;
                X = posB.x;
                Y = posB.y;
                other.X = posA.x;
                other.Y = posA.y;

                var matchesA = GetMatches();
                var matchesB = other.GetMatches();
                if (matchesA.Count > 0 || matchesB.Count > 0)
                    validSwaps[direction] = other;

                X = posA.x;
                Y = posA.y;
                other.X = posB.x;
                other.Y = posB.y;
            }
        }

        public List<TileModel> GetMatches()
        {
            var matches = new List<TileModel>();

            var horizontalMatches = new List<TileModel>();
            CollectInDirection(Direction.Left, horizontalMatches);
            CollectInDirection(Direction.Right, horizontalMatches);
            if (horizontalMatches.Count >= 2)
                matches.AddRange(horizontalMatches);

            var verticalMatches = new List<TileModel>();
            CollectInDirection(Direction.Top, verticalMatches);
            CollectInDirection(Direction.Down, verticalMatches);
            if (verticalMatches.Count >= 2)
                matches.AddRange(verticalMatches);

            return matches;

            void CollectInDirection(Direction direction, List<TileModel> collection)
            {
                var lastTile = this;
                var axisLength = direction == Direction.Left || direction == Direction.Right ? GameModel.Tiles.GetLength(0) : GameModel.Tiles.GetLength(1);
                for (var i = 0; i < axisLength; i++)
                {
                    var otherTile = GameModel.GetTileAtDirection(lastTile.Position, direction);
                    if (otherTile != null && otherTile.Color == Color && otherTile != this)
                    {
                        collection.Add(otherTile);
                        lastTile = otherTile;
                    }
                    else
                        break;
                }
            }
        }

        public bool TryFall()
        {
            var oldY = Y;
            var targetY = Y;
            for (var checkY = Y - 1; checkY >= 0; checkY--)
            {
                var tile = GameModel.Tiles[X, checkY];
                if (tile != null)
                    break;

                targetY = checkY;
            }

            if (targetY == Y)
                return false;

            GameModel.Tiles[X, oldY] = null;
            Y = targetY;
            GameModel.Tiles[X, targetY] = this;

            return true;
        }

        public void TrySwap(Direction direction)
        {
            var other = validSwaps[direction];
            if (other == null)
            {
                OnFailedSwap?.Invoke();
                return;
            }

            var posA = Position;
            var posB = other.Position;
            X = posB.x;
            Y = posB.y;
            GameModel.Tiles[posB.x, posB.y] = this;

            other.X = posA.x;
            other.Y = posA.y;
            GameModel.Tiles[posA.x, posA.y] = other;

            other.OnSuccessfulSwap?.Invoke(direction.GetOpposite());
            OnSuccessfulSwap?.Invoke(direction);

            GameModel.EvaluateMatches();
        }
    }
}