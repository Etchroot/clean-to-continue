using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanToContinue.Flow
{
    [DefaultExecutionOrder(-10000)]
    public sealed class SceneTransitionController : MonoBehaviour
    {
        public const float DefaultFadeSeconds = 0.35f;
        public const float EndingFadeSeconds = DefaultFadeSeconds * 2f;
        public const string EndingSceneName = "06.Ending";

        private const string RuntimeObjectName = "__CleanToContinueSceneTransition";

        private static SceneTransitionController instance;
        private CanvasGroup overlay;
        private bool transitioning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeFirstScene()
        {
            EnsureInstance();
        }

        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            EnsureInstance().BeginTransition(sceneName);
        }

        private static SceneTransitionController EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            var root = new GameObject(RuntimeObjectName);
            instance = root.AddComponent<SceneTransitionController>();
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
            DontDestroyOnLoad(gameObject);
            BuildOverlay();
        }

        private IEnumerator Start()
        {
            overlay.alpha = 1f;
            overlay.blocksRaycasts = true;
            yield return FadeTo(0f, DefaultFadeSeconds);
            overlay.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void BeginTransition(string sceneName)
        {
            if (transitioning)
            {
                return;
            }

            StartCoroutine(Transition(sceneName));
        }

        private IEnumerator Transition(string sceneName)
        {
            transitioning = true;
            overlay.blocksRaycasts = true;
            var duration = sceneName == EndingSceneName ? EndingFadeSeconds : DefaultFadeSeconds;

            yield return FadeTo(1f, duration);
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return FadeTo(0f, duration);

            overlay.blocksRaycasts = false;
            transitioning = false;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            var startAlpha = overlay.alpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            overlay.alpha = targetAlpha;
        }

        private void BuildOverlay()
        {
            var canvasObject = new GameObject(
                "TransitionCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            overlay = canvasObject.GetComponent<CanvasGroup>();

            var imageObject = new GameObject("BlackFade", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = imageObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;
        }
    }
}
