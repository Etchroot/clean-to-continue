using System;
using System.Collections;
using System.Collections.Generic;
using CleanToContinue.Audio;
using CleanToContinue.Core;
using CleanToContinue.Gap;
using CleanToContinue.Input;
using CleanToContinue.Progress;
using CleanToContinue.Surface;
using CleanToContinue.UI;
using UnityEngine;

namespace CleanToContinue.Stage
{
    public sealed class StageController : MonoBehaviour
    {
        public const string MasterVolumeKey = "ctc.masterVolume";
        public const string MusicVolumeKey = "ctc.musicVolume";
        public const string SfxVolumeKey = "ctc.sfxVolume";
        public const string RotationSensitivityKey = "ctc.rotationSensitivity";

        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultMusicVolume = 0.7f;
        public const float DefaultSfxVolume = 1f;
        public const float DefaultRotationSensitivity = 1f;

        private const float CompletionThreshold = 0.9f;
        private const float CompletionWheelSeconds = 0.35f;

        [SerializeField] private SurfaceMaskLayer[] surfaceLayers = Array.Empty<SurfaceMaskLayer>();
        [SerializeField] private GapDirtGroup gapDirtGroup;
        [SerializeField] private StageInputController inputController;
        [SerializeField] private StageInteractionController interactionController;
        [SerializeField] private EquipmentRotator equipmentRotator;
        [SerializeField] private ProgressWheelView progressWheel;
        [SerializeField] private ToolSelectorView toolSelector;
        [SerializeField] private MemoryPanelView memoryPanel;
        [SerializeField] private CleaningAudioController cleaningAudio;
        [SerializeField] private float minimumPitch = -35f;
        [SerializeField] private float maximumPitch = 55f;

        private ToolSelectionModel selectionModel;
        private StageProgressModel progressModel;
        private IProgressSource[] progressSources = Array.Empty<IProgressSource>();
        private Coroutine completionWheelRoutine;
        private bool initialized;
        private bool cleaningHeld;
        private bool cleanInputWasHeld;
        private bool cleanPressStartedOverUi;

        public bool InputLocked { get; private set; }

        public void Configure(
            ToolSelectionModel selection,
            StageProgressModel progress,
            IProgressSource[] sources,
            MemoryPanelView memory,
            ProgressWheelView wheel = null,
            ToolSelectorView selector = null,
            CleaningAudioController audio = null)
        {
            Unsubscribe();
            selectionModel = selection;
            progressModel = progress;
            progressSources = sources ?? Array.Empty<IProgressSource>();
            memoryPanel = memory;
            progressWheel = wheel;
            toolSelector = selector;
            cleaningAudio = audio;
            initialized = false;
        }

        public void ConfigureScene(
            SurfaceMaskLayer[] surfaces,
            GapDirtGroup gaps,
            StageInputController input,
            StageInteractionController interaction,
            EquipmentRotator rotator,
            ProgressWheelView wheel,
            ToolSelectorView selector,
            MemoryPanelView memory,
            CleaningAudioController audio)
        {
            Unsubscribe();
            surfaceLayers = surfaces ?? Array.Empty<SurfaceMaskLayer>();
            gapDirtGroup = gaps;
            inputController = input;
            interactionController = interaction;
            equipmentRotator = rotator;
            progressWheel = wheel;
            toolSelector = selector;
            memoryPanel = memory;
            cleaningAudio = audio;
            selectionModel = null;
            progressModel = null;
            progressSources = Array.Empty<IProgressSource>();
            initialized = false;
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            selectionModel ??= inputController != null
                ? inputController.ToolSelection
                : new ToolSelectionModel();
            if (progressSources.Length == 0)
            {
                progressSources = CollectProgressSources();
            }

            progressModel ??= new StageProgressModel(progressSources, CompletionThreshold);
            progressModel.Completed += CompleteStage;
            selectionModel.SelectionChanged += HandleSelectionChanged;
            foreach (var source in progressSources)
            {
                if (source != null)
                {
                    source.ProgressChanged += HandleProgressChanged;
                }
            }

            if (inputController != null)
            {
                inputController.CleanContextChanged += UpdateCleaningAudio;
            }

            ApplySettings();
            progressWheel?.Render(progressModel.Progress01);
            toolSelector?.Configure(selectionModel, progressSources, null, cleaningAudio);
            progressModel.Refresh();
            RenderProgressViews();
        }

        public void CompleteStage()
        {
            if (InputLocked)
            {
                return;
            }

            InputLocked = true;
            cleaningHeld = false;
            if (inputController != null)
            {
                inputController.enabled = false;
            }

            if (interactionController != null)
            {
                interactionController.enabled = false;
            }

            cleaningAudio?.StopContinuousToolAudio();
            toolSelector?.SetInteractable(false);
            if (progressWheel != null)
            {
                var crossedProgress = progressModel != null
                    ? progressModel.Progress01
                    : progressWheel.DisplayedProgress01;
                progressWheel.Render(crossedProgress);
                completionWheelRoutine = StartCoroutine(AnimateWheelToComplete(crossedProgress));
            }

            ForceFinishAllLayers();
            cleaningAudio?.PlayCompletion();
            memoryPanel?.OpenMouseMemory();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandleProgressChanged()
        {
            progressModel.Refresh();
            if (!InputLocked)
            {
                RenderProgressViews();
            }
        }

        private void HandleSelectionChanged(CleaningTool selectedTool)
        {
            toolSelector?.RenderSelection(selectedTool);
            if (cleaningHeld && !InputLocked)
            {
                cleaningAudio?.BeginCleaning(selectedTool);
            }
        }

        public void UpdateCleaningAudio(bool held, bool pointerOverUi)
        {
            if (held && !cleanInputWasHeld)
            {
                cleanPressStartedOverUi = pointerOverUi;
            }
            else if (!held)
            {
                cleanPressStartedOverUi = false;
            }

            cleanInputWasHeld = held;
            cleaningHeld = held && !cleanPressStartedOverUi && !pointerOverUi && !InputLocked;
            if (cleaningHeld)
            {
                cleaningAudio?.BeginCleaning(selectionModel.Selected);
            }
            else
            {
                cleaningAudio?.StopContinuousToolAudio();
            }
        }

        private void ApplySettings()
        {
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
            cleaningAudio?.SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume));
            equipmentRotator?.Configure(
                minimumPitch,
                maximumPitch,
                PlayerPrefs.GetFloat(RotationSensitivityKey, DefaultRotationSensitivity));
        }

        private void RenderProgressViews()
        {
            progressWheel?.Render(progressModel.Progress01);
            toolSelector?.RenderAllProgress();
        }

        private IEnumerator AnimateWheelToComplete(float start)
        {
            var elapsed = 0f;
            yield return null;
            while (elapsed < CompletionWheelSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                progressWheel.Render(Mathf.Lerp(start, 1f, elapsed / CompletionWheelSeconds));
                yield return null;
            }

            progressWheel.Render(1f);
            completionWheelRoutine = null;
        }

        private void ForceFinishAllLayers()
        {
            var finishedObjects = new HashSet<UnityEngine.Object>();
            foreach (var source in progressSources)
            {
                if (source is SurfaceMaskLayer surface && finishedObjects.Add(surface))
                {
                    surface.ForceFinish();
                }
                else if (source is GapDirtGroup gaps && finishedObjects.Add(gaps))
                {
                    gaps.ForceFinish();
                }
            }

            foreach (var surface in surfaceLayers)
            {
                if (surface != null && finishedObjects.Add(surface))
                {
                    surface.ForceFinish();
                }
            }

            if (gapDirtGroup != null && finishedObjects.Add(gapDirtGroup))
            {
                gapDirtGroup.ForceFinish();
            }
        }

        private IProgressSource[] CollectProgressSources()
        {
            var sources = new List<IProgressSource>(surfaceLayers.Length + 1);
            foreach (var surface in surfaceLayers)
            {
                if (surface != null)
                {
                    sources.Add(surface);
                }
            }

            if (gapDirtGroup != null)
            {
                sources.Add(gapDirtGroup);
            }

            return sources.ToArray();
        }

        private void Unsubscribe()
        {
            if (progressModel != null)
            {
                progressModel.Completed -= CompleteStage;
            }

            if (selectionModel != null)
            {
                selectionModel.SelectionChanged -= HandleSelectionChanged;
            }

            foreach (var source in progressSources)
            {
                if (source != null)
                {
                    source.ProgressChanged -= HandleProgressChanged;
                }
            }

            if (inputController != null)
            {
                inputController.CleanContextChanged -= UpdateCleaningAudio;
            }

            if (completionWheelRoutine != null)
            {
                StopCoroutine(completionWheelRoutine);
                completionWheelRoutine = null;
            }
        }
    }
}
