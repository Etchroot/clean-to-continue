using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.Flow
{
    public sealed class OpeningSequence : MonoBehaviour
    {
        public const float DefaultDurationSeconds = 10f;
        public const float DefaultLineIntervalSeconds = 3f;
        public const string MouseSceneName = "03.Mouse";

        private static readonly string[] DefaultSentences =
        {
            "얼마만에 생긴 휴식시간인지 모르겠다.",
            "옛날에는 게임을 정말 재밌게 했었는데.",
            "오랜만에 게임이나 해볼까?",
            "그 전에 장비에 쌓인 먼지부터 닦아야겠는걸."
        };

        [SerializeField] private Button skipButton;
        [SerializeField] private Text openingText;
        [SerializeField] private string[] sentences = DefaultSentences;
        [SerializeField] private float lineIntervalSeconds = DefaultLineIntervalSeconds;
        [SerializeField] private string nextScene = MouseSceneName;

        private bool leaving;

        public void Configure(Button skip, float duration = DefaultDurationSeconds, string destination = MouseSceneName)
        {
            skipButton = skip;
            sentences = DefaultSentences;
            lineIntervalSeconds = Mathf.Max(0f, duration / DefaultSentences.Length);
            nextScene = destination;
        }

        public void Configure(
            Button skip,
            Text line,
            string[] lines,
            float interval,
            string destination = MouseSceneName)
        {
            skipButton = skip;
            openingText = line;
            sentences = lines == null || lines.Length == 0 ? DefaultSentences : lines;
            lineIntervalSeconds = Mathf.Max(0f, interval);
            nextScene = string.IsNullOrWhiteSpace(destination) ? MouseSceneName : destination;
        }

        public void Skip()
        {
            LoadNextScene();
        }

        private void Awake()
        {
            skipButton?.onClick.AddListener(Skip);
        }

        private IEnumerator Start()
        {
            if (openingText != null)
            {
                openingText.text = string.Empty;
            }

            for (var index = 0; index < sentences.Length; index++)
            {
                if (openingText != null)
                {
                    openingText.text = index == 0
                        ? sentences[index]
                        : openingText.text + "\n" + sentences[index];
                }

                yield return new WaitForSecondsRealtime(lineIntervalSeconds);
            }

            LoadNextScene();
        }

        private void OnDestroy()
        {
            skipButton?.onClick.RemoveListener(Skip);
        }

        private void LoadNextScene()
        {
            if (leaving)
            {
                return;
            }

            leaving = true;
            SceneFlow.Load(nextScene);
        }
    }
}
