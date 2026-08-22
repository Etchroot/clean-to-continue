using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class FinalMediaAssetTests
    {
        private static readonly string[] UiSpritePaths =
        {
            "Assets/ThirdParty/intro img.png",
            "Assets/ThirdParty/end img.png",
            "Assets/ThirdParty/airgun.png",
            "Assets/ThirdParty/rag.png",
            "Assets/ThirdParty/album1.png",
            "Assets/ThirdParty/album2.png",
            "Assets/ThirdParty/album3.png"
        };

        [Test]
        public void FinalImagesAreSingleSpritesWithoutMipmaps()
        {
            foreach (var path in UiSpritePaths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
                Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), path);
                Assert.That(importer.mipmapEnabled, Is.False, path);
                Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(path), Is.Not.Null, path);
            }
        }

        [Test]
        public void RoundedRectangleHasVisibleNineSliceBorder()
        {
            const string path = "Assets/CleanToContinue/Sprites/Generated/RoundedRect.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            Assert.That(sprite, Is.Not.Null, path);
            Assert.That(sprite.border.x, Is.GreaterThanOrEqualTo(16f));
            Assert.That(sprite.border.y, Is.GreaterThanOrEqualTo(16f));
            Assert.That(sprite.border.z, Is.GreaterThanOrEqualTo(16f));
            Assert.That(sprite.border.w, Is.GreaterThanOrEqualTo(16f));
        }

        [Test]
        public void StreamingIntroVideoMatchesThirdPartySource()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var source = Path.Combine(projectRoot, "Assets", "ThirdParty", "intro video.mp4");
            var streaming = Path.Combine(projectRoot, "Assets", "StreamingAssets", "intro video.mp4");

            Assert.That(File.Exists(streaming), Is.True, streaming);
            Assert.That(new FileInfo(streaming).Length, Is.EqualTo(new FileInfo(source).Length));
        }
    }
}
