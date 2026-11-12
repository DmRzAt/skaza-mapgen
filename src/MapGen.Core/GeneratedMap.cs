using System;
using System.Collections.Generic;

namespace Skaza.MapGen
{
    public sealed class GeneratedMap
    {
        private readonly Zone[] _zones;

        public GeneratedMap(ulong seed, WorldGrid grid, Zone[] zones, GridPosition heart, IReadOnlyList<GridPosition> shrines)
        {
            if (zones.Length != grid.Width * grid.Height)
            {
                throw new ArgumentException("Zone count must match the grid size.", nameof(zones));
            }

            Seed = seed;
            Grid = grid;
            _zones = zones;
            Heart = heart;
            Shrines = shrines;
        }

        public ulong Seed { get; }
        public WorldGrid Grid { get; }
        public GridPosition Heart { get; }
        public IReadOnlyList<GridPosition> Shrines { get; }
        public int Width => Grid.Width;
        public int Height => Grid.Height;

        public Zone GetZone(GridPosition position)
        {
            if (!Grid.IsInside(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Position is outside the map.");
            }

            return _zones[position.Y * Grid.Width + position.X];
        }
    }
}
