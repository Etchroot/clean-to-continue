using System;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using UnityEngine;

namespace CleanToContinue.Surface
{
    public sealed class SurfaceMaskLayer : MonoBehaviour, IProgressSource
    {
        private static readonly int HighlightPulseId = Shader.PropertyToID("_HighlightPulse");

        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private CleaningTool tool = CleaningTool.AirGun;
        [SerializeField] private string maskProperty = "_DustMask";
        [SerializeField, Min(1)] private int coverageResolution = 64;
        [SerializeField, Min(1)] private int visualResolution = 512;

        private MaterialPropertyBlock propertyBlock;
        private RuntimeMaskPainter painter;
        private CoverageGrid coverage;
        private int maskPropertyId;

        public CleaningTool Tool => tool;
        public float Progress01 => coverage?.Progress01 ?? 0f;
        public RenderTexture CurrentMask => painter?.CurrentMask;

        public event Action ProgressChanged;

        public void Configure(
            Renderer renderer,
            CleaningTool requiredTool,
            string shaderMaskProperty,
            int progressGridResolution = 64,
            int textureResolution = 512)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (string.IsNullOrWhiteSpace(shaderMaskProperty))
            {
                throw new ArgumentException("A shader mask property is required.", nameof(shaderMaskProperty));
            }

            targetRenderer = renderer;
            tool = requiredTool;
            maskProperty = shaderMaskProperty;
            coverageResolution = Mathf.Max(1, progressGridResolution);
            visualResolution = Mathf.Max(1, textureResolution);
            InitializeRuntimeState();
        }

        public bool TryClean(CleaningTool selectedTool, RaycastHit hit, float normalizedRadius)
        {
            if (selectedTool != tool || painter == null || coverage == null)
            {
                return false;
            }

            var newlyCleaned = coverage.ApplyDisc(hit.textureCoord, normalizedRadius);
            if (newlyCleaned == 0)
            {
                return false;
            }

            painter.Stamp(hit.textureCoord, normalizedRadius, 0f);
            ApplyMaskToRenderer();
            ProgressChanged?.Invoke();
            return true;
        }

        public void ForceFinish()
        {
            if (painter == null || coverage == null || Progress01 >= 1f)
            {
                return;
            }

            coverage.ApplyDisc(new Vector2(0.5f, 0.5f), 1f);
            painter.Stamp(new Vector2(0.5f, 0.5f), 1f, 0f);
            ApplyMaskToRenderer();
            ProgressChanged?.Invoke();
        }

        public void SetHighlight(float intensity)
        {
            if (targetRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(HighlightPulseId, Mathf.Clamp01(intensity));
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        private void Awake()
        {
            if (targetRenderer != null && !string.IsNullOrWhiteSpace(maskProperty))
            {
                InitializeRuntimeState();
            }
        }

        private void OnDestroy()
        {
            painter?.Dispose();
            painter = null;
        }

        private void InitializeRuntimeState()
        {
            painter?.Dispose();
            painter = new RuntimeMaskPainter();
            painter.Initialize(visualResolution, Color.white);
            coverage = CoverageGrid.CreateFilled(coverageResolution, coverageResolution);
            maskPropertyId = Shader.PropertyToID(maskProperty);
            ApplyMaskToRenderer();
        }

        private void ApplyMaskToRenderer()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(maskPropertyId, painter.CurrentMask);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
