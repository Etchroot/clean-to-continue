using System;
using System.Collections.Generic;
using System.Linq;
using CleanToContinue.Audio;
using CleanToContinue.Core;
using CleanToContinue.Flow;
using CleanToContinue.Gap;
using CleanToContinue.Highlight;
using CleanToContinue.Input;
using CleanToContinue.Stage;
using CleanToContinue.Surface;
using CleanToContinue.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CleanToContinue.Editor
{
    public static class VerticalSliceSceneBuilder
    {
        public const string MenuPath = "Clean to Continue/Build Vertical Slice Scenes";
        public const string RepairEquipmentMenuPath = "Clean to Continue/Repair Keyboard and Headset Cleaning";

        public const string MousePrefabPath = "Assets/ThirdParty/Mouse.prefab";
        public const string KeyboardPrefabPath = "Assets/ThirdParty/Keyboard.prefab";
        public const string HeadsetPrefabPath = "Assets/ThirdParty/Headset Type1.prefab";
        public const string StageRootPrefabPath = "Assets/CleanToContinue/Prefabs/StageRoot.prefab";
        public const string PrototypeMousePrefabPath = "Assets/CleanToContinue/Prefabs/PrototypeMouse.prefab";

        private const string MainMenuScenePath = "Assets/CleanToContinue/Scenes/01.MainMenu.unity";
        private const string OpeningScenePath = "Assets/CleanToContinue/Scenes/02.Opening.unity";
        private const string MouseScenePath = "Assets/CleanToContinue/Scenes/03.Mouse.unity";
        private const string KeyboardScenePath = "Assets/CleanToContinue/Scenes/04.Keyboard.unity";
        private const string HeadsetScenePath = "Assets/CleanToContinue/Scenes/05.Headset.unity";
        private const string OwnedRootName = "__CleanToContinueVerticalSlice";
        private const string OwnershipMarkerName = "__GeneratedByVerticalSliceSceneBuilder";
        private const string DustOverlayMaterialPath = "Assets/CleanToContinue/Materials/MouseDustOverlay.mat";
        private const string PolishOverlayMaterialPath = "Assets/CleanToContinue/Materials/MousePolishOverlay.mat";

        private static readonly string[] BuildScenePaths =
        {
            MainMenuScenePath,
            OpeningScenePath,
            MouseScenePath,
            "Assets/CleanToContinue/Scenes/04.Keyboard.unity",
            "Assets/CleanToContinue/Scenes/05.Headset.unity",
            "Assets/CleanToContinue/Scenes/06.Ending.unity"
        };

        private sealed class StageDefinition
        {
            public static readonly StageDefinition Mouse = new StageDefinition(
                "Mouse", MousePrefabPath, "Mouse (Playable)", "MouseEquipment",
                "처음 오락기 앞에 섰던 날, 바라보는 것만으로도 새로운 세계가 열렸다.",
                "04.Keyboard", 24f, 0.38f, 3.65f);

            public static readonly StageDefinition Keyboard = new StageDefinition(
                "Keyboard", KeyboardPrefabPath, "Keyboard (Playable)", "KeyboardEquipment",
                "처음 친구와 나란히 앉아, 같은 게임 속을 함께 달렸다.",
                "05.Headset", 0f, 0.62f, 3.45f);

            public static readonly StageDefinition Headset = new StageDefinition(
                "Headset Type1", HeadsetPrefabPath, "Headset (Playable)", "HeadsetEquipment",
                "PC방을 가득 채운 우리의 환호가, 아직도 귓가에 선명하다.",
                "06.Ending", 0f, 0.46f, 3.75f);

            private StageDefinition(
                string authoringName,
                string prefabPath,
                string playableName,
                string equipmentName,
                string memoryText,
                string nextScene,
                float fixedScale,
                float deskWidthFraction,
                float cameraDistance)
            {
                AuthoringName = authoringName;
                PrefabPath = prefabPath;
                PlayableName = playableName;
                EquipmentName = equipmentName;
                MemoryText = memoryText;
                NextScene = nextScene;
                FixedScale = fixedScale;
                DeskWidthFraction = deskWidthFraction;
                CameraDistance = cameraDistance;
            }

            public string AuthoringName { get; }
            public string PrefabPath { get; }
            public string PlayableName { get; }
            public string EquipmentName { get; }
            public string MemoryText { get; }
            public string NextScene { get; }
            public float FixedScale { get; }
            public float DeskWidthFraction { get; }
            public float CameraDistance { get; }
        }

        [MenuItem(MenuPath)]
        public static void Build()
        {
            EnsureFolder("Assets/CleanToContinue", "Prefabs");
            EnsureFolder("Assets/CleanToContinue", "Materials");
            var stagePrefab = EnsureStageRootPrefab();
            var dustOverlayMaterial = EnsureOverlayMaterial(
                DustOverlayMaterialPath,
                "MouseDustOverlay",
                0f);
            var polishOverlayMaterial = EnsureOverlayMaterial(
                PolishOverlayMaterialPath,
                "MousePolishOverlay",
                1f);

            BuildScene(MainMenuScenePath, "01.MainMenu", BuildMainMenu);
            BuildScene(OpeningScenePath, "02.Opening", BuildOpening);
            BuildScene(MouseScenePath, "03.Mouse", (scene, root) =>
                BuildEquipmentStage(scene, root, stagePrefab, dustOverlayMaterial, polishOverlayMaterial, StageDefinition.Mouse));
            BuildScene(KeyboardScenePath, "04.Keyboard", (scene, root) =>
                BuildEquipmentStage(scene, root, stagePrefab, dustOverlayMaterial, polishOverlayMaterial, StageDefinition.Keyboard));
            BuildScene(HeadsetScenePath, "05.Headset", (scene, root) =>
                BuildEquipmentStage(scene, root, stagePrefab, dustOverlayMaterial, polishOverlayMaterial, StageDefinition.Headset));

            EditorBuildSettings.scenes = BuildScenePaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CTC_VERTICAL_SLICE] Built menu, opening, mouse, keyboard and headset scenes. Equipment assets remain user-owned.");
        }

        [MenuItem(RepairEquipmentMenuPath)]
        public static void RepairKeyboardAndHeadsetCleaning()
        {
            RepairExistingEquipmentScene(KeyboardScenePath, StageDefinition.Keyboard);
            RepairExistingEquipmentScene(HeadsetScenePath, StageDefinition.Headset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CTC_VERTICAL_SLICE] Repaired keyboard/headset cleaning assets and light positions without rebuilding either scene.");
        }

        private static void RepairExistingEquipmentScene(string path, StageDefinition definition)
        {
            var loaded = SceneManager.GetSceneByPath(path);
            var wasAlreadyLoaded = loaded.IsValid() && loaded.isLoaded;
            var scene = wasAlreadyLoaded
                ? loaded
                : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var playableEquipment = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Select(transform => transform.gameObject)
                    .FirstOrDefault(gameObject => gameObject.name == definition.PlayableName);
                if (playableEquipment == null)
                {
                    throw new InvalidOperationException($"{definition.PlayableName} was not found in {path}.");
                }

                var cleanRenderers = GetCleanableRenderers(playableEquipment);
                for (var rendererIndex = 0; rendererIndex < cleanRenderers.Length; rendererIndex++)
                {
                    var cleanRenderer = cleanRenderers[rendererIndex];
                    var sourceMesh = cleanRenderer.GetComponent<MeshFilter>().sharedMesh;
                    var cleaningMesh = EnsureRuntimeReadableCleaningMesh(definition, rendererIndex, sourceMesh);
                    foreach (var overlayName in new[] { "DustOverlay", "PolishOverlay" })
                    {
                        var overlayFilter = cleanRenderer.transform.Find(overlayName)?.GetComponent<MeshFilter>();
                        if (overlayFilter == null)
                        {
                            throw new InvalidOperationException($"{definition.PlayableName}/{cleanRenderer.name}/{overlayName} was not found.");
                        }

                        Undo.RecordObject(overlayFilter, "Assign readable cleaning mesh");
                        overlayFilter.sharedMesh = cleaningMesh;
                        EditorUtility.SetDirty(overlayFilter);
                    }
                }

                var lights = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Light>(true))
                    .ToArray();
                var warmKeyLight = lights.Single(light => light.name == "WarmKeyLight");
                var coolRimLight = lights.Single(light => light.name == "CoolRimLight");
                Undo.RecordObject(warmKeyLight.transform, "Move warm key light above equipment");
                Undo.RecordObject(coolRimLight.transform, "Move cool rim light above equipment");
                PlaceDirectionalLightsAboveEquipment(playableEquipment, warmKeyLight, coolRimLight);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new InvalidOperationException($"Failed to save repaired equipment scene: {path}");
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

        private static void BuildScene(string path, string sceneName, Action<Scene, Transform> assemble)
        {
            var loaded = SceneManager.GetSceneByPath(path);
            var wasAlreadyLoaded = loaded.IsValid() && loaded.isLoaded;
            var scene = wasAlreadyLoaded
                ? loaded
                : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var root = PrepareOwnedRoot(scene);
                NumberedSceneBuilder.TrySetUntouchedGuideVisible(scene, sceneName, false);
                EnsureUiInputModule(scene);
                assemble(scene, root.transform);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new InvalidOperationException($"Failed to save vertical slice scene: {path}");
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

        private static GameObject PrepareOwnedRoot(Scene scene)
        {
            var candidates = scene.GetRootGameObjects()
                .Where(root => root.name == OwnedRootName || root.name == $"{OwnedRootName}.Generated")
                .ToArray();
            var owned = candidates.FirstOrDefault(root => root.transform.Find(OwnershipMarkerName) != null);
            if (owned == null)
            {
                var requestedName = candidates.Any(root => root.name == OwnedRootName)
                    ? $"{OwnedRootName}.Generated"
                    : OwnedRootName;
                owned = CreateSceneObject(scene, requestedName, null);
                CreateChild(owned.transform, OwnershipMarkerName);
                return owned;
            }

            for (var index = owned.transform.childCount - 1; index >= 0; index--)
            {
                var child = owned.transform.GetChild(index);
                if (child.name != OwnershipMarkerName)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            RemoveOwnedComponents<MainMenuView>(owned);
            RemoveOwnedComponents<OpeningSequence>(owned);

            return owned;
        }

        private static void RemoveOwnedComponents<T>(GameObject ownedRoot) where T : Component
        {
            foreach (var component in ownedRoot.GetComponents<T>())
            {
                Undo.DestroyObjectImmediate(component);
            }
        }

        private static void BuildMainMenu(Scene scene, Transform root)
        {
            var canvas = CreateCanvas(root, "Canvas", 100);
            CreateImage(canvas.transform, "MenuBackground", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.025f, 0.045f, 0.075f, 1f), false);
            CreateText(canvas.transform, "Title", "CLEAN TO CONTINUE", 78, FontStyle.Bold,
                new Vector2(0.5f, 0.73f), new Vector2(0.5f, 0.73f), new Vector2(980f, 120f), Vector2.zero,
                new Color(0.95f, 0.86f, 0.62f, 1f));
            CreateText(canvas.transform, "Subtitle", "장비를 닦고, 기억을 이어가세요.", 30, FontStyle.Normal,
                new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), new Vector2(900f, 70f), Vector2.zero,
                new Color(0.78f, 0.82f, 0.88f, 1f));

            var start = CreateButton(canvas.transform, "StartButton", "시작", new Vector2(0f, 75f), new Vector2(420f, 84f));
            var settings = CreateButton(canvas.transform, "SettingsButton", "설정", new Vector2(0f, -30f), new Vector2(420f, 76f));
            var credits = CreateButton(canvas.transform, "CreditsButton", "크레딧", new Vector2(0f, -125f), new Vector2(420f, 76f));

            var settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", new Vector2(940f, 680f));
            CreateText(settingsPanel.transform, "Heading", "설정", 46, FontStyle.Bold,
                new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(700f, 70f), Vector2.zero, Color.white);
            var master = CreateSettingSlider(settingsPanel.transform, "MasterVolume", "전체 음량", 150f, 0f, 1f);
            var effects = CreateSettingSlider(settingsPanel.transform, "EffectsVolume", "효과음", 10f, 0f, 1f);
            var rotation = CreateSettingSlider(settingsPanel.transform, "RotationSensitivity", "회전 감도", -130f, 0.25f, 2f);
            var settingsClose = CreateButton(settingsPanel.transform, "SettingsCloseButton", "닫기", new Vector2(0f, -270f), new Vector2(260f, 64f));

            var creditsPanel = CreatePanel(canvas.transform, "CreditsPanel", new Vector2(1050f, 700f));
            CreateText(creditsPanel.transform, "CreditsText",
                "CLEAN TO CONTINUE\n\nCreator: 차명근\nDevelopment collaboration: Codex\nEngine: Unity\n\nIntegrated third-party asset:\nMouse — Unity Asset Store\n\nDesk Table White: available, manual placement pending\nCreator and product links: pending verification",
                30, FontStyle.Normal, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(900f, 480f),
                Vector2.zero, new Color(0.92f, 0.93f, 0.96f, 1f));
            var creditsClose = CreateButton(creditsPanel.transform, "CreditsCloseButton", "닫기", new Vector2(0f, -280f), new Vector2(260f, 64f));

            var view = Undo.AddComponent<MainMenuView>(root.gameObject);
            view.Configure(start, settings, credits, settingsClose, creditsClose,
                settingsPanel, creditsPanel, master, null, effects, rotation);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            EditorUtility.SetDirty(view);
        }

        private static void BuildOpening(Scene scene, Transform root)
        {
            var openingCamera = CreateCamera(root, "OpeningCamera", new Vector3(0f, 3.1f, -6.8f), new Vector3(0f, 0.15f, 0f),
                new Color(0.055f, 0.045f, 0.04f, 1f));
            openingCamera.fieldOfView = 40f;
            CreateLight(root, "OpeningLight", new Vector3(45f, -25f, 0f), 1.35f);
            var mouse = InstantiateMouse(root);
            mouse.name = "OpeningMouse";
            mouse.transform.localPosition = new Vector3(0f, -0.25f, 0.1f);
            mouse.transform.localScale = Vector3.one * 23f;

            var canvas = CreateCanvas(root, "Canvas", 100);
            CreateImage(canvas.transform, "LetterboxTop", new Vector2(0f, 0.82f), Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.5f), false);
            var line = CreateText(canvas.transform, "OpeningLine", "정말 오랜만이다. 그런데… 이걸 먼저 치워야겠는데.",
                38, FontStyle.Normal, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(1500f, 100f),
                Vector2.zero, Color.white);
            line.alignment = TextAnchor.MiddleCenter;
            var skip = CreateButtonTopRight(canvas.transform, "SkipButton", "건너뛰기", new Vector2(-70f, -55f), new Vector2(240f, 62f));
            var sequence = Undo.AddComponent<OpeningSequence>(root.gameObject);
            sequence.Configure(skip);
            EditorUtility.SetDirty(sequence);
        }

        private static void BuildEquipmentStage(
            Scene scene,
            Transform root,
            GameObject stagePrefab,
            Material dustOverlayMaterial,
            Material polishOverlayMaterial,
            StageDefinition definition)
        {
            CopyMissingEnvironmentRoots(scene);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.19f, 0.135f, 0.09f, 1f);
            RenderSettings.fog = false;

            var camera = CreateCamera(root, "StageCamera", new Vector3(0f, 3.1f, -7f), new Vector3(-0.3f, 0.2f, 0f),
                new Color(0.075f, 0.045f, 0.028f, 1f));
            camera.fieldOfView = 38f;
            var warmKeyLight = CreateDirectionalLight(root, "WarmKeyLight", new Vector3(52f, -35f, -18f),
                new Color(1f, 0.72f, 0.48f, 1f), 1.35f, true);
            var coolRimLight = CreateDirectionalLight(root, "CoolRimLight", new Vector3(18f, 145f, 0f),
                new Color(0.36f, 0.52f, 0.78f, 1f), 0.48f, false);

            var stageRoot = (GameObject)PrefabUtility.InstantiatePrefab(stagePrefab, root);
            Undo.RegisterCreatedObjectUndo(stageRoot, "Create StageRoot");
            stageRoot.name = "StageRoot";
            var stageController = stageRoot.GetComponent<StageController>();
            var inputController = stageRoot.GetComponent<StageInputController>();
            var interactionController = stageRoot.GetComponent<StageInteractionController>();
            var highlightController = stageRoot.GetComponent<HighlightController>();
            var audioController = stageRoot.GetComponent<CleaningAudioController>();
            var bootstrap = stageRoot.GetComponent<EquipmentStageBootstrap>();

            var equipment = CreateChild(stageRoot.transform, definition.EquipmentName);
            equipment.transform.localPosition = new Vector3(-0.45f, -0.25f, 0f);
            var rotator = Undo.AddComponent<EquipmentRotator>(equipment);
            var playableEquipment = InstantiatePlayableEquipment(scene, equipment.transform, definition);
            playableEquipment.name = definition.PlayableName;
            playableEquipment.transform.localScale = Vector3.one * (definition.FixedScale > 0f ? definition.FixedScale : 1f);
            var surfaceLayers = ConfigurePlayableEquipment(
                playableEquipment,
                definition,
                dustOverlayMaterial,
                polishOverlayMaterial);
            ComposeEquipmentDeskShot(scene, root, equipment.transform, playableEquipment, camera, definition);
            PlaceDirectionalLightsAboveEquipment(playableEquipment, warmKeyLight, coolRimLight);

            var canvas = CreateCanvas(stageRoot.transform, "StageCanvas", 110);
            var progressPanel = CreateImage(canvas.transform, "ProgressPanel",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(420f, 340f), new Vector2(-35f, -35f),
                new Color(0.095f, 0.065f, 0.05f, 0.94f), true, new Vector2(1f, 1f));
            var restorationTitle = CreateText(progressPanel.transform, "RestorationTitle", "복원도", 48, FontStyle.Normal,
                new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.84f), new Vector2(300f, 62f), Vector2.zero,
                new Color(0.96f, 0.87f, 0.68f, 1f));
            restorationTitle.verticalOverflow = VerticalWrapMode.Overflow;
            var progressTrackObject = CreateImage(progressPanel.transform, "ProgressTrack",
                new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(224f, 224f), Vector2.zero,
                new Color(0.18f, 0.12f, 0.09f, 0.96f), false);
            var progressTrack = progressTrackObject.GetComponent<Image>();
            progressTrack.sprite = GetCircleSprite();
            progressTrack.type = Image.Type.Simple;
            var progressFillObject = CreateImage(progressPanel.transform, "ProgressFill",
                new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(210f, 210f), Vector2.zero,
                new Color(0.93f, 0.72f, 0.3f, 1f), false);
            var progressFill = progressFillObject.GetComponent<Image>();
            progressFill.sprite = GetCircleSprite();
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Radial360;
            var percent = CreateText(progressPanel.transform, "PercentText", "0%", 84, FontStyle.Normal,
                new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(190f, 110f), Vector2.zero, Color.white);
            percent.verticalOverflow = VerticalWrapMode.Overflow;
            var progressWheel = Undo.AddComponent<ProgressWheelView>(progressPanel);

            var toolPanel = CreateImage(canvas.transform, "ToolPanel",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(340f, 540f), new Vector2(-35f, -190f),
                new Color(0.095f, 0.065f, 0.05f, 0.94f), true, new Vector2(1f, 0.5f));
            var bindings = new[]
            {
                CreateToolButton(toolPanel.transform, "AirGunButton", "에어건 (먼지 제거)", CleaningTool.AirGun, 130f),
                CreateToolButton(toolPanel.transform, "ClothButton", "헝겊 (광택 내기)", CleaningTool.Cloth, -130f)
            };
            var selector = Undo.AddComponent<ToolSelectorView>(toolPanel);

            var instructionPanel = CreateImage(canvas.transform, "InstructionPanel",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(480f, 310f), new Vector2(35f, -180f),
                new Color(0.095f, 0.065f, 0.05f, 0.94f), false, new Vector2(0f, 0.5f));
            var instructionTitle = CreateText(instructionPanel.transform, "InstructionTitle", "조작 방법", 46, FontStyle.Normal,
                new Vector2(0.5f, 0.79f), new Vector2(0.5f, 0.79f), new Vector2(420f, 66f), Vector2.zero,
                new Color(0.96f, 0.87f, 0.68f, 1f));
            instructionTitle.verticalOverflow = VerticalWrapMode.Overflow;
            var instructionBody = CreateText(instructionPanel.transform, "InstructionBody",
                "마우스 좌클릭 - 청소\n마우스 우클릭 - 회전\n스페이스 바 - 오염 부분 확인",
                36, FontStyle.Normal, new Vector2(0.5f, 0.39f), new Vector2(0.5f, 0.39f),
                new Vector2(430f, 190f), Vector2.zero, new Color(0.92f, 0.9f, 0.86f, 1f));
            instructionBody.alignment = TextAnchor.MiddleLeft;
            instructionBody.lineSpacing = 1.25f;

            var cursorObject = CreateUiChild(canvas.transform, "CleaningCursor");
            SetRect(cursorObject.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(92f, 92f), Vector2.zero, new Vector2(0.5f, 0.5f));
            var cursorHalo = CreateImage(cursorObject.transform, "Halo",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.72f, 0.9f, 1f, 0.18f), false);
            var cursorCore = CreateImage(cursorObject.transform, "Core",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(10f, 10f), Vector2.zero,
                new Color(0.85f, 0.96f, 1f, 0.8f), false);
            var cursorView = Undo.AddComponent<CleaningCursorView>(cursorObject);
            cursorObject.SetActive(false);

            var memoryRoot = CreateImage(canvas.transform, "MemoryPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.82f), true);
            var memoryDimmer = memoryRoot.GetComponent<Image>();
            var memoryImageObject = CreateImage(memoryRoot.transform, "MemoryImage",
                new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(820f, 390f), Vector2.zero,
                new Color(0.18f, 0.24f, 0.32f, 1f), false);
            var memoryImage = memoryImageObject.GetComponent<Image>();
            CreateText(memoryImageObject.transform, "ImagePlaceholder", "MEMORY", 48, FontStyle.Bold,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 100f), Vector2.zero,
                new Color(0.93f, 0.78f, 0.45f, 1f));
            var memoryLine = CreateText(memoryRoot.transform, "MemoryLine", definition.MemoryText,
                44, FontStyle.Normal, new Vector2(0.5f, 0.365f), new Vector2(0.5f, 0.365f), new Vector2(1300f, 110f),
                Vector2.zero, Color.white);
            var memoryNext = CreateButton(memoryRoot.transform, "NextStageButton", "다음 단계 진행", new Vector2(0f, -270f), new Vector2(420f, 76f));
            var memoryMenu = CreateButton(memoryRoot.transform, "MainMenuButton", "메인 메뉴로 돌아가기", new Vector2(0f, -365f), new Vector2(420f, 76f));
            var memoryView = Undo.AddComponent<MemoryPanelView>(stageRoot);
            memoryRoot.SetActive(false);

            bootstrap.Configure(
                stageController,
                inputController,
                interactionController,
                rotator,
                highlightController,
                audioController,
                camera,
                surfaceLayers,
                progressWheel,
                progressFill,
                percent,
                selector,
                bindings,
                cursorView,
                memoryView,
                memoryRoot,
                memoryDimmer,
                memoryImage,
                memoryLine,
                memoryNext,
                memoryMenu,
                definition.MemoryText,
                definition.NextScene);
            EditorUtility.SetDirty(bootstrap);
        }

        private static void ComposeEquipmentDeskShot(
            Scene scene,
            Transform generatedRoot,
            Transform equipment,
            GameObject playableEquipment,
            Camera camera,
            StageDefinition definition)
        {
            var equipmentBounds = EncapsulateRenderers(GetCleanableRenderers(playableEquipment));

            if (TryGetUserDeskBounds(scene, out var deskBounds))
            {
                if (definition.FixedScale <= 0f)
                {
                    var horizontalSize = Mathf.Max(equipmentBounds.size.x, equipmentBounds.size.z);
                    var scale = deskBounds.size.x * definition.DeskWidthFraction / Mathf.Max(0.001f, horizontalSize);
                    playableEquipment.transform.localScale *= scale;
                    equipmentBounds = EncapsulateRenderers(GetCleanableRenderers(playableEquipment));
                }

                var desiredCenter = deskBounds.center;
                desiredCenter.x -= deskBounds.size.x * 0.08f;
                desiredCenter.z -= deskBounds.size.z * 0.06f;
                var centerOffset = equipmentBounds.center - equipment.position;
                playableEquipment.transform.position -= centerOffset;
                equipmentBounds = EncapsulateRenderers(GetCleanableRenderers(playableEquipment));
                var rotationRadius = equipmentBounds.extents.magnitude;
                equipment.position = new Vector3(
                    desiredCenter.x,
                    deskBounds.max.y + rotationRadius + Mathf.Max(0.05f, rotationRadius * 0.04f),
                    desiredCenter.z);
                equipmentBounds = EncapsulateRenderers(GetCleanableRenderers(playableEquipment));
            }
            else
            {
                Debug.LogWarning($"[CTC_VERTICAL_SLICE] Desk was not found for {definition.PlayableName}. Fallback framing is active.");
            }

            var radius = Mathf.Max(0.65f, equipmentBounds.extents.magnitude);
            var target = equipmentBounds.center + Vector3.up * equipmentBounds.size.y * 0.05f;
            var cameraPosition = target + new Vector3(radius * 0.45f, radius * 1.55f, -radius * definition.CameraDistance);
            camera.transform.position = cameraPosition;
            camera.transform.rotation = Quaternion.LookRotation(target - cameraPosition, Vector3.up);
            camera.fieldOfView = 40f;
            camera.nearClipPlane = Mathf.Max(0.02f, radius * 0.025f);
            camera.farClipPlane = Mathf.Max(100f, radius * 25f);

            CreatePointLight(
                generatedRoot,
                "DeskLampFill",
                target + new Vector3(-radius * 0.8f, radius * 1.8f, -radius * 0.7f),
                new Color(1f, 0.55f, 0.29f, 1f),
                Mathf.Max(2.2f, radius * 1.2f),
                radius * 5f);
        }

        private static bool TryGetUserDeskBounds(Scene scene, out Bounds bounds)
        {
            var desk = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Desk");
            var renderers = desk == null
                ? Array.Empty<Renderer>()
                : desk.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return true;
        }

        private static SurfaceMaskLayer[] ConfigurePlayableEquipment(
            GameObject playableEquipment,
            StageDefinition definition,
            Material dustOverlayMaterial,
            Material polishOverlayMaterial)
        {
            var cleanRenderers = GetCleanableRenderers(playableEquipment);
            if (cleanRenderers.Length == 0)
            {
                throw new InvalidOperationException($"No supported MeshRenderer + MeshFilter was found in {definition.PrefabPath}.");
            }

            foreach (var collider in playableEquipment.GetComponentsInChildren<Collider>(true))
            {
                Undo.RecordObject(collider, "Disable broad equipment collider");
                collider.enabled = false;
            }

            var lodGroup = playableEquipment.GetComponentInChildren<LODGroup>(true);
            if (lodGroup != null)
            {
                Undo.RecordObject(lodGroup, "Disable equipment LOD switching");
                lodGroup.enabled = false;
            }

            var layers = new List<SurfaceMaskLayer>(cleanRenderers.Length * 2);
            for (var rendererIndex = 0; rendererIndex < cleanRenderers.Length; rendererIndex++)
            {
                var cleanRenderer = cleanRenderers[rendererIndex];
                var cleanMesh = cleanRenderer.GetComponent<MeshFilter>().sharedMesh;
                var materialCount = Mathf.Max(1, cleanMesh.subMeshCount);
                var cleaningMesh = EnsureRuntimeReadableCleaningMesh(definition, rendererIndex, cleanMesh);
                var dustMaterials = Enumerable.Repeat(dustOverlayMaterial, materialCount).ToArray();
                var polishMaterials = EnsurePolishMaterials(
                    definition,
                    rendererIndex,
                    materialCount,
                    cleanRenderer.sharedMaterials,
                    polishOverlayMaterial);
                var dustRenderer = CreateCleaningOverlay(
                    cleanRenderer.transform,
                    "DustOverlay",
                    cleaningMesh,
                    dustMaterials);
                var polishRenderer = CreateCleaningOverlay(
                    cleanRenderer.transform,
                    "PolishOverlay",
                    cleaningMesh,
                    polishMaterials);
                var cleaningCollider = cleanRenderer.GetComponent<MeshCollider>();
                if (cleaningCollider == null)
                {
                    cleaningCollider = Undo.AddComponent<MeshCollider>(cleanRenderer.gameObject);
                }

                Undo.RecordObject(cleaningCollider, "Match cleaning collider to visible equipment mesh");
                cleaningCollider.sharedMesh = cleanMesh;
                cleaningCollider.enabled = true;

                var dust = Undo.AddComponent<SurfaceMaskLayer>(cleanRenderer.gameObject);
                var polish = Undo.AddComponent<SurfaceMaskLayer>(cleanRenderer.gameObject);
                dust.Configure(dustRenderer, CleaningTool.AirGun, "_DustMask", 32, 256);
                polish.Configure(polishRenderer, CleaningTool.Cloth, "_PolishRemainingMask", 32, 256);
                EditorUtility.SetDirty(dust);
                EditorUtility.SetDirty(polish);
                layers.Add(dust);
                layers.Add(polish);
            }

            SetLayerRecursively(playableEquipment, StageInteractionController.CleanableLayer);
            return layers.ToArray();
        }

        private static Mesh EnsureRuntimeReadableCleaningMesh(
            StageDefinition definition,
            int rendererIndex,
            Mesh sourceMesh)
        {
            if (sourceMesh.isReadable)
            {
                return sourceMesh;
            }

            EnsureFolder("Assets/CleanToContinue", "Meshes");
            EnsureFolder("Assets/CleanToContinue/Meshes", "Generated");
            var stageKey = definition.PlayableName.Replace(" ", string.Empty).Replace("(Playable)", string.Empty);
            var path = $"Assets/CleanToContinue/Meshes/Generated/{stageKey}Cleaning_{rendererIndex}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            var copy = CreateReadableMeshCopy(sourceMesh, $"{stageKey}Cleaning_{rendererIndex}");
            if (existing == null || !existing.isReadable)
            {
                if (existing != null && !AssetDatabase.DeleteAsset(path))
                {
                    Object.DestroyImmediate(copy);
                    throw new InvalidOperationException($"Could not replace unreadable generated mesh: {path}");
                }

                AssetDatabase.CreateAsset(copy, path);
                return copy;
            }

            EditorUtility.CopySerialized(copy, existing);
            Object.DestroyImmediate(copy);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Mesh CreateReadableMeshCopy(Mesh source, string name)
        {
            var copy = new Mesh
            {
                name = name,
                indexFormat = source.indexFormat,
                vertices = source.vertices,
                normals = source.normals,
                tangents = source.tangents,
                colors32 = source.colors32,
                bounds = source.bounds,
                hideFlags = HideFlags.None
            };

            for (var channel = 0; channel < 8; channel++)
            {
                var uv = new List<Vector4>();
                source.GetUVs(channel, uv);
                if (uv.Count > 0)
                {
                    copy.SetUVs(channel, uv);
                }
            }

            copy.subMeshCount = source.subMeshCount;
            for (var subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                copy.SetIndices(
                    source.GetIndices(subMesh),
                    source.GetTopology(subMesh),
                    subMesh,
                    false,
                    checked((int)source.GetBaseVertex(subMesh)));
            }

            copy.UploadMeshData(false);
            return copy;
        }

        private static void PlaceDirectionalLightsAboveEquipment(
            GameObject playableEquipment,
            Light warmKeyLight,
            Light coolRimLight)
        {
            var bounds = EncapsulateRenderers(GetCleanableRenderers(playableEquipment));
            var radius = Mathf.Max(0.65f, bounds.extents.magnitude);
            warmKeyLight.transform.position = bounds.center + new Vector3(-radius * 0.8f, radius * 1.8f, -radius * 0.6f);
            coolRimLight.transform.position = bounds.center + new Vector3(radius * 0.9f, radius * 1.45f, radius * 0.5f);
            warmKeyLight.transform.rotation =
                Quaternion.LookRotation(bounds.center - warmKeyLight.transform.position, Vector3.up);
            coolRimLight.transform.rotation =
                Quaternion.LookRotation(bounds.center - coolRimLight.transform.position, Vector3.up);
        }

        private static MeshRenderer CreateCleaningOverlay(
            Transform sourceRenderer,
            string name,
            Mesh mesh,
            Material[] materials)
        {
            var overlay = CreateChild(sourceRenderer, name);
            var filter = Undo.AddComponent<MeshFilter>(overlay);
            filter.sharedMesh = mesh;
            var renderer = Undo.AddComponent<MeshRenderer>(overlay);
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            return renderer;
        }

        private static MeshRenderer[] GetCleanableRenderers(GameObject equipment)
        {
            var all = equipment.GetComponentsInChildren<MeshRenderer>(true)
                .Where(renderer =>
                    renderer.name != "DustOverlay" &&
                    renderer.name != "PolishOverlay" &&
                    renderer.GetComponent<MeshFilter>()?.sharedMesh != null)
                .ToArray();
            var lodGroup = equipment.GetComponentInChildren<LODGroup>(true);
            if (lodGroup == null || lodGroup.GetLODs().Length == 0)
            {
                var enabled = all.Where(renderer => renderer.enabled).ToArray();
                return enabled.Length > 0 ? enabled : all;
            }

            var firstLod = lodGroup.GetLODs()[0].renderers
                .OfType<MeshRenderer>()
                .Where(all.Contains)
                .ToArray();
            foreach (var renderer in all)
            {
                Undo.RecordObject(renderer, "Select highest equipment LOD");
                renderer.enabled = firstLod.Contains(renderer);
            }

            return firstLod.Length > 0 ? firstLod : all;
        }

        private static Bounds EncapsulateRenderers(Renderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0)
            {
                throw new InvalidOperationException("At least one equipment renderer is required for framing.");
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static Material[] EnsurePolishMaterials(
            StageDefinition definition,
            int rendererIndex,
            int materialCount,
            Material[] sourceMaterials,
            Material template)
        {
            EnsureFolder("Assets/CleanToContinue/Materials", "Generated");
            var stageKey = definition.PlayableName.Replace(" ", string.Empty).Replace("(Playable)", string.Empty);
            var results = new Material[materialCount];
            for (var materialIndex = 0; materialIndex < materialCount; materialIndex++)
            {
                var path = $"Assets/CleanToContinue/Materials/Generated/{stageKey}Polish_{rendererIndex}_{materialIndex}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(template.shader)
                    {
                        name = $"{stageKey}Polish_{rendererIndex}_{materialIndex}"
                    };
                    AssetDatabase.CreateAsset(material, path);
                }

                material.CopyPropertiesFromMaterial(template);
                var source = sourceMaterials != null && sourceMaterials.Length > 0
                    ? sourceMaterials[Mathf.Min(materialIndex, sourceMaterials.Length - 1)]
                    : null;
                if (source != null)
                {
                    SurfaceMaterialTransfer.CopyToCleanable(source, material);
                }

                EditorUtility.SetDirty(material);
                results[materialIndex] = material;
            }

            return results;
        }

        private static GameObject EnsureStageRootPrefab()
        {
            var temporary = new GameObject("StageRoot");
            try
            {
                temporary.AddComponent<StageController>();
                temporary.AddComponent<StageInputController>();
                temporary.AddComponent<StageInteractionController>();
                temporary.AddComponent<HighlightController>();
                temporary.AddComponent<CleaningAudioController>();
                temporary.AddComponent<EquipmentStageBootstrap>();
                var prefab = PrefabUtility.SaveAsPrefabAsset(temporary, StageRootPrefabPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException("Could not create StageRoot prefab.");
                }

                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(temporary);
            }
        }

        private static Material EnsureOverlayMaterial(
            string path,
            string materialName,
            float overlayMode)
        {
            var shader = Shader.Find("CleanToContinue/Cleaning Overlay");
            if (shader == null)
            {
                throw new InvalidOperationException("Cleaning Overlay shader is unavailable.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = materialName };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetFloat("_OverlayMode", overlayMode);
            material.SetColor("_DustColor", new Color(0.46f, 0.43f, 0.38f, 1f));
            material.SetFloat("_DustOpacity", 0.72f);
            material.SetFloat("_DullOpacity", 0.72f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject InstantiatePlayableEquipment(
            Scene scene,
            Transform parent,
            StageDefinition definition)
        {
            var authoringEquipment = scene.GetRootGameObjects()
                .FirstOrDefault(root => root.name == definition.AuthoringName);
            if (authoringEquipment == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(definition.PrefabPath);
                if (prefab == null && definition == StageDefinition.Mouse)
                {
                    prefab = EnsurePrototypeMousePrefab();
                }

                if (prefab == null)
                {
                    throw new InvalidOperationException($"Equipment prefab is missing: {definition.PrefabPath}");
                }

                authoringEquipment = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                SceneManager.MoveGameObjectToScene(authoringEquipment, scene);
                authoringEquipment.name = definition.AuthoringName;
                Undo.RegisterCreatedObjectUndo(authoringEquipment, $"Create {definition.AuthoringName} authoring object");
            }

            var instance = Object.Instantiate(authoringEquipment, parent, false);
            Undo.RegisterCreatedObjectUndo(instance, $"Instantiate {definition.AuthoringName} for play");
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.SetActive(true);

            Undo.RecordObject(authoringEquipment, $"Hide {definition.AuthoringName} authoring object");
            authoringEquipment.SetActive(false);
            EditorUtility.SetDirty(authoringEquipment);
            return instance;
        }

        private static void CopyMissingEnvironmentRoots(Scene targetScene)
        {
            if (targetScene.path == MouseScenePath)
            {
                return;
            }

            var sourceScene = SceneManager.GetSceneByPath(MouseScenePath);
            var sourceWasLoaded = sourceScene.IsValid() && sourceScene.isLoaded;
            if (!sourceWasLoaded)
            {
                sourceScene = EditorSceneManager.OpenScene(MouseScenePath, OpenSceneMode.Additive);
            }

            try
            {
                foreach (var rootName in new[] { "Desk", "Wall" })
                {
                    if (targetScene.GetRootGameObjects().Any(root => root.name == rootName))
                    {
                        continue;
                    }

                    var source = sourceScene.GetRootGameObjects().FirstOrDefault(root => root.name == rootName);
                    if (source == null)
                    {
                        continue;
                    }

                    var copy = Object.Instantiate(source);
                    copy.name = rootName;
                    SceneManager.MoveGameObjectToScene(copy, targetScene);
                    Undo.RegisterCreatedObjectUndo(copy, $"Copy {rootName} from mouse scene");
                }
            }
            finally
            {
                if (!sourceWasLoaded)
                {
                    EditorSceneManager.CloseScene(sourceScene, true);
                }
            }
        }

        private static GameObject InstantiateMouse(Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MousePrefabPath);
            if (prefab == null)
            {
                prefab = EnsurePrototypeMousePrefab();
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate mouse prefab");
            return instance;
        }

        private static GameObject EnsurePrototypeMousePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeMousePrefabPath);
            if (existing != null)
            {
                return existing;
            }

            var temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            try
            {
                temporary.name = "PrototypeMouse";
                temporary.transform.localScale = new Vector3(1.25f, 0.55f, 1.65f);
                var sphereCollider = temporary.GetComponent<SphereCollider>();
                Object.DestroyImmediate(sphereCollider);
                var filter = temporary.GetComponent<MeshFilter>();
                var meshCollider = temporary.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = filter.sharedMesh;
                return PrefabUtility.SaveAsPrefabAsset(temporary, PrototypeMousePrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(temporary);
            }
        }

        private static Camera CreateCamera(Transform parent, string name, Vector3 position, Vector3 target, Color background)
        {
            var cameraObject = CreateChild(parent, name);
            cameraObject.transform.localPosition = position;
            cameraObject.transform.rotation = Quaternion.LookRotation(target - position, Vector3.up);
            var camera = Undo.AddComponent<Camera>(cameraObject);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.05f;
            camera.depth = 10f;
            Undo.AddComponent<AudioListener>(cameraObject);
            return camera;
        }

        private static void CreateLight(Transform parent, string name, Vector3 euler, float intensity)
        {
            CreateDirectionalLight(parent, name, euler, new Color(1f, 0.92f, 0.82f, 1f), intensity, true);
        }

        private static Light CreateDirectionalLight(
            Transform parent,
            string name,
            Vector3 euler,
            Color color,
            float intensity,
            bool castShadows)
        {
            var lightObject = CreateChild(parent, name);
            lightObject.transform.localRotation = Quaternion.Euler(euler);
            var light = Undo.AddComponent<Light>(lightObject);
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            return light;
        }

        private static void CreatePointLight(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            float intensity,
            float range)
        {
            var lightObject = CreateChild(parent, name);
            lightObject.transform.position = position;
            var light = Undo.AddComponent<Light>(lightObject);
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
        {
            var canvasObject = CreateUiChild(parent, name);
            var canvas = Undo.AddComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = Undo.AddComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Undo.AddComponent<GraphicRaycaster>(canvasObject);
            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            return CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero,
                new Color(0.055f, 0.075f, 0.11f, 0.98f), true);
        }

        private static Slider CreateSettingSlider(Transform parent, string name, string label, float y, float minimum, float maximum)
        {
            CreateText(parent, $"{name}Label", label, 28, FontStyle.Normal,
                new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(260f, 55f), new Vector2(0f, y), Color.white);
            var sliderRoot = CreateImage(parent, name,
                new Vector2(0.67f, 0.5f), new Vector2(0.67f, 0.5f), new Vector2(430f, 24f), new Vector2(0f, y),
                new Color(0.18f, 0.22f, 0.3f, 1f), true);
            var fillObject = CreateImage(sliderRoot.transform, "Fill", Vector2.zero, new Vector2(0.85f, 1f), Vector2.zero, Vector2.zero,
                new Color(0.92f, 0.7f, 0.28f, 1f), false);
            var handleObject = CreateImage(sliderRoot.transform, "Handle",
                new Vector2(0.85f, 0.5f), new Vector2(0.85f, 0.5f), new Vector2(32f, 42f), Vector2.zero, Color.white, true);
            var fill = fillObject.GetComponent<Image>();
            var handle = handleObject.GetComponent<Image>();
            var slider = Undo.AddComponent<Slider>(sliderRoot);
            slider.minValue = minimum;
            slider.maxValue = maximum;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            return slider;
        }

        private static ToolSelectorView.ToolButtonBinding CreateToolButton(
            Transform parent,
            string name,
            string label,
            CleaningTool tool,
            float y)
        {
            var button = CreateButton(parent, name, label, new Vector2(0f, y), new Vector2(290f, 240f));
            var outline = Undo.AddComponent<Outline>(button.gameObject);
            outline.enabled = false;
            var labelText = button.GetComponentInChildren<Text>();
            labelText.fontSize = 34;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 18;
            labelText.resizeTextMaxSize = 34;
            labelText.horizontalOverflow = HorizontalWrapMode.Wrap;
            labelText.verticalOverflow = VerticalWrapMode.Truncate;
            labelText.alignment = TextAnchor.MiddleCenter;
            SetRect(labelText.rectTransform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(270f, 58f), new Vector2(0f, 9f), new Vector2(0.5f, 0f));
            var fillObject = CreateImage(button.transform, "ToolProgress",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.92f, 0.7f, 0.28f, 0.3f), false);
            var fill = fillObject.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            var imageSlotName = tool == CleaningTool.AirGun ? "AirGunImageSlot" : "ClothImageSlot";
            var imageSlot = CreateImage(button.transform, imageSlotName,
                new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f),
                new Vector2(160f, 160f), Vector2.zero,
                new Color(0.055f, 0.04f, 0.035f, 0.72f), false);
            imageSlot.GetComponent<Image>().preserveAspect = true;
            CreateToolIcon(imageSlot.transform, tool);
            labelText.transform.SetAsLastSibling();
            var check = CreateText(button.transform, "CheckMark", "✓", 34, FontStyle.Bold,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(42f, 42f), new Vector2(-4f, -4f),
                new Color(0.55f, 1f, 0.65f, 1f), new Vector2(1f, 1f));
            check.gameObject.SetActive(false);
            return new ToolSelectorView.ToolButtonBinding
            {
                Tool = tool,
                Button = button,
                Root = button.GetComponent<RectTransform>(),
                SelectionBorder = outline,
                AccessibleLabel = labelText,
                ProgressFill = fill,
                CheckMark = check.gameObject
            };
        }

        private static void CreateToolIcon(Transform parent, CleaningTool tool)
        {
            var iconRoot = CreateUiChild(parent, "ToolIcon");
            SetRect(iconRoot.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f),
                new Vector2(74f, 48f), Vector2.zero, new Vector2(0.5f, 0.5f));
            var ivory = new Color(0.96f, 0.87f, 0.68f, 1f);
            var accent = new Color(0.78f, 0.47f, 0.22f, 1f);

            if (tool == CleaningTool.AirGun)
            {
                CreateIconPart(iconRoot.transform, "Body", new Vector2(-5f, -2f), new Vector2(42f, 24f), ivory, 0f);
                CreateIconPart(iconRoot.transform, "Nozzle", new Vector2(27f, 4f), new Vector2(27f, 8f), accent, 0f);
                CreateIconPart(iconRoot.transform, "Grip", new Vector2(-8f, -20f), new Vector2(13f, 25f), ivory, -13f);
            }
            else if (tool == CleaningTool.CottonSwab)
            {
                CreateIconPart(iconRoot.transform, "Stem", Vector2.zero, new Vector2(62f, 5f), accent, -24f);
                CreateIconPart(iconRoot.transform, "TipA", new Vector2(-29f, 13f), new Vector2(16f, 11f), ivory, -24f);
                CreateIconPart(iconRoot.transform, "TipB", new Vector2(29f, -13f), new Vector2(16f, 11f), ivory, -24f);
            }
            else
            {
                CreateIconPart(iconRoot.transform, "Cloth", Vector2.zero, new Vector2(49f, 39f), ivory, 9f);
                CreateIconPart(iconRoot.transform, "Fold", new Vector2(10f, 5f), new Vector2(22f, 4f), accent, -14f);
            }
        }

        private static void CreateIconPart(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color,
            float rotation)
        {
            var part = CreateImage(parent, name,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                size, position, color, false);
            part.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var buttonObject = CreateImage(parent, name,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position,
                new Color(0.13f, 0.2f, 0.3f, 0.98f), true);
            var button = Undo.AddComponent<Button>(buttonObject);
            button.targetGraphic = buttonObject.GetComponent<Image>();
            CreateText(buttonObject.transform, "Label", label, 28, FontStyle.Bold,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            return button;
        }

        private static Button CreateButtonTopRight(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var buttonObject = CreateImage(parent, name, Vector2.one, Vector2.one, size, position,
                new Color(0.08f, 0.1f, 0.14f, 0.9f), true, Vector2.one);
            var button = Undo.AddComponent<Button>(buttonObject);
            button.targetGraphic = buttonObject.GetComponent<Image>();
            CreateText(buttonObject.transform, "Label", label, 24, FontStyle.Bold,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            return button;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string text,
            int fontSize,
            FontStyle style,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color color,
            Vector2? pivot = null)
        {
            var textObject = CreateUiChild(parent, name);
            SetRect(textObject.GetComponent<RectTransform>(), anchorMin, anchorMax, size, position, pivot ?? new Vector2(0.5f, 0.5f));
            var label = Undo.AddComponent<Text>(textObject);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        private static GameObject CreateImage(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color color,
            bool raycastTarget,
            Vector2? pivot = null)
        {
            var imageObject = CreateUiChild(parent, name);
            SetRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, size, position, pivot ?? new Vector2(0.5f, 0.5f));
            var image = Undo.AddComponent<Image>(imageObject);
            image.color = color;
            image.raycastTarget = raycastTarget;
            if (size != Vector2.zero)
            {
                var roundedSprite = GetRoundedSprite();
                if (roundedSprite != null)
                {
                    image.sprite = roundedSprite;
                    image.type = Image.Type.Sliced;
                }
            }
            return imageObject;
        }

        private static Sprite GetRoundedSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd")
                ?? AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static Sprite GetCircleSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            if (anchorMin != anchorMax && size == Vector2.zero)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        private static void EnsureUiInputModule(Scene scene)
        {
            var eventSystem = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EventSystem>(true))
                .FirstOrDefault();
            if (eventSystem == null || eventSystem.GetComponent<BaseInputModule>() != null)
            {
                return;
            }

            var inputModuleType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule"))
                .FirstOrDefault(type => type != null);
            if (inputModuleType != null)
            {
                Undo.AddComponent(eventSystem.gameObject, inputModuleType);
            }
        }

        private static GameObject CreateSceneObject(Scene scene, string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(child, $"Create {name}");
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject CreateUiChild(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(child, $"Create {name}");
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
