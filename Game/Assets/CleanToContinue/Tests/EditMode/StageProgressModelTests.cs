using System;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using NUnit.Framework;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class StageProgressModelTests
    {
        [Test]
        public void ThreeSourcesContributeWithEqualWeight()
        {
            var model = CreateModel(1f, 0.5f, 0f);

            model.Refresh();

            Assert.That(model.Progress01, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void DoesNotCompleteBelowNinetyPercent()
        {
            var model = CreateModel(0.9f, 0.9f, 0.899f);

            model.Refresh();

            Assert.That(model.IsComplete, Is.False);
        }

        [Test]
        public void CompletesOnceWhenAverageReachesNinetyPercent()
        {
            var air = new FakeSource(CleaningTool.AirGun, 0.9f);
            var gaps = new FakeSource(CleaningTool.CottonSwab, 0.9f);
            var polish = new FakeSource(CleaningTool.Cloth, 0.899f);
            var model = new StageProgressModel(
                new IProgressSource[] { air, gaps, polish },
                0.9f);
            var completionCalls = 0;
            model.Completed += () => completionCalls++;

            model.Refresh();
            polish.Set(0.901f);
            model.Refresh();
            model.Refresh();

            Assert.That(model.IsComplete, Is.True);
            Assert.That(completionCalls, Is.EqualTo(1));
        }

        [Test]
        public void CompletesAtExactlyNinetyPercent()
        {
            var model = CreateModel(0.9f, 0.9f, 0.9f);

            model.Refresh();

            Assert.That(model.IsComplete, Is.True);
        }

        private static StageProgressModel CreateModel(float air, float gaps, float polish)
        {
            return new StageProgressModel(
                new IProgressSource[]
                {
                    new FakeSource(CleaningTool.AirGun, air),
                    new FakeSource(CleaningTool.CottonSwab, gaps),
                    new FakeSource(CleaningTool.Cloth, polish)
                },
                0.9f);
        }

        private sealed class FakeSource : IProgressSource
        {
            public FakeSource(CleaningTool tool, float value)
            {
                Tool = tool;
                Progress01 = value;
            }

            public CleaningTool Tool { get; }
            public float Progress01 { get; private set; }

            public event Action ProgressChanged;

            public void Set(float value)
            {
                Progress01 = value;
                ProgressChanged?.Invoke();
            }
        }
    }
}
