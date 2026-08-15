using System;
using UnityEngine;

namespace CleanToContinue.Surface
{
    public sealed class RuntimeMaskPainter : IDisposable
    {
        private static readonly int BrushUvId = Shader.PropertyToID("_BrushUV");
        private static readonly int BrushRadiusId = Shader.PropertyToID("_BrushRadius");
        private static readonly int WriteValueId = Shader.PropertyToID("_WriteValue");

        private RenderTexture current;
        private RenderTexture scratch;
        private Material stampMaterial;

        public RenderTexture CurrentMask => current;

        public void Initialize(int resolution, Color initialColor)
        {
            if (resolution <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution));
            }

            Dispose();

            var shader = Shader.Find("Hidden/CleanToContinue/MaskStamp");
            if (shader == null)
            {
                throw new InvalidOperationException("Mask stamp shader was not found.");
            }

            stampMaterial = new Material(shader)
            {
                name = "Runtime Mask Stamp",
                hideFlags = HideFlags.HideAndDontSave
            };
            current = CreateMask(resolution, "Current");
            scratch = CreateMask(resolution, "Scratch");
            Clear(current, initialColor);
            Clear(scratch, initialColor);
        }

        public void Stamp(Vector2 uv, float normalizedRadius, float writeValue)
        {
            if (current == null || scratch == null || stampMaterial == null)
            {
                throw new InvalidOperationException("Initialize must be called before Stamp.");
            }

            if (normalizedRadius <= 0f)
            {
                return;
            }

            stampMaterial.SetVector(BrushUvId, new Vector4(
                Mathf.Clamp01(uv.x),
                Mathf.Clamp01(uv.y),
                0f,
                0f));
            stampMaterial.SetFloat(BrushRadiusId, normalizedRadius);
            stampMaterial.SetFloat(WriteValueId, Mathf.Clamp01(writeValue));
            Graphics.Blit(current, scratch, stampMaterial);
            (current, scratch) = (scratch, current);
        }

        public void Dispose()
        {
            ReleaseMask(ref current);
            ReleaseMask(ref scratch);

            if (stampMaterial != null)
            {
                DestroyObject(stampMaterial);
                stampMaterial = null;
            }
        }

        private static RenderTexture CreateMask(int resolution, string suffix)
        {
            var texture = new RenderTexture(
                resolution,
                resolution,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear)
            {
                name = $"Clean Mask {suffix}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.Create();
            return texture;
        }

        private static void Clear(RenderTexture texture, Color color)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, color);
            RenderTexture.active = previous;
        }

        private static void ReleaseMask(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (texture.IsCreated())
            {
                texture.Release();
            }

            DestroyObject(texture);
            texture = null;
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(value);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(value);
            }
        }
    }
}
