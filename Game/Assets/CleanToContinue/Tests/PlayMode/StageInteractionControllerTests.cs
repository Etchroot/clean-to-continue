using System.Collections;
using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Gap;
using CleanToContinue.Input;
using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class StageInteractionControllerTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [UnityTest]
        public IEnumerator AirGunRoutesOnlyToDust()
        {
            var fixture = CreateSurfaceFixture();
            fixture.Selection.Select(CleaningTool.AirGun);

            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, true, false, false);
            yield return null;

            Assert.That(fixture.Dust.Progress01, Is.GreaterThan(0f));
            Assert.That(fixture.Polish.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Gap.Progress01, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator ClothRoutesOnlyToPolish()
        {
            var fixture = CreateSurfaceFixture();
            fixture.Selection.Select(CleaningTool.Cloth);

            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, true, false, false);
            yield return null;

            Assert.That(fixture.Dust.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Polish.Progress01, Is.GreaterThan(0f));
            Assert.That(fixture.Gap.Progress01, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator CottonSwabRoutesOnlyToGapDirt()
        {
            var fixture = CreateGapFixture();
            fixture.Selection.Select(CleaningTool.CottonSwab);

            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, true, false, false);
            yield return null;

            Assert.That(fixture.Gap.Progress01, Is.GreaterThan(0f));
            Assert.That(fixture.Dust.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Polish.Progress01, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator RightDragRotatesWithoutCleaning()
        {
            var fixture = CreateSurfaceFixture();
            var pitchBefore = fixture.Rotator.Pitch;

            fixture.Interaction.ProcessFrame(fixture.CenterPointer, new Vector2(0f, 10f), true, true, false);
            yield return null;

            Assert.That(fixture.Rotator.Pitch, Is.Not.EqualTo(pitchBefore));
            Assert.That(fixture.Dust.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Polish.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Gap.Progress01, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator PressBeginningOverUiStaysBlockedUntilRelease()
        {
            var fixture = CreateSurfaceFixture();

            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, true, false, true);
            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, true, false, false);
            yield return null;

            Assert.That(fixture.Dust.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Polish.Progress01, Is.EqualTo(0f));
            Assert.That(fixture.Gap.Progress01, Is.EqualTo(0f));

            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, false, false, false);
            fixture.Interaction.ProcessFrame(fixture.CenterPointer, Vector2.zero, true, false, false);
            yield return null;

            Assert.That(fixture.Dust.Progress01, Is.GreaterThan(0f));
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

        private InteractionFixture CreateSurfaceFixture()
        {
            var fixture = CreateBaseFixture();
            var surface = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            surface.name = "Cleanable Surface";
            surface.layer = 8;
            createdObjects.Add(surface);

            fixture.Dust = surface.AddComponent<SurfaceMaskLayer>();
            fixture.Dust.Configure(surface.GetComponent<Renderer>(), CleaningTool.AirGun, "_DustMask", 16, 32);
            fixture.Polish = surface.AddComponent<SurfaceMaskLayer>();
            fixture.Polish.Configure(surface.GetComponent<Renderer>(), CleaningTool.Cloth, "_PolishRemainingMask", 16, 32);
            CreateGapDirt(fixture, new Vector3(5f, 0f, 0f));
            fixture.Interaction.Configure(
                fixture.Camera,
                fixture.Selection,
                fixture.Rotator,
                new[] { fixture.Dust, fixture.Polish },
                fixture.Gap,
                0.1f,
                0.5f);
            Physics.SyncTransforms();
            return fixture;
        }

        private InteractionFixture CreateGapFixture()
        {
            var fixture = CreateBaseFixture();
            CreateGapDirt(fixture, Vector3.zero);

            var surface = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            surface.name = "Other Cleanable Surface";
            surface.layer = 8;
            surface.transform.position = new Vector3(5f, 0f, 0f);
            createdObjects.Add(surface);
            fixture.Dust = surface.AddComponent<SurfaceMaskLayer>();
            fixture.Dust.Configure(surface.GetComponent<Renderer>(), CleaningTool.AirGun, "_DustMask", 16, 32);
            fixture.Polish = surface.AddComponent<SurfaceMaskLayer>();
            fixture.Polish.Configure(surface.GetComponent<Renderer>(), CleaningTool.Cloth, "_PolishRemainingMask", 16, 32);
            fixture.Interaction.Configure(
                fixture.Camera,
                fixture.Selection,
                fixture.Rotator,
                new[] { fixture.Dust, fixture.Polish },
                fixture.Gap,
                0.1f,
                0.5f);
            Physics.SyncTransforms();
            return fixture;
        }

        private void CreateGapDirt(InteractionFixture fixture, Vector3 position)
        {
            var dirt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dirt.name = "Cleanable Gap Dirt";
            dirt.layer = 8;
            dirt.transform.position = position;
            createdObjects.Add(dirt);
            var spot = dirt.AddComponent<GapDirtSpot>();
            spot.Configure(dirt.GetComponent<Collider>(), dirt.transform, dirt.GetComponent<Renderer>());
            fixture.Gap.Configure(new[] { spot });
        }

        private InteractionFixture CreateBaseFixture()
        {
            var root = new GameObject("Stage Interaction Test");
            createdObjects.Add(root);

            var cameraObject = new GameObject("Interaction Camera");
            cameraObject.transform.SetParent(root.transform);
            cameraObject.transform.position = new Vector3(0f, 0f, -3f);
            cameraObject.transform.rotation = Quaternion.identity;
            var camera = cameraObject.AddComponent<Camera>();

            var rotatorObject = new GameObject("Equipment Rotator");
            rotatorObject.transform.SetParent(root.transform);
            var rotator = rotatorObject.AddComponent<EquipmentRotator>();
            rotator.Configure(-35f, 55f, 1f);

            var gap = root.AddComponent<GapDirtGroup>();
            var interaction = root.AddComponent<StageInteractionController>();
            return new InteractionFixture
            {
                Camera = camera,
                Selection = new ToolSelectionModel(),
                Rotator = rotator,
                Gap = gap,
                Interaction = interaction
            };
        }

        private sealed class InteractionFixture
        {
            public Camera Camera;
            public ToolSelectionModel Selection;
            public EquipmentRotator Rotator;
            public SurfaceMaskLayer Dust;
            public SurfaceMaskLayer Polish;
            public GapDirtGroup Gap;
            public StageInteractionController Interaction;

            public Vector2 CenterPointer => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
    }
}
