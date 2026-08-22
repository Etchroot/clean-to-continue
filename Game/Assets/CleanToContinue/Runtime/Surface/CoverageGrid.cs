using System;
using UnityEngine;

namespace CleanToContinue.Surface
{
    public sealed class CoverageGrid
    {
        private readonly int width;
        private readonly int height;
        private readonly bool[] remaining;
        private int targetCount;
        private int remainingCount;

        private CoverageGrid(int width, int height, bool fillAll)
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
            remaining = new bool[width * height];
            if (fillAll)
            {
                Array.Fill(remaining, true);
                targetCount = width * height;
                remainingCount = targetCount;
            }
        }

        public float Progress01 => targetCount == 0
            ? 1f
            : 1f - remainingCount / (float)targetCount;

        public static CoverageGrid CreateFilled(int width, int height)
        {
            return new CoverageGrid(width, height, true);
        }

        public static CoverageGrid CreateFromUvTriangles(
            int width,
            int height,
            Vector2[] uv,
            int[] triangles)
        {
            if (uv == null)
            {
                throw new ArgumentNullException(nameof(uv));
            }

            if (triangles == null)
            {
                throw new ArgumentNullException(nameof(triangles));
            }

            var grid = new CoverageGrid(width, height, false);
            if (width == 0 || height == 0)
            {
                return grid;
            }

            for (var index = 0; index + 2 < triangles.Length; index += 3)
            {
                var first = triangles[index];
                var second = triangles[index + 1];
                var third = triangles[index + 2];
                if (first < 0 || second < 0 || third < 0 ||
                    first >= uv.Length || second >= uv.Length || third >= uv.Length)
                {
                    continue;
                }

                grid.MarkTriangle(uv[first], uv[second], uv[third]);
            }

            return grid;
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

        private void MarkTriangle(Vector2 first, Vector2 second, Vector2 third)
        {
            var minimum = Vector2.Max(Vector2.zero, Vector2.Min(first, Vector2.Min(second, third)));
            var maximum = Vector2.Min(Vector2.one, Vector2.Max(first, Vector2.Max(second, third)));
            var minimumX = Mathf.Clamp(Mathf.FloorToInt(minimum.x * width), 0, width - 1);
            var maximumX = Mathf.Clamp(Mathf.CeilToInt(maximum.x * width) - 1, 0, width - 1);
            var minimumY = Mathf.Clamp(Mathf.FloorToInt(minimum.y * height), 0, height - 1);
            var maximumY = Mathf.Clamp(Mathf.CeilToInt(maximum.y * height) - 1, 0, height - 1);

            for (var y = minimumY; y <= maximumY; y++)
            {
                for (var x = minimumX; x <= maximumX; x++)
                {
                    var cellMinimum = new Vector2(x / (float)width, y / (float)height);
                    var cellMaximum = new Vector2((x + 1f) / width, (y + 1f) / height);
                    if (!TriangleOverlapsCell(first, second, third, cellMinimum, cellMaximum))
                    {
                        continue;
                    }

                    var cellIndex = y * width + x;
                    if (remaining[cellIndex])
                    {
                        continue;
                    }

                    remaining[cellIndex] = true;
                    remainingCount++;
                    targetCount++;
                }
            }
        }

        private static bool TriangleOverlapsCell(
            Vector2 first,
            Vector2 second,
            Vector2 third,
            Vector2 cellMinimum,
            Vector2 cellMaximum)
        {
            if (InsideCell(first, cellMinimum, cellMaximum) ||
                InsideCell(second, cellMinimum, cellMaximum) ||
                InsideCell(third, cellMinimum, cellMaximum))
            {
                return true;
            }

            var center = (cellMinimum + cellMaximum) * 0.5f;
            if (PointInTriangle(center, first, second, third))
            {
                return true;
            }

            var bottomRight = new Vector2(cellMaximum.x, cellMinimum.y);
            var topLeft = new Vector2(cellMinimum.x, cellMaximum.y);
            return SegmentsIntersect(first, second, cellMinimum, bottomRight) ||
                   SegmentsIntersect(first, second, bottomRight, cellMaximum) ||
                   SegmentsIntersect(first, second, cellMaximum, topLeft) ||
                   SegmentsIntersect(first, second, topLeft, cellMinimum) ||
                   SegmentsIntersect(second, third, cellMinimum, bottomRight) ||
                   SegmentsIntersect(second, third, bottomRight, cellMaximum) ||
                   SegmentsIntersect(second, third, cellMaximum, topLeft) ||
                   SegmentsIntersect(second, third, topLeft, cellMinimum) ||
                   SegmentsIntersect(third, first, cellMinimum, bottomRight) ||
                   SegmentsIntersect(third, first, bottomRight, cellMaximum) ||
                   SegmentsIntersect(third, first, cellMaximum, topLeft) ||
                   SegmentsIntersect(third, first, topLeft, cellMinimum);
        }

        private static bool InsideCell(Vector2 point, Vector2 minimum, Vector2 maximum)
        {
            return point.x >= minimum.x && point.x <= maximum.x &&
                   point.y >= minimum.y && point.y <= maximum.y;
        }

        private static bool PointInTriangle(Vector2 point, Vector2 first, Vector2 second, Vector2 third)
        {
            var firstSign = Cross(point - first, second - first);
            var secondSign = Cross(point - second, third - second);
            var thirdSign = Cross(point - third, first - third);
            var hasNegative = firstSign < 0f || secondSign < 0f || thirdSign < 0f;
            var hasPositive = firstSign > 0f || secondSign > 0f || thirdSign > 0f;
            return !(hasNegative && hasPositive);
        }

        private static bool SegmentsIntersect(Vector2 firstA, Vector2 firstB, Vector2 secondA, Vector2 secondB)
        {
            var firstDirection = firstB - firstA;
            var secondDirection = secondB - secondA;
            var denominator = Cross(firstDirection, secondDirection);
            if (Mathf.Abs(denominator) < 0.000001f)
            {
                return false;
            }

            var offset = secondA - firstA;
            var firstTime = Cross(offset, secondDirection) / denominator;
            var secondTime = Cross(offset, firstDirection) / denominator;
            return firstTime >= 0f && firstTime <= 1f && secondTime >= 0f && secondTime <= 1f;
        }

        private static float Cross(Vector2 first, Vector2 second)
        {
            return first.x * second.y - first.y * second.x;
        }
    }
}
