using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class RuntimeMaskPainterTests
    {
        [Test]
        public void InitializeCreatesAndDisposeReleasesCurrentMask()
        {
            var masksBefore = CountRuntimeMasks();
            var painter = new RuntimeMaskPainter();

            try
            {
                painter.Initialize(32, Color.white);

                Assert.That(painter.CurrentMask, Is.Not.Null);
                Assert.That(painter.CurrentMask.IsCreated(), Is.True);
                Assert.That(CountRuntimeMasks(), Is.EqualTo(masksBefore + 2));
            }
            finally
            {
                painter.Dispose();
            }

            Assert.That(painter.CurrentMask, Is.Null);
            Assert.That(CountRuntimeMasks(), Is.EqualTo(masksBefore));
        }

        [Test]
        public void MaskStampShaderIsAlwaysIncludedInPlayerBuilds()
        {
            var shader = Shader.Find("Hidden/CleanToContinue/MaskStamp");
            Assert.That(shader, Is.Not.Null);

            var graphicsSettingsAssets = AssetDatabase.LoadAllAssetsAtPath(
                "ProjectSettings/GraphicsSettings.asset");
            Assert.That(graphicsSettingsAssets, Is.Not.Empty);

            var settings = new SerializedObject(graphicsSettingsAssets[0]);
            var includedShaders = settings.FindProperty("m_AlwaysIncludedShaders");
            Assert.That(includedShaders, Is.Not.Null);

            var isIncluded = false;
            for (var index = 0; index < includedShaders.arraySize; index++)
            {
                if (includedShaders.GetArrayElementAtIndex(index).objectReferenceValue == shader)
                {
                    isIncluded = true;
                    break;
                }
            }

            Assert.That(isIncluded, Is.True,
                "The runtime-only mask stamp shader must not be stripped from Web builds.");
        }

        [Test]
        public void StampChangesCenterButPreservesCorner()
        {
            var painter = new RuntimeMaskPainter();
            painter.Initialize(32, Color.white);
            Texture2D snapshot = null;

            try
            {
                painter.Stamp(new Vector2(0.5f, 0.5f), 0.2f, 0f);
                snapshot = ReadBack(painter.CurrentMask);

                Assert.That(snapshot.GetPixel(16, 16).r, Is.LessThan(0.1f));
                Assert.That(snapshot.GetPixel(0, 0).r, Is.GreaterThan(0.9f));
            }
            finally
            {
                painter.Dispose();
                if (snapshot != null)
                {
                    Object.DestroyImmediate(snapshot);
                }
            }
        }

        private static Texture2D ReadBack(RenderTexture source)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = source;
            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            return texture;
        }

        private static int CountRuntimeMasks()
        {
            var count = 0;
            foreach (var texture in Resources.FindObjectsOfTypeAll<RenderTexture>())
            {
                if (texture != null && texture.name.StartsWith("Clean Mask "))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
