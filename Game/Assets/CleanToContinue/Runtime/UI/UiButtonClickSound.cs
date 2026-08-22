using CleanToContinue.Audio;
using CleanToContinue.Stage;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class UiButtonClickSound : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlayClick);
        }

        private void OnDestroy()
        {
            button?.onClick.RemoveListener(PlayClick);
        }

        private static void PlayClick()
        {
            PersistentMusicPlayer.Instance?.StartMusicFromUserGesture();
            UiClickAudioPlayer.Play();
        }
    }

    internal sealed class UiClickAudioPlayer : MonoBehaviour
    {
        private const string PlayerName = "__CleanToContinueUiClickAudio";
        private const int SampleRate = 44100;
        private static UiClickAudioPlayer instance;

        private AudioSource source;
        private AudioClip clickClip;

        public static void Play()
        {
            EnsureInstance().PlayClick();
        }

        private static UiClickAudioPlayer EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            var playerObject = new GameObject(PlayerName);
            instance = playerObject.AddComponent<UiClickAudioPlayer>();
            DontDestroyOnLoad(playerObject);
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            clickClip = CreateClickClip();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            if (clickClip != null)
            {
                Destroy(clickClip);
            }
        }

        private void PlayClick()
        {
            var volume = Mathf.Clamp01(PlayerPrefs.GetFloat(
                StageController.SfxVolumeKey,
                StageController.DefaultSfxVolume));
            source.PlayOneShot(clickClip, volume);
        }

        private static AudioClip CreateClickClip()
        {
            const float duration = 0.09f;
            var sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var samples = new float[sampleCount];
            var randomState = 0x51F15EEDu;

            for (var index = 0; index < sampleCount; index++)
            {
                var time = index / (float)SampleRate;
                samples[index] = CreateTransient(time, 0f, 1f, ref randomState)
                                 + CreateTransient(time, 0.032f, 0.58f, ref randomState);
            }

            var clip = AudioClip.Create("UI Dry Click", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float CreateTransient(float time, float onset, float strength, ref uint randomState)
        {
            var localTime = time - onset;
            if (localTime < 0f)
            {
                return 0f;
            }

            randomState = randomState * 1664525u + 1013904223u;
            var noise = ((randomState >> 8) / 16777215f) * 2f - 1f;
            var envelope = Mathf.Exp(-localTime * 95f);
            var body = Mathf.Sin(2f * Mathf.PI * 2100f * localTime);
            return strength * envelope * (noise * 0.22f + body * 0.18f);
        }
    }
}
