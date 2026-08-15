using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanToContinue.Editor
{
    public static class NumberedSceneBuilder
    {
        public const string MenuPath = "Clean to Continue/Build Numbered Scene Skeletons";
        public const string SceneFolder = "Assets/CleanToContinue/Scenes";

        private static readonly SceneDefinition[] Definitions =
        {
            new SceneDefinition("01.MainMenu", "메인 메뉴 UI가 이곳에 들어갑니다.", new Color(0.035f, 0.055f, 0.09f, 1f)),
            new SceneDefinition("02.Opening", "더러운 책상을 발견하는 오프닝이 이곳에 들어갑니다.", new Color(0.075f, 0.055f, 0.055f, 1f)),
            new SceneDefinition("03.Mouse", "마우스 청소 튜토리얼이 이곳에 들어갑니다.", new Color(0.04f, 0.08f, 0.105f, 1f)),
            new SceneDefinition("04.Keyboard", "키보드 청소 스테이지가 이곳에 들어갑니다.", new Color(0.055f, 0.075f, 0.055f, 1f)),
            new SceneDefinition("05.Headset", "헤드셋 청소 피날레가 이곳에 들어갑니다.", new Color(0.07f, 0.05f, 0.09f, 1f)),
            new SceneDefinition("06.Ending", "깨끗해진 책상과 엔딩이 이곳에 들어갑니다.", new Color(0.085f, 0.07f, 0.035f, 1f))
        };

        public static string[] ScenePaths => Definitions
            .Select(definition => $"{SceneFolder}/{definition.Name}.unity")
            .ToArray();

        [MenuItem(MenuPath)]
        public static void Build()
        {
            EnsureFolder("Assets", "CleanToContinue");
            EnsureFolder("Assets/CleanToContinue", "Scenes");

            foreach (var definition in Definitions)
            {
                BuildScene(definition);
            }

            EditorBuildSettings.scenes = ScenePaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CTC_SCENES] Six numbered scene skeletons and Build Settings were updated.");
        }

        private static void BuildScene(SceneDefinition definition)
        {
            var path = $"{SceneFolder}/{definition.Name}.unity";
            EnsureSceneAsset(path, definition.Name, definition.Guide, definition.Background);
        }

        public static void EnsureSceneAsset(
            string path,
            string sceneName,
            string guide,
            Color background)
        {
            var existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null;
            var loadedScene = SceneManager.GetSceneByPath(path);
            var wasAlreadyLoaded = loadedScene.IsValid() && loadedScene.isLoaded;
            var scene = wasAlreadyLoaded
                ? loadedScene
                : existing
                    ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive)
                    : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            try
            {
                EnsureSkeleton(scene, sceneName, guide, background);

                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new InvalidOperationException($"Failed to save numbered scene: {path}");
                }
            }
            finally
            {
                if (!wasAlreadyLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        public static void EnsureSkeleton(
            Scene scene,
            string sceneName,
            string guide,
            Color background)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded scene is required.", nameof(scene));
            }

            var sceneRoot = EnsureRoot(scene, "SceneRoot", out _);
            EnsureChild(sceneRoot.transform, "EnvironmentRoot", out _);
            EnsureChild(sceneRoot.transform, "ContentRoot", out _);
            EnsureChild(sceneRoot.transform, "GameplayRoot", out _);

            var cameraObject = EnsureRoot(scene, "Main Camera", out var cameraObjectCreated);
            ConfigureCamera(cameraObject, background, cameraObjectCreated);
            var lightObject = EnsureRoot(scene, "Directional Light", out var lightObjectCreated);
            ConfigureLight(lightObject, lightObjectCreated);
            var uiRoot = EnsureRoot(scene, "UIRoot", out var uiRootCreated);
            ConfigureUi(uiRoot, new SceneDefinition(sceneName, guide, background), uiRootCreated);
            var eventSystem = EnsureRoot(scene, "EventSystem", out _);
            EnsureComponent<EventSystem>(eventSystem, out _);
        }

        private static GameObject EnsureRoot(Scene scene, string name, out bool created)
        {
            var root = scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.name == name);
            if (root != null)
            {
                created = false;
                return root;
            }

            root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            created = true;
            return root;
        }

        private static GameObject EnsureChild(Transform parent, string name, out bool created)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                created = false;
                return child.gameObject;
            }

            var childObject = new GameObject(name);
            childObject.transform.SetParent(parent, false);
            created = true;
            return childObject;
        }

        private static void ConfigureCamera(
            GameObject cameraObject,
            Color background,
            bool objectCreated)
        {
            var camera = EnsureComponent<Camera>(cameraObject, out var componentCreated);
            if (objectCreated)
            {
                cameraObject.tag = "MainCamera";
                cameraObject.transform.SetPositionAndRotation(
                    new Vector3(0f, 2f, -6f),
                    Quaternion.Euler(10f, 0f, 0f));
            }

            if (objectCreated || componentCreated)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = background;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 100f;
            }
        }

        private static void ConfigureLight(GameObject lightObject, bool objectCreated)
        {
            var light = EnsureComponent<Light>(lightObject, out var componentCreated);
            if (objectCreated)
            {
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            if (objectCreated || componentCreated)
            {
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.94f, 0.86f, 1f);
                light.intensity = 1.2f;
            }
        }

        private static void ConfigureUi(
            GameObject uiRoot,
            SceneDefinition definition,
            bool objectCreated)
        {
            var canvas = EnsureComponent<Canvas>(uiRoot, out var canvasCreated);
            if (objectCreated || canvasCreated)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var scaler = EnsureComponent<CanvasScaler>(uiRoot, out var scalerCreated);
            if (objectCreated || scalerCreated)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            EnsureComponent<GraphicRaycaster>(uiRoot, out _);

            var background = EnsureUiChild(
                uiRoot.transform,
                "PlaceholderBackground",
                out var backgroundCreated);
            var backgroundImage = EnsureComponent<Image>(background, out var backgroundImageCreated);
            if (backgroundCreated || backgroundImageCreated)
            {
                StretchToParent(background.GetComponent<RectTransform>());
                backgroundImage.color = definition.Background;
                backgroundImage.raycastTarget = false;
            }

            var titleObject = EnsureUiChild(uiRoot.transform, "SceneTitle", out var titleObjectCreated);
            var title = EnsureComponent<Text>(titleObject, out var titleCreated);
            if (titleObjectCreated || titleCreated)
            {
                var titleRect = titleObject.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0.5f);
                titleRect.anchorMax = new Vector2(0.5f, 0.5f);
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = new Vector2(0f, 36f);
                titleRect.sizeDelta = new Vector2(1200f, 120f);
                title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                title.text = definition.Name;
                title.fontSize = 72;
                title.fontStyle = FontStyle.Bold;
                title.alignment = TextAnchor.MiddleCenter;
                title.color = new Color(0.95f, 0.88f, 0.7f, 1f);
                title.raycastTarget = false;
            }

            var guideObject = EnsureUiChild(uiRoot.transform, "SceneGuide", out var guideObjectCreated);
            var guide = EnsureComponent<Text>(guideObject, out var guideCreated);
            if (guideObjectCreated || guideCreated)
            {
                var guideRect = guideObject.GetComponent<RectTransform>();
                guideRect.anchorMin = new Vector2(0.5f, 0.5f);
                guideRect.anchorMax = new Vector2(0.5f, 0.5f);
                guideRect.pivot = new Vector2(0.5f, 0.5f);
                guideRect.anchoredPosition = new Vector2(0f, -52f);
                guideRect.sizeDelta = new Vector2(1400f, 80f);
                guide.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                guide.text = $"SCENE SKELETON  |  {definition.Guide}";
                guide.fontSize = 28;
                guide.alignment = TextAnchor.MiddleCenter;
                guide.color = new Color(0.78f, 0.8f, 0.84f, 1f);
                guide.raycastTarget = false;
            }
        }

        private static GameObject EnsureUiChild(Transform parent, string name, out bool created)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                created = false;
                return existing.gameObject;
            }

            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            created = true;
            return child;
        }

        private static T EnsureComponent<T>(GameObject gameObject, out bool created)
            where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var component))
            {
                created = false;
                return component;
            }

            created = true;
            return gameObject.AddComponent<T>();
        }

        private static void StretchToParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private readonly struct SceneDefinition
        {
            public SceneDefinition(string name, string guide, Color background)
            {
                Name = name;
                Guide = guide;
                Background = background;
            }

            public string Name { get; }
            public string Guide { get; }
            public Color Background { get; }
        }
    }
}
