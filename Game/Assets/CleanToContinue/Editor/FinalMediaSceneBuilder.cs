using System;
using System.IO;
using System.Linq;
using CleanToContinue.Audio;
using CleanToContinue.Flow;
using CleanToContinue.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace CleanToContinue.Editor
{
    public static class FinalMediaSceneBuilder
    {
        public const string RoundedSpritePath = "Assets/CleanToContinue/Sprites/Generated/RoundedRect.png";
        public const string StreamingVideoPath = "Assets/StreamingAssets/intro video.mp4";
        private const string SourceVideoPath = "Assets/ThirdParty/intro video.mp4";
        private const string MusicPath = "Assets/ThirdParty/sunshine desk.mp3";
        private const string IntroImagePath = "Assets/ThirdParty/intro img.png";
        private const string EndImagePath = "Assets/ThirdParty/end img.png";
        private const string AirGunImagePath = "Assets/ThirdParty/airgun.png";
        private const string RagImagePath = "Assets/ThirdParty/rag.png";
        private const string RenderTexturePath = "Assets/CleanToContinue/RenderTextures/IntroVideo.renderTexture";
        private const string KoreanFontPath = "Assets/CleanToContinue/Fonts/NotoSansCJKkr-Regular.otf";
        private const string FinalRootName = "__CleanToContinueFinalMedia";

        private const string Credits =
            "Creator : 차명근\nAI Agent : Codex\nEngine : Unity\nAsset : Unity Asset Store\n" +
            "Title Video & Image : Nanobanana\nAlbum Image : GPT\nSound : Suno";

        private static readonly string[] OpeningLines =
        {
            "얼마만에 생긴 휴식시간인지 모르겠다.",
            "옛날에는 게임을 정말 재밌게 했었는데.",
            "오랜만에 게임이나 해볼까?",
            "그 전에 장비에 쌓인 먼지부터 닦아야겠는걸."
        };

        private static readonly string[] UiSpritePaths =
        {
            IntroImagePath, EndImagePath, AirGunImagePath, RagImagePath,
            "Assets/ThirdParty/album1.png", "Assets/ThirdParty/album2.png", "Assets/ThirdParty/album3.png"
        };

        [MenuItem("Clean to Continue/Apply Final Media and UI")]
        public static void ApplyFinalMediaAndUi()
        {
            PrepareAssets();
            BuildMainMenu();
            BuildOpening();
            PatchEquipmentStage("Assets/CleanToContinue/Scenes/03.Mouse.unity", "Assets/ThirdParty/album1.png");
            PatchEquipmentStage("Assets/CleanToContinue/Scenes/04.Keyboard.unity", "Assets/ThirdParty/album2.png");
            PatchEquipmentStage("Assets/CleanToContinue/Scenes/05.Headset.unity", "Assets/ThirdParty/album3.png");
            BuildEnding();
            AssetDatabase.SaveAssets();
            Debug.Log("[Clean to Continue] Final media, rounded UI, stage albums and audio are applied.");
        }

        public static void PrepareAssets()
        {
            foreach (var path in UiSpritePaths) ConfigureUiSprite(path);
            EnsureRoundedSprite();
            EnsureStreamingVideo();
            EnsureIntroRenderTexture();
            AssetDatabase.ImportAsset(KoreanFontPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildMainMenu()
        {
            EditScene("Assets/CleanToContinue/Scenes/01.MainMenu.unity", scene =>
            {
                foreach (var existingMusic in FindAll<PersistentMusicPlayer>(scene).ToArray())
                {
                    Object.DestroyImmediate(existingMusic.gameObject);
                }

                var root = RecreateFinalRoot(scene);
                var canvas = CreateCanvas(root.transform, "FinalCanvas", 200);
                var menuRoot = CreateUiObject(canvas.transform, "MenuRoot");
                Stretch(menuRoot.GetComponent<RectTransform>());
                CreateFullscreenImage(menuRoot.transform, "MenuBackground", Load<Sprite>(IntroImagePath), Color.white);
                var start = CreateButton(menuRoot.transform, "StartButton", "시작", new Vector2(0f, -145f), new Vector2(430f, 82f));
                var settings = CreateButton(menuRoot.transform, "SettingsButton", "설정", new Vector2(0f, -245f), new Vector2(430f, 76f));
                var credits = CreateButton(menuRoot.transform, "CreditsButton", "크레딧", new Vector2(0f, -340f), new Vector2(430f, 76f));

                var settingsPanel = CreatePanel(menuRoot.transform, "SettingsPanel", new Vector2(960f, 750f));
                CreateText(settingsPanel.transform, "SettingsHeading", "설정", 52, FontStyle.Bold,
                    new Vector2(0.5f, 0.89f), new Vector2(0.5f, 0.89f), new Vector2(700f, 70f), Vector2.zero);
                var master = CreateSlider(settingsPanel.transform, "MasterVolume", "전체 음량", 180f, 0f, 1f);
                var music = CreateSlider(settingsPanel.transform, "MusicVolume", "배경음", 55f, 0f, 1f);
                var effects = CreateSlider(settingsPanel.transform, "EffectsVolume", "효과음", -70f, 0f, 1f);
                var rotation = CreateSlider(settingsPanel.transform, "RotationSensitivity", "회전 감도", -195f, 0.25f, 2f);
                var settingsClose = CreateButton(settingsPanel.transform, "SettingsCloseButton", "닫기", new Vector2(0f, -315f), new Vector2(260f, 64f));

                var creditsPanel = CreatePanel(menuRoot.transform, "CreditsPanel", new Vector2(1080f, 760f));
                CreateText(creditsPanel.transform, "CreditsHeading", "크레딧", 52, FontStyle.Bold,
                    new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(850f, 70f), Vector2.zero);
                var creditsBody = CreateText(creditsPanel.transform, "CreditsBody", Credits, 32, FontStyle.Normal,
                    new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), new Vector2(920f, 500f), Vector2.zero);
                creditsBody.alignment = TextAnchor.MiddleLeft;
                var creditsClose = CreateButton(creditsPanel.transform, "CreditsCloseButton", "닫기", new Vector2(0f, -310f), new Vector2(260f, 64f));

                var menu = root.AddComponent<MainMenuView>();
                menu.Configure(start, settings, credits, settingsClose, creditsClose,
                    settingsPanel, creditsPanel, master, music, effects, rotation);
                settingsPanel.SetActive(false);
                creditsPanel.SetActive(false);

                var musicObject = new GameObject("PersistentMusic", typeof(AudioSource), typeof(PersistentMusicPlayer));
                SceneManager.MoveGameObjectToScene(musicObject, scene);
                var musicPlayer = musicObject.GetComponent<PersistentMusicPlayer>();
                musicPlayer.Configure(Load<AudioClip>(MusicPath));

                var videoObject = new GameObject("IntroVideoPlayer", typeof(VideoPlayer));
                SceneManager.MoveGameObjectToScene(videoObject, scene);
                videoObject.transform.SetParent(root.transform, false);
                var player = videoObject.GetComponent<VideoPlayer>();
                var target = Load<RenderTexture>(RenderTexturePath);
                player.renderMode = VideoRenderMode.RenderTexture;
                player.targetTexture = target;
                // Muted video can autoplay on Web; music begins from the first menu gesture.
                player.audioOutputMode = VideoAudioOutputMode.None;
                player.playOnAwake = false;
                player.isLooping = false;

                var videoScreenObject = CreateUiObject(canvas.transform, "IntroVideoScreen");
                Stretch(videoScreenObject.GetComponent<RectTransform>());
                var videoScreen = videoScreenObject.AddComponent<RawImage>();
                videoScreen.texture = target;
                videoScreen.color = Color.white;
                videoScreen.raycastTarget = true;
                var retry = videoScreenObject.AddComponent<Button>();
                retry.targetGraphic = videoScreen;
                var controller = root.AddComponent<IntroVideoController>();
                controller.Configure(player, videoScreen, retry, menuRoot, musicPlayer, "intro video.mp4", 8f);
                menuRoot.SetActive(false);
            });
        }

        private static void BuildOpening()
        {
            EditScene("Assets/CleanToContinue/Scenes/02.Opening.unity", scene =>
            {
                var root = RecreateFinalRoot(scene);
                var canvas = CreateCanvas(root.transform, "FinalCanvas", 200);
                CreateFullscreenImage(canvas.transform, "OpeningBackground", null, Color.black);
                var line = CreateText(canvas.transform, "OpeningLine", string.Empty, 46, FontStyle.Normal,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1480f, 580f), Vector2.zero);
                line.alignment = TextAnchor.MiddleCenter;
                line.lineSpacing = 1.35f;
                var skip = CreateButton(canvas.transform, "SkipButton", "건너뛰기", new Vector2(-55f, -45f), new Vector2(240f, 66f), true);
                var sequence = root.AddComponent<OpeningSequence>();
                sequence.Configure(skip, line, OpeningLines, 3f, "03.Mouse");
            });
        }

        private static void BuildEnding()
        {
            EditScene("Assets/CleanToContinue/Scenes/06.Ending.unity", scene =>
            {
                var root = RecreateFinalRoot(scene);
                var canvas = CreateCanvas(root.transform, "FinalCanvas", 200);
                CreateFullscreenImage(canvas.transform, "EndingBackground", Load<Sprite>(EndImagePath), Color.white);
                CreateImage(canvas.transform, "EndingShade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                    new Color(0f, 0f, 0f, 0.24f), false);
                var thanks = CreateText(canvas.transform, "EndingThanks", "플레이 해주셔서 감사합니다", 68, FontStyle.Bold,
                    new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(1300f, 120f), Vector2.zero);
                var shadow = thanks.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
                shadow.effectDistance = new Vector2(6f, -6f);
                shadow.useGraphicAlpha = true;
                var outline = thanks.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                outline.effectDistance = new Vector2(3f, -3f);
                outline.useGraphicAlpha = true;
                var restart = CreateButton(canvas.transform, "RestartButton", "처음으로 돌아가기", new Vector2(0f, -105f), new Vector2(430f, 82f));
                var ending = root.AddComponent<EndingView>();
                ending.Configure(restart);
            });
        }

        private static void PatchEquipmentStage(string path, string albumPath)
        {
            EditScene(path, scene =>
            {
                SetImage(scene, "AirGunImageSlot", Load<Sprite>(AirGunImagePath), false);
                SetImage(scene, "ClothImageSlot", Load<Sprite>(RagImagePath), false);
                ConfigureEquipmentUiLayout(scene);
                var album = Load<Sprite>(albumPath);
                SetImage(scene, "MemoryImage", album, false);
                foreach (var name in new[] { "ProgressPanel", "ToolPanel", "InstructionPanel", "AirGunButton", "ClothButton", "NextStageButton", "MainMenuButton" })
                {
                    SetImage(scene, name, Load<Sprite>(RoundedSpritePath), true);
                }
                foreach (var icon in FindAll<Transform>(scene).Where(value => value.name == "ToolIcon").ToArray()) Object.DestroyImmediate(icon.gameObject);
                foreach (var placeholder in FindAll<Transform>(scene).Where(value => value.name == "ImagePlaceholder").ToArray()) Object.DestroyImmediate(placeholder.gameObject);
                foreach (var textName in new[] { "RestorationTitle", "PercentText", "InstructionTitle" })
                {
                    var text = FindAll<Text>(scene).Single(value => value.name == textName);
                    text.fontStyle = FontStyle.Normal;
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                    EditorUtility.SetDirty(text);
                }
                var nextStageLabel = FindAll<Button>(scene)
                    .Single(button => button.name == "NextStageButton")
                    .GetComponentInChildren<Text>(true);
                nextStageLabel.text = path.EndsWith("05.Headset.unity", StringComparison.Ordinal)
                    ? "청소 완료!"
                    : "다음 단계 진행";
                EditorUtility.SetDirty(nextStageLabel);
                var bootstrap = FindAll<EquipmentStageBootstrap>(scene).Single();
                var serialized = new SerializedObject(bootstrap);
                serialized.FindProperty("memorySprite").objectReferenceValue = album;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bootstrap);
            });
        }

        private static void SetImage(Scene scene, string name, Sprite sprite, bool sliced)
        {
            var image = FindAll<Image>(scene).Single(value => value.name == name);
            image.sprite = sprite;
            image.color = sliced ? image.color : Color.white;
            image.preserveAspect = !sliced;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            EditorUtility.SetDirty(image);
        }

        private static void ConfigureEquipmentUiLayout(Scene scene)
        {
            var toolPanel = FindAll<RectTransform>(scene).Single(value => value.name == "ToolPanel");
            toolPanel.sizeDelta = new Vector2(340f, 540f);

            ConfigureToolButton(scene, "AirGunButton", "AirGunImageSlot", 130f);
            ConfigureToolButton(scene, "ClothButton", "ClothImageSlot", -130f);

            var instructionPanel = FindAll<RectTransform>(scene).Single(value => value.name == "InstructionPanel");
            instructionPanel.sizeDelta = new Vector2(480f, 310f);
            FindAll<RectTransform>(scene).Single(value => value.name == "InstructionTitle").sizeDelta = new Vector2(420f, 66f);
            FindAll<RectTransform>(scene).Single(value => value.name == "InstructionBody").sizeDelta = new Vector2(430f, 190f);

            var memoryLine = FindAll<Text>(scene).Single(value => value.name == "MemoryLine");
            memoryLine.rectTransform.anchorMin = new Vector2(0.5f, 0.365f);
            memoryLine.rectTransform.anchorMax = new Vector2(0.5f, 0.365f);
            memoryLine.rectTransform.anchoredPosition = Vector2.zero;
            memoryLine.rectTransform.sizeDelta = new Vector2(1300f, 110f);
            memoryLine.fontSize = 44;
            memoryLine.verticalOverflow = VerticalWrapMode.Overflow;
            EditorUtility.SetDirty(memoryLine);
        }

        private static void ConfigureToolButton(Scene scene, string buttonName, string imageName, float y)
        {
            var button = FindAll<RectTransform>(scene).Single(value => value.name == buttonName);
            button.anchoredPosition = new Vector2(0f, y);
            button.sizeDelta = new Vector2(290f, 240f);

            var image = FindAll<Image>(scene).Single(value => value.name == imageName);
            image.rectTransform.anchorMin = new Vector2(0.5f, 0.64f);
            image.rectTransform.anchorMax = new Vector2(0.5f, 0.64f);
            image.rectTransform.sizeDelta = new Vector2(160f, 160f);
            image.rectTransform.anchoredPosition = Vector2.zero;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
        }

        private static void EditScene(string path, Action<Scene> edit)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            EnsureEventSystem(scene);
            edit(scene);
            EnsureAudioListener(scene);
            EnsureButtonBehaviors(scene);
            foreach (var text in FindAll<Text>(scene))
            {
                text.font = Load<Font>(KoreanFontPath);
                EditorUtility.SetDirty(text);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException($"Failed to save {path}");
        }

        private static void EnsureAudioListener(Scene scene)
        {
            if (FindAll<AudioListener>(scene).Any())
            {
                return;
            }

            var camera = FindAll<Camera>(scene).FirstOrDefault(value => value.CompareTag("MainCamera"))
                         ?? FindAll<Camera>(scene).FirstOrDefault(value => value.enabled);
            if (camera == null)
            {
                throw new InvalidOperationException($"Scene {scene.name} has no camera for its AudioListener.");
            }

            camera.gameObject.AddComponent<AudioListener>();
            EditorUtility.SetDirty(camera.gameObject);
        }

        private static void EnsureButtonBehaviors(Scene scene)
        {
            foreach (var button in FindAll<Button>(scene).Where(value => value.name != "IntroVideoScreen"))
            {
                if (button.GetComponent<UiButtonClickSound>() == null)
                {
                    button.gameObject.AddComponent<UiButtonClickSound>();
                }

                var hover = button.GetComponent<UiButtonHoverBackground>();
                if (hover == null)
                {
                    hover = button.gameObject.AddComponent<UiButtonHoverBackground>();
                }

                hover.Configure(button.targetGraphic);
            }
        }

        private static GameObject RecreateFinalRoot(Scene scene)
        {
            foreach (var old in scene.GetRootGameObjects().Where(value => value.name == FinalRootName).ToArray()) Object.DestroyImmediate(old);
            var generated = scene.GetRootGameObjects().FirstOrDefault(value => value.name.StartsWith("__CleanToContinueVerticalSlice", StringComparison.Ordinal));
            if (generated != null)
            {
                foreach (var component in generated.GetComponents<MonoBehaviour>())
                    if (component is MainMenuView || component is OpeningSequence || component is IntroVideoController || component is EndingView) Object.DestroyImmediate(component);
                foreach (var child in generated.transform.Cast<Transform>().Where(value => value.GetComponent<Canvas>() != null || value.name == "OpeningMouse" || value.name == "OpeningCamera" || value.name == "OpeningLight").ToArray())
                    Object.DestroyImmediate(child.gameObject);
            }
            var root = new GameObject(FinalRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            var eventSystem = FindAll<EventSystem>(scene).FirstOrDefault();
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
                SceneManager.MoveGameObjectToScene(eventSystemObject, scene);
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<BaseInputModule>() != null) return;
            var inputModuleType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule"))
                .FirstOrDefault(type => type != null);
            if (inputModuleType == null)
            {
                throw new InvalidOperationException("InputSystemUIInputModule is unavailable.");
            }

            eventSystem.gameObject.AddComponent(inputModuleType);
        }

        private static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
        {
            var root = CreateUiObject(parent, name);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size) =>
            CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero, new Color(0.12f, 0.12f, 0.12f, 0.92f), true, true);

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool topRight = false)
        {
            var anchor = topRight ? Vector2.one : new Vector2(0.5f, 0.5f);
            var root = CreateImage(parent, name, anchor, anchor, size, position, new Color(0.22f, 0.22f, 0.22f, 0.78f), true, true, topRight ? Vector2.one : (Vector2?)null);
            var button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            CreateText(root.transform, "Label", label, 34, FontStyle.Bold, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, string label, float y, float minimum, float maximum)
        {
            CreateText(parent, name + "Label", label, 30, FontStyle.Normal, new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(280f, 55f), new Vector2(0f, y));
            var root = CreateImage(parent, name, new Vector2(0.67f, 0.5f), new Vector2(0.67f, 0.5f), new Vector2(430f, 26f), new Vector2(0f, y), new Color(0.25f, 0.25f, 0.25f, 1f), true, true);
            var fill = CreateImage(root.transform, "Fill", Vector2.zero, new Vector2(0.85f, 1f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.72f, 0.38f, 1f), false, true).GetComponent<Image>();
            var handle = CreateImage(root.transform, "Handle", new Vector2(0.85f, 0.5f), new Vector2(0.85f, 0.5f), new Vector2(34f, 44f), Vector2.zero, Color.white, true, true).GetComponent<Image>();
            var slider = root.AddComponent<Slider>();
            slider.minValue = minimum; slider.maxValue = maximum; slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            return slider;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, FontStyle style, Vector2 anchorMin, Vector2 anchorMax, Vector2 dimensions, Vector2 position)
        {
            var root = CreateUiObject(parent, name);
            SetRect(root.GetComponent<RectTransform>(), anchorMin, anchorMax, dimensions, position);
            var text = root.AddComponent<Text>();
            text.font = Load<Font>(KoreanFontPath); text.text = value; text.fontSize = size; text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateFullscreenImage(Transform parent, string name, Sprite sprite, Color color)
        {
            var root = CreateImage(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, color, false);
            var image = root.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = false;
            return root;
        }

        private static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, Color color, bool raycast, bool rounded = false, Vector2? pivot = null)
        {
            var root = CreateUiObject(parent, name);
            SetRect(root.GetComponent<RectTransform>(), anchorMin, anchorMax, size, position, pivot);
            var image = root.AddComponent<Image>(); image.color = color; image.raycastTarget = raycast;
            if (rounded) { image.sprite = Load<Sprite>(RoundedSpritePath); image.type = Image.Type.Sliced; }
            return root;
        }

        private static GameObject CreateUiObject(Transform parent, string name)
        {
            var root = new GameObject(name, typeof(RectTransform)); root.transform.SetParent(parent, false); return root;
        }

        private static void Stretch(RectTransform rect) => SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, Vector2? pivot = null)
        { rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot ?? new Vector2(0.5f, 0.5f); rect.sizeDelta = size; rect.anchoredPosition = position; }
        private static T[] FindAll<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new FileNotFoundException($"Required asset was not imported: {path}");

        private static void ConfigureUiSprite(string path)
        {
            if (!File.Exists(ToAbsolutePath(path))) throw new FileNotFoundException($"Required final media image is missing: {path}");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter ?? throw new InvalidOperationException($"Could not load TextureImporter for {path}");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
        }

        private static void EnsureRoundedSprite()
        {
            var absolutePath = ToAbsolutePath(RoundedSpritePath); Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            const int size = 64; const float radius = 20f; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size]; var half = size * 0.5f; var inner = half - radius;
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
            { var dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inner, 0f); var dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inner, 0f); var alpha = Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy)); pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f)); }
            texture.SetPixels32(pixels); texture.Apply(false, false); File.WriteAllBytes(absolutePath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(RoundedSpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(RoundedSpritePath); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.spriteBorder = new Vector4(20f, 20f, 20f, 20f); importer.SaveAndReimport();
        }

        private static void EnsureStreamingVideo()
        {
            var source = ToAbsolutePath(SourceVideoPath); if (!File.Exists(source)) throw new FileNotFoundException(SourceVideoPath);
            var destination = ToAbsolutePath(StreamingVideoPath); Directory.CreateDirectory(Path.GetDirectoryName(destination));
            if (!File.Exists(destination) || new FileInfo(destination).Length != new FileInfo(source).Length) File.Copy(source, destination, true);
            AssetDatabase.ImportAsset(StreamingVideoPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static void EnsureIntroRenderTexture()
        {
            if (AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath) != null) return;
            if (!AssetDatabase.IsValidFolder("Assets/CleanToContinue/RenderTextures")) AssetDatabase.CreateFolder("Assets/CleanToContinue", "RenderTextures");
            AssetDatabase.CreateAsset(new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32) { name = "IntroVideo" }, RenderTexturePath);
        }

        private static string ToAbsolutePath(string assetPath)
        {
            var root = Directory.GetParent(Application.dataPath)?.FullName ?? throw new InvalidOperationException("Unity project root could not be resolved.");
            return Path.Combine(root, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
