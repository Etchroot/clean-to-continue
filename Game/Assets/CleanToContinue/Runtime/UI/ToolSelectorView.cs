using System;
using System.Collections.Generic;
using CleanToContinue.Audio;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class ToolSelectorView : MonoBehaviour
    {
        [Serializable]
        public sealed class ToolButtonBinding
        {
            public CleaningTool Tool;
            public Button Button;
            public RectTransform Root;
            public Outline SelectionBorder;
            public Text AccessibleLabel;
            public Image ProgressFill;
            public GameObject CheckMark;
        }

        private static readonly Color SelectedBorderColor = new Color32(234, 217, 165, 255);

        [SerializeField] private ToolButtonBinding[] buttons = Array.Empty<ToolButtonBinding>();
        [SerializeField] private CleaningAudioController cleaningAudio;

        private readonly Dictionary<Button, UnityAction> buttonListeners = new Dictionary<Button, UnityAction>();
        private ToolSelectionModel selectionModel;
        private IProgressSource[] progressSources = Array.Empty<IProgressSource>();

        public void Configure(
            ToolSelectionModel selection,
            IProgressSource[] sources,
            ToolButtonBinding[] toolButtons = null,
            CleaningAudioController audio = null)
        {
            Unbind();
            selectionModel = selection ?? new ToolSelectionModel();
            progressSources = sources ?? Array.Empty<IProgressSource>();
            if (toolButtons != null)
            {
                buttons = toolButtons;
            }

            if (audio != null)
            {
                cleaningAudio = audio;
            }

            selectionModel.SelectionChanged += RenderSelection;
            foreach (var source in progressSources)
            {
                if (source != null)
                {
                    source.ProgressChanged += RenderAllProgress;
                }
            }

            BindButtons();
            RenderSelection(selectionModel.Selected);
            RenderAllProgress();
        }

        public void RenderSelection(CleaningTool selectedTool)
        {
            foreach (var binding in buttons)
            {
                if (binding == null)
                {
                    continue;
                }

                var selected = binding.Tool == selectedTool;
                if (binding.Root != null)
                {
                    binding.Root.localScale = Vector3.one * (selected ? 1.08f : 1f);
                }

                if (binding.SelectionBorder != null)
                {
                    binding.SelectionBorder.enabled = selected;
                    binding.SelectionBorder.effectColor = SelectedBorderColor;
                    binding.SelectionBorder.effectDistance = new Vector2(4f, -4f);
                }

                if (binding.AccessibleLabel != null)
                {
                    binding.AccessibleLabel.text = selected
                        ? $"{GetToolLabel(binding.Tool)} (선택됨)"
                        : GetToolLabel(binding.Tool);
                }
            }
        }

        public void RenderProgress(CleaningTool tool, float progress01)
        {
            var clamped = Mathf.Clamp01(progress01);
            foreach (var binding in buttons)
            {
                if (binding == null || binding.Tool != tool)
                {
                    continue;
                }

                if (binding.ProgressFill != null)
                {
                    binding.ProgressFill.type = Image.Type.Filled;
                    binding.ProgressFill.fillMethod = Image.FillMethod.Radial360;
                    binding.ProgressFill.fillAmount = clamped;
                    binding.ProgressFill.gameObject.SetActive(clamped < 1f);
                }

                if (binding.CheckMark != null)
                {
                    binding.CheckMark.SetActive(clamped >= 1f);
                }
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void BindButtons()
        {
            foreach (var binding in buttons)
            {
                if (binding?.Button == null)
                {
                    continue;
                }

                var tool = binding.Tool;
                UnityAction listener = () => SelectFromUi(tool);
                binding.Button.onClick.AddListener(listener);
                buttonListeners[binding.Button] = listener;
            }
        }

        private void SelectFromUi(CleaningTool tool)
        {
            cleaningAudio?.NotifyUserInteraction();
            cleaningAudio?.StopContinuousToolAudio();
            selectionModel?.Select(tool);
        }

        private void RenderAllProgress()
        {
            foreach (var source in progressSources)
            {
                if (source != null)
                {
                    RenderProgress(source.Tool, source.Progress01);
                }
            }
        }

        private void Unbind()
        {
            if (selectionModel != null)
            {
                selectionModel.SelectionChanged -= RenderSelection;
            }

            foreach (var source in progressSources)
            {
                if (source != null)
                {
                    source.ProgressChanged -= RenderAllProgress;
                }
            }

            foreach (var pair in buttonListeners)
            {
                if (pair.Key != null)
                {
                    pair.Key.onClick.RemoveListener(pair.Value);
                }
            }

            buttonListeners.Clear();
        }

        private static string GetToolLabel(CleaningTool tool)
        {
            switch (tool)
            {
                case CleaningTool.AirGun:
                    return "에어건";
                case CleaningTool.CottonSwab:
                    return "면봉";
                case CleaningTool.Cloth:
                    return "헝겊";
                default:
                    return tool.ToString();
            }
        }
    }
}
