using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEngine;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class CoverageGridTests
    {
        [Test]
        public void RepeatingSameStrokeDoesNotDoubleCount()
        {
            var grid = CoverageGrid.CreateFilled(32, 32);
            grid.ApplyDisc(new Vector2(0.5f, 0.5f), 0.1f);
            var once = grid.Progress01;

            grid.ApplyDisc(new Vector2(0.5f, 0.5f), 0.1f);

            Assert.That(grid.Progress01, Is.EqualTo(once));
        }

        [Test]
        public void UvOutsideRangeIsClampedToCleanableEdge()
        {
            var grid = CoverageGrid.CreateFilled(16, 16);

            Assert.DoesNotThrow(() =>
                grid.ApplyDisc(new Vector2(-1f, 2f), 0.15f));
            Assert.That(grid.Progress01, Is.GreaterThan(0f));
        }

        [Test]
        public void MeshUvCoverageIgnoresEmptyAtlasSpace()
        {
            var uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0.25f, 0f),
                new Vector2(0f, 0.25f)
            };
            var grid = CoverageGrid.CreateFromUvTriangles(16, 16, uv, new[] { 0, 1, 2 });

            grid.ApplyDisc(new Vector2(0.1f, 0.1f), 0.25f);

            Assert.That(grid.Progress01, Is.EqualTo(1f));
        }
    }
}
