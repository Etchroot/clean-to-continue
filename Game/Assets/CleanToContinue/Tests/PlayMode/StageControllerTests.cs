using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CleanToContinue.Audio;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using CleanToContinue.Stage;
using CleanToContinue.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

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

        [UnityTest]
        public IEnumerator UiHeldCleanNeverStartsAndStopsAnExistingToolLoop()
        {
            var stage = CreateStageControllerWithFakeSources();

            stage.Controller.UpdateCleaningAudio(true, true);
            yield return null;
            Assert.That(AudibleLoopCount(stage.Audio), Is.EqualTo(0));

            stage.Controller.UpdateCleaningAudio(true, false);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(AudibleLoopCount(stage.Audio), Is.EqualTo(1));

            stage.Controller.UpdateCleaningAudio(true, true);
            yield return null;
            Assert.That(AudibleLoopCount(stage.Audio), Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator CompletionLocksToolButtonCallbacks()
        {
            var stage = CreateStageControllerWithFakeSources();

            stage.SetProgress(0.9f);
            stage.CottonSwabButton.onClick.Invoke();
            yield return null;

            Assert.That(stage.ToolSelector.Interactable, Is.False);
            Assert.That(stage.Selection.Selected, Is.EqualTo(CleaningTool.AirGun));
        }

        [UnityTest]
        public IEnumerator CrossedProgressRendersBeforeAnimationAndSubthresholdTextStaysAt89()
        {
            var stage = CreateStageControllerWithFakeSources();

            stage.SetProgress(0.899f);
            Assert.That(stage.ProgressText.text, Is.EqualTo("89%"));

            stage.SetProgress(0.9f);
            Assert.That(stage.ProgressWheel.DisplayedProgress01, Is.EqualTo(0.9f).Within(0.0001f));
            yield return null;

            Assert.That(stage.ProgressWheel.DisplayedProgress01, Is.GreaterThanOrEqualTo(0.9f));
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
            var wheelObject = new GameObject("Progress Wheel Test", typeof(RectTransform));
            var selectorObject = new GameObject("Tool Selector Test");
            var buttonObject = new GameObject("Cotton Swab Button Test", typeof(RectTransform));
            var audioObject = new GameObject("Cleaning Audio Test");
            createdObjects.Add(stageObject);
            createdObjects.Add(memoryObject);
            createdObjects.Add(wheelObject);
            createdObjects.Add(selectorObject);
            createdObjects.Add(buttonObject);
            createdObjects.Add(audioObject);

            var sources = new[]
            {
                new MutableProgressSource(CleaningTool.AirGun),
                new MutableProgressSource(CleaningTool.CottonSwab),
                new MutableProgressSource(CleaningTool.Cloth)
            };
            var progressModel = new StageProgressModel(sources, 0.9f);
            var selection = new ToolSelectionModel();
            var memoryView = memoryObject.AddComponent<MemoryPanelView>();
            var progressText = wheelObject.AddComponent<Text>();
            var progressWheel = wheelObject.AddComponent<ProgressWheelView>();
            progressWheel.Configure(null, progressText);
            buttonObject.AddComponent<CanvasRenderer>();
            buttonObject.AddComponent<Image>();
            var cottonSwabButton = buttonObject.AddComponent<Button>();
            var toolSelector = selectorObject.AddComponent<ToolSelectorView>();
            var cleaningAudio = audioObject.AddComponent<CleaningAudioController>();
            toolSelector.Configure(
                selection,
                sources,
                new[]
                {
                    new ToolSelectorView.ToolButtonBinding
                    {
                        Tool = CleaningTool.CottonSwab,
                        Button = cottonSwabButton,
                        Root = buttonObject.GetComponent<RectTransform>()
                    }
                },
                cleaningAudio);
            var controller = stageObject.AddComponent<StageController>();
            var fixture = new StageFixture(
                controller,
                progressModel,
                sources,
                selection,
                progressWheel,
                progressText,
                toolSelector,
                cottonSwabButton,
                cleaningAudio);
            memoryView.Opened += fixture.RecordMemoryOpen;
            controller.Configure(
                selection,
                progressModel,
                sources,
                memoryView,
                progressWheel,
                toolSelector,
                cleaningAudio);
            controller.Initialize();
            return fixture;
        }

        private static int AudibleLoopCount(CleaningAudioController audio)
        {
            return audio.GetComponentsInChildren<AudioSource>()
                .Count(source => source.isPlaying && source.loop && source.volume > 0f);
        }

        private sealed class StageFixture
        {
            private readonly StageProgressModel progressModel;
            private readonly MutableProgressSource[] sources;

            public StageFixture(
                StageController controller,
                StageProgressModel progressModel,
                MutableProgressSource[] sources,
                ToolSelectionModel selection,
                ProgressWheelView progressWheel,
                Text progressText,
                ToolSelectorView toolSelector,
                Button cottonSwabButton,
                CleaningAudioController audio)
            {
                Controller = controller;
                this.progressModel = progressModel;
                this.sources = sources;
                Selection = selection;
                ProgressWheel = progressWheel;
                ProgressText = progressText;
                ToolSelector = toolSelector;
                CottonSwabButton = cottonSwabButton;
                Audio = audio;
            }

            public StageController Controller { get; }
            public ToolSelectionModel Selection { get; }
            public ProgressWheelView ProgressWheel { get; }
            public Text ProgressText { get; }
            public ToolSelectorView ToolSelector { get; }
            public Button CottonSwabButton { get; }
            public CleaningAudioController Audio { get; }
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
