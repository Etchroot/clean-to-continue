using CleanToContinue.Stage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanToContinue.Audio
{
    public sealed class PersistentMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        private bool playRequested;

        public static PersistentMusicPlayer Instance { get; private set; }
        public AudioSource Source => source;

        public void Configure(AudioClip clip)
        {
            EnsureSource();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            SetMusicVolume(PlayerPrefs.GetFloat(
                StageController.MusicVolumeKey,
                StageController.DefaultMusicVolume));
        }

        public void StartMusic()
        {
            playRequested = true;
            PlayIfReady();
        }

        private void PlayIfReady()
        {
            EnsureSource();
            if (source.clip != null && !source.isPlaying)
            {
                source.Play();
            }
        }

        public void RequestMusicStart()
        {
            StartMusic();
        }

        public void StartMusicFromUserGesture()
        {
            StartMusic();
        }

        public void SetMusicVolume(float value)
        {
            EnsureSource();
            source.volume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSource();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            if (playRequested)
            {
                PlayIfReady();
            }
        }

        private void EnsureSource()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
                if (source == null)
                {
                    source = gameObject.AddComponent<AudioSource>();
                }
            }
        }
    }
}
