using System;
using CleanToContinue.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class MemoryPanelView : MonoBehaviour
    {
        public const string TemporaryContinueScene = "01.MainMenu";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image backgroundDimmer;
        [SerializeField] private Image memoryImage;
        [SerializeField] private Text memoryLine;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string nextSceneName = TemporaryContinueScene;
        [SerializeField] private string mainMenuSceneName = TemporaryContinueScene;
        [SerializeField] private Sprite mouseMemorySprite;
        [SerializeField] private string mouseMemoryText = "그때는 바라보는 것만으로도 새로운 세계가 열렸다.";

        private bool isOpen;

        public bool IsOpen => isOpen;
        public string NextSceneName => nextSceneName;
        public string MainMenuSceneName => mainMenuSceneName;

        public event Action Opened;

        public void Configure(
            GameObject root,
            Image dimmer,
            Image image,
            Text line,
            Button nextAction,
            Button menuAction,
            string nextScene,
            Sprite mouseSprite = null,
            string mouseLine = null)
        {
            UnbindButtons();

            panelRoot = root;
            backgroundDimmer = dimmer;
            memoryImage = image;
            memoryLine = line;
            nextStageButton = nextAction;
            mainMenuButton = menuAction;
            nextSceneName = string.IsNullOrWhiteSpace(nextScene) ? TemporaryContinueScene : nextScene;
            mainMenuSceneName = TemporaryContinueScene;
            mouseMemorySprite = mouseSprite;
            if (!string.IsNullOrWhiteSpace(mouseLine))
            {
                mouseMemoryText = mouseLine;
            }

            if (nextStageButton != null)
            {
                nextStageButton.onClick.AddListener(ContinueToNextStage);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void Configure(
            GameObject root,
            Image dimmer,
            Image image,
            Text line,
            Button continueAction,
            Sprite mouseSprite = null,
            string mouseLine = null)
        {
            Configure(
                root,
                dimmer,
                image,
                line,
                null,
                continueAction,
                TemporaryContinueScene,
                mouseSprite,
                mouseLine);
        }

        public void OpenMouseMemory()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            if (backgroundDimmer != null)
            {
                var color = backgroundDimmer.color;
                color.a = Mathf.Max(color.a, 0.78f);
                backgroundDimmer.color = color;
            }

            if (memoryImage != null)
            {
                memoryImage.sprite = mouseMemorySprite;
            }

            if (memoryLine != null)
            {
                memoryLine.text = mouseMemoryText;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            Opened?.Invoke();
        }

        public void ContinueToNextStage()
        {
            SceneFlow.Load(nextSceneName);
        }

        public void ReturnToMainMenu()
        {
            SceneFlow.Load(mainMenuSceneName);
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        private void UnbindButtons()
        {
            if (nextStageButton != null)
            {
                nextStageButton.onClick.RemoveListener(ContinueToNextStage);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }
        }
    }
}
