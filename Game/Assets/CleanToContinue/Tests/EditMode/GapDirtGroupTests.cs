using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Gap;
using NUnit.Framework;
using UnityEngine;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class GapDirtGroupTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [Test]
        public void CottonSwabReducesOnlyMatchingSpot()
        {
            var group = CreateGroupWithTwoSpots();

            Assert.That(
                group.TryClean(CleaningTool.AirGun, group.Spots[0].CleaningCollider, 0.5f),
                Is.False);
            Assert.That(group.Progress01, Is.EqualTo(0f));

            Assert.That(
                group.TryClean(CleaningTool.CottonSwab, group.Spots[0].CleaningCollider, 0.5f),
                Is.True);
            Assert.That(group.Spots[0].Remaining01, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(group.Spots[1].Remaining01, Is.EqualTo(1f));
            Assert.That(group.Progress01, Is.EqualTo(0.25f).Within(0.001f));
        }

        [Test]
        public void CompletedSpotDisablesOnlyItsCleaningCollider()
        {
            var group = CreateGroupWithTwoSpots();

            Assert.That(
                group.TryClean(CleaningTool.CottonSwab, group.Spots[0].CleaningCollider, 1f),
                Is.True);

            Assert.That(group.Spots[0].Remaining01, Is.EqualTo(0f));
            Assert.That(group.Spots[0].CleaningCollider.enabled, Is.False);
            Assert.That(group.Spots[1].CleaningCollider.enabled, Is.True);
            Assert.That(group.Progress01, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void ForceFinishCompletesEverySpot()
        {
            var group = CreateGroupWithTwoSpots();

            group.ForceFinish();

            Assert.That(group.Progress01, Is.EqualTo(1f));
            Assert.That(group.Spots[0].CleaningCollider.enabled, Is.False);
            Assert.That(group.Spots[1].CleaningCollider.enabled, Is.False);
        }

        [Test]
        public void MissingSpotReferenceDoesNotCountAsAlreadyCleaned()
        {
            var root = new GameObject("Gap Dirt Missing Reference Test");
            createdObjects.Add(root);
            var validSpot = CreateSpot(root.transform, "Valid Gap Spot");
            var group = root.AddComponent<GapDirtGroup>();
            group.Configure(new[] { validSpot, null });

            Assert.That(group.Progress01, Is.EqualTo(0f));

            Assert.That(
                group.TryClean(CleaningTool.CottonSwab, validSpot.CleaningCollider, 1f),
                Is.True);
            Assert.That(group.Progress01, Is.EqualTo(1f));
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

        private GapDirtGroup CreateGroupWithTwoSpots()
        {
            var root = new GameObject("Gap Dirt Test Group");
            createdObjects.Add(root);
            var spots = new[]
            {
                CreateSpot(root.transform, "Gap Spot A"),
                CreateSpot(root.transform, "Gap Spot B")
            };
            var group = root.AddComponent<GapDirtGroup>();
            group.Configure(spots);
            return group;
        }

        private static GapDirtSpot CreateSpot(Transform parent, string name)
        {
            var spotObject = new GameObject(name);
            spotObject.transform.SetParent(parent, false);
            var collider = spotObject.AddComponent<SphereCollider>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visible Dirt";
            visual.transform.SetParent(spotObject.transform, false);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            var spot = spotObject.AddComponent<GapDirtSpot>();
            spot.Configure(collider, visual.transform, visual.GetComponent<Renderer>());
            return spot;
        }
    }
}
