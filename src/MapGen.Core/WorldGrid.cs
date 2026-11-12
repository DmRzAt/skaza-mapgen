using System;

namespace Skaza.MapGen
{
    public sealed class WorldGrid
    {
        private readonly TileType[] _tiles;

        public WorldGrid(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
            _tiles = new TileType[width * height];
        }

        public int Width { get; }
        public int Height { get; }

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0 && position.X < Width
                && position.Y >= 0 && position.Y < Height;
        }

        public TileType GetTile(GridPosition position)
        {
            return _tiles[IndexOf(position)];
        }

        public void SetTile(GridPosition position, TileType type)
        {
            _tiles[IndexOf(position)] = type;
        }

        public bool IsWalkable(GridPosition position)
        {
            return IsInside(position) && GetTile(position) != TileType.Rock;
        }

        private int IndexOf(GridPosition position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Position is outside the grid.");
            }

            return position.Y * Width + position.X;
        }
    }
}