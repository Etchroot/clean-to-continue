using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class NumberedSceneScaffoldTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/CleanToContinue/Scenes/01.MainMenu.unity",
            "Assets/CleanToContinue/Scenes/02.Opening.unity",
            "Assets/CleanToContinue/Scenes/03.Mouse.unity",
            "Assets/CleanToContinue/Scenes/04.Keyboard.unity",
            "Assets/CleanToContinue/Scenes/05.Headset.unity",
            "Assets/CleanToContinue/Scenes/06.Ending.unity"
        };

        [Test]
        public void NumberedScenesExistAndContainInspectableSkeletons()
        {
            foreach (var path in ScenePaths)
            {
                Assert.That(
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(path),
                    Is.Not.Null,
                    $"Missing numbered scene: {path}");

                var loadedScene = SceneManager.GetSceneByPath(path);
                var wasAlreadyLoaded = loadedScene.IsValid() && loadedScene.isLoaded;
                var scene = wasAlreadyLoaded
                    ? loadedScene
                    : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    var roots = scene.GetRootGameObjects();
                    AssertSkeletonStructure(scene, path);

                    var title = roots
                        .SelectMany(root => root.GetComponentsInChildren<Text>(true))
                        .SingleOrDefault(text => text.name == "SceneTitle");
                    Assert.That(title, Is.Not.Null, path);
                    Assert.That(title.text, Is.EqualTo(scene.name), path);
                }
                finally
                {
                    if (!wasAlreadyLoaded)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }

        [Test]
        public void BuildSettingsUseTheExactNumberedSceneOrder()
        {
            var scenes = EditorBuildSettings.scenes;
            var paths = scenes.Select(scene => scene.path).ToArray();

            Assert.That(paths, Is.EqualTo(ScenePaths));
            Assert.That(scenes.All(scene => scene.enabled), Is.True);
        }

        [Test]
        public void BuilderKeepsAnAlreadyOpenTemporarySceneLoaded()
        {
            var path = $"Assets/CleanToContinue/Tests/NumberedSceneBuilderTemp_{Guid.NewGuid():N}.unity";
            Assert.That(AssetDatabase.CopyAsset(ScenePaths[0], path), Is.True);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                InvokeBuilderMethod(
                    "EnsureSceneAsset",
                    path,
                    "99.Test",
                    "Temporary test scene",
                    Color.black);

                var sceneAfterBuild = SceneManager.GetSceneByPath(path);
                Assert.That(sceneAfterBuild.IsValid() && sceneAfterBuild.isLoaded, Is.True);
            }
            finally
            {
                var cleanupScene = SceneManager.GetSceneByPath(path);
                if (cleanupScene.IsValid() && cleanupScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(cleanupScene, true);
                }

                AssetDatabase.DeleteAsset(path);
            }
        }

        [Test]
        public void SkeletonAddsMissingComponentsWithoutMovingExistingObjects()
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.transform.position = new Vector3(9f, 8f, 7f);
            cameraObject.tag = "Untagged";

            var lightObject = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            lightObject.transform.rotation = Quaternion.Euler(11f, 22f, 33f);

            try
            {
                InvokeBuilderMethod(
                    "EnsureSkeleton",
                    scene,
                    "99.Test",
                    "Temporary test scene",
                    Color.black);

                Assert.That(cameraObject.transform.position, Is.EqualTo(new Vector3(9f, 8f, 7f)));
                Assert.That(cameraObject.tag, Is.EqualTo("Untagged"));
                Assert.That(cameraObject.GetComponent<Camera>(), Is.Not.Null);
                Assert.That(lightObject.transform.rotation.eulerAngles.x, Is.EqualTo(11f).Within(0.01f));
                Assert.That(lightObject.transform.rotation.eulerAngles.y, Is.EqualTo(22f).Within(0.01f));
                Assert.That(lightObject.transform.rotation.eulerAngles.z, Is.EqualTo(33f).Within(0.01f));
                Assert.That(lightObject.GetComponent<Light>(), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void SkeletonApplicationIsIdempotentAndPreservesExistingConfiguration()
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.transform.position = new Vector3(9f, 8f, 7f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.backgroundColor = Color.magenta;

            try
            {
                InvokeBuilderMethod(
                    "EnsureSkeleton",
                    scene,
                    "99.Test",
                    "Temporary test scene",
                    Color.black);
                InvokeBuilderMethod(
                    "EnsureSkeleton",
                    scene,
                    "99.Test",
                    "Temporary test scene",
                    Color.black);

                Assert.That(cameraObject.transform.position, Is.EqualTo(new Vector3(9f, 8f, 7f)));
                Assert.That(camera.backgroundColor, Is.EqualTo(Color.magenta));
                AssertSkeletonStructure(scene, "temporary scene");
                var title = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Text>(true))
                    .Single(text => text.name == "SceneTitle");
                Assert.That(title.text, Is.EqualTo("99.Test"));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void AssertSkeletonStructure(Scene scene, string context)
        {
            var roots = scene.GetRootGameObjects();
            Assert.That(roots.Count(root => root.name == "SceneRoot"), Is.EqualTo(1), context);
            Assert.That(roots.Count(root => root.name == "Main Camera"), Is.EqualTo(1), context);
            Assert.That(roots.Count(root => root.name == "Directional Light"), Is.EqualTo(1), context);
            Assert.That(roots.Count(root => root.name == "UIRoot"), Is.EqualTo(1), context);
            Assert.That(roots.Count(root => root.name == "EventSystem"), Is.EqualTo(1), context);

            var sceneRoot = roots.Single(root => root.name == "SceneRoot");
            Assert.That(sceneRoot.transform.Cast<Transform>().Count(child => child.name == "EnvironmentRoot"), Is.EqualTo(1), context);
            Assert.That(sceneRoot.transform.Cast<Transform>().Count(child => child.name == "ContentRoot"), Is.EqualTo(1), context);
            Assert.That(sceneRoot.transform.Cast<Transform>().Count(child => child.name == "GameplayRoot"), Is.EqualTo(1), context);
            Assert.That(roots.Single(root => root.name == "Main Camera").GetComponents<Camera>(), Has.Length.EqualTo(1), context);
            Assert.That(roots.Single(root => root.name == "Directional Light").GetComponents<Light>(), Has.Length.EqualTo(1), context);
            Assert.That(roots.Single(root => root.name == "UIRoot").GetComponents<Canvas>(), Has.Length.EqualTo(1), context);
            Assert.That(roots.Single(root => root.name == "EventSystem").GetComponents<EventSystem>(), Has.Length.EqualTo(1), context);
        }

        private static object InvokeBuilderMethod(string methodName, params object[] arguments)
        {
            var builderType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("CleanToContinue.Editor.NumberedSceneBuilder"))
                .FirstOrDefault(type => type != null);
            Assert.That(builderType, Is.Not.Null);
            var method = builderType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, $"Missing builder method: {methodName}");
            return method.Invoke(null, arguments);
        }
    }
}
