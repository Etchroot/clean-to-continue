using System.Collections;
using System.Linq;
using CleanToContinue.Core;
using CleanToContinue.Audio;
using CleanToContinue.Flow;
using CleanToContinue.Input;
using CleanToContinue.Progress;
using CleanToContinue.Stage;
using CleanToContinue.Surface;
using CleanToContinue.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class VerticalSliceSceneTests
    {
        [UnityTest]
        public IEnumerator MainMenuLoadsWithEventSystem()
        {
            yield return AssertSceneLoadsWithEventSystem("01.MainMenu");
        }

        [UnityTest]
        public IEnumerator OpeningLoadsWithEventSystem()
        {
            yield return AssertSceneLoadsWithEventSystem("02.Opening");
        }

        [UnityTest]
        public IEnumerator MouseLoadsWithEventSystem()
        {
            yield return AssertSceneLoadsWithEventSystem("03.Mouse");
        }

        [UnityTest]
        public IEnumerator KeyboardLoadsWithEventSystem()
        {
            yield return AssertSceneLoadsWithEventSystem("04.Keyboard");
        }

        [UnityTest]
        public IEnumerator HeadsetLoadsWithEventSystem()
        {
            yield return AssertSceneLoadsWithEventSystem("05.Headset");
        }

        [UnityTest]
        public IEnumerator EveryVisibleUiButtonAcrossTheGameHasClickAudio()
        {
            foreach (var sceneName in new[]
                     {
                         "01.MainMenu", "02.Opening", "03.Mouse",
                         "04.Keyboard", "05.Headset", "06.Ending"
                     })
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                var visibleButtons = Object.FindObjectsByType<Button>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Where(button => button.name != "IntroVideoScreen")
                    .ToArray();
                Assert.That(visibleButtons, Is.Not.Empty, sceneName);
                foreach (var button in visibleButtons)
                {
                    Assert.That(
                        button.GetComponent("CleanToContinue.UI.UiButtonClickSound"),
                        Is.Not.Null,
                        $"{sceneName}/{button.name}");
                }
            }
        }

        [UnityTest]
        public IEnumerator EverySceneHasExactlyOneActiveAudioListener()
        {
            var invalidScenes = new System.Collections.Generic.List<string>();
            foreach (var sceneName in new[]
                     {
                         "01.MainMenu", "02.Opening", "03.Mouse",
                         "04.Keyboard", "05.Headset", "06.Ending"
                     })
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                var listeners = Object.FindObjectsByType<AudioListener>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None)
                    .Count(listener => listener.enabled);
                if (listeners != 1)
                {
                    invalidScenes.Add($"{sceneName}: {listeners}");
                }
            }

            Assert.That(invalidScenes, Is.Empty,
                "each scene needs exactly one active listener for audible BGM and button effects");
        }

        [UnityTest]
        public IEnumerator EveryVisibleUiButtonTurnsOpaqueBlackOnHover()
        {
            foreach (var sceneName in new[]
                     {
                         "01.MainMenu", "02.Opening", "03.Mouse",
                         "04.Keyboard", "05.Headset", "06.Ending"
                     })
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                if (sceneName == "01.MainMenu")
                {
                    Object.FindFirstObjectByType<IntroVideoController>(FindObjectsInactive.Include).CompleteIntro();
                    yield return null;
                }

                var visibleButtons = Object.FindObjectsByType<Button>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Where(button => button.name != "IntroVideoScreen" && button.gameObject.activeInHierarchy)
                    .ToArray();
                Assert.That(visibleButtons, Is.Not.Empty, sceneName);
                foreach (var button in visibleButtons)
                {
                    Assert.That(
                        button.GetComponent("CleanToContinue.UI.UiButtonHoverBackground"),
                        Is.Not.Null,
                        $"{sceneName}/{button.name}");

                    var background = button.targetGraphic;
                    var originalColor = background.color;
                    var pointer = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                    yield return new WaitForSecondsRealtime(0.12f);
                    Assert.That(background.color, Is.EqualTo(new Color(0f, 0f, 0f, 1f)),
                        $"{sceneName}/{button.name}");
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                    yield return new WaitForSecondsRealtime(0.12f);
                    Assert.That(background.color, Is.EqualTo(originalColor),
                        $"{sceneName}/{button.name}");
                }
            }
        }

        [UnityTest]
        public IEnumerator EquipmentStagesUseTheirDeclaredAssetsAndFlow()
        {
            yield return AssertEquipmentStage(
                "03.Mouse",
                "Mouse (Playable)",
                "04.Keyboard",
                "처음 오락기 앞에 섰던 날, 바라보는 것만으로도 새로운 세계가 열렸다.");
            yield return AssertEquipmentStage(
                "04.Keyboard",
                "Keyboard (Playable)",
                "05.Headset",
                "처음 친구와 나란히 앉아, 같은 게임 속을 함께 달렸다.");
            yield return AssertEquipmentStage(
                "05.Headset",
                "Headset (Playable)",
                "06.Ending",
                "PC방을 가득 채운 우리의 환호가, 아직도 귓가에 선명하다.");
        }

        [UnityTest]
        public IEnumerator MainMenuProvidesStartSettingsAndCredits()
        {
            yield return SceneManager.LoadSceneAsync("01.MainMenu", LoadSceneMode.Single);

            var buttonNames = Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Select(button => button.name)
                .ToArray();

            Assert.That(buttonNames, Does.Contain("StartButton"));
            Assert.That(buttonNames, Does.Contain("SettingsButton"));
            Assert.That(buttonNames, Does.Contain("CreditsButton"));
        }

        [UnityTest]
        public IEnumerator OpeningProvidesSkipAndTimedTransition()
        {
            yield return SceneManager.LoadSceneAsync("02.Opening", LoadSceneMode.Single);

            var skip = Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(button => button.name == "SkipButton");
            var timedTransition = Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(component => component.GetType().Name == "OpeningSequence");

            Assert.That(skip, Is.Not.Null);
            Assert.That(timedTransition, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MainMenuUsesFinalIntroMediaMusicAndCredits()
        {
            yield return SceneManager.LoadSceneAsync("01.MainMenu", LoadSceneMode.Single);

            var images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var menuBackground = images.Single(image => image.name == "MenuBackground");
            var credits = texts.Single(text => text.name == "CreditsBody");

            Assert.That(menuBackground.sprite.name, Is.EqualTo("intro img"));
            Assert.That(texts.Where(text => text.text.Any(character => character >= 0xAC00 && character <= 0xD7A3))
                .All(text => text.font != null && text.font.HasCharacter('한')), Is.True, "all Korean UI needs an embedded Hangul font");
            Assert.That(Object.FindFirstObjectByType<IntroVideoController>(FindObjectsInactive.Include), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<VideoPlayer>(FindObjectsInactive.Include), Is.Not.Null);
            var musicPlayer = Object.FindFirstObjectByType<PersistentMusicPlayer>(FindObjectsInactive.Include);
            Assert.That(musicPlayer, Is.Not.Null);
            Assert.That(SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PersistentMusicPlayer>(true))
                .ToArray(), Has.Length.EqualTo(1),
                "the authoring scene must contain exactly one persistent music source");
            Assert.That(musicPlayer.transform.parent, Is.Null, "DontDestroyOnLoad music must be a scene root");
            Assert.That(Object.FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(4));
            Assert.That(credits.text, Is.EqualTo(
                "Creator : 차명근\n" +
                "AI Agent : Codex\n" +
                "Engine : Unity\n" +
                "Asset : Unity Asset Store\n" +
                "Title Video & Image : Nanobanana\n" +
                "Album Image : GPT\n" +
                "Sound : Suno"));
        }

        [UnityTest]
        public IEnumerator StartButtonBeginsMusicBeforeOpeningSceneLoads()
        {
            yield return SceneManager.LoadSceneAsync("01.MainMenu", LoadSceneMode.Single);
            yield return null;

            var start = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(button => button.name == "StartButton");
            var music = Object.FindFirstObjectByType<PersistentMusicPlayer>(FindObjectsInactive.Include);
            start.onClick.Invoke();

            Assert.That(music.Source.isPlaying, Is.True,
                "the browser gesture frame must start music before changing scenes");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("01.MainMenu"),
                "the opening scene must wait one frame so WebGL can commit AudioSource.Play");

            music.Source.Stop();
            yield return WaitForScene("02.Opening", 1.5f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("02.Opening"));
            Assert.That(PersistentMusicPlayer.Instance.Source.isPlaying, Is.True,
                "a requested music session must resume if WebGL interrupts the source during scene loading");
        }

        [UnityTest]
        public IEnumerator OpeningIsBlackTextOnlyAndCumulative()
        {
            yield return SceneManager.LoadSceneAsync("02.Opening", LoadSceneMode.Single);

            Assert.That(GameObject.Find("OpeningMouse"), Is.Null);
            var background = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(image => image.name == "OpeningBackground");
            var line = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(text => text.name == "OpeningLine");
            Assert.That(background.color, Is.EqualTo(Color.black));
            Assert.That(line.color, Is.EqualTo(Color.white));
            Assert.That(Object.FindFirstObjectByType<OpeningSequence>(FindObjectsInactive.Include), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator EndingUsesFinalImageAndReturnsToMainMenu()
        {
            yield return SceneManager.LoadSceneAsync("06.Ending", LoadSceneMode.Single);

            var background = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(image => image.name == "EndingBackground");
            var thanks = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(text => text.name == "EndingThanks");
            var ending = Object.FindFirstObjectByType<EndingView>(FindObjectsInactive.Include);

            Assert.That(background.sprite.name, Is.EqualTo("end img"));
            Assert.That(thanks.text, Is.EqualTo("플레이 해주셔서 감사합니다"));
            Assert.That(ending, Is.Not.Null);
            Assert.That(ending.DestinationScene, Is.EqualTo("01.MainMenu"));
            Assert.That(Object.FindFirstObjectByType<EventSystem>().GetComponent<BaseInputModule>(), Is.Not.Null,
                "the ending button needs an input module to receive real browser clicks");
        }

        [UnityTest]
        public IEnumerator EndingRestartButtonReturnsToTheFirstScene()
        {
            yield return SceneManager.LoadSceneAsync("06.Ending", LoadSceneMode.Single);

            var restart = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(button => button.name == "RestartButton");
            restart.onClick.Invoke();
            yield return WaitForScene("01.MainMenu", 1.5f);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("01.MainMenu"));
        }

        [UnityTest]
        public IEnumerator RegularSceneTransitionWaitsForFadeOut()
        {
            yield return SceneManager.LoadSceneAsync("04.Keyboard", LoadSceneMode.Single);
            yield return new WaitForSecondsRealtime(0.5f);

            SceneFlow.Load("05.Headset");
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("04.Keyboard"));
            yield return WaitForScene("05.Headset", 1f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("05.Headset"));
        }

        [UnityTest]
        public IEnumerator EndingTransitionUsesTwiceTheNormalFadeDuration()
        {
            yield return SceneManager.LoadSceneAsync("05.Headset", LoadSceneMode.Single);
            yield return new WaitForSecondsRealtime(0.5f);

            SceneFlow.Load("06.Ending");
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("05.Headset"));
            yield return WaitForScene("06.Ending", 1f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("06.Ending"));
        }

        [UnityTest]
        public IEnumerator MouseSceneWiresTwoDistinctToolsAndCompletionUi()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var progressSources = Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OfType<IProgressSource>()
                .ToArray();

            Assert.That(Object.FindObjectsByType<StageController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(progressSources, Has.Length.EqualTo(2));
            Assert.That(progressSources.Select(source => source.Tool).Distinct(), Is.EquivalentTo(new[]
            {
                CleaningTool.AirGun,
                CleaningTool.Cloth
            }));
            Assert.That(GameObject.Find("CottonSwabButton"), Is.Null);
            Assert.That(GameObject.Find("GapDirtGroup"), Is.Null);
            Assert.That(Object.FindFirstObjectByType<EquipmentRotator>(FindObjectsInactive.Include), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CleaningCursorView>(FindObjectsInactive.Include), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<MemoryPanelView>(FindObjectsInactive.Include), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MouseUsesVisibleLodMeshForCleaningCollider()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var playableMouse = GameObject.Find("Mouse (Playable)");
            Assert.That(playableMouse, Is.Not.Null);
            var visibleFilter = playableMouse.GetComponentsInChildren<MeshFilter>(true)
                .Single(filter => filter.name == "mouse_LOD0");
            var cleaningCollider = visibleFilter.GetComponent<MeshCollider>();

            Assert.That(cleaningCollider, Is.Not.Null);
            Assert.That(cleaningCollider.sharedMesh, Is.SameAs(visibleFilter.sharedMesh));
        }

        [UnityTest]
        public IEnumerator PlayableMouseUsesTheExactOriginalMaterialUnderCleaningOverlays()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var referenceMouse = Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Single(transform => transform.parent == null && transform.name == "Mouse")
                .gameObject;
            var playableMouse = GameObject.Find("Mouse (Playable)");
            Assert.That(referenceMouse, Is.Not.Null);
            Assert.That(playableMouse, Is.Not.Null);
            Assert.That(referenceMouse.activeSelf, Is.False);

            var referenceMaterial = referenceMouse.GetComponentsInChildren<MeshRenderer>(true)
                .Single(renderer => renderer.name == "mouse_LOD0")
                .sharedMaterial;
            var playableMaterial = playableMouse.GetComponentsInChildren<MeshRenderer>(true)
                .Single(renderer => renderer.name == "mouse_LOD0")
                .sharedMaterial;

            Assert.That(playableMaterial, Is.SameAs(referenceMaterial));
            Assert.That(playableMouse.transform.Find("mouse_LOD0/DustOverlay"), Is.Not.Null);
            var polishOverlay = playableMouse.transform.Find("mouse_LOD0/PolishOverlay")
                .GetComponent<MeshRenderer>();
            Assert.That(polishOverlay, Is.Not.Null);
            Assert.That(
                polishOverlay.sharedMaterial.GetTexture("_BaseMap"),
                Is.SameAs(ReadBaseMap(referenceMaterial)));
        }

        [UnityTest]
        public IEnumerator StageCameraLeavesContextAroundThePlayableMouse()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var playableMouse = GameObject.Find("Mouse (Playable)");
            var stageCamera = GameObject.Find("StageCamera").GetComponent<Camera>();
            var mouseBounds = playableMouse.GetComponentsInChildren<MeshRenderer>(true)
                .Single(renderer => renderer.name == "mouse_LOD0")
                .bounds;
            var radius = Mathf.Max(0.65f, mouseBounds.extents.magnitude);

            Assert.That(
                Vector3.Distance(stageCamera.transform.position, mouseBounds.center),
                Is.InRange(radius * 3.8f, radius * 4.2f));
        }

        [UnityTest]
        public IEnumerator MouseRotationPivotClearsTheDeskAtEveryAngle()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var playableMouse = GameObject.Find("Mouse (Playable)");
            var equipment = GameObject.Find("MouseEquipment").transform;
            var mouseBounds = playableMouse.GetComponentsInChildren<MeshRenderer>(true)
                .Single(renderer => renderer.name == "mouse_LOD0")
                .bounds;
            var deskBounds = EncapsulateRenderers(GameObject.Find("Desk"));
            var rotationRadius = mouseBounds.extents.magnitude;

            Assert.That(
                equipment.position.y - deskBounds.max.y,
                Is.GreaterThanOrEqualTo(rotationRadius));
        }

        [UnityTest]
        public IEnumerator MouseStageWallUsesItsOwnWarmMatteMaterial()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var wall = GameObject.Find("Wall");
            var material = wall.GetComponent<MeshRenderer>().sharedMaterial;

            Assert.That(material, Is.Not.Null);
            Assert.That(material.name, Is.EqualTo("WallCozy"));
            Assert.That(material.color.r, Is.GreaterThan(material.color.b), "wall should remain warm rather than blue");
            Assert.That(material.color.g, Is.GreaterThanOrEqualTo(material.color.b), "wall should remain a warm neutral");
            Assert.That(material.GetFloat("_Metallic"), Is.EqualTo(0f).Within(0.001f));
            Assert.That(material.GetFloat("_Smoothness"), Is.LessThanOrEqualTo(0.15f));
        }

        [UnityTest]
        public IEnumerator MouseStageUsesRoundedReadableReplaceableUi()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var restorationTitle = GameObject.Find("RestorationTitle").GetComponent<Text>();
            var percent = GameObject.Find("PercentText").GetComponent<Text>();
            var airButton = GameObject.Find("AirGunButton");
            var clothButton = GameObject.Find("ClothButton");
            var progressFill = GameObject.Find("ProgressFill").GetComponent<Image>();

            Assert.That(restorationTitle.text, Is.EqualTo("복원도"));
            Assert.That(restorationTitle.fontSize, Is.GreaterThanOrEqualTo(48));
            Assert.That(restorationTitle.fontStyle, Is.EqualTo(FontStyle.Normal));
            Assert.That(restorationTitle.verticalOverflow, Is.EqualTo(VerticalWrapMode.Overflow));
            Assert.That(percent.fontSize, Is.GreaterThanOrEqualTo(84));
            Assert.That(percent.fontStyle, Is.EqualTo(FontStyle.Normal));
            Assert.That(percent.verticalOverflow, Is.EqualTo(VerticalWrapMode.Overflow));
            Assert.That(GameObject.Find("InputHint"), Is.Null);
            Assert.That(
                airButton.transform.Find("Label").GetComponent<Text>().text,
                Is.EqualTo("에어건 (먼지 제거)"));
            Assert.That(
                clothButton.transform.Find("Label").GetComponent<Text>().text,
                Is.EqualTo("헝겊 (광택 내기)"));
            AssertBestFitLabel(airButton);
            AssertBestFitLabel(clothButton);
            Assert.That(airButton.transform.Find("AirGunImageSlot")?.GetComponent<Image>(), Is.Not.Null);
            Assert.That(clothButton.transform.Find("ClothImageSlot")?.GetComponent<Image>(), Is.Not.Null);

            AssertRoundedBackground(GameObject.Find("ProgressPanel"));
            AssertRoundedBackground(GameObject.Find("ToolPanel"));
            AssertRoundedBackground(GameObject.Find("InstructionPanel"));
            AssertRoundedBackground(airButton);
            AssertRoundedBackground(clothButton);
            Assert.That(progressFill.sprite, Is.Not.Null);
            Assert.That(progressFill.type, Is.EqualTo(Image.Type.Filled));
            Assert.That(progressFill.fillMethod, Is.EqualTo(Image.FillMethod.Radial360));
        }

        [UnityTest]
        public IEnumerator MouseStageShowsTheConfirmedControlsOnTheLeft()
        {
            yield return SceneManager.LoadSceneAsync("03.Mouse", LoadSceneMode.Single);

            var panel = GameObject.Find("InstructionPanel");
            var title = GameObject.Find("InstructionTitle").GetComponent<Text>();
            var body = GameObject.Find("InstructionBody").GetComponent<Text>();

            Assert.That(panel.GetComponent<RectTransform>().anchorMin.x, Is.EqualTo(0f));
            Assert.That(title.text, Is.EqualTo("조작 방법"));
            Assert.That(title.fontSize, Is.GreaterThanOrEqualTo(44));
            Assert.That(title.fontStyle, Is.EqualTo(FontStyle.Normal));
            Assert.That(title.verticalOverflow, Is.EqualTo(VerticalWrapMode.Overflow));
            Assert.That(body.text, Is.EqualTo(
                "마우스 좌클릭 - 청소\n" +
                "마우스 우클릭 - 회전\n" +
                "스페이스 바 - 오염 부분 확인"));
            Assert.That(body.fontSize, Is.GreaterThanOrEqualTo(34));
            Assert.That(body.alignment, Is.EqualTo(TextAnchor.MiddleLeft));
            Assert.That(panel.GetComponent<RectTransform>().sizeDelta.x, Is.LessThanOrEqualTo(480f));
            Assert.That(body.rectTransform.sizeDelta.x, Is.LessThanOrEqualTo(430f));
        }

        [Test]
        public void ClothPrototypeUsesClearlySeparatedSqueakBursts()
        {
            var clips = PrototypeAudioFactory.Create(12345);
            try
            {
                Assert.That(clips.Cloth.name, Does.Contain("Squeak"));
                var samples = new float[clips.Cloth.samples];
                Assert.That(clips.Cloth.GetData(samples, 0), Is.True);

                const int windowSize = PrototypeAudioFactory.SampleRate / 20;
                var windowCount = samples.Length / windowSize;
                var rms = new float[windowCount];
                for (var window = 0; window < windowCount; window++)
                {
                    var sum = 0f;
                    for (var sample = 0; sample < windowSize; sample++)
                    {
                        var value = samples[window * windowSize + sample];
                        sum += value * value;
                    }

                    rms[window] = Mathf.Sqrt(sum / windowSize);
                }

                Assert.That(rms.Max(), Is.GreaterThan(rms.Min() * 3f));
                Assert.That(rms.Count(value => value > rms.Max() * 0.55f), Is.InRange(3, 10));
            }
            finally
            {
                Object.DestroyImmediate(clips.AirGun);
                Object.DestroyImmediate(clips.Cloth);
                Object.DestroyImmediate(clips.Completion);
            }
        }

        [Test]
        public void CompletionPrototypeIsAThreeNoteBell()
        {
            var clips = PrototypeAudioFactory.Create(12345);
            try
            {
                Assert.That(clips.Completion.name, Does.Contain("Three Note Bell"));
                Assert.That(clips.Completion.length, Is.InRange(0.6f, 0.9f));

                var samples = new float[clips.Completion.samples];
                Assert.That(clips.Completion.GetData(samples, 0), Is.True);
                foreach (var onset in new[] { 0f, 0.18f, 0.38f })
                {
                    var start = Mathf.RoundToInt(onset * PrototypeAudioFactory.SampleRate);
                    var count = Mathf.RoundToInt(0.045f * PrototypeAudioFactory.SampleRate);
                    var energy = 0f;
                    for (var index = start; index < start + count; index++)
                    {
                        energy += samples[index] * samples[index];
                    }

                    Assert.That(Mathf.Sqrt(energy / count), Is.GreaterThan(0.04f), $"onset {onset}");
                }
            }
            finally
            {
                Object.DestroyImmediate(clips.AirGun);
                Object.DestroyImmediate(clips.Cloth);
                Object.DestroyImmediate(clips.Completion);
            }
        }

        [UnityTest]
        public IEnumerator FinalStageImagesAndRoundedUiAreAssigned()
        {
            var stages = new[]
            {
                (Scene: "03.Mouse", Album: "album1"),
                (Scene: "04.Keyboard", Album: "album2"),
                (Scene: "05.Headset", Album: "album3")
            };

            foreach (var stage in stages)
            {
                yield return SceneManager.LoadSceneAsync(stage.Scene, LoadSceneMode.Single);

                Assert.That(GameObject.Find("AirGunImageSlot").GetComponent<Image>().sprite.name, Is.EqualTo("airgun"));
                Assert.That(GameObject.Find("ClothImageSlot").GetComponent<Image>().sprite.name, Is.EqualTo("rag"));
                foreach (var imageName in new[] { "AirGunImageSlot", "ClothImageSlot" })
                {
                    var toolImage = GameObject.Find(imageName).GetComponent<Image>();
                    Assert.That(toolImage.type, Is.EqualTo(Image.Type.Simple), $"{stage.Scene}/{imageName}");
                    Assert.That(toolImage.preserveAspect, Is.True, $"{stage.Scene}/{imageName}");
                    Assert.That(toolImage.rectTransform.sizeDelta.x,
                        Is.EqualTo(toolImage.rectTransform.sizeDelta.y).Within(0.01f), $"{stage.Scene}/{imageName}");
                }
                var memoryImage = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Single(image => image.name == "MemoryImage");
                Assert.That(memoryImage.sprite.name, Is.EqualTo(stage.Album));

                foreach (var name in new[]
                         {
                             "ProgressPanel", "ToolPanel", "InstructionPanel", "AirGunButton", "ClothButton",
                             "NextStageButton", "MainMenuButton"
                         })
                {
                    var image = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .Single(value => value.name == name);
                    Assert.That(image.sprite.name, Is.EqualTo("RoundedRect"), $"{stage.Scene}/{name}");
                    Assert.That(image.type, Is.EqualTo(Image.Type.Sliced), $"{stage.Scene}/{name}");
                }

                foreach (var button in Object.FindObjectsByType<Button>(
                             FindObjectsInactive.Include,
                             FindObjectsSortMode.None))
                {
                    Assert.That(
                        button.GetComponent("CleanToContinue.UI.UiButtonClickSound"),
                        Is.Not.Null,
                        $"{stage.Scene}/{button.name}");
                }
            }
        }

        [UnityTest]
        public IEnumerator MemoryCaptionsSitBetweenAlbumAndButtonsAtReadableSize()
        {
            foreach (var sceneName in new[] { "03.Mouse", "04.Keyboard", "05.Headset" })
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                var line = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Single(value => value.name == "MemoryLine");

                Assert.That(line.rectTransform.anchorMin.y, Is.InRange(0.35f, 0.39f),
                    $"{sceneName}: caption should sit in the gap between the album and buttons");
                Assert.That(line.fontSize, Is.GreaterThanOrEqualTo(44),
                    $"{sceneName}: caption should be readable over the completion screen");
                Assert.That(line.rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(110f),
                    $"{sceneName}: enlarged Korean text needs enough vertical room");
            }
        }

        [UnityTest]
        public IEnumerator EndingThanksUsesOutlineAndShadowWithoutAButtonLikePanel()
        {
            yield return SceneManager.LoadSceneAsync("06.Ending", LoadSceneMode.Single);

            var thanks = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(value => value.name == "EndingThanks");
            var outline = thanks.GetComponent<Outline>();
            var shadows = thanks.GetComponents<Shadow>();

            Assert.That(outline, Is.Not.Null, "bright ending artwork needs a dark text outline");
            Assert.That(shadows.Any(value => value is not Outline), Is.True,
                "ending thanks needs a separate drop shadow");
            Assert.That(outline.effectColor.a, Is.GreaterThanOrEqualTo(0.8f));
            Assert.That(thanks.GetComponent<Image>(), Is.Null,
                "the thanks text should not gain a button-like background panel");
        }

        private static void AssertRoundedBackground(GameObject target)
        {
            var image = target.GetComponent<Image>();
            Assert.That(image.sprite, Is.Not.Null, target.name);
            Assert.That(image.type, Is.EqualTo(Image.Type.Sliced), target.name);
        }

        private static void AssertBestFitLabel(GameObject button)
        {
            var label = button.transform.Find("Label").GetComponent<Text>();
            Assert.That(label.resizeTextForBestFit, Is.True, button.name);
            Assert.That(label.resizeTextMinSize, Is.LessThanOrEqualTo(20), button.name);
            Assert.That(label.resizeTextMaxSize, Is.GreaterThanOrEqualTo(34), button.name);
        }

        private static Texture ReadBaseMap(Material material)
        {
            if (material.HasProperty("_BaseMap"))
            {
                return material.GetTexture("_BaseMap");
            }

            return material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        }

        private static Bounds EncapsulateRenderers(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static IEnumerator AssertSceneLoadsWithEventSystem(string expectedName)
        {
            yield return SceneManager.LoadSceneAsync(expectedName, LoadSceneMode.Single);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(expectedName));
            Assert.That(Object.FindFirstObjectByType<EventSystem>(), Is.Not.Null);
        }

        private static IEnumerator WaitForScene(string expectedName, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (SceneManager.GetActiveScene().name != expectedName && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }

        private static IEnumerator AssertEquipmentStage(
            string sceneName,
            string playableName,
            string nextScene,
            string memoryText)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            var playable = GameObject.Find(playableName);
            var bootstrap = Object.FindFirstObjectByType<EquipmentStageBootstrap>(FindObjectsInactive.Include);
            var memory = Object.FindFirstObjectByType<MemoryPanelView>(FindObjectsInactive.Include);
            var nextButton = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(button => button.name == "NextStageButton");
            var menuButton = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(button => button.name == "MainMenuButton");
            var memoryLine = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(text => text.name == "MemoryLine");
            Assert.That(playable, Is.Not.Null, sceneName);
            Assert.That(bootstrap, Is.Not.Null, sceneName);
            Assert.That(memory, Is.Not.Null, sceneName);
            var layers = playable.GetComponentsInChildren<SurfaceMaskLayer>(true);
            Assert.That(layers.Select(layer => layer.Tool).Distinct(),
                Is.EquivalentTo(new[] { CleaningTool.AirGun, CleaningTool.Cloth }), sceneName);
            Assert.That(memory.NextSceneName, Is.EqualTo(nextScene), sceneName);
            Assert.That(memoryLine.text, Is.EqualTo(memoryText), sceneName);
            Assert.That(nextButton.GetComponent<RectTransform>().anchoredPosition.y,
                Is.GreaterThan(menuButton.GetComponent<RectTransform>().anchoredPosition.y), sceneName);
            Assert.That(nextButton.GetComponentInChildren<Text>(true).text,
                Is.EqualTo(sceneName == "05.Headset" ? "청소 완료!" : "다음 단계 진행"), sceneName);

            var cleanableRenderers = playable.GetComponentsInChildren<MeshRenderer>(true)
                .Where(renderer =>
                    renderer.enabled &&
                    renderer.name != "DustOverlay" &&
                    renderer.name != "PolishOverlay" &&
                    renderer.GetComponent<MeshFilter>()?.sharedMesh != null)
                .ToArray();
            Assert.That(cleanableRenderers, Is.Not.Empty, sceneName);
            Assert.That(layers.Length, Is.EqualTo(cleanableRenderers.Length * 2), sceneName);
            foreach (var renderer in cleanableRenderers)
            {
                Assert.That(renderer.transform.Find("DustOverlay"), Is.Not.Null, $"{sceneName}/{renderer.name}");
                Assert.That(renderer.transform.Find("PolishOverlay"), Is.Not.Null, $"{sceneName}/{renderer.name}");
                Assert.That(renderer.GetComponent<MeshCollider>()?.sharedMesh,
                    Is.SameAs(renderer.GetComponent<MeshFilter>().sharedMesh), $"{sceneName}/{renderer.name}");
            }

            if (sceneName == "04.Keyboard" || sceneName == "05.Headset")
            {
                var bounds = EncapsulateRenderers(playable);
                foreach (var lightName in new[] { "WarmKeyLight", "CoolRimLight" })
                {
                    var light = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .Single(value => value.name == lightName);
                    Assert.That(light.transform.position.y, Is.GreaterThan(bounds.max.y), $"{sceneName}/{lightName}");
                    var directionToEquipment = (bounds.center - light.transform.position).normalized;
                    Assert.That(Vector3.Dot(light.transform.forward, directionToEquipment), Is.GreaterThan(0.999f),
                        $"{sceneName}/{lightName} must point toward the equipment");
                }
            }

            if (sceneName == "05.Headset")
            {
                foreach (var renderer in cleanableRenderers)
                {
                    Assert.That(renderer.transform.Find("DustOverlay").GetComponent<MeshFilter>().sharedMesh.isReadable,
                        Is.True, $"{sceneName}/{renderer.name}/DustOverlay");
                    Assert.That(renderer.transform.Find("PolishOverlay").GetComponent<MeshFilter>().sharedMesh.isReadable,
                        Is.True, $"{sceneName}/{renderer.name}/PolishOverlay");
                }
            }
        }
    }
}
