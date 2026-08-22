using System;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using CleanToContinue.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class ToolSelectorViewTests
    {
        [Test]
        public void ProgressButtonsShowTheAverageOfEverySurfaceForTheirTool()
        {
            var root = new GameObject("Tool Selector Average Test");
            var airObject = new GameObject("Air Progress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var clothObject = new GameObject("Cloth Progress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            try
            {
                var airFill = airObject.GetComponent<Image>();
                var clothFill = clothObject.GetComponent<Image>();
                var selector = root.AddComponent<ToolSelectorView>();
                selector.Configure(
                    new ToolSelectionModel(),
                    new IProgressSource[]
                    {
                        new FakeProgress(CleaningTool.AirGun, 0.25f),
                        new FakeProgress(CleaningTool.AirGun, 0.75f),
                        new FakeProgress(CleaningTool.Cloth, 0.2f)
                    },
                    new[]
                    {
                        new ToolSelectorView.ToolButtonBinding
                        {
                            Tool = CleaningTool.AirGun,
                            ProgressFill = airFill
                        },
                        new ToolSelectorView.ToolButtonBinding
                        {
                            Tool = CleaningTool.Cloth,
                            ProgressFill = clothFill
                        }
                    });

                Assert.That(airFill.fillAmount, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(clothFill.fillAmount, Is.EqualTo(0.2f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(airObject);
                UnityEngine.Object.DestroyImmediate(clothObject);
            }
        }

        private sealed class FakeProgress : IProgressSource
        {
            public FakeProgress(CleaningTool tool, float progress)
            {
                Tool = tool;
                Progress01 = progress;
            }

            public CleaningTool Tool { get; }
            public float Progress01 { get; }
            public event Action ProgressChanged;
        }
    }
}
