using System.Collections;
using System.Linq;
using System.Reflection;
using CleanToContinue.Audio;
using CleanToContinue.Flow;
using CleanToContinue.Stage;
using CleanToContinue.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class FinalMediaFlowTests
    {
        private AudioClip clip;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            PlayerPrefs.DeleteKey(StageController.MusicVolumeKey);
            var introFlag = typeof(IntroVideoController).GetField(
                "hasPlayedThisSession",
                BindingFlags.Static | BindingFlags.NonPublic);
            introFlag?.SetValue(null, false);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMenuStartsBackgroundMusicWhileIntroVideoIsStillVisible()
        {
            yield return SceneManager.LoadSceneAsync("01.MainMenu", LoadSceneMode.Single);
            yield return null;

            var introScreen = GameObject.Find("IntroVideoScreen");
            var music = Object.FindFirstObjectByType<PersistentMusicPlayer>(FindObjectsInactive.Include);

            Assert.That(introScreen, Is.Not.Null);
            Assert.That(introScreen.activeInHierarchy, Is.True);
            Assert.That(music, Is.Not.Null);
            Assert.That(music.Source.isPlaying, Is.True,
                "background music should be requested before the intro video completes");
        }

        [UnityTest]
        public IEnumerator VisibleMenuButtonPlaysTheSharedClickSound()
        {
            yield return SceneManager.LoadSceneAsync("01.MainMenu", LoadSceneMode.Single);
            yield return null;

            Object.FindFirstObjectByType<IntroVideoController>(FindObjectsInactive.Include).CompleteIntro();
            yield return null;

            var settings = Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Single(button => button.name == "SettingsButton");
            settings.onClick.Invoke();
            yield return null;

            var clickPlayer = GameObject.Find("__CleanToContinueUiClickAudio");
            Assert.That(clickPlayer, Is.Not.Null);
            Assert.That(clickPlayer.GetComponent<AudioSource>().isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator AnyAudibleButtonClickRestartsStoppedBackgroundMusic()
        {
            var musicObject = new GameObject("Persistent Music");
            var music = musicObject.AddComponent<PersistentMusicPlayer>();
            clip = AudioClip.Create("Test Music", 4410, 1, 44100, false);
            music.Configure(clip);
            music.StartMusic();
            music.Source.Stop();

            var buttonObject = new GameObject(
                "Test Button",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(UiButtonClickSound));
            var button = buttonObject.GetComponent<Button>();
            yield return null;

            button.onClick.Invoke();

            Assert.That(music.Source.isPlaying, Is.True,
                "a game-canvas button gesture should resume WebGL background music");

            Object.Destroy(buttonObject);
            Object.Destroy(musicObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMenuOpeningAndEndingKeepPersistentMusicAudible()
        {
            yield return SceneManager.LoadSceneAsync("01.MainMenu", LoadSceneMode.Single);
            yield return null;

            var music = Object.FindFirstObjectByType<PersistentMusicPlayer>(FindObjectsInactive.Include);
            Assert.That(Object.FindObjectsByType<AudioListener>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None), Has.Length.EqualTo(1), "MainMenu");
            Assert.That(music.Source.isPlaying, Is.True, "MainMenu BGM");

            yield return SceneManager.LoadSceneAsync("02.Opening", LoadSceneMode.Single);
            yield return null;

            Assert.That(Object.FindObjectsByType<AudioListener>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None), Has.Length.EqualTo(1), "Opening");
            Assert.That(PersistentMusicPlayer.Instance, Is.SameAs(music));
            Assert.That(music.Source.isPlaying, Is.True, "Opening BGM");

            yield return SceneManager.LoadSceneAsync("06.Ending", LoadSceneMode.Single);
            yield return null;

            Assert.That(Object.FindObjectsByType<AudioListener>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None), Has.Length.EqualTo(1), "Ending");
            Assert.That(PersistentMusicPlayer.Instance, Is.SameAs(music));
            Assert.That(music.Source.isPlaying, Is.True, "Ending BGM");
        }

        [UnityTest]
        public IEnumerator CompletingIntroRevealsMenuAndStartsLoopingMusic()
        {
            var musicObject = new GameObject("Persistent Music");
            var music = musicObject.AddComponent<PersistentMusicPlayer>();
            clip = AudioClip.Create("Test Music", 4410, 1, 44100, false);
            music.Configure(clip);

            var root = new GameObject("Intro Root");
            var menu = new GameObject("Menu Root");
            menu.transform.SetParent(root.transform);
            var screen = new GameObject("Video Screen").AddComponent<RawImage>();
            screen.transform.SetParent(root.transform);
            var retry = screen.gameObject.AddComponent<Button>();
            var player = root.AddComponent<VideoPlayer>();
            var intro = root.AddComponent<IntroVideoController>();
            intro.Configure(player, screen, retry, menu, music, "intro video.mp4", 5f);

            intro.CompleteIntro();
            yield return null;

            Assert.That(menu.activeSelf, Is.True);
            Assert.That(screen.gameObject.activeSelf, Is.False);
            Assert.That(music.Source.clip, Is.SameAs(clip));
            Assert.That(music.Source.loop, Is.True);
            Assert.That(music.Source.isPlaying, Is.True);

            Object.Destroy(root);
            Object.Destroy(musicObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateMusicPlayerDoesNotCreateSecondPersistentSource()
        {
            var firstObject = new GameObject("First Music");
            var first = firstObject.AddComponent<PersistentMusicPlayer>();
            var secondObject = new GameObject("Second Music");
            secondObject.AddComponent<PersistentMusicPlayer>();
            yield return null;

            Assert.That(PersistentMusicPlayer.Instance, Is.SameAs(first));
            Assert.That(secondObject == null, Is.True);

            Object.Destroy(firstObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MusicSliderUpdatesPreferenceAndPlayingSourceImmediately()
        {
            var musicObject = new GameObject("Persistent Music");
            var music = musicObject.AddComponent<PersistentMusicPlayer>();
            clip = AudioClip.Create("Test Music", 4410, 1, 44100, false);
            music.Configure(clip);
            music.StartMusic();

            var root = new GameObject("Menu View");
            root.SetActive(false);
            var view = root.AddComponent<MainMenuView>();
            var start = CreateButton(root.transform, "Start");
            var settings = CreateButton(root.transform, "Settings");
            var credits = CreateButton(root.transform, "Credits");
            var settingsClose = CreateButton(root.transform, "Settings Close");
            var creditsClose = CreateButton(root.transform, "Credits Close");
            var settingsPanel = new GameObject("Settings Panel");
            var creditsPanel = new GameObject("Credits Panel");
            settingsPanel.transform.SetParent(root.transform);
            creditsPanel.transform.SetParent(root.transform);
            var master = CreateSlider(root.transform, "Master");
            var backgroundMusic = CreateSlider(root.transform, "Music");
            var effects = CreateSlider(root.transform, "Effects");
            var rotation = CreateSlider(root.transform, "Rotation");
            view.Configure(start, settings, credits, settingsClose, creditsClose,
                settingsPanel, creditsPanel, master, backgroundMusic, effects, rotation);
            root.SetActive(true);
            yield return null;

            backgroundMusic.value = 0.31f;
            yield return null;

            Assert.That(PlayerPrefs.GetFloat(StageController.MusicVolumeKey), Is.EqualTo(0.31f).Within(0.001f));
            Assert.That(music.Source.volume, Is.EqualTo(0.31f).Within(0.001f));

            Object.Destroy(root);
            Object.Destroy(musicObject);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var music in Object.FindObjectsByType<PersistentMusicPlayer>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                Object.Destroy(music.gameObject);
            }

            if (clip != null)
            {
                Object.Destroy(clip);
            }

            yield return null;
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            child.transform.SetParent(parent);
            return child.GetComponent<Button>();
        }

        private static Slider CreateSlider(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Slider));
            child.transform.SetParent(parent);
            child.SetActive(false);
            var slider = child.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            child.SetActive(true);
            return slider;
        }
    }
}
