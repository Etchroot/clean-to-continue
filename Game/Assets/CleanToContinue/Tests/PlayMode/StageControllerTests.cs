using System;
using System.Collections;
using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using CleanToContinue.Stage;
using CleanToContinue.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class StageControllerTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [UnityTest]
        public IEnumerator NinetyPercentCompletesOnceAndLocksInput()
        {
            var stage = CreateStageControllerWithFakeSources();

            stage.SetProgress(0.899f);
            Assert.That(stage.Controller.InputLocked, Is.False);

            stage.SetProgress(0.9f);
            stage.SetProgress(1f);
            yield return null;

            Assert.That(stage.Controller.InputLocked, Is.True);
            Assert.That(stage.MemoryOpenCount, Is.EqualTo(1));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    UnityEngine.Object.Destroy(createdObject);
                }
            }

            createdObjects.Clear();
            yield return null;
        }

        private StageFixture CreateStageControllerWithFakeSources()
        {
            var stageObject = new GameObject("Stage Controller Test");
            var memoryObject = new GameObject("Memory Panel Test");
            createdObjects.Add(stageObject);
            createdObjects.Add(memoryObject);

            var sources = new[]
            {
                new MutableProgressSource(CleaningTool.AirGun),
                new MutableProgressSource(CleaningTool.CottonSwab),
                new MutableProgressSource(CleaningTool.Cloth)
            };
            var progressModel = new StageProgressModel(sources, 0.9f);
            var memoryView = memoryObject.AddComponent<MemoryPanelView>();
            var controller = stageObject.AddComponent<StageController>();
            var fixture = new StageFixture(controller, progressModel, sources);
            memoryView.Opened += fixture.RecordMemoryOpen;
            controller.Configure(
                new ToolSelectionModel(),
                progressModel,
                sources,
                memoryView);
            controller.Initialize();
            return fixture;
        }

        private sealed class StageFixture
        {
            private readonly StageProgressModel progressModel;
            private readonly MutableProgressSource[] sources;

            public StageFixture(
                StageController controller,
                StageProgressModel progressModel,
                MutableProgressSource[] sources)
            {
                Controller = controller;
                this.progressModel = progressModel;
                this.sources = sources;
            }

            public StageController Controller { get; }
            public int MemoryOpenCount { get; private set; }

            public void SetProgress(float value)
            {
                foreach (var source in sources)
                {
                    source.SetProgress(value);
                }

                progressModel.Refresh();
            }

            public void RecordMemoryOpen()
            {
                MemoryOpenCount++;
            }
        }

        private sealed class MutableProgressSource : IProgressSource
        {
            public MutableProgressSource(CleaningTool tool)
            {
                Tool = tool;
            }

            public CleaningTool Tool { get; }
            public float Progress01 { get; private set; }

            public event Action ProgressChanged;

            public void SetProgress(float value)
            {
                Progress01 = Mathf.Clamp01(value);
                ProgressChanged?.Invoke();
            }
        }
    }
}
