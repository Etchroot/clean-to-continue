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

        private static RaycastHit CreateCenterHit(GameObject surface)
        {
            Physics.SyncTransforms();
            var ray = new Ray(surface.transform.position + Vector3.back * 3f, Vector3.forward);
            Assert.That(Physics.Raycast(ray, out var hit, 10f), Is.True);
            Assert.That(hit.collider.gameObject, Is.EqualTo(surface));
            return hit;
        }
    }
}
