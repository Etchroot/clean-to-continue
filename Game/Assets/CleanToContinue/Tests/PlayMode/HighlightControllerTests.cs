using System.Collections;
using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Gap;
using CleanToContinue.Highlight;
using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class HighlightControllerTests
    {
        private static readonly int HighlightPulseId = Shader.PropertyToID("_HighlightPulse");

        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private float originalTimeScale;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator PulseHighlightsOnlyUnfinishedAreasAndEndsUsingUnscaledTime()
        {
            var surface = CreateSurface();
            var unfinishedSpot = CreateGapSpot("Unfinished Gap");
            var finishedSpot = CreateGapSpot("Finished Gap");
            finishedSpot.Apply(1f);
            var controllerObject = new GameObject("Highlight Controller");
            createdObjects.Add(controllerObject);
            var controller = controllerObject.AddComponent<HighlightController>();
            controller.Configure(
                new[] { surface },
                new[] { unfinishedSpot, finishedSpot });
            var surfaceProgressBefore = surface.Progress01;

            controller.Pulse();
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.That(ReadSurfaceHighlight(surface), Is.GreaterThan(0f));
            Assert.That(unfinishedSpot.Highlight01, Is.GreaterThan(0f));
            Assert.That(finishedSpot.Highlight01, Is.EqualTo(0f));
            Assert.That(surface.Progress01, Is.EqualTo(surfaceProgressBefore));

            yield return new WaitForSecondsRealtime(1.1f);

            Assert.That(ReadSurfaceHighlight(surface), Is.EqualTo(0f).Within(0.001f));
            Assert.That(unfinishedSpot.Highlight01, Is.EqualTo(0f).Within(0.001f));
            Assert.That(finishedSpot.Highlight01, Is.EqualTo(0f));
            Assert.That(surface.Progress01, Is.EqualTo(surfaceProgressBefore));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = originalTimeScale;
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

        private SurfaceMaskLayer CreateSurface()
        {
            var surfaceObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            createdObjects.Add(surfaceObject);
            var layer = surfaceObject.AddComponent<SurfaceMaskLayer>();
            layer.Configure(
                surfaceObject.GetComponent<Renderer>(),
                CleaningTool.AirGun,
                "_DustMask",
                16,
                32);
            return layer;
        }

        private GapDirtSpot CreateGapSpot(string name)
        {
            var spotObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spotObject.name = name;
            createdObjects.Add(spotObject);
            var spot = spotObject.AddComponent<GapDirtSpot>();
            spot.Configure(
                spotObject.GetComponent<Collider>(),
                spotObject.transform,
                spotObject.GetComponent<Renderer>());
            return spot;
        }

        private static float ReadSurfaceHighlight(SurfaceMaskLayer surface)
        {
            var block = new MaterialPropertyBlock();
            surface.GetComponent<Renderer>().GetPropertyBlock(block);
            return block.GetFloat(HighlightPulseId);
        }
    }
}
