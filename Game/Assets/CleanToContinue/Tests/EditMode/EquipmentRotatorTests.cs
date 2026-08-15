using System.Collections.Generic;
using CleanToContinue.Input;
using NUnit.Framework;
using UnityEngine;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class EquipmentRotatorTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [Test]
        public void PitchStaysInsideConfiguredBounds()
        {
            var rotator = CreateRotator(-35f, 55f);

            rotator.ApplyDrag(new Vector2(0f, 10000f));

            Assert.That(rotator.Pitch, Is.InRange(-35f, 55f));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }

            createdObjects.Clear();
        }

        private EquipmentRotator CreateRotator(float minPitch, float maxPitch)
        {
            var rotatorObject = new GameObject("Equipment Rotator Test");
            createdObjects.Add(rotatorObject);
            var rotator = rotatorObject.AddComponent<EquipmentRotator>();
            rotator.Configure(minPitch, maxPitch, 1f);
            return rotator;
        }
    }
}
