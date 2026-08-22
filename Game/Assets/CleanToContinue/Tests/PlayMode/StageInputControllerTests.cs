using System.Collections;
using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Highlight;
using CleanToContinue.Input;
using CleanToContinue.Surface;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class StageInputControllerTests
    {
        private static readonly int HighlightPulseId = Shader.PropertyToID("_HighlightPulse");

        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private readonly List<InputDevice> createdDevices = new List<InputDevice>();

        [UnityTest]
        public IEnumerator SpaceUsesConfiguredHighlightWhenInteractionHasNoHighlight()
        {
            var fixture = CreateFixture();
            var keyboard = CreateKeyboard();

            yield return PressAndRelease(keyboard, Key.Space);
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.That(ReadHighlight(fixture.Surface), Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator SpaceUsesInteractionHighlightWhenNoDirectHighlightIsConfigured()
        {
            var fixture = CreateFixture(false, true);
            var keyboard = CreateKeyboard();

            yield return PressAndRelease(keyboard, Key.Space);
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.That(ReadHighlight(fixture.Surface), Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator NumberKeysSelectToolsThroughEnabledInputActions()
        {
            var fixture = CreateFixture();
            var keyboard = CreateKeyboard();
            var selections = new List<CleaningTool>();
            fixture.Input.NumericToolSelected += selections.Add;

            yield return PressAndRelease(keyboard, Key.Digit3);
            yield return PressAndRelease(keyboard, Key.Digit2);
            yield return PressAndRelease(keyboard, Key.Digit1);

            Assert.That(fixture.Selection.Selected, Is.EqualTo(CleaningTool.AirGun));
            Assert.That(
                selections,
                Is.EqualTo(new[]
                {
                    CleaningTool.Cloth,
                    CleaningTool.AirGun
                }));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var device in createdDevices)
            {
                if (device != null && device.added)
                {
                    InputSystem.RemoveDevice(device);
                }
            }

            createdDevices.Clear();
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

        private StageInputFixture CreateFixture(
            bool provideDirectHighlight = true,
            bool configureInteractionHighlight = false)
        {
            var root = new GameObject("Stage Input Test");
            createdObjects.Add(root);
            var interaction = root.AddComponent<StageInteractionController>();
            var input = root.AddComponent<StageInputController>();
            var selection = new ToolSelectionModel();

            var surfaceObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            surfaceObject.name = "Highlight Surface";
            createdObjects.Add(surfaceObject);
            var surface = surfaceObject.AddComponent<SurfaceMaskLayer>();
            surface.Configure(
                surfaceObject.GetComponent<Renderer>(),
                CleaningTool.AirGun,
                "_DustMask",
                16,
                32);

            var highlightObject = new GameObject("Configured Highlight");
            createdObjects.Add(highlightObject);
            var highlight = highlightObject.AddComponent<HighlightController>();
            highlight.Configure(new[] { surface }, new CleanToContinue.Gap.GapDirtSpot[0]);
            if (configureInteractionHighlight)
            {
                interaction.Configure(
                    null,
                    selection,
                    null,
                    new SurfaceMaskLayer[0],
                    null,
                    0.1f,
                    0.5f,
                    highlight);
            }

            input.Configure(interaction, provideDirectHighlight ? highlight : null, selection);

            return new StageInputFixture
            {
                Input = input,
                Selection = selection,
                Surface = surface
            };
        }

        private Keyboard CreateKeyboard()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            createdDevices.Add(keyboard);
            return keyboard;
        }

        private static IEnumerator PressAndRelease(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
            yield return null;

            InputSystem.QueueStateEvent(keyboard, default(KeyboardState));
            InputSystem.Update();
            yield return null;
        }

        private static float ReadHighlight(SurfaceMaskLayer surface)
        {
            var block = new MaterialPropertyBlock();
            surface.GetComponent<Renderer>().GetPropertyBlock(block);
            return block.GetFloat(HighlightPulseId);
        }

        private sealed class StageInputFixture
        {
            public StageInputController Input;
            public ToolSelectionModel Selection;
            public SurfaceMaskLayer Surface;
        }
    }
}
