using System;
using UnityEngine;

namespace Match3.Model
{
    public class TileModel
    {
        public event Action<Direction> OnSuccessfulSwap;
        public event Action<Direction> OnFailedSwap;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Color { get; private set; }

        GameModel gameModel;

        public TileModel(GameModel gameModel, int x, int y, int color)
        {
            this.gameModel = gameModel;
            X = x;
            Y = y;
            Color = color;
        }

        TileModel GetTileAtDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Top => Y < gameModel.Tiles.GetLength(1) - 1 ? gameModel.Tiles[X, Y + 1] : null,
                Direction.Down => Y > 0 ? gameModel.Tiles[X, Y - 1] : null,
                Direction.Left => X > 0 ? gameModel.Tiles[X - 1, Y] : null,
                Direction.Right => X < gameModel.Tiles.GetLength(0) - 1 ? gameModel.Tiles[X + 1, Y] : null,
                _ => null,
            };
        }

        public void TrySwap(Direction direction)
        {
            var other = GetTileAtDirection(direction);
            if (other == null)
            {
                OnFailedSwap?.Invoke(direction);
                return;
            }

            if (Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y) == 1)
            {
                var tempX = X;
                var tempY = Y;
                X = other.X;
                Y = other.Y;
                other.X = tempX;
                other.Y = tempY;
                gameModel.Tiles[X, Y] = this;
                gameModel.Tiles[other.X, other.Y] = other;
                other.OnSuccessfulSwap?.Invoke(direction.GetOpposite());
                OnSuccessfulSwap?.Invoke(direction);
            }

            OnFailedSwap?.Invoke(direction);
        }
    }
}