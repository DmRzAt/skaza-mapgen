namespace Skaza.MapGen
{
    public static class RockLayer
    {
        public const float InitialRockChance = 0.30f;
        public const int SmoothingPasses = 3;

        private const int BecomeRockNeighbours = 5;
        private const int BecomeOpenNeighbours = 2;
        private const ulong RockStream = 1;

        public static void Paint(WorldGrid grid, ulong seed)
        {
            bool[] rocks = Scatter(grid.Width, grid.Height, new SimRandom(seed, RockStream));

            for (int pass = 0; pass < SmoothingPasses; pass++)
            {
                rocks = Smooth(rocks, grid.Width, grid.Height);
            }

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (rocks[y * grid.Width + x] || IsBorder(x, y, grid.Width, grid.Height))
                    {
                        grid.SetTile(new GridPosition(x, y), TileType.Rock);
                    }
                }
            }
        }

        private static bool[] Scatter(int width, int height, SimRandom random)
        {
            var rocks = new bool[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rocks[y * width + x] = IsBorder(x, y, width, height) || random.NextFloat() < InitialRockChance;
                }
            }

            return rocks;
        }

        private static bool[] Smooth(bool[] rocks, int width, int height)
        {
            var next = new bool[rocks.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int neighbours = CountRockNeighbours(rocks, width, height, x, y);
                    bool current = rocks[y * width + x];
                    next[y * width + x] = neighbours >= BecomeRockNeighbours || (current && neighbours > BecomeOpenNeighbours);
                }
            }

            return next;
        }

        private static int CountRockNeighbours(bool[] rocks, int width, int height, int x, int y)
        {
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int nx = x + dx;
                    int ny = y + dy;
                    bool outside = nx < 0 || ny < 0 || nx >= width || ny >= height;
                    if (outside || rocks[ny * width + nx])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static bool IsBorder(int x, int y, int width, int height)
        {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
        }
    }
}
