using System;
using UnityEngine;

namespace CleanToContinue.Surface
{
    public sealed class CoverageGrid
    {
        private readonly int width;
        private readonly int height;
        private readonly bool[] remaining;
        private readonly int targetCount;
        private int remainingCount;

        private CoverageGrid(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            this.width = width;
            this.height = height;
            targetCount = width * height;
            remainingCount = targetCount;
            remaining = new bool[targetCount];
            Array.Fill(remaining, true);
        }

        public float Progress01 => targetCount == 0
            ? 1f
            : 1f - remainingCount / (float)targetCount;

        public static CoverageGrid CreateFilled(int width, int height)
        {
            return new CoverageGrid(width, height);
        }

        public int ApplyDisc(Vector2 uv, float normalizedRadius)
        {
            if (targetCount == 0 || normalizedRadius <= 0f)
            {
                return 0;
            }

            var center = new Vector2(
                Mathf.Clamp01(uv.x),
                Mathf.Clamp01(uv.y));
            var radiusSquared = normalizedRadius * normalizedRadius;
            var newlyCleaned = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    if (!remaining[index])
                    {
                        continue;
                    }

                    var cellCenter = new Vector2(
                        (x + 0.5f) / width,
                        (y + 0.5f) / height);
                    if ((cellCenter - center).sqrMagnitude > radiusSquared)
                    {
                        continue;
                    }

                    remaining[index] = false;
                    remainingCount--;
                    newlyCleaned++;
                }
            }

            return newlyCleaned;
        }
    }
}
