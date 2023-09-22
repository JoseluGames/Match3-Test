namespace Match3.Model
{
    public class TileModel
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Color { get; private set; }

        public TileModel(int x, int y, int color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}