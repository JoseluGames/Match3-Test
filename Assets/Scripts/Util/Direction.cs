namespace Match3.Model
{
    public enum Direction
    {
        Top,
        Down,
        Left,
        Right
    }

    public static class DirectionExtensions
    {
        public static Direction GetOpposite(this Direction direction)
        {
            return direction switch
            {
                Direction.Top => Direction.Down,
                Direction.Down => Direction.Top,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}