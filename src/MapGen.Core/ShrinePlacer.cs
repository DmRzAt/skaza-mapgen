using System;
using System.Collections.Generic;

namespace Skaza.MapGen
{
    public static class ShrinePlacer
    {
        public const int ShrineCount = 3;
        public const int MinDistanceFromHeart = 4;
        public const int MinDistanceBetweenShrines = 8;

        private const ulong ShrineStream = 2;
        private const double SectorHalfWidth = Math.PI / 3.0;

        private static readonly Zone[] ShrineZones = { Zone.Clean, Zone.Borderland, Zone.DeepCorruption };

        public static IReadOnlyList<GridPosition> Place(WorldGrid grid, Zone[] zones, GridPosition heart, ulong seed)
        {
            var random = new SimRandom(seed, ShrineStream);
            double baseAngle = random.NextFloat() * 2.0 * Math.PI;
            var shrines = new List<GridPosition>(ShrineCount);

            for (int i = 0; i < ShrineCount; i++)
            {
                double sectorAngle = baseAngle + i * 2.0 * Math.PI / ShrineCount;
                List<GridPosition> candidates = FindCandidates(grid, zones, heart, ShrineZones[i], shrines, sectorAngle, true);
                if (candidates.Count == 0)
                {
                    candidates = FindCandidates(grid, zones, heart, ShrineZones[i], shrines, sectorAngle, false);
                }

                if (candidates.Count == 0)
                {
                    throw new InvalidOperationException($"No reachable tile for a shrine in zone {ShrineZones[i]}.");
                }

                shrines.Add(candidates[random.NextInt(0, candidates.Count)]);
            }

            return shrines;
        }

        private static List<GridPosition> FindCandidates(
            WorldGrid grid, Zone[] zones, GridPosition heart, Zone zone,
            List<GridPosition> placed, double sectorAngle, bool requireSector)
        {
            var candidates = new List<GridPosition>();
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    if (zones[y * grid.Width + x] != zone || !grid.IsWalkable(position))
                    {
                        continue;
                    }

                    if (Distance(position, heart) < MinDistanceFromHeart || IsTooClose(position, placed))
                    {
                        continue;
                    }

                    if (requireSector && AngleDifference(AngleFrom(heart, position), sectorAngle) > SectorHalfWidth)
                    {
                        continue;
                    }

                    candidates.Add(position);
                }
            }

            return candidates;
        }

        private static bool IsTooClose(GridPosition position, List<GridPosition> placed)
        {
            foreach (GridPosition other in placed)
            {
                if (Distance(position, other) < MinDistanceBetweenShrines)
                {
                    return true;
                }
            }

            return false;
        }

        private static double Distance(GridPosition a, GridPosition b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double AngleFrom(GridPosition origin, GridPosition target)
        {
            return Math.Atan2(target.Y - origin.Y, target.X - origin.X);
        }

        private static double AngleDifference(double a, double b)
        {
            double difference = Math.IEEERemainder(a - b, 2.0 * Math.PI);
            return Math.Abs(difference);
        }
    }
}
