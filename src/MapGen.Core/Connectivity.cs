using System.Collections.Generic;

namespace Skaza.MapGen
{
    public static class Connectivity
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public static bool[] FindReachable(WorldGrid grid, GridPosition start)
        {
            var reachable = new bool[grid.Width * grid.Height];
            if (!grid.IsWalkable(start))
            {
                return reachable;
            }

            var queue = new Queue<GridPosition>();
            queue.Enqueue(start);
            reachable[Index(grid, start)] = true;

            while (queue.Count > 0)
            {
                GridPosition current = queue.Dequeue();
                foreach (GridPosition direction in Directions)
                {
                    var next = new GridPosition(current.X + direction.X, current.Y + direction.Y);
                    if (grid.IsWalkable(next) && !reachable[Index(grid, next)])
                    {
                        reachable[Index(grid, next)] = true;
                        queue.Enqueue(next);
                    }
                }
            }

            return reachable;
        }

        public static int SealUnreachable(WorldGrid grid, GridPosition start)
        {
            bool[] reachable = FindReachable(grid, start);
            int sealedTiles = 0;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    if (grid.IsWalkable(position) && !reachable[Index(grid, position)])
                    {
                        grid.SetTile(position, TileType.Rock);
                        sealedTiles++;
                    }
                }
            }

            return sealedTiles;
        }

        private static int Index(WorldGrid grid, GridPosition position)
        {
            return position.Y * grid.Width + position.X;
        }
    }
}
