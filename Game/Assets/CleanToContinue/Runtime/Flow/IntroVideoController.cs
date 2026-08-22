using System.Collections;
using System.IO;
using CleanToContinue.Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CleanToContinue.Flow
{
    public sealed class IntroVideoController : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoScreen;
        [SerializeField] private Button retrySurface;
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private PersistentMusicPlayer musicPlayer;
        [SerializeField] private string streamingFileName = "intro video.mp4";
        [SerializeField] private float prepareTimeoutSeconds = 8f;
        [SerializeField] private float playbackFallbackSeconds = 11.5f;

        private static bool hasPlayedThisSession;
        private bool completed;

        public static bool HasPlayedThisSession => hasPlayedThisSession;

        public void Configure(
            VideoPlayer player,
            RawImage screen,
            Button retry,
            GameObject menu,
            PersistentMusicPlayer music,
            string fileName,
            float timeoutSeconds)
        {
            Unbind();
            videoPlayer = player;
            videoScreen = screen;
            retrySurface = retry;
            menuRoot = menu;
            musicPlayer = music;
            streamingFileName = string.IsNullOrWhiteSpace(fileName) ? "intro video.mp4" : fileName;
            prepareTimeoutSeconds = Mathf.Max(0.1f, timeoutSeconds);
            Bind();
        }

        public void CompleteIntro()
        {
            if (completed)
            {
                return;
            }

            completed = true;
            hasPlayedThisSession = true;
            if (videoScreen != null)
            {
                videoScreen.gameObject.SetActive(false);
            }

            menuRoot?.SetActive(true);
            musicPlayer?.RequestMusicStart();
        }

        public void RetryPlayback()
        {
            if (completed || videoPlayer == null)
            {
                return;
            }

            musicPlayer?.StartMusicFromUserGesture();
            videoPlayer.Play();
        }

        private void Awake()
        {
            Bind();
        }

        private IEnumerator Start()
        {
            // Request the persistent soundtrack immediately. Web browsers may suspend
            // audio until their first user gesture; RetryPlayback and menu buttons
            // provide that gesture-backed fallback without restarting the track.
            musicPlayer?.RequestMusicStart();

            if (hasPlayedThisSession)
            {
                CompleteIntro();
                yield break;
            }

            menuRoot?.SetActive(false);
            if (videoScreen != null)
            {
                videoScreen.gameObject.SetActive(true);
            }

            if (videoPlayer == null)
            {
                CompleteIntro();
                yield break;
            }

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = Path.Combine(Application.streamingAssetsPath, streamingFileName).Replace('\\', '/');
            videoPlayer.isLooping = false;
            videoPlayer.playOnAwake = false;
            videoPlayer.Prepare();

            var elapsed = 0f;
            while (!completed && !videoPlayer.isPrepared && elapsed < prepareTimeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!completed && videoPlayer.isPrepared)
            {
                videoPlayer.Play();
                var playbackElapsed = 0f;
                while (!completed && playbackElapsed < playbackFallbackSeconds)
                {
                    playbackElapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (!completed)
                {
                    CompleteIntro();
                }
            }
            else if (!completed)
            {
                CompleteIntro();
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Bind()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= HandleVideoEnded;
                videoPlayer.loopPointReached += HandleVideoEnded;
                videoPlayer.errorReceived -= HandleVideoError;
                videoPlayer.errorReceived += HandleVideoError;
            }

            if (retrySurface != null)
            {
                retrySurface.onClick.RemoveListener(RetryPlayback);
                retrySurface.onClick.AddListener(RetryPlayback);
            }
        }

        private void Unbind()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= HandleVideoEnded;
                videoPlayer.errorReceived -= HandleVideoError;
            }

            retrySurface?.onClick.RemoveListener(RetryPlayback);
        }

        private void HandleVideoEnded(VideoPlayer _) => CompleteIntro();
        private void HandleVideoError(VideoPlayer _, string __) => CompleteIntro();
    }
}
