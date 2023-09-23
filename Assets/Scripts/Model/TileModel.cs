using System;
using System.Collections.Generic;
using UnityEngine;

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

        Vector2Int Position => new(X, Y);

        public TileModel(GameModel gameModel, int x, int y, int color)
        {
            GameModel = gameModel;
            X = x;
            Y = y;
            Color = color;
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
                while (true)
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

            OnFall?.Invoke(oldY, targetY);
        }

        public void TrySwap(Direction direction)
        {
            var other = GameModel.GetTileAtDirection(new(X, Y), direction);
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

            var matchesA = GetMatches();
            var matchesB = other.GetMatches();
            if (matchesA.Count == 0 && matchesB.Count == 0)
            {
                RevertPositions();
                OnFailedSwap?.Invoke(direction);
                return;
            }

            if (Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y) == 1)
            {
                GameModel.Tiles[X, Y] = this;
                GameModel.Tiles[other.X, other.Y] = other;
                other.OnSuccessfulSwap?.Invoke(direction.GetOpposite());
                OnSuccessfulSwap?.Invoke(direction);
                return;
            }

            RevertPositions();
            OnFailedSwap?.Invoke(direction);

            void RevertPositions()
            {
                X = posA.x;
                Y = posA.y;
                other.X = posB.x;
                other.Y = posB.y;
            }
        }
    }
}