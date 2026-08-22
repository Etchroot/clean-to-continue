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
        private TriangleSurfaceCleaner triangleCleaner;
        private int maskPropertyId;

        public CleaningTool Tool => tool;
        public float Progress01 => triangleCleaner?.Progress01 ?? coverage?.Progress01 ?? 0f;
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
            if (selectedTool != tool)
            {
                return false;
            }

            if (triangleCleaner != null)
            {
                var newlyCleanedTriangles = triangleCleaner.Clean(hit.point, normalizedRadius);
                if (newlyCleanedTriangles > 0)
                {
                    ProgressChanged?.Invoke();
                }

                return true;
            }

            if (painter == null || coverage == null)
            {
                return false;
            }

            var newlyCleaned = coverage.ApplyDisc(hit.textureCoord, normalizedRadius);
            painter.Stamp(hit.textureCoord, normalizedRadius, 0f);
            ApplyMaskToRenderer();
            if (newlyCleaned > 0)
            {
                ProgressChanged?.Invoke();
            }

            return true;
        }

        public void ForceFinish()
        {
            if (Progress01 >= 1f)
            {
                return;
            }

            if (triangleCleaner != null)
            {
                triangleCleaner.ForceFinish();
                ProgressChanged?.Invoke();
                return;
            }

            if (painter == null || coverage == null)
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
            triangleCleaner?.Dispose();
            triangleCleaner = null;
        }

        private void InitializeRuntimeState()
        {
            painter?.Dispose();
            painter = null;
            triangleCleaner?.Dispose();
            triangleCleaner = null;
            coverage = null;

            var meshFilter = targetRenderer.GetComponent<MeshFilter>();
            var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (!HasUsableUv(mesh))
            {
                triangleCleaner = new TriangleSurfaceCleaner(meshFilter);
                return;
            }

            painter = new RuntimeMaskPainter();
            painter.Initialize(visualResolution, Color.white);
            coverage = CoverageGrid.CreateFromUvTriangles(
                coverageResolution,
                coverageResolution,
                mesh.uv,
                mesh.triangles);
            maskPropertyId = Shader.PropertyToID(maskProperty);
            ApplyMaskToRenderer();
        }

        private static bool HasUsableUv(Mesh mesh)
        {
            if (mesh == null || !mesh.isReadable || mesh.uv == null || mesh.uv.Length != mesh.vertexCount)
            {
                return false;
            }

            var uv = mesh.uv;
            if (uv.Length == 0)
            {
                return false;
            }

            var first = uv[0];
            for (var index = 1; index < uv.Length; index++)
            {
                if ((uv[index] - first).sqrMagnitude > 0.00000001f)
                {
                    return true;
                }
            }

            return false;
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
