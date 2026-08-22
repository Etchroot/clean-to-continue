using System.Collections;
using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class SurfaceMaskLayerTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [UnityTest]
        public IEnumerator WrongToolDoesNotChangeDustProgress()
        {
            var layer = CreateTestSurface(CleaningTool.AirGun);
            var hit = CreateCenterHit(layer.gameObject);
            var before = layer.Progress01;

            layer.TryClean(CleaningTool.Cloth, hit, 0.1f);
            yield return null;

            Assert.That(layer.Progress01, Is.EqualTo(before));
        }

        [UnityTest]
        public IEnumerator RepeatingSameCorrectStrokeDoesNotDoubleCount()
        {
            var layer = CreateTestSurface(CleaningTool.AirGun);
            var hit = CreateCenterHit(layer.gameObject);

            layer.TryClean(CleaningTool.AirGun, hit, 0.1f);
            yield return null;
            var once = layer.Progress01;
            layer.TryClean(CleaningTool.AirGun, hit, 0.1f);
            yield return null;

            Assert.That(once, Is.GreaterThan(0f));
            Assert.That(layer.Progress01, Is.EqualTo(once));
        }

        [UnityTest]
        public IEnumerator StrokeStillPaintsVisualMaskWhenCoverageCellWasAlreadyCounted()
        {
            var layer = CreateTestQuad(CleaningTool.AirGun, 1, 64);
            var firstHit = CreateQuadHit(layer.gameObject, -0.25f);
            var secondHit = CreateQuadHit(layer.gameObject, 0.25f);

            Assert.That(layer.TryClean(CleaningTool.AirGun, firstHit, 0.3f), Is.True);
            yield return null;
            var beforeSecondStroke = ReadMaskRed(layer.CurrentMask, secondHit.textureCoord);

            Assert.That(layer.TryClean(CleaningTool.AirGun, secondHit, 0.3f), Is.True);
            yield return null;

            Assert.That(layer.Progress01, Is.EqualTo(1f));
            Assert.That(beforeSecondStroke, Is.GreaterThan(0.9f));
            Assert.That(ReadMaskRed(layer.CurrentMask, secondHit.textureCoord), Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator ForceFinishSetsProgressToOne()
        {
            var layer = CreateTestSurface(CleaningTool.Cloth);

            layer.ForceFinish();
            yield return null;

            Assert.That(layer.Progress01, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator DustAndPolishLayersPreserveEachOthersRendererProperties()
        {
            var dustLayer = CreateTestSurface(CleaningTool.AirGun);
            var renderer = dustLayer.GetComponent<Renderer>();
            var preservedValueId = Shader.PropertyToID("_TestPreservedValue");
            var dustMaskId = Shader.PropertyToID("_DustMask");
            var polishMaskId = Shader.PropertyToID("_PolishRemainingMask");
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat(preservedValueId, 0.42f);
            renderer.SetPropertyBlock(block);

            var polishLayer = dustLayer.gameObject.AddComponent<SurfaceMaskLayer>();
            polishLayer.Configure(
                renderer,
                CleaningTool.Cloth,
                "_PolishRemainingMask",
                32,
                64);
            var hit = CreateCenterHit(dustLayer.gameObject);

            Assert.That(dustLayer.TryClean(CleaningTool.AirGun, hit, 0.1f), Is.True);
            Assert.That(polishLayer.TryClean(CleaningTool.Cloth, hit, 0.1f), Is.True);
            yield return null;

            renderer.GetPropertyBlock(block);
            Assert.That(block.GetTexture(dustMaskId), Is.SameAs(dustLayer.CurrentMask));
            Assert.That(block.GetTexture(polishMaskId), Is.SameAs(polishLayer.CurrentMask));
            Assert.That(block.GetFloat(preservedValueId), Is.EqualTo(0.42f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator MeshWithoutUsableUvCleansOnlyNearbyTriangles()
        {
            var source = new GameObject("NoUvSource");
            createdObjects.Add(source);
            var sourceFilter = source.AddComponent<MeshFilter>();
            var sourceRenderer = source.AddComponent<MeshRenderer>();
            var mesh = CreateTwoIslandMeshWithoutUv();
            sourceFilter.sharedMesh = mesh;
            var collider = source.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            var overlay = new GameObject("NoUvOverlay");
            overlay.transform.SetParent(source.transform, false);
            var overlayFilter = overlay.AddComponent<MeshFilter>();
            overlayFilter.sharedMesh = mesh;
            var overlayRenderer = overlay.AddComponent<MeshRenderer>();
            var layer = source.AddComponent<SurfaceMaskLayer>();
            layer.Configure(overlayRenderer, CleaningTool.AirGun, "_DustMask", 16, 32);
            var originalIndexCount = overlayFilter.sharedMesh.GetIndices(0).Length;

            Physics.SyncTransforms();
            var ray = new Ray(new Vector3(-1.5f, 0f, -2f), Vector3.forward);
            Assert.That(Physics.Raycast(ray, out var hit, 4f), Is.True);
            Assert.That(layer.TryClean(CleaningTool.AirGun, hit, 0.1f), Is.True);
            yield return null;

            var remainingIndexCount = overlayFilter.sharedMesh.GetIndices(0).Length;
            Assert.That(remainingIndexCount, Is.GreaterThan(0));
            Assert.That(remainingIndexCount, Is.LessThan(originalIndexCount));
            Assert.That(layer.Progress01, Is.GreaterThan(0f).And.LessThan(1f));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.Destroy(createdObject);
                }
            }

            createdObjects.Clear();
            yield return null;
        }

        private SurfaceMaskLayer CreateTestSurface(CleaningTool tool)
        {
            var surface = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            createdObjects.Add(surface);

            var primitiveCollider = surface.GetComponent<Collider>();
            Object.DestroyImmediate(primitiveCollider);
            var meshCollider = surface.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = surface.GetComponent<MeshFilter>().sharedMesh;

            var layer = surface.AddComponent<SurfaceMaskLayer>();
            layer.Configure(
                surface.GetComponent<Renderer>(),
                tool,
                tool == CleaningTool.AirGun ? "_DustMask" : "_PolishRemainingMask",
                32,
                64);
            return layer;
        }

        private SurfaceMaskLayer CreateTestQuad(CleaningTool tool, int coverageResolution, int visualResolution)
        {
            var surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            createdObjects.Add(surface);

            var primitiveCollider = surface.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            var meshCollider = surface.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = surface.GetComponent<MeshFilter>().sharedMesh;
            var layer = surface.AddComponent<SurfaceMaskLayer>();
            layer.Configure(
                surface.GetComponent<Renderer>(),
                tool,
                tool == CleaningTool.AirGun ? "_DustMask" : "_PolishRemainingMask",
                coverageResolution,
                visualResolution);
            return layer;
        }

        private static RaycastHit CreateQuadHit(GameObject surface, float localX)
        {
            Physics.SyncTransforms();
            var worldPoint = surface.transform.TransformPoint(new Vector3(localX, 0f, 0f));
            var ray = new Ray(worldPoint + Vector3.back, Vector3.forward);
            Assert.That(Physics.Raycast(ray, out var hit, 2f), Is.True);
            Assert.That(hit.collider.gameObject, Is.EqualTo(surface));
            return hit;
        }

        private static float ReadMaskRed(RenderTexture texture, Vector2 uv)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            var sample = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            var x = Mathf.Clamp(Mathf.RoundToInt(uv.x * (texture.width - 1)), 0, texture.width - 1);
            var y = Mathf.Clamp(Mathf.RoundToInt(uv.y * (texture.height - 1)), 0, texture.height - 1);
            sample.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
            sample.Apply();
            var red = sample.GetPixel(0, 0).r;
            Object.DestroyImmediate(sample);
            RenderTexture.active = previous;
            return red;
        }

        private static RaycastHit CreateCenterHit(GameObject surface)
        {
            Physics.SyncTransforms();
            var ray = new Ray(surface.transform.position + Vector3.back * 3f, Vector3.forward);
            Assert.That(Physics.Raycast(ray, out var hit, 10f), Is.True);
            Assert.That(hit.collider.gameObject, Is.EqualTo(surface));
            return hit;
        }

        private static Mesh CreateTwoIslandMeshWithoutUv()
        {
            var mesh = new Mesh { name = "Two islands without UV" };
            mesh.vertices = new[]
            {
                new Vector3(-2f, -0.5f, 0f),
                new Vector3(-1f, -0.5f, 0f),
                new Vector3(-2f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 0f),
                new Vector3(1f, -0.5f, 0f),
                new Vector3(2f, -0.5f, 0f),
                new Vector3(1f, 0.5f, 0f),
                new Vector3(2f, 0.5f, 0f)
            };
            mesh.triangles = new[]
            {
                0, 2, 1,
                2, 3, 1,
                4, 6, 5,
                6, 7, 5
            };
            mesh.uv = new Vector2[mesh.vertexCount];
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
