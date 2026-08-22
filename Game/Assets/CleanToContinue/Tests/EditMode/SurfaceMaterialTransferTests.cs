using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEngine;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class SurfaceMaterialTransferTests
    {
        private Material source;
        private Material target;
        private Texture2D baseMap;
        private Texture2D normalMap;
        private Texture2D metallicMap;

        [Test]
        public void CopiesOriginalUrpSurfaceIntoCleanableMaterial()
        {
            var sourceShader = Shader.Find("Universal Render Pipeline/Lit");
            var targetShader = Shader.Find("CleanToContinue/Cleanable Surface");
            Assert.That(sourceShader, Is.Not.Null);
            Assert.That(targetShader, Is.Not.Null);

            source = new Material(sourceShader);
            target = new Material(targetShader);
            baseMap = new Texture2D(2, 2);
            normalMap = new Texture2D(2, 2);
            metallicMap = new Texture2D(2, 2);
            var color = new Color(0.31f, 0.42f, 0.53f, 1f);
            var scale = new Vector2(1.7f, 0.8f);
            var offset = new Vector2(0.13f, 0.27f);

            source.SetTexture("_BaseMap", baseMap);
            source.SetColor("_BaseColor", color);
            source.SetTextureScale("_BaseMap", scale);
            source.SetTextureOffset("_BaseMap", offset);
            source.SetTexture("_BumpMap", normalMap);
            source.SetFloat("_BumpScale", 0.65f);
            source.SetTexture("_MetallicGlossMap", metallicMap);
            source.SetFloat("_Metallic", 0.37f);
            source.SetFloat("_Smoothness", 0.71f);

            SurfaceMaterialTransfer.CopyToCleanable(source, target);

            Assert.That(target.GetTexture("_BaseMap"), Is.SameAs(baseMap));
            Assert.That(Vector4.Distance(target.GetColor("_BaseColor"), color), Is.LessThan(0.0001f));
            Assert.That(target.GetTextureScale("_BaseMap"), Is.EqualTo(scale));
            Assert.That(target.GetTextureOffset("_BaseMap"), Is.EqualTo(offset));
            Assert.That(target.GetTexture("_BumpMap"), Is.SameAs(normalMap));
            Assert.That(target.GetFloat("_BumpScale"), Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(target.GetTexture("_MetallicGlossMap"), Is.SameAs(metallicMap));
            Assert.That(target.GetFloat("_Metallic"), Is.EqualTo(0.37f).Within(0.001f));
            Assert.That(target.GetFloat("_CleanSmoothness"), Is.EqualTo(0.71f).Within(0.001f));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(baseMap);
            Object.DestroyImmediate(normalMap);
            Object.DestroyImmediate(metallicMap);
        }
    }
}
