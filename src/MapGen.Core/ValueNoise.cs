using System;

namespace Skaza.MapGen
{
    public sealed class ValueNoise
    {
        private const double UnitScale = 1.0 / (1UL << 53);

        private readonly ulong _seed;
        private readonly uint _layer;

        public ValueNoise(ulong seed, uint layer)
        {
            _seed = seed;
            _layer = layer;
        }

        public double Sample(double x, double y)
        {
            long x0 = (long)Math.Floor(x);
            long y0 = (long)Math.Floor(y);
            double tx = Smooth(x - x0);
            double ty = Smooth(y - y0);

            double top = Lerp(Lattice(x0, y0), Lattice(x0 + 1, y0), tx);
            double bottom = Lerp(Lattice(x0, y0 + 1), Lattice(x0 + 1, y0 + 1), tx);
            return Lerp(top, bottom, ty);
        }

        public double Fractal(double x, double y, int octaves)
        {
            if (octaves < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(octaves));
            }

            double sum = 0.0;
            double amplitude = 1.0;
            double frequency = 1.0;
            double totalAmplitude = 0.0;

            for (int i = 0; i < octaves; i++)
            {
                sum += Sample(x * frequency, y * frequency) * amplitude;
                totalAmplitude += amplitude;
                amplitude *= 0.5;
                frequency *= 2.0;
            }

            return sum / totalAmplitude;
        }

        private double Lattice(long x, long y)
        {
            unchecked
            {
                ulong hash = _seed ^ (_layer * 0x9E3779B97F4A7C15UL);
                hash = Mix(hash ^ ((ulong)x * 0xBF58476D1CE4E5B9UL));
                hash = Mix(hash ^ ((ulong)y * 0x94D049BB133111EBUL));
                return (hash >> 11) * UnitScale;
            }
        }

        private static ulong Mix(ulong value)
        {
            unchecked
            {
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }

        private static double Smooth(double t)
        {
            return t * t * (3.0 - 2.0 * t);
        }

        private static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }
    }
}
