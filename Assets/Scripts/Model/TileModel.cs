using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Match3.Model
{
    public class TileModel
    {
        public event Action<Direction> OnSuccessfulSwap;
        public event Action<Direction> OnFailedSwap;
        public event Action<bool, List<TileModel>> OnMatch;
        public event Action<int, int> OnFall;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Color { get; private set; }

        public GameModel GameModel { get; private set; }

        readonly Dictionary<Direction, TileModel> validSwaps = new() {
            {Direction.Top, null},
            {Direction.Down, null},
            {Direction.Left, null},
            {Direction.Right, null}
        };

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
                X = other.X;
                Y = other.Y;
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

        List<TileModel> GetMatches()
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

        public void ResolveMatch()
        {
            var matches = GetMatches();
            if (matches.Count == 0)
                return;

            matches.Add(this);

            foreach (var match in matches)
                match.OnMatch?.Invoke(match == this, matches);
        }

        public void TryFall()
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
                return;

            GameModel.Tiles[X, oldY] = null;
            Y = targetY;
            GameModel.Tiles[X, targetY] = this;
            RefreshValidSwaps();

            OnFall?.Invoke(oldY, targetY);
        }

        public void TrySwap(Direction direction)
        {
            var other = validSwaps[direction];
            if (other == null)
            {
                OnFailedSwap?.Invoke(direction);
                return;
            }

            var posA = Position;
            var posB = other.Position;
            X = other.X;
            Y = other.Y;
            other.X = posA.x;
            other.Y = posA.y;

            GameModel.Tiles[X, Y] = this;
            RefreshValidSwaps();

            GameModel.Tiles[other.X, other.Y] = other;
            other.RefreshValidSwaps();

            other.OnSuccessfulSwap?.Invoke(direction.GetOpposite());
            OnSuccessfulSwap?.Invoke(direction);
        }
    }
}