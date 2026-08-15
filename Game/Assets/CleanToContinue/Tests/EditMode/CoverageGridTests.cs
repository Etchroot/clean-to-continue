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
    }
}
