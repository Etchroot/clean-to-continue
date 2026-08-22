using System.Collections;
using CleanToContinue.Audio;
using CleanToContinue.Flow;
using CleanToContinue.Stage;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button settingsCloseButton;
        [SerializeField] private Button creditsCloseButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private Slider masterVolume;
        [SerializeField] private Slider musicVolume;
        [SerializeField] private Slider effectsVolume;
        [SerializeField] private Slider rotationSensitivity;
        private bool startingGame;

        public void Configure(
            Button start,
            Button settings,
            Button credits,
            Button settingsClose,
            Button creditsClose,
            GameObject settingsRoot,
            GameObject creditsRoot,
            Slider master,
            Slider music,
            Slider effects,
            Slider rotation)
        {
            startButton = start;
            settingsButton = settings;
            creditsButton = credits;
            settingsCloseButton = settingsClose;
            creditsCloseButton = creditsClose;
            settingsPanel = settingsRoot;
            creditsPanel = creditsRoot;
            masterVolume = master;
            musicVolume = music;
            effectsVolume = effects;
            rotationSensitivity = rotation;
        }

        public void StartGame()
        {
            if (startingGame)
            {
                return;
            }

            startingGame = true;
            StartMusicFromGesture();
            StartCoroutine(LoadOpeningAfterAudioFrame());
        }

        public void ShowSettings()
        {
            StartMusicFromGesture();
            settingsPanel?.SetActive(true);
            creditsPanel?.SetActive(false);
        }

        public void ShowCredits()
        {
            StartMusicFromGesture();
            creditsPanel?.SetActive(true);
            settingsPanel?.SetActive(false);
        }

        public void HidePanels()
        {
            settingsPanel?.SetActive(false);
            creditsPanel?.SetActive(false);
        }

        private void Awake()
        {
            HidePanels();
            startButton?.onClick.AddListener(StartGame);
            settingsButton?.onClick.AddListener(ShowSettings);
            creditsButton?.onClick.AddListener(ShowCredits);
            settingsCloseButton?.onClick.AddListener(HidePanels);
            creditsCloseButton?.onClick.AddListener(HidePanels);

            InitializeSlider(masterVolume, StageController.MasterVolumeKey, StageController.DefaultMasterVolume);
            InitializeSlider(musicVolume, StageController.MusicVolumeKey, StageController.DefaultMusicVolume);
            InitializeSlider(effectsVolume, StageController.SfxVolumeKey, StageController.DefaultSfxVolume);
            InitializeSlider(
                rotationSensitivity,
                StageController.RotationSensitivityKey,
                StageController.DefaultRotationSensitivity);

            if (masterVolume != null)
            {
                masterVolume.onValueChanged.AddListener(SetMasterVolume);
            }

            if (effectsVolume != null)
            {
                effectsVolume.onValueChanged.AddListener(SetEffectsVolume);
            }

            if (musicVolume != null)
            {
                musicVolume.onValueChanged.AddListener(SetMusicVolume);
            }

            if (rotationSensitivity != null)
            {
                rotationSensitivity.onValueChanged.AddListener(SetRotationSensitivity);
            }
        }

        private void OnDestroy()
        {
            startButton?.onClick.RemoveListener(StartGame);
            settingsButton?.onClick.RemoveListener(ShowSettings);
            creditsButton?.onClick.RemoveListener(ShowCredits);
            settingsCloseButton?.onClick.RemoveListener(HidePanels);
            creditsCloseButton?.onClick.RemoveListener(HidePanels);
            masterVolume?.onValueChanged.RemoveListener(SetMasterVolume);
            musicVolume?.onValueChanged.RemoveListener(SetMusicVolume);
            effectsVolume?.onValueChanged.RemoveListener(SetEffectsVolume);
            rotationSensitivity?.onValueChanged.RemoveListener(SetRotationSensitivity);
        }

        private static void InitializeSlider(Slider slider, string key, float fallback)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, fallback));
            }
        }

        private static void SetMasterVolume(float value)
        {
            PlayerPrefs.SetFloat(StageController.MasterVolumeKey, value);
            AudioListener.volume = Mathf.Clamp01(value);
        }

        private static void SetEffectsVolume(float value)
        {
            PlayerPrefs.SetFloat(StageController.SfxVolumeKey, value);
        }

        private static void SetMusicVolume(float value)
        {
            var clamped = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(StageController.MusicVolumeKey, clamped);
            PersistentMusicPlayer.Instance?.SetMusicVolume(clamped);
        }

        private static void SetRotationSensitivity(float value)
        {
            PlayerPrefs.SetFloat(StageController.RotationSensitivityKey, value);
        }

        private static void StartMusicFromGesture()
        {
            PersistentMusicPlayer.Instance?.StartMusicFromUserGesture();
        }

        private static IEnumerator LoadOpeningAfterAudioFrame()
        {
            yield return null;
            SceneFlow.Load("02.Opening");
        }
    }
}
