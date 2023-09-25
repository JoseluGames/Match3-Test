using System.Collections.Generic;
using Match3.Model;
using UnityEngine;

namespace Match3.View.Action
{
    public abstract class TileAction { }

    public class SwapAction : TileAction
    {
        public bool Successful;
        public Direction Direction;
        public Vector2Int TargetPos;
    }

    public class MatchAction : TileAction
    {
        public List<TileView> Companions;
    }

    public class FallAction : TileAction
    {
        public int EndY;
    }
}